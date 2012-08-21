using System;
using System.Threading;
using Garm.Base.Interfaces;
using Garm.Base.Helper;

namespace Garm.Debug
{
    /// <summary>
    /// Provides the CollabServer functionality
    /// </summary>
    public class CollabServerView : Base.Abstract.View
    {
        /// <summary>
        /// Main workthread
        /// </summary>
        protected Thread MainThread;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="manager">The RunManager instance to be used</param>
        public CollabServerView(IRunManager manager) : base(manager)
        {
        }

        /// <summary>
        /// Starts serving edit-clients
        /// </summary>
        public override void Run()
        {
#if DEBUG 
            Console.WriteLine("Starting CollabServer");
#endif
            MainThread = ThreadHelper.Start(Serve, "CollabServer_View");
        }

        /// <summary>
        /// Main workloop for serving clients
        /// </summary>
        protected void Serve()
        {
            //Init

            //Work
        }

        /// <summary>
        /// Disconnects from all clients and releases any allocated resources
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();
        }
    }
}
