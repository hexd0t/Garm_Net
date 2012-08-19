using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Garm.Base.Helper
{
    public static class ThreadHelper
    {
#if DEBUG
        private static readonly List<WeakReference> _threads = new List<WeakReference>();
#endif

        public static Thread Create(ThreadStart start, string name = "TempThread")
        {
            var thread = new Thread(start);
#if DEBUG
            _threads.Add(new WeakReference(thread));
            thread.Name = name;
#endif
            return thread;
        }

        public static Thread Start(ThreadStart start, string name = "TempThread")
        {
            var thread = new Thread(start);
#if DEBUG
            _threads.Add(new WeakReference(thread));
            thread.Name = name;
#endif
            thread.Start();
            return thread;
        }

#if DEBUG
        public static void Track(Thread target)
        {
            _threads.Add(new WeakReference(target));
        }
#endif

        public static IEnumerable<Thread> CurrentThreads
        {
            get
            {
#if DEBUG
                return _threads.ToArray().Where(reference => reference.IsAlive).Select(reference => (Thread)reference.Target);
#else
                return null;
#endif
            }
        }

        public static void Cleanup()
        {
#if DEBUG
            _threads.RemoveAll(reference => !reference.IsAlive);
#endif
        }
    }
}
