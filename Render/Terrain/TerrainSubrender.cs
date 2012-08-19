using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Garm.Base.Interfaces;
using SlimDX;
using SlimDX.D3DCompiler;
using SlimDX.Direct3D11;
using SlimDX.DXGI;
using Buffer = SlimDX.Direct3D11.Buffer;
using BTerrain = Garm.Base.Content.Terrain.Terrain;

namespace Garm.View.Human.Render.Terrain
{
    public class TerrainSubrender : Base.Abstract.Base, IRenderable
    {
        protected RenderManager Renderer;
        protected Base.Content.Terrain.Terrain CurrentTerrain;
        protected bool FrustumCheck;
        protected float[] LodDistancesSquared;
        protected int[] LodFactors;
        protected int Lods;
        protected int QuadCountZ;
        protected int QuadEdgeLength;
        protected Quad[] Quads;
        protected QuadContainer FrustumTree;
        protected int[] QuadLods;
        protected Dictionary<uint, KeyValuePair<Buffer, int>> Indexbuffers;
        //Indexbufferkey: 12bit stripeWidth/stitchLod 12bit ownLod 8bit content (0 = mid, 0x1 top, 0x2 bot, 0x4 left, 0x8 right, 0x10 stitchflag)
        protected bool Disposed;
        protected Effect Effect;
        private EffectTechnique _technique;
        private EffectPass _pass;
        private InputLayout _vertexLayout;



        public TerrainSubrender(BTerrain terrain, RenderManager renderer, IRunManager manager) : base(manager)
        {
            Renderer = renderer;
            
            FrustumCheck = Manager.Opts.Get<bool>("rndr_terrain_frustumCheck");
            NotifyHandlers.Add(Manager.Opts.RegisterChangeNotification("rndr_terrain_frustumCheck", delegate(string key, object value) { FrustumCheck = (bool)value; }));
            try
            {
                Lods = Manager.Opts.Get<int>("rndr_terrain_lods");
                if (Lods > 1)
                {
                    LodDistancesSquared = new float[Lods - 1];
                    LodFactors = new int[Lods -1];
                    var list = Manager.Opts.Get<List<float>>("rndr_terrain_lodDistances");
                    var list2 = Manager.Opts.Get<List<int>>("rndr_terrain_lodFactors");
                    for (int i = 0; i < Lods-1; i++)
                    {
                        LodDistancesSquared[i] = list[i] * list[i];
                        LodFactors[i] = list2[i];
                    }
                }
            }
            catch (Exception)
            {
                Console.WriteLine("[Error] Could not read terrain-LOD settings, LOD disabled!");
                Lods = 1;
                LodDistancesSquared = null;
            }
            var shaderdeffile = Manager.Files.Get(@"Shaders\Terrain.hlsl");
            var bbuffer = new byte[shaderdeffile.Length];
            shaderdeffile.Read(bbuffer, 0, bbuffer.Length);
            shaderdeffile.Dispose();
            var bytecode = ShaderBytecode.Compile(
                Encoding.ASCII.GetBytes(
                    Encoding.ASCII.GetString(bbuffer).Replace("%TEXTURECOUNT%", "2").Replace("%OVERLAYCOUNT%", "1"))
                , "fx_5_0");
            bbuffer = null;
            Effect = new Effect(Renderer.D3DDevice, bytecode);
            bytecode.Dispose();
            _technique = Effect.GetTechniqueByName("Terrain");
            _pass = _technique.GetPassByIndex(0);

            _vertexLayout = new InputLayout(Renderer.D3DDevice, _pass.Description.Signature, new[]
                { new InputElement("POSITION", 0, Format.R32G32B32_Float, 0),
                new InputElement("NORMAL", 0, Format.R32G32B32_Float, 0),
                new InputElement("TEXCOORD",0,Format.R32G32_Float, 0) });

            Effect.GetVariableByName("texRepeat").AsScalar().Set(new[] { 0.1f, 0.1f });

            var sampleMode = SamplerState.FromDescription(Renderer.D3DDevice, new SamplerDescription()
            {
                AddressU = TextureAddressMode.Wrap,
                AddressV = TextureAddressMode.Wrap,
                AddressW = TextureAddressMode.Clamp,
                BorderColor = new Color4(0, 0, 0, 0),
                Filter = Filter.MinMagMipLinear,
                ComparisonFunction = Comparison.Always,
                MipLodBias = 0f,
                MaximumAnisotropy = 8,
                MinimumLod = 0f,
                MaximumLod = float.MaxValue
            });
            Effect.GetVariableByName("textureSampler").AsSampler().SetSamplerState(0, sampleMode);
            Effect.GetVariableByName("alphaSampler").AsSampler().SetSamplerState(0, sampleMode);

            
            UpdateStaticVars();

            CurrentTerrain = terrain;
            QuadEdgeLength = Manager.Opts.Get<int>("rndr_terrain_quadVerticesPerEdge");

            var verticesX = terrain.PointsX;
            var verticesZ = terrain.PointsZ;
            var quadsX = (int)Math.Ceiling((double)(verticesX - 1) / (QuadEdgeLength - 1));
            var quadsZ = (int)Math.Ceiling((double)(verticesZ - 1) / (QuadEdgeLength - 1));
#if DEBUG
            if ((verticesX - 1) % (QuadEdgeLength - 1) != 0)
                Console.WriteLine("[Info] The terrain X-Vertices are no multiple of the quad-length, filling to fit IndexBuffers");
            if ((verticesZ - 1) % (QuadEdgeLength - 1) != 0)
                Console.WriteLine("[Info] The terrain Z-Vertices are no multiple of the quad-length, filling to fit IndexBuffers");
#endif
            Quads = new Quad[quadsX*quadsZ];
            QuadCountZ = quadsZ;
            for (int i = 0; i < quadsX; i++)
            {
                for (int j = 0; j < quadsZ; j++)
                {
                    Quads[i * quadsZ + j] = new Quad(i * (QuadEdgeLength - 1), (i + 1) * (QuadEdgeLength - 1), j * (QuadEdgeLength - 1), (j + 1) * (QuadEdgeLength - 1), CurrentTerrain, Renderer);
                }
            }

            FrustumTree = new QuadContainer(0,quadsX-1, 0, quadsZ-1, ref Quads, QuadCountZ);

            QuadLods = new int[Quads.Length];
            Indexbuffers = new Dictionary<uint, KeyValuePair<Buffer, int>>();
        }

