using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Garm.Base.Interfaces;
using Garm.View.Human.Render.D2DInterop;
using SlimDX;
using SlimDX.D3DCompiler;
using SlimDX.DXGI;
using SlimDX.Direct2D;
using SlimDX.Direct3D11;
using SlimDX.DirectWrite;
using Buffer = SlimDX.Direct3D11.Buffer;
using FontStyle = SlimDX.DirectWrite.FontStyle;
using Garm.Base;
using Garm.Base.Helper;

namespace Garm.View.Human.Render.Menu
{
    /// <summary>
    /// Provides menu-rendering functionality
    /// </summary>
    public class MainMenuSubrender : Base.Abstract.Base, IRenderable
    {
        protected readonly RenderManager Renderer;
        protected Buffer FullscreenQuad_Buffer;
        protected VertexBufferBinding FullscreenQuad;

        protected Effect MenuEffect;
        private EffectTechnique _technique;
        private EffectPass _pass;
        private InputLayout _vertexLayout;

        private SharedTexture _d2dtexture;
        //private ShaderResourceView _textureSRV;
        private SamplerState _textureSampler;
        private TextFormat _textFormat;

        private HTimer _animTimer;

        public MainMenuSubrender(RenderManager renderer, IRunManager manager)
            : base(manager)
        {
            Renderer = renderer;
            _animTimer = new HTimer();

            var vertices = new DataStream(20 * 4, true, true);
            vertices.Write(new Vector3(-1f, -1f, -1f));
            vertices.Write(new Vector2(0f, 1f));
            vertices.Write(new Vector3(-1f,  1f, -1f));
            vertices.Write(new Vector2(0f, 0f));
            vertices.Write(new Vector3( 1f, -1f, -1f));
            vertices.Write(new Vector2(1f, 1f));
            vertices.Write(new Vector3( 1f,  1f, -1f));
            vertices.Write(new Vector2(1f, 0f));
            vertices.Position = 0;
            FullscreenQuad_Buffer = new Buffer(Renderer.D3DDevice, vertices, 20 * 4, ResourceUsage.Default, BindFlags.VertexBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
            FullscreenQuad = new VertexBufferBinding(FullscreenQuad_Buffer, 20, 0);
            vertices.Dispose();

            var shaderdeffile = Manager.Files.Get(@"shaders\menu.hlsl", false);
            var bbuffer = new byte[shaderdeffile.Length];
            shaderdeffile.Read(bbuffer, 0, bbuffer.Length);
            shaderdeffile.Dispose();
            var bytecode = ShaderBytecode.Compile(bbuffer, "fx_5_0");
            bbuffer = null;
            MenuEffect = new Effect(Renderer.D3DDevice, bytecode);
            bytecode.Dispose();
            _technique = MenuEffect.GetTechniqueByName("Menu");
            _pass = _technique.GetPassByIndex(0);
            _vertexLayout = new InputLayout(Renderer.D3DDevice, _pass.Description.Signature, new[] { new InputElement("POSITION", 0, Format.R32G32B32_Float, 0), new InputElement("TEXCOORD", 0, Format.R32G32_Float, 0) });

            _textureSampler = SamplerState.FromDescription(Renderer.D3DDevice, new SamplerDescription()
            {
                AddressU = TextureAddressMode.Clamp,
                AddressV = TextureAddressMode.Clamp,
                AddressW = TextureAddressMode.Clamp,
                BorderColor = new Color4(0, 0, 0, 0),
                Filter = Filter.MinMagMipLinear,
                ComparisonFunction = Comparison.Always,
                MipLodBias = 0f,
                MaximumAnisotropy = 8,
                MinimumLod = 0f,
                MaximumLod = float.MaxValue
            });
            MenuEffect.GetVariableByName("textureSampler").AsSampler().SetSamplerState(0, _textureSampler);

            _textFormat = new TextFormat(renderer.D2DInterop.DWFactory, "Arial", FontWeight.Normal, FontStyle.Normal, FontStretch.Normal, 20f, "");
            _textFormat.ParagraphAlignment = ParagraphAlignment.Center;
            _textFormat.TextAlignment = TextAlignment.Center;
            
            UpdateStaticVars();
        }

        public void Render()
        {
            double time = _animTimer.Peek;

            _d2dtexture.Mutex10.Acquire(0, 100);
            _d2dtexture.As2DTarget.BeginDraw();
            _d2dtexture.As2DTarget.Clear(Color.Transparent);
            var gradientstops = new GradientStop[3];
            gradientstops[0].Position = 0f;
            gradientstops[1].Position = 0.7f;
            gradientstops[2].Position = 1f;

            //===Active border
            gradientstops[0].Color = Color.Green;
            gradientstops[1].Color = Color.GreenYellow;
            gradientstops[2].Color = Color.Green;
            var gsc = new GradientStopCollection(_d2dtexture.As2DTarget, gradientstops);
            var b_border_active = new LinearGradientBrush(_d2dtexture.As2DTarget, gsc,
                new LinearGradientBrushProperties() { StartPoint = new PointF(0, 0), EndPoint = new PointF(250, 50) });
            b_border_active.Transform = Matrix3x2.Translation((float)(time % 5)*100-250, 0);
            gsc.Dispose();

            //===Inactive border
            gradientstops[0].Color = Color.MediumTurquoise;
            gradientstops[1].Color = Color.DodgerBlue;
            gradientstops[2].Color = Color.MediumTurquoise;
            gsc = new GradientStopCollection(_d2dtexture.As2DTarget, gradientstops);
            var b_border = new LinearGradientBrush(_d2dtexture.As2DTarget, gsc,
                new LinearGradientBrushProperties() { StartPoint = new PointF(0, 0), EndPoint = new PointF(250, 50) });
            gsc.Dispose();

            //===Fill
            gradientstops[0].Position = 0f;
            gradientstops[1].Position = 0.3f;
            gradientstops[2].Position = 1f;
            gradientstops[0].Color = Color.DarkSlateGray;
            gradientstops[1].Color = Color.DarkCyan;
            gradientstops[2].Color = Color.DarkSlateGray;
            gsc = new GradientStopCollection(_d2dtexture.As2DTarget, gradientstops);
            var b_fill = new LinearGradientBrush(_d2dtexture.As2DTarget, gsc,
                new LinearGradientBrushProperties() { StartPoint = new PointF(0, 0), EndPoint = new PointF(250, 50) });
            gsc.Dispose();

            //===Text
            var b_text = new SolidColorBrush(_d2dtexture.As2DTarget, Color.Black);

            var geo = new PathGeometry(Renderer.D2DInterop.D2DFactory);
            var geosink = geo.Open();
            geosink.BeginFigure(new PointF(00, 0), FigureBegin.Filled);
            geosink.AddLine(new PointF(230, 0));
            geosink.AddLine(new PointF(210, 30));
            geosink.AddLine(new PointF(20, 30));
            geosink.EndFigure(FigureEnd.Closed);
            geosink.Close();

            _d2dtexture.As2DTarget.Transform = Matrix3x2.Translation(30, Renderer.Output.ClientSize.Height - 50);

            _d2dtexture.As2DTarget.FillGeometry(geo, b_fill);
            _d2dtexture.As2DTarget.DrawGeometry(geo, b_border_active, 3f);
            _d2dtexture.As2DTarget.DrawText("Testtext", _textFormat, new Rectangle(25,5,180,20), b_text);
            
            geosink.Dispose();
            geo.Dispose();
            b_border.Dispose();
            b_border_active.Dispose();
            b_fill.Dispose();
            b_text.Dispose();
            _d2dtexture.As2DTarget.EndDraw();
            
            _d2dtexture.Mutex10.Release(0);

            _d2dtexture.Mutex11.Acquire(0, 100);

            var srv = new ShaderResourceView(Renderer.D3DDevice, _d2dtexture.As11Tex);
            MenuEffect.GetVariableByName("menuTexture").AsResource().SetResource(srv);
            _pass.Apply(Renderer.Context);
            Renderer.Context.InputAssembler.SetVertexBuffers(0, FullscreenQuad);
            Renderer.Context.InputAssembler.InputLayout = _vertexLayout;
            Renderer.Context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleStrip;
            Renderer.Context.Draw(4, 0);
            srv.Dispose();

            _d2dtexture.Mutex11.Release(0);
        }

        public void UpdateStaticVars()
        {
            /*if (_textureSRV != null)
                _textureSRV.Dispose();*/
            if (_d2dtexture != null)
                _d2dtexture.Dispose();

            _d2dtexture = new SharedTexture(Renderer.D2DInterop, new Texture2DDescription
            {
                Width = Renderer.Output.ClientSize.Width,
                Height = Renderer.Output.ClientSize.Height,
                MipLevels = 1,
                ArraySize = 1,
                Format = Format.R8G8B8A8_UNorm,
                Usage = ResourceUsage.Default,
                SampleDescription = new SampleDescription(1, 0),
                BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None
            });
            /*_textureSRV = new ShaderResourceView(Renderer.D3DDevice, _d2dtexture.As11Tex);
            MenuEffect.GetVariableByName("menuTexture").AsResource().SetResource(_textureSRV);*/

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

        public override void Dispose()
        {
            _textFormat.Dispose();
            //_textureSRV.Dispose();
            _d2dtexture.Dispose();
            _textureSampler.Dispose();
            FullscreenQuad_Buffer.Dispose();
            _vertexLayout.Dispose();
            MenuEffect.Dispose();
            base.Dispose();
        }
    }
}
