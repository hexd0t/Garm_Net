using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using Garm.Base.Helper;
using Garm.Base.Interfaces;
using SlimDX;
using SlimDX.D3DCompiler;
using SlimDX.Direct3D11;
using SlimDX.DXGI;
using SlimDX.Windows;
using Buffer = SlimDX.Direct3D11.Buffer;
using Device = SlimDX.Direct3D11.Device;
using Resource = SlimDX.Direct3D11.Resource;

namespace Garm.View.Human.Render
{
    public class RenderManager : Base.Abstract.Base
    {
        public Device D3DDevice;
        public SwapChain SChain;
        public DeviceContext Context;
        public Control Output;
        public RenderTargetView RTVScreen;
        public bool Disposed;

        public Viewport OutputViewport;
        public Texture2D DiffuseTexture, ZTexture, NormalTexture;
        public RenderTargetView[] RTVs;
        public DepthStencilView DepthBufferView;
        public ShaderResourceView DepthBufferShaderView, DiffuseShaderView, NormalShaderView;

        public RasterizerState SceneRasterizer;

        public RenderablesList Content;

        public Matrix GeoProjectionMatrix;
        public Matrix ViewMatrix;
        public Vector3 CameraLocation
        {
            get { return _camIncorporeal ? _camLocationIncorporeal : ViewerLocation; }
            set { if (_camIncorporeal) _camLocationIncorporeal = value; else ViewerLocation = value; }
        }
        public Vector3 CameraLookAt
        {
            get { return _camIncorporeal ? _camLookAtIncorporeal : ViewerLookAt; }
            set { if (_camIncorporeal) _camLookAtIncorporeal = value; else ViewerLookAt = value; }
        }
        public Vector3 CameraUpVector
        {
            get { return _camIncorporeal ? _camUpVectorIncorporeal : ViewerUpVector; }
            set { if (_camIncorporeal) _camUpVectorIncorporeal = value; else ViewerUpVector = value; }
        }
        private Vector3 _camLocationIncorporeal;
        private Vector3 _camLookAtIncorporeal;
        private Vector3 _camUpVectorIncorporeal;
        private bool _camIncorporeal;
        public Vector3 ViewerLocation;
        public Vector3 ViewerLookAt;
        public Vector3 ViewerUpVector;
        public Frustum ViewerFrustum;
        public double CurrentFps {get { return _fpsRingbuffer.Sum() / _fpsRingbuffer.Length;}}
        private double[] _fpsRingbuffer;
        private int _fpsRingbufferIndex;
        private HTimer _fpsTimer;

        private Effect _effect;
        private EffectTechnique _composeTechnique;
        private EffectPass _composePass;
        private Buffer _composeVertices;
        private VertexBufferBinding _composeVerticesBB;
        private InputLayout _composeLayout;
        private RasterizerState _composeRasterizer;

        public RenderManager(IRunManager manager, Control output) : base(manager)
        {
            Output = output;
            Content = new RenderablesList();
        }

        public void Initialize()
        {
            var swapDescription = new SwapChainDescription()
            {
                BufferCount = 1,
                Usage = Usage.RenderTargetOutput,
                OutputHandle = Output.Handle,
                IsWindowed = true,
                ModeDescription =
                    new ModeDescription(0, 0, new Rational(60, 1), Format.R8G8B8A8_UNorm),
                SampleDescription = new SampleDescription(1, 0),
                Flags = SwapChainFlags.AllowModeSwitch,
                SwapEffect = SwapEffect.Discard
            };
            Device.CreateWithSwapChain(DriverType.Hardware, DeviceCreationFlags.None,new[]{FeatureLevel.Level_10_0}, swapDescription, out D3DDevice,
                                       out SChain);
            Context = D3DDevice.ImmediateContext;
            
            EnableWinEvents();
            InitOnce();
            InitDevice();
        }

        protected void EnableWinEvents ()
        {
            using (var factory = SChain.GetParent<Factory>())
                factory.SetWindowAssociation(Output.Handle, WindowAssociationFlags.IgnoreAltEnter);
            Output.KeyDown += (o, e) =>
            {
                if (e.Alt && e.KeyCode == Keys.Enter)
                    SChain.IsFullScreen = !SChain.IsFullScreen;
            };

            Output.Resize += (o, e) => ResetDevice();
        }

        protected void ResetDevice()
        {
            DisposeTexturesRTVs();
            SChain.ResizeBuffers(2, 0, 0, Format.R8G8B8A8_UNorm, SwapChainFlags.AllowModeSwitch);

            InitDevice();
        }

