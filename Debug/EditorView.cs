using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Garm.Audio;
using Garm.Base.Interfaces;
using Garm.Base.Helper;
using Garm.View.Human.Render;
using SlimDX.Windows;

namespace Garm.Debug
{
    public class EditorView : Base.Abstract.View
    {
        public RenderManager Render;
        public AudioManager Audio;
        protected Editor Window;
        protected Thread MainThread;
        protected Thread AudioThread;
        protected AutoResetEvent StartSync;

        public EditorView(IRunManager manager) : base(manager)
        {
            StartSync = new AutoResetEvent(false);
            MainThread = ThreadHelper.Start(StartProcedure, "Editor_View");
            if (!StartSync.WaitOne(Manager.Opts.Get<int>("sys_initTimeout")))
            {
                Console.WriteLine("[Error] Renderer-init timed out");
                MainThread.Abort();
            }
        }

        public override void Dispose()
        {
#if DEBUG
            Console.WriteLine("[Info] EditorView shutting down");
#endif
            if (Window != null && Window.Created)
            {
                Window.Invoke((Action)Window.Close);
                Window.Dispose();
            }
            if(Audio != null)
                Audio.Dispose();
            if(Render != null)
                Render.Dispose();
        }

        public override void Run()
        {
            StartSync.Set();
        }

        protected void StartProcedure()
        {
            Window = new Editor(this, Manager);
            Window.Text = Manager.Opts.Get<string>("gui_windowTitle_edit");
            NotifyHandlers.Add(Manager.Opts.RegisterChangeNotification("gui_windowTitle_edit",
                        (key, value) => Window.BeginInvoke((Action)delegate { Window.Text = (string)value; })));
            Render = new RenderManager(Manager, Window.RenderTarget);
            StartSync.Set();
            Thread.Sleep(50);

            while(!StartSync.WaitOne(1000) && Manager.DoRun)
            {
#if DEBUG
                Console.WriteLine("[Info] EditView waiting for go");
#endif
            }
#if DEBUG
            Console.WriteLine("[Info] Initializing RenderManager");
#endif
            Render.Initialize();
#if DEBUG
            Console.WriteLine("[Info] Initializing RenderManager finished");
            Console.WriteLine("[Info] Initializing AudioManager");
#endif
            Audio = new AudioManager(Manager);
#if DEBUG
            Console.WriteLine("[Info] Initializing AudioManager finished");
#endif

            Window.Show();
            Window.Focus();
            MessagePump.Run(Window, Loop);
            foreach (var notifyHandler in NotifyHandlers)
            {
                Manager.Opts.UnregisterChangeNotification(notifyHandler);
            }
#if DEBUG
            Console.WriteLine("[Info] Render stopped");
#endif
            Manager.DoRun = false;
        }

        protected void Loop()
        {
            if(!Render.Disposed)
                Render.Render();
        }
    }
}