        public override void Dispose()
        {
            Disposed = true;
            foreach (ValueChangedHandler notifyHandler in NotifyHandlers)
            {
                Manager.Opts.UnregisterChangeNotification(notifyHandler);
            }
            foreach(Quad quad in Quads)
                quad.Dispose();
        }

        public void Render()
        {
            if (Disposed)
                return;
            Effect.GetVariableByName("viewMatrix").AsMatrix().SetMatrix(Renderer.ViewMatrix);
            _pass.Apply(Renderer.Context);
            Renderer.Context.InputAssembler.InputLayout = _vertexLayout;

            IEnumerable<int> quadIndices;
            if (FrustumCheck)
                quadIndices = FrustumTree.GetQuadsInFrustum(Renderer.ViewerFrustum, true);
            else
            {
                quadIndices = new List<int>(Quads.Length);
                for (int i = 0; i < Quads.Length; i++)
                    ((List<int>)quadIndices).Add(i);
            }

            for (int i = 0; i < Quads.Length; i++)
                QuadLods[i] = int.MaxValue;
            
            foreach (var quadIndex in quadIndices)
            {
                var xdist = (Quads[quadIndex].HorizontalCenter.X - Renderer.ViewerLocation.X) * (Quads[quadIndex].HorizontalCenter.X - Renderer.ViewerLocation.X);
                var zdist = (Quads[quadIndex].HorizontalCenter.Y - Renderer.ViewerLocation.Z) * (Quads[quadIndex].HorizontalCenter.Y - Renderer.ViewerLocation.Z);
                var distancesqared = Math.Max(xdist, zdist);
                int lod = 0;
                if (Lods > 1)
                    while (true)
                    {
                        if (lod == Lods - 1)
                            break; //MinimumLodReached
                        if (LodDistancesSquared[lod] > distancesqared)
                            break; //Distance closer than next Lod
                        lod++;
                    }
                if (lod != 0)
                    lod = LodFactors[lod - 1];
                QuadLods[quadIndex] = lod;
            }



            for (int i = 0; i < Quads.Length; i++)
            {
                if (QuadLods[i] == int.MaxValue)
                    continue;
                Renderer.Context.InputAssembler.SetVertexBuffers(0, Quads[i].VertexBuffer);
                Renderer.Context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleStrip;
                int lod = QuadLods[i];
                int stripewidth = lod +1;
                int stripe = stripewidth;//start mid part with one stripe less for the border to fit in

                var indices = SetIndexBuffer(new IndexBufferDef { Highword = stripewidth, Lod = lod, Border = 0 });

                while (stripe < QuadEdgeLength - 2*stripewidth)//end mid part with enough space left for last normal stripe with variable width and border
                {
                    Renderer.Context.DrawIndexed(indices, 0, QuadEdgeLength * (int)stripe);
                    stripe += stripewidth;
                }

                indices = SetIndexBuffer(new IndexBufferDef { Highword = (QuadEdgeLength - stripe - 1 - stripewidth), Lod = lod, Border = 0 }); //last normal stripe, leaving enough space for border while chlinching to fill quad
                Renderer.Context.DrawIndexed(indices, 0, QuadEdgeLength * stripe);

                if (!Manager.Opts.Get<bool>("rndr_terrain_renderEdges"))
                    continue;

                Renderer.Context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
                var borderids = new [] {
                    new IndexBufferDef { Border = 0x1, Lod = lod },
                    new IndexBufferDef { Border = 0x2, Lod = lod },
                    new IndexBufferDef { Border = 0x4, Lod = lod },
                    new IndexBufferDef { Border = 0x8, Lod = lod } };
                if (lod != 0)//Stitching detection follows; LOD 0 never stitches
                {
                    //Check top neighbor
                    if (i % CurrentTerrain.PointsZ > 0 && QuadLods[i - 1] < lod)
                    {
                        borderids[0].Border += 0x10;
                        borderids[0].Highword = QuadLods[i - 1];
                    }
                    //Check bot neighbor
                    if (i % CurrentTerrain.PointsZ < CurrentTerrain.PointsZ - 1 && QuadLods[i + 1] < lod)
                    {
                        borderids[1].Border += 0x10;
                        borderids[1].Highword = QuadLods[i + 1];
                    }
                    //Check left neighbor
                    if (i >= CurrentTerrain.PointsZ && QuadLods[i - CurrentTerrain.PointsZ] < lod)
                    {
                        borderids[2].Border += 0x10;
                        borderids[2].Highword = QuadLods[i - CurrentTerrain.PointsZ];
                    }
                    //Check right neighbor
                    if (i + CurrentTerrain.PointsZ < Quads.Length && QuadLods[i + CurrentTerrain.PointsZ] < lod)
                    {
                        borderids[3].Border += 0x10;
                        borderids[3].Highword = QuadLods[i + CurrentTerrain.PointsZ];
                    }
                }
                for(int j = 0; j < /*4*/1; j++)
                {
                    indices = SetIndexBuffer(borderids[j]);
                    Renderer.Context.DrawIndexed(indices, 0, 0);
                }
            }
        }

