using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using D3D11 = SlimDX.Direct3D11;
using D3D10 = SlimDX.Direct3D10;
using D3D101 = SlimDX.Direct3D10_1;
using D2D = SlimDX.Direct2D;
using DW = SlimDX.DirectWrite;

namespace Garm.View.Human.Render.D2DInterop
{
    public class D2DInteropHandler : IDisposable
    {
        private readonly RenderManager Manager;
        public D3D11.Device D3DDevice11 { get { return Manager.D3DDevice; } }
        public readonly D3D101.Device1 D3DDevice10;
        public readonly D2D.Factory D2DFactory;
        public readonly DW.Factory DWFactory;

        public D2DInteropHandler(RenderManager manager)
        {
            Manager = manager;
#if DEBUG
            D3DDevice10 = new D3D101.Device1(manager.DXGIAdapter, D3D10.DriverType.Hardware, D3D10.DeviceCreationFlags.BgraSupport | D3D10.DeviceCreationFlags.Debug, D3D101.FeatureLevel.Level_10_0);
#else
            D3DDevice10 = new D3D101.Device1(D3D10.DriverType.Hardware, D3D10.DeviceCreationFlags.BgraSupport, D3D101.FeatureLevel.Level_10_0);
#endif
            D2DFactory = new D2D.Factory(D2D.FactoryType.SingleThreaded);

            DWFactory = new DW.Factory(DW.FactoryType.Shared);
        }

        public void Dispose()
        {
            DWFactory.Dispose();
            D2DFactory.Dispose();
            D3DDevice10.Dispose();
        }
    }
}
