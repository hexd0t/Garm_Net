using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Garm.Base.Interfaces;
using SlimDX;
using SlimDX.D3DCompiler;
using SlimDX.DXGI;
using SlimDX.Direct3D11;
using Buffer = SlimDX.Direct3D11.Buffer;

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

        public MainMenuSubrender(RenderManager renderer, IRunManager manager)
            : base(manager)
        {
            Renderer = renderer;

            var vertices = new DataStream(20 * 4, true, true);
            vertices.Write(new Vector3(-1f, -1f, -1f));
            vertices.Write(new Vector2(0f, 1f));
            vertices.Write(new Vector3(-1f, 1f, -1f));
            vertices.Write(new Vector2(0f, 0f));
            vertices.Write(new Vector3(1f, -1f, -1f));
            vertices.Write(new Vector2(1f, 1f));
            vertices.Write(new Vector3(1f, 1f, -1f));
            vertices.Write(new Vector2(1f, 0f));
            vertices.Position = 0;
            FullscreenQuad_Buffer = new Buffer(Renderer.D3DDevice, vertices, 20 * 4, ResourceUsage.Default, BindFlags.VertexBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
            FullscreenQuad = new VertexBufferBinding(FullscreenQuad_Buffer, 20, 0);
            vertices.Dispose();

            var shaderdeffile = Manager.Files.Get(@"Shaders\Menu.hlsl", false);
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
            UpdateStaticVars();
        }

        public void Render()
        {
            _pass.Apply(Renderer.Context);
            Renderer.Context.InputAssembler.SetVertexBuffers(0, FullscreenQuad);
            Renderer.Context.InputAssembler.InputLayout = _vertexLayout;
            Renderer.Context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleStrip;
            Renderer.Context.Draw(4,0);
        }

        public void UpdateStaticVars()
        {
            MenuEffect.GetVariableByName("screenSize").AsVector().Set( new Vector2(
                Renderer.Output.ClientSize.Width,
                Renderer.Output.ClientSize.Height));
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
    }
}
