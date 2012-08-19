using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Garm.Audio;
using Garm.Base.Interfaces;
using Garm.Base.Helper;

namespace Garm.Debug
{
    public class CollabServerView : Base.Abstract.View
    {
        protected Thread MainThread;

        public CollabServerView(IRunManager manager) : base(manager)
        {
        }

        public override void Dispose()
        {
            
        }

        public override void Run()
        {
#if DEBUG 
            Console.WriteLine("Starting CollabServer");
#endif
            MainThread = ThreadHelper.Start(Serve, "CollabServer_View");
        }

        protected void Serve()
        {
            //Init

            //Work
        }
    }
}