        public void UpdateStaticVars()
        {
            Effect.GetVariableByName("projectionMatrix").AsMatrix().SetMatrix(Renderer.GeoProjectionMatrix);
        }

        public void RenderTransparent()
        {
            
        }

        public void RenderStaticShadows()
        {
            
        }

        public void RenderDynamicShadows()
        {

        }

        protected int SetIndexBuffer(IndexBufferDef indexbufferid)
        {
            KeyValuePair<Buffer, int> indexbuffer;
            if (Indexbuffers.ContainsKey(indexbufferid.Id))
                indexbuffer = Indexbuffers[indexbufferid.Id];
            else
                indexbuffer = AllocateIndexBuffer(indexbufferid);
            Renderer.Context.InputAssembler.SetIndexBuffer(indexbuffer.Key, Format.R16_UInt, 0);
            return indexbuffer.Value;
        }

        protected KeyValuePair<Buffer, int> AllocateIndexBuffer(IndexBufferDef indexbufferid)
        {
            var lod = indexbufferid.Lod;
            var highword = indexbufferid.Highword;
            var indicesStream = new DataStream(sizeof(uint) * QuadEdgeLength * (indexbufferid.Border == 0 ? 2 : 6), true, true);
            var indicesCount = 0;
            var borderwidth = 1 + lod;
            switch (indexbufferid.Id & 0xF)
            {
                case 0x0://Quad Indices
                    for (int i = borderwidth; i < QuadEdgeLength - borderwidth; i++)
                    {
                        indicesStream.Write((short)(QuadEdgeLength * highword + i));
                        indicesStream.Write((short)i);
                        indicesCount += 2;
                        for (int j = 0; j < lod; j++)
                            if (i + 2 < QuadEdgeLength - borderwidth)//So last Index is always part
                                i++;
                            else
                                break;
                    }
                    break;
                case 0x1://Border top
                    if ((indexbufferid.Id & 0x10) == 0x10)
                    {//stitch
                        #region BuildIndicesLists
                        var IndicesA = new List<int>();
                        for (int i = 0; i < QuadEdgeLength - borderwidth; i++)
                        {
                            IndicesA.Add(i * QuadEdgeLength);// * QuadEdgeLength for horizontal iteration (top & bot border)
                            for (uint j = 0; j < lod; j++)
                                if (i + 2 < QuadEdgeLength - borderwidth)//So last Index is always part
                                    i++;
                                else
                                    break;
                        }
                        IndicesA.Add(QuadEdgeLength-1);

                        var IndicesB = new List<int>();
                        var borderwidthB = (highword + 1);
                        for (int i = 0; i < QuadEdgeLength - borderwidthB; i++)
                        {
                            IndicesB.Add(i * QuadEdgeLength);// * QuadEdgeLength for horizontal iteration (top & bot border)
                            for (int j = 0; j < highword; j++)
                                if (i + 2 < QuadEdgeLength - borderwidthB)
                                    i++;
                                else
                                    break;
                        }
                        IndicesB.Add(QuadEdgeLength - 1);
                        #endregion
                        #region DrawStitchedBorder
                        int edgeanchor = IndicesA[1];
                        for (int i = 0; IndicesB[i] < edgeanchor; i++)
                        {//Draw left edge
                            indicesStream.Write((short)(edgeanchor + borderwidth));
                            indicesStream.Write((short)(IndicesB[i+1]));
                            indicesStream.Write((short)(IndicesB[i]));
                            indicesCount += 3;
                        }

                        for(int i = 1; i < IndicesA.Count - 2; i++)//-1 because right edge needs to be drawn seperately and -1 because each iteration uses i and i+1 as anchors
                        {
                            var anchorA = IndicesA[i];
                            var anchorB = IndicesA[i+1];
                            var partnerpoints = IndicesB.Where(index => index >= anchorA && index < anchorB).ToList();
                            partnerpoints.Add(IndicesB.First(index=> index>= anchorB));
                            var firstsmall = partnerpoints.Count/2;
                            for (int j = 0; j < firstsmall; j++)
                            {
                                indicesStream.Write((short)(anchorA + borderwidth));
                                indicesStream.Write((short)(partnerpoints[j + 1]));
                                indicesStream.Write((short)(partnerpoints[j]));
                                indicesCount += 3;
                            }
                            indicesStream.Write((short)(anchorA + borderwidth));
                            indicesStream.Write((short)(anchorB + borderwidth));
                            indicesStream.Write((short)(partnerpoints[firstsmall]));
                            indicesCount += 3;
                            for (int j = firstsmall; j < partnerpoints.Count-1; j++)
                            {
                                indicesStream.Write((short)(anchorB + borderwidth));
                                indicesStream.Write((short)(partnerpoints[j + 1]));
                                indicesStream.Write((short)(partnerpoints[j]));
                                indicesCount += 3;
                            }
                        }

                        edgeanchor = IndicesA[IndicesA.Count-2];
                        for (int i = IndicesB.Count-2; IndicesB[i] >= edgeanchor; i--)
                        {//Draw left edge
                            indicesStream.Write((short)(edgeanchor + borderwidth));
                            indicesStream.Write((short)(IndicesB[i+1]));
                            indicesStream.Write((short)(IndicesB[i]));
                            indicesCount += 3;
                        }
                        #endregion
                    }
                    else
                    {//nostitch
                        #region DrawNonStitchedBorder
                        indicesStream.Write((short)(0));
                        indicesStream.Write((short)((lod + 1)*QuadEdgeLength + borderwidth));
                        indicesStream.Write((short)((lod + 1) * QuadEdgeLength));
                        indicesCount += 3;

                        int i = borderwidth;
                        int nexti = i;
                        while(i < QuadEdgeLength - 1 - borderwidth)
                        {
                            for (int j = 0; j < lod + 1; j++)
                                if (nexti + 1 < QuadEdgeLength - borderwidth)//So last Index is always part
                                    nexti++;
                                else
                                    break; //j-loop
                            indicesStream.Write((short)(i*QuadEdgeLength));
                            indicesStream.Write((short)(i*QuadEdgeLength + borderwidth));
                            indicesStream.Write((short)(nexti * QuadEdgeLength));

                            indicesStream.Write((short)(nexti * QuadEdgeLength));
                            indicesStream.Write((short)(i*QuadEdgeLength + borderwidth));
                            indicesStream.Write((short)(nexti * QuadEdgeLength + borderwidth));

                            indicesCount += 6;
                            i = nexti;
                        }
                        int k = QuadEdgeLength - borderwidth - 1;
                        indicesStream.Write((short)(k*QuadEdgeLength));
                        indicesStream.Write((short)(k*QuadEdgeLength + borderwidth));
                        indicesStream.Write((short)((QuadEdgeLength - 1) * QuadEdgeLength));
                        indicesCount += 3;

                        indicesCount += 3;
                        #endregion
                    }
                    break;
                case 0x2://Border bot
                    
                    break;
                case 0x4://Border left
                    
                    break;
                case 0x8://Border right
                    
                    break;
            }
            indicesStream.Position = 0;
            var buffer = new Buffer(Renderer.D3DDevice, indicesStream, sizeof(uint) * indicesCount, ResourceUsage.Default, BindFlags.IndexBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
            var indexbuffer = new KeyValuePair<Buffer, int>(buffer, indicesCount);
            Indexbuffers.Add(indexbufferid.Id, indexbuffer);
            indicesStream.Dispose();
            return indexbuffer;
        }
    }
}
