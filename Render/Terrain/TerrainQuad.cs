using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX;
using SlimDX.Direct3D11;
using Buffer = SlimDX.Direct3D11.Buffer;

namespace Garm.View.Human.Render.Terrain
{
    public class Quad : IDisposable
    {
        public struct QuadBounds
        {
            public float MinX;
            public float MaxX;
            public float MinY;
            public float MaxY;
            public float MinZ;
            public float MaxZ;
        }

        private QuadBounds _bounds;
        public QuadBounds Bounds
        {
            get { return _bounds; }
        }

        public Vector2 HorizontalCenter { get; private set; }
        protected Buffer VBuffer;
        public VertexBufferBinding VertexBuffer;

        public Quad(int startX,int endX, int startZ, int endZ, Base.Content.Terrain.Terrain terrain, RenderManager renderer)
        {
            _bounds = new QuadBounds
            {
                MinX = startX / terrain.PointsPerMeter,
                MaxX = endX / terrain.PointsPerMeter,
                MinZ = startZ / terrain.PointsPerMeter,
                MaxZ = endZ / terrain.PointsPerMeter,
                MinY = terrain.Height[0],
                MaxY = terrain.Height[0]
            };
            HorizontalCenter = new Vector2(Bounds.MinX + (Bounds.MaxX - Bounds.MinX) / 2, Bounds.MinZ + (Bounds.MaxZ - Bounds.MinZ) / 2);

            int verticesX = endX - startX + 1;
            int verticesZ = endZ - startZ + 1;

            var dataStream = new DataStream(32 * verticesX * verticesZ, true, true);

            for (int i = 0; i < verticesX; i++)
            {
                for (int j = 0; j < verticesZ; j++)
                {
                    //Position
                    int xindex = Math.Min(i + startX, terrain.PointsX - 1);//Clamp to arraybounds if neccessary
                    int zindex = Math.Min(j + startZ, terrain.PointsZ - 1);//(Quadsize needs to be consistent for sharing IndexBuffers)
                    float x = xindex / terrain.PointsPerMeter;
                    float z = zindex / terrain.PointsPerMeter;
                    float y = terrain.Height[xindex * terrain.PointsZ + zindex];
                    dataStream.Write(new Vector3(x, y, z));

                    //Normal
                    float deltax = (terrain.Height[(xindex < terrain.PointsX - 1 ? xindex + 1 : xindex) * terrain.PointsZ + zindex]
                        - terrain.Height[(xindex != 0 ? xindex - 1 : xindex) * terrain.PointsZ + zindex]);

                    float deltaz = (terrain.Height[xindex * terrain.PointsZ + (zindex < terrain.PointsZ - 1 ? zindex + 1 : zindex)]
                        - terrain.Height[xindex * terrain.PointsZ + (zindex != 0 ? zindex - 1 : zindex)]);
                    if (xindex == 0 || xindex == terrain.PointsX - 1)
                        deltax *= 2;
                    if (zindex == 0 || zindex == terrain.PointsZ - 1)
                        deltaz *= 2;
                    var normal = new Vector3(-deltax, 2 / terrain.PointsPerMeter, deltaz);
                    normal.Normalize();
                    dataStream.Write(normal);

                    //TextureCoordinates
                    dataStream.Write(new Vector2(x / terrain.PointsX, z / terrain.PointsZ));

                    //Boundingbox-Params
                    if (y < _bounds.MinY)
                        _bounds.MinY = y;
                    if (y > _bounds.MaxY)
                        _bounds.MaxY = y;
                }
            }

            dataStream.Position = 0;
            VBuffer = new Buffer(renderer.D3DDevice, dataStream, 32 * verticesX * verticesZ, ResourceUsage.Default, BindFlags.VertexBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
            VertexBuffer = new VertexBufferBinding(VBuffer, 32, 0);
            dataStream.Dispose();
        }

        public void Dispose()
        {
            if (VBuffer != null)
                VBuffer.Dispose();
        }
    }
}
