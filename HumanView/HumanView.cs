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

        protected MainMenuSubrender MainMenu;

        public HumanView(IRunManager manager) : base(manager)
        {

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
            MainThread = ThreadHelper.Start(StartProcedure, "Human_View");
        }

        protected void StartProcedure()
        {
            Window = new RenderForm();
            Window.Text = Manager.Opts.Get<string>("gui_windowTitle_client");
            NotifyHandlers.Add(Manager.Opts.RegisterChangeNotification("gui_windowTitle_client",
                        (key, value) => Window.BeginInvoke((Action)delegate { Window.Text = (string)value; })));
            Render = new RenderManager(Manager, Window);
            Thread.Sleep(50);

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
