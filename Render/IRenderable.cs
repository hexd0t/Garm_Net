using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Garm.View.Human.Render
{
    public interface IRenderable : IDisposable
    {
        void Render();
        void UpdateStaticVars();
        void RenderTransparent();
        void RenderStaticShadows();
        void RenderDynamicShadows();
    }
}
