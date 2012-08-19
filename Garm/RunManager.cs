using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Debug;
using Garm.Base.Abstract;
using Garm.Base.Enums;
using Garm.Base.Helper;
using Garm.Base.Interfaces;
using Garm.Debug;
using Garm.View.Human;
using ICommandExecutor = Garm.Base.Interfaces.ICommandExecutor;
using Opts = Garm.Options.Options;
using Point = System.Drawing.Point;

namespace Garm
{
    /// <summary>
    /// The RunManager parses commandline-parameters, initialises basic modules (e.g. Options) and starts the requested parts of the engine (Views, Server)
    /// </summary>
    public partial class RunManager : IRunManager
    {
        private bool _doRun;
        public bool DoRun
        {
            get { return _doRun; }
            set
            {
                _doRun = value;
                if (!value)
                {
#if DEBUG
                    Console.WriteLine("[Info]Shutting down");
#endif
                    _abortOnExit.Set();
                    if (OnExit != null)
                        OnExit();
                }
            }
        }
        public event Action OnExit;
        private readonly ICommandExecutor _commands;
        public ICommandExecutor Commands { get { return _commands; } }
        private readonly IOptionsProvider _opts;
        public IOptionsProvider Opts { get { return _opts; } }
        private readonly List<Base.Abstract.View> _views;
        public IReadOnlyList<Base.Abstract.View> Views { get { return _views.AsReadOnly(); } }
        private readonly ManualResetEvent _abortOnExit;
        public ManualResetEvent AbortOnExit { get { return _abortOnExit; } }
        private readonly FileManager _files;
        public FileManager Files { get { return _files; } }
        private int waitOnShutdown;
        public int WaitOnShutdown { get { return waitOnShutdown; } set { waitOnShutdown = value; } }

        private List<ValueChangedHandler> _notifyHandlers;

        /// <summary>
        /// Creates a new RunManager instance
        /// </summary>
        /// <param name="args">Commandline parameters</param>
        public RunManager(string[] args)
        {
            _doRun = true;
            InstallMultiplexer();
#if DEBUG
            ShowConsole();
            Thread.CurrentThread.Name = "Main";
            ThreadHelper.Track(Thread.CurrentThread);
#endif
            _opts = new Opts();
            _files = new FileManager(this);
            _commands = new Commands.Executor(this);
            _abortOnExit = new ManualResetEvent(false);
            _notifyHandlers = new List<ValueChangedHandler>();

            foreach (var arg in args)
            {
                switch(arg.Substring(0,1))
                {
                    case "-":
                        var argparts = arg.Substring(1).Split(new[] {"="}, StringSplitOptions.None);
                        switch ( argparts[0].ToLower() )
                        {
                            case "runmode":
                                #region RunmodeSelector

                                RunMode runMode = RunMode.Play;
                                if (argparts.Length < 2)
                                {
                                    Console.WriteLine("[Warning] No runmode supplied!");
                                    break;
                                }
                                switch (argparts[1])
                                {
                                    case "select":
                                        var selectForm = new Form();
                                        selectForm.Text = "Select Runmode";
                                        selectForm.FormBorderStyle = FormBorderStyle.FixedToolWindow;
                                        selectForm.Size = new Size(330, 75);
                                        selectForm.StartPosition = FormStartPosition.CenterScreen;
                                        selectForm.Controls.Add(new Button()
                                                                    {
                                                                        Text = "Game",
                                                                        Location = new Point(5, 3),
                                                                        Size = new Size(75, 44)
                                                                    });
                                        selectForm.Controls.Add(new Button()
                                                                    {
                                                                        Text = "Editor",
                                                                        Location = new Point(84, 3),
                                                                        Size = new Size(75, 44)
                                                                    });
                                        selectForm.Controls.Add(new Button()
                                                                    {
                                                                        Text = "Server",
                                                                        Location = new Point(163, 3),
                                                                        Size = new Size(75, 44)
                                                                    });
                                        selectForm.Controls.Add(new Button()
                                                                    {
                                                                        Text = "Collab Server",
                                                                        Location = new Point(242, 3),
                                                                        Size = new Size(75, 44)
                                                                    });
                                        selectForm.Controls[0].Click +=
                                            delegate
                                                {
                                                    runMode = RunMode.Play;
                                                    selectForm.Close();
                                                };
                                        selectForm.Controls[1].Click +=
                                            delegate
                                                {
                                                    runMode = RunMode.Edit;
                                                    selectForm.Close();
                                                };
                                        selectForm.Controls[2].Click +=
                                            delegate
                                                {
                                                    runMode = RunMode.GameServer;
                                                    selectForm.Close();
                                                };
                                        selectForm.Controls[3].Click +=
                                            delegate
                                                {
                                                    runMode = RunMode.CollabServer;
                                                    selectForm.Close();
                                                };
                                        selectForm.Show();
                                        selectForm.Focus();
                                        Application.Run(selectForm);
                                        break;
                                    case "game":
                                        runMode = RunMode.Play;
                                        break;
                                    case "gameserver":
                                        runMode = RunMode.ServerPlay;
                                        break;
                                    case "edit":
                                    case "editor":
                                        runMode = RunMode.Edit;
                                        break;
                                    case "server":
                                        runMode = RunMode.GameServer;
                                        break;
                                    case "collabserver":
                                    case "editserver":
                                        runMode = RunMode.CollabServer;
                                        break;
                                    case "aioserver":
                                        runMode = RunMode.AIOServer;
                                        break;
                                    default:
                                        Console.WriteLine("[Warning] Unknown runMode: '"+argparts[0]+"'");
                                        break;
                                }
                                Opts.Set("sys_runMode", runMode);
                                #endregion
                                break;
                            case "console":
                                #region ConsoleSection
#if !DEBUG
                                ShowConsole();
#else
                                Console.WriteLine("[Info] Commandline-parameter '-console' given, ignoring...");
#endif
                                #endregion
                                break;
                        }
                        break;
                    case "+":
                        if (arg.Length < 2)
                            continue;
                        _commands.Parse(arg.Substring(1));
                        break;
                }
            }
            ((Options.Options)_opts).LockDownReadonly();
            _views = new List<Base.Abstract.View>();
#if DEBUG
            ThreadHelper.Start(delegate
                                   {
                                       var dbgWnd = new DebugWindow(this);
                                       Application.Run(dbgWnd);
                                   }, "Dbg_Wnd");
#endif
            Console.ForegroundColor = _opts.Get<ConsoleColor>("gui_consoleColor");
            _notifyHandlers.Add(_opts.RegisterChangeNotification("gui_consoleColor",
                delegate(string key, object value) { Console.ForegroundColor = (ConsoleColor)value; }));
        }

