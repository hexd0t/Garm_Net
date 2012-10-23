using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Garm.View.Human.Render
{
    /// <summary>
    /// Interface for Subrenderer
    /// </summary>
    public interface IRenderable : IDisposable
    {
        /// <summary>
        /// Render opaque elements for displaying
        /// </summary>
        void Render();
        /// <summary>
        /// Notifies on events changing the render-environment, notably the client size
        /// </summary>
        void UpdateStaticVars();
        /// <summary>
        /// Render transparent elements for displying
        /// </summary>
        void RenderTransparent();
        /// <summary>
        /// Render all non-moving elements without textures to create basic shadowmaps
        /// </summary>
        void RenderStaticShadows();
        /// <summary>
        /// Render all moving elements without textures to create dynamic shadowmaps
        /// </summary>
        void RenderDynamicShadows();
    }
}
