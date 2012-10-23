using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SlimDX.DXGI;
using SlimDX.Direct2D;
using SlimDX.Direct3D11;
using Device = SlimDX.Direct3D11.Device;
using FeatureLevel = SlimDX.Direct2D.FeatureLevel;

namespace Garm.View.Human.Render.D2DInterop
{
    /// <summary>
    /// Derived class to secure that only Textures with Shared-Flag get to the D2D-Interop
    /// </summary>
    public class SharedTexture : IDisposable
    {
        /// <summary>
        /// Creates a default D3D11 Texture with forced Shared-Flag
        /// </summary>
        /// <param name="device"></param>
        /// <param name="description"></param>
        /// <param name="D3D10Dev"> </param>
        /// <param name="D2DFactory"> </param>
        public SharedTexture(D2DInteropHandler handler, Texture2DDescription description)
        {
            As11Tex = new Texture2D(handler.D3DDevice11, new Texture2DDescription()
                {
                    ArraySize = description.ArraySize,
                    BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
                    CpuAccessFlags = description.CpuAccessFlags,
                    Format = description.Format,
                    Height = description.Height,
                    MipLevels = description.MipLevels,
                    OptionFlags = ResourceOptionFlags.KeyedMutex,
                    SampleDescription = description.SampleDescription,
                    Usage = description.Usage,
                    Width = description.Width
                });

            Mutex11 = new KeyedMutex(As11Tex);
            AsResource = new SlimDX.DXGI.Resource(As11Tex);
            
            
            As10Tex = handler.D3DDevice10.OpenSharedResource<SlimDX.Direct3D10.Texture2D>(AsResource.SharedHandle);
            Mutex10 = new KeyedMutex(As10Tex);
            AsSurface = As10Tex.AsSurface();
            As2DTarget = SlimDX.Direct2D.RenderTarget.FromDXGI(handler.D2DFactory, AsSurface, new RenderTargetProperties()
                                                                                                            {
                                                                                                                MinimumFeatureLevel = FeatureLevel.Direct3D10,
                                                                                                                Usage = RenderTargetUsage.None,
                                                                                                                Type = RenderTargetType.Hardware,
                                                                                                                PixelFormat = new PixelFormat(Format.Unknown, AlphaMode.Premultiplied)
                                                                                                            });
        }

        public readonly SlimDX.Direct3D11.Texture2D As11Tex;
        private readonly SlimDX.DXGI.Resource AsResource;
        private readonly SlimDX.Direct3D10.Texture2D As10Tex;
        public readonly SlimDX.Direct2D.RenderTarget As2DTarget;
        private readonly SlimDX.DXGI.Surface AsSurface;
        public readonly KeyedMutex Mutex11;
        public readonly KeyedMutex Mutex10;

        public virtual new void Dispose()
        {
            Mutex10.Dispose();
            Mutex11.Dispose();
            As2DTarget.Dispose();
            AsSurface.Dispose();
            As10Tex.Dispose();
            AsResource.Dispose();
            As11Tex.Dispose();
        }
    }

}