        /// <summary>
        /// Starts the different components of the game defined by the runMode
        /// </summary>
        public void Run()
        {
            var runMode = Opts.Get<RunMode>("sys_runMode");

            if(runMode.HasFlag(RunMode.GameServer) || runMode.HasFlag(RunMode.CollabServer))
                ShowConsole(); //Server is console-output only
            if (runMode.HasFlag(RunMode.Play))
                _views.Add(new HumanView(this));
            if (runMode.HasFlag(RunMode.Edit))
                _views.Add(new EditorView(this));
            if(runMode.HasFlag(RunMode.CollabServer))
                _views.Add(new CollabServerView(this));
            foreach (var view in Views)
            {
                view.Run();
            }
            if(consoleShown)
                while(_doRun)
                {
                    var input = Console.ReadLine();
                    _commands.Parse(input);
                }
            else
            {
                _abortOnExit.WaitOne();
            }

            _commands.Dispose();

            foreach (var view in Views)
            {
                view.Dispose();
            }

            foreach (var notfiyHandler in _notifyHandlers)
            {
                _opts.UnregisterChangeNotification(notfiyHandler);
            }

            for (int i = 0; i < _opts.Get<int>("sys_shutdownTimeout"); i+=100)
            {
                if(waitOnShutdown == 0)
                    break;
#if DEBUG
                Console.WriteLine("[Info] Waiting on shutdown, " + waitOnShutdown + " remaining");
#endif
                Thread.Sleep(100);
            }
#if DEBUG
            if (waitOnShutdown > 0)
                Console.WriteLine("[Warning] Shutdown timed out, counter at " + waitOnShutdown + ", forcing shutdown");
#endif
            _files.Dispose();
            _opts.Dispose();

#if DEBUG
            Console.WriteLine("Press Enter to exit");
            Console.ReadLine();
#endif
        }
    }
}
