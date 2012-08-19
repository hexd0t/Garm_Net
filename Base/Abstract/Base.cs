using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Garm.Base.Interfaces;

namespace Garm.Base.Abstract
{
    public abstract class Base : Object, IDisposable
    {
        protected IRunManager Manager;
        protected readonly List<ValueChangedHandler> NotifyHandlers;
        protected Base(IRunManager manager)
        {
            Manager = manager;
            NotifyHandlers = new List<ValueChangedHandler>();
        }

        public abstract void Dispose();
        /// <summary>
        /// This flag is set by the Main-Function and can be used to test if any component is run in the VS-Designer
        /// </summary>
        public static bool IsApplication { get; set; }
    }
}