        public override void Dispose()
        {
            Disposed = true;
            base.Dispose();
            DisposeTexturesRTVs();
            _composeLayout.Dispose();
            _composeVertices.Dispose();
            _effect.Dispose();
            SceneRasterizer.Dispose();
            _composeRasterizer.Dispose();
            SChain.Dispose();
            D3DDevice.Dispose();
        }

        protected void DisposeTexturesRTVs()
        {
            RTVScreen.Dispose();
            RTVs[0].Dispose();
            DiffuseShaderView.Dispose();
            DiffuseTexture.Dispose();
            RTVs[1].Dispose();
            NormalShaderView.Dispose();
            NormalTexture.Dispose();
            DepthBufferView.Dispose();
            DepthBufferShaderView.Dispose();
            ZTexture.Dispose();
        }

        protected void InitDevice()
        {
            OutputViewport = new Viewport(0.0f, 0.0f, Output.ClientSize.Width, Output.ClientSize.Height);

            using (var resource = Resource.FromSwapChain<Texture2D>(SChain, 0))
                RTVScreen = new RenderTargetView(D3DDevice, resource);

            #region GBufferInit
            RTVs = new RenderTargetView[2];
            DiffuseTexture = new Texture2D(D3DDevice, new Texture2DDescription
            {
                Width = Output.ClientSize.Width,
                Height = Output.ClientSize.Height,
                MipLevels = 1,
                ArraySize = 1,
                Format = Format.R16G16B16A16_Float,
                Usage = ResourceUsage.Default,
                SampleDescription = new SampleDescription(1, 0),
                BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None
            });

            RTVs[0] = new RenderTargetView(D3DDevice, DiffuseTexture, new RenderTargetViewDescription()
                                                                             {
                                                                                 Format = DiffuseTexture.Description.Format,
                                                                                 Dimension = RenderTargetViewDimension.Texture2D,
                                                                                 MipSlice = 0
                                                                             });
            DiffuseShaderView = new ShaderResourceView(D3DDevice, DiffuseTexture, new ShaderResourceViewDescription()
                                                                                      {
                                                                                          Format = DiffuseTexture.Description.Format,
                                                                                          Dimension = ShaderResourceViewDimension.Texture2D,
                                                                                          MostDetailedMip = 0,
                                                                                          MipLevels = 1
                                                                                      });
            _effect.GetVariableByName("composeDiffuse").AsResource().SetResource(DiffuseShaderView);
            
            NormalTexture = new Texture2D(D3DDevice, new Texture2DDescription
            {
                Width = Output.ClientSize.Width,
                Height = Output.ClientSize.Height,
                MipLevels = 1,
                ArraySize = 1,
                Format = Format.R16G16B16A16_Float,
                Usage = ResourceUsage.Default,
                SampleDescription = new SampleDescription(1, 0),
                BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None
            });
            RTVs[1] = new RenderTargetView(D3DDevice, NormalTexture, new RenderTargetViewDescription()
            {
                Format = NormalTexture.Description.Format,
                Dimension = RenderTargetViewDimension.Texture2D,
                MipSlice = 0
            });
            NormalShaderView = new ShaderResourceView(D3DDevice, NormalTexture, new ShaderResourceViewDescription()
            {
                Format = NormalTexture.Description.Format,
                Dimension = ShaderResourceViewDimension.Texture2D,
                MostDetailedMip = 0,
                MipLevels = 1
            });
            _effect.GetVariableByName("composeNormal").AsResource().SetResource(NormalShaderView);

            ZTexture = new Texture2D(D3DDevice, new Texture2DDescription
            {
                Width = Output.ClientSize.Width,
                Height = Output.ClientSize.Height,
                MipLevels = 1,
                ArraySize = 1,
                Format = Format.R32_Typeless,
                Usage = ResourceUsage.Default,
                SampleDescription = new SampleDescription(1, 0),
                BindFlags = BindFlags.DepthStencil | BindFlags.ShaderResource,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None
            });

            var depthBufferViewDesc = new DepthStencilViewDescription
            {
                Format = Format.D32_Float,
                Dimension = DepthStencilViewDimension.Texture2D,
                FirstArraySlice = 0,
                ArraySize = 1,
                MipSlice = 0
            };
            DepthBufferView = new DepthStencilView(D3DDevice, ZTexture, depthBufferViewDesc);
            var depthBufferShaderViewDesc = new ShaderResourceViewDescription
            {
                Format = Format.R32_Float,
                Dimension = ShaderResourceViewDimension.Texture2D,
                FirstArraySlice = 0,
                ArraySize = 1,
                MipLevels = 1,
            };
            DepthBufferShaderView = new ShaderResourceView(D3DDevice, ZTexture, depthBufferShaderViewDesc);
            _effect.GetVariableByName("composeDepth").AsResource().SetResource(DepthBufferShaderView);
            #endregion

            GeoProjectionMatrix = Matrix.PerspectiveFovLH((float)Math.PI / 4,
                                                           ((float)Output.ClientSize.Width) /
                                                           ((float)Output.ClientSize.Height),
                                                           Manager.Opts.Get<float>("rndr_nearPlane"),
                                                           Manager.Opts.Get<float>("rndr_farPlane"));
            foreach (IRenderable renderable in Content)
                renderable.UpdateStaticVars();
        }

