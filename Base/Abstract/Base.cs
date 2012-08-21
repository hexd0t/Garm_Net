using System;
using System.Collections.Generic;
using Garm.Base.Interfaces;

namespace Garm.Base.Abstract
{
    /// <summary>
    /// Baseclass for many classes, implementing common fields, e.g. to store a reference to the current RunManager
    /// </summary>
    public abstract class Base : Object, IDisposable
    {
        /// <summary>
        /// The current RunManager
        /// </summary>
        protected readonly IRunManager Manager;
        /// <summary>
        /// Provides a list to store all Notifyhandlers this instance has registered
        /// </summary>
        protected readonly List<ValueChangedHandler> NotifyHandlers;
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="manager">Current runmanager</param>
        protected Base(IRunManager manager)
        {
            Manager = manager;
            NotifyHandlers = new List<ValueChangedHandler>();
        }
        /// <summary>
        /// Releases all Resources stored in baseclass; use <code>virtual new Dispose () { base.Dispose(); /*...*/ }</code> if you need to dispose something in a derived class
        /// </summary>
        public virtual void Dispose()
        {
            foreach (var valueChangedHandler in NotifyHandlers)
                Manager.Opts.UnregisterChangeNotification(valueChangedHandler);
        }
        /// <summary>
        /// This flag is set by the Main-Function and can be used to test if any component is run in the VS-Designer
        /// </summary>

// ReSharper disable UnusedAutoPropertyAccessor.Global
        public static bool IsApplication { get; set; }
// ReSharper restore UnusedAutoPropertyAccessor.Global
    }
}
