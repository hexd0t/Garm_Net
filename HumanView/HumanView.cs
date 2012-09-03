using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Garm.Base.Content.Terrain;
using Garm.Base.Helper;
using Garm.Base.Interfaces;
using Garm.View.Human.Render;
using Garm.View.Human.Render.Terrain;
using Garm.View.Human.Render.Menu;
using SlimDX.Windows;

namespace Garm.View.Human
{
    public class HumanView : Base.Abstract.View
    {
        public RenderManager Render;
        protected RenderForm Window;
        protected Thread MainThread;
        protected AutoResetEvent StartSync;

        protected MainMenuSubrender MainMenu;

        public HumanView(IRunManager manager) : base(manager)
        {
            StartSync = new AutoResetEvent(false);
            MainThread = ThreadHelper.Start(StartProcedure, "Human_View");
            if (!StartSync.WaitOne(Manager.Opts.Get<int>("sys_initTimeout")))
            {
                Console.WriteLine("[Error] Renderer-init timed out");
                MainThread.Abort();
            }
        }

        public override void Dispose()
        {
#if DEBUG
            Console.WriteLine("[Info] HumanView shutting down");
#endif
            base.Dispose();
            if (Window != null && Window.Created)
            {
                Window.Invoke((Action)Window.Close);
                Window.Dispose();
            }
            if(Render != null)
                Render.Dispose();
        }

        public override void Run()
        {
            StartSync.Set();
        }

        protected void StartProcedure()
        {
            Window = new RenderForm();
            Window.Text = Manager.Opts.Get<string>("gui_windowTitle_client");
            NotifyHandlers.Add(Manager.Opts.RegisterChangeNotification("gui_windowTitle_client",
                        (key, value) => Window.BeginInvoke((Action)delegate { Window.Text = (string)value; })));
            Render = new RenderManager(Manager, Window);
            StartSync.Set();
            Thread.Sleep(50);

            while(!StartSync.WaitOne(1000) && Manager.DoRun)
            {
#if DEBUG
                Console.WriteLine("[Info] HumanView waiting for go");
#endif
            }
#if DEBUG
            Console.WriteLine("[Info] Initializing RenderManager");
#endif
            Render.Initialize();
#if DEBUG
            Console.WriteLine("[Info] Initializing RenderManager finished");
#endif
            //temp Terrainloading
            /*var terraindef = Manager.Files.Get("..\\Content\\maps\\default.xml");
            var terrain = new Terrain(terraindef, Manager);
            terraindef.Dispose();
            Render.Content.Add(new TerrainSubrender(terrain,Render, Manager));*/

            MainMenu = new MainMenuSubrender(Render, Manager);
            Render.Content.Add(MainMenu);

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