        protected void InitOnce()
        {
            var shaderdeffile = Manager.Files.Get(@"Shaders\DeferredRendering.hlsl", false);
            var bbuffer = new byte[shaderdeffile.Length];
            shaderdeffile.Read(bbuffer,0, bbuffer.Length);
            shaderdeffile.Dispose();
            var bytecode = ShaderBytecode.Compile(bbuffer, "fx_5_0");
            bbuffer = null;
            _effect = new Effect(D3DDevice, bytecode);
            bytecode.Dispose();

            _composeTechnique = _effect.GetTechniqueByName("Compose");
            _composePass = _composeTechnique.GetPassByIndex(0);
            
            var vertices = new DataStream(20 * 4, true, true);
            vertices.Write(new Vector3(-1f, -1f, 1f));
            vertices.Write(new Vector2(0f,1f));
            vertices.Write(new Vector3(-1f, 1f, 1f));
            vertices.Write(new Vector2(0f, 0f));
            vertices.Write(new Vector3(1f, -1f, 1f));
            vertices.Write(new Vector2(1f, 1f));
            vertices.Write(new Vector3(1f, 1f, 1f));
            vertices.Write(new Vector2(1f, 0f));
            vertices.Position = 0;
            _composeVertices = new Buffer(D3DDevice, vertices, 20 * 4, ResourceUsage.Default, BindFlags.VertexBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
            _composeVerticesBB = new VertexBufferBinding(_composeVertices, 20, 0);
            vertices.Dispose();
            _composeLayout = new InputLayout(D3DDevice, _composePass.Description.Signature, new[] { new InputElement("POSITION", 0, Format.R32G32B32_Float, 0), new InputElement("TEXCOORD", 0, Format.R32G32_Float, 0) });
            var sampleMode = SamplerState.FromDescription(D3DDevice, new SamplerDescription()
            {
                AddressU = TextureAddressMode.Clamp,
                AddressV = TextureAddressMode.Clamp,
                AddressW = TextureAddressMode.Clamp,
                BorderColor = new Color4(0, 0, 0, 0),
                Filter = Filter.MinLinearMagMipPoint,
                ComparisonFunction = Comparison.Always,
                MipLodBias = 0f,
                MaximumAnisotropy = 8,
                MinimumLod = 0f,
                MaximumLod = float.MaxValue
            });
            _effect.GetVariableByName("composeSampler").AsSampler().SetSamplerState(0, sampleMode);
            _effect.GetVariableByName("composeFlags").AsScalar().Set(
                Manager.Opts.Get<bool>("rndr_rawGBufferView")?0x1:0
                );

            NotifyHandlers.Add(Manager.Opts.RegisterChangeNotification("rndr_rawGBufferView", delegate(string key, object value)
            {
                Output.BeginInvoke((Action)delegate
                {
                    if ((bool)value)
                        _effect.GetVariableByName("composeFlags").AsScalar().Set(_effect.GetVariableByName("composeFlags").AsScalar().GetInt() | 0x1);
                    else
                        _effect.GetVariableByName("composeFlags").AsScalar().Set(_effect.GetVariableByName("composeFlags").AsScalar().GetInt() & (int.MaxValue - 0x1));
                });
            }));


            NotifyHandlers.Add(Manager.Opts.RegisterChangeNotification("rndr_nearPlane", delegate {
                Output.BeginInvoke((Action)ResetDevice);
            }));
            NotifyHandlers.Add(Manager.Opts.RegisterChangeNotification("rndr_farPlane", delegate {
                Output.BeginInvoke((Action)ResetDevice);
            }));

            SceneRasterizer = RasterizerState.FromDescription(D3DDevice, new RasterizerStateDescription()
                                                                                      {
                                                                                          FillMode = (Manager.Opts.Get<bool>("rndr_wireframe") ? FillMode.Wireframe : FillMode.Solid),
                                                                                          CullMode = (Manager.Opts.Get<bool>("rndr_cull") ? CullMode.Back : CullMode.None)
                                                                                      });
            _composeRasterizer = RasterizerState.FromDescription(D3DDevice, new RasterizerStateDescription()
            {
                FillMode = FillMode.Solid,
                CullMode = CullMode.Back
            });

            NotifyHandlers.Add(Manager.Opts.RegisterChangeNotification("rndr_wireframe", delegate(string key, object value)
            {
                Output.BeginInvoke((Action)delegate {
                    SceneRasterizer = RasterizerState.FromDescription(D3DDevice, new RasterizerStateDescription()
                    {
                        FillMode = (((bool)value) ? FillMode.Wireframe : FillMode.Solid),
                        CullMode = SceneRasterizer.Description.CullMode
                    });
                });
            }));
            NotifyHandlers.Add(Manager.Opts.RegisterChangeNotification("rndr_cull", delegate(string key, object value)
            {
                Output.BeginInvoke((Action)delegate
                {
                    SceneRasterizer = RasterizerState.FromDescription(D3DDevice, new RasterizerStateDescription()
                    {
                        FillMode =  SceneRasterizer.Description.FillMode,
                        CullMode = (((bool)value) ? CullMode.Back : CullMode.None)
                    });
                });
            }));

            Context.OutputMerger.DepthStencilState = DepthStencilState.FromDescription(D3DDevice, new DepthStencilStateDescription()
                                                                                                      {
                                                                                                          IsDepthEnabled = true,
                                                                                                          DepthWriteMask = DepthWriteMask.All,
                                                                                                          DepthComparison = Comparison.Less,
                                                                                                          IsStencilEnabled = false,
                                                                                                      });
            _camIncorporeal = Manager.Opts.Get<bool>("rndr_incorporeal");
            NotifyHandlers.Add(Manager.Opts.RegisterChangeNotification("rndr_incorporeal", delegate(string key, object value) { _camIncorporeal = (bool)value; }));

            ViewerLocation = new Vector3(-1, 1, -1);
            ViewerLookAt = new Vector3(0, 0, 0);
            ViewerUpVector = Vector3.UnitY;
            _camLocationIncorporeal = new Vector3(-1, 1, -1);
            _camLookAtIncorporeal = new Vector3(0, 0, 0);
            _camUpVectorIncorporeal = Vector3.UnitY;
            ViewerFrustum = new Frustum();
            _fpsTimer = new HTimer();
            _fpsRingbuffer = new double[60];
            _fpsRingbufferIndex = 0;
        }

        public void Render()
        {
            Context.Rasterizer.SetViewports(OutputViewport);//Defines Output-dimensions, use in all passes except shadows

            ViewMatrix = Matrix.LookAtLH(ViewerLocation, ViewerLookAt, ViewerUpVector);
            ViewerFrustum.Update(ViewMatrix, GeoProjectionMatrix);
            if(_camIncorporeal)
                ViewMatrix = Matrix.LookAtLH(_camLocationIncorporeal, _camLookAtIncorporeal, _camUpVectorIncorporeal);

            Context.Rasterizer.State = SceneRasterizer;

            Context.ClearRenderTargetView(RTVs[0], Color.ForestGreen);
            Context.ClearRenderTargetView(RTVs[1], Color.Gray);
            Context.ClearDepthStencilView(DepthBufferView, DepthStencilClearFlags.Depth, 1f,0);
            //Context.ClearRenderTargetView(RTVScreen, Color.DeepSkyBlue); //Clear screen (is overwritten anyways, for debugging draw-related things only)
            Context.OutputMerger.SetTargets(DepthBufferView, RTVs);

            Content.Update();
            foreach (IRenderable renderable in Content)
            {
                renderable.Render();
            }
            
            Context.OutputMerger.SetTargets(RTVScreen);
            Context.Rasterizer.State = _composeRasterizer;
            _composePass.Apply(Context);
            Context.InputAssembler.InputLayout = _composeLayout;
            Context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleStrip;
            Context.InputAssembler.SetVertexBuffers(0, _composeVerticesBB);

            Context.Draw(4, 0);

            SChain.Present(0, PresentFlags.None);

            _fpsRingbuffer[_fpsRingbufferIndex] = 1 / _fpsTimer.Elapsed;
            _fpsRingbufferIndex++;
            if (_fpsRingbufferIndex >= _fpsRingbuffer.Length)
                _fpsRingbufferIndex = 0;
        }
    }
}
