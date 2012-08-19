using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using Garm.Base.Interfaces;

namespace Debug
{
    ///<summary>
    /// A Winforms window displaying debuginfo
    ///</summary>
    public sealed partial class DebugWindow : Form
    {
        private readonly IRunManager _runManager;
        private readonly List<ValueChangedHandler> _notifyHandlers;
        private Timer _updateTimer;
        [Flags]
        private enum View : uint
        {
            TabLowLevel = 0x0,
            LLThreads = 0x00,
            LLAssemblys = 0x10,
        }
        private bool hexa{get {return hexaDecimalCB.Checked;}}
        private View _currentView;
        private View CurrentView
        {
            get{return _currentView;} set { _currentView = value; update(true);}
        }

        /// <summary>
        /// Creates a new DebugWindow
        /// </summary>
        /// <param name="runManager">RunManager reference</param>
        public DebugWindow(IRunManager runManager)
        {
            _runManager = runManager;
            _runManager.WaitOnShutdown++;
            _notifyHandlers = new List<ValueChangedHandler>();
            InitializeComponent();
            Text = _runManager.Opts.Get<string>("gui_windowTitle_debug");
            _notifyHandlers.Add(_runManager.Opts.RegisterChangeNotification("gui_windowTitle_debug",
                        (key, value) => BeginInvoke((Action)delegate { Text = (string)value; })));
            _runManager.OnExit += delegate { if(Created) BeginInvoke((Action) Close); };

            CurrentView = View.TabLowLevel | View.LLThreads;
            _updateTimer = new Timer {Enabled = true, Interval = 250};
            _updateTimer.Tick += delegate {update(false);};
            DebugWindow_Resize(this, null);
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            foreach (var notifyHandler in _notifyHandlers)
                _runManager.Opts.UnregisterChangeNotification(notifyHandler);
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
            _runManager.WaitOnShutdown--;
        }

        private void update (bool viewChanged)
        {
            if(viewChanged)
                doHeaders();
            switch (CurrentView)
            {
                case View.LLThreads:
                    //LLThreads
                    var output = new StringBuilder();
                    output.AppendLine("Native Threads:");
                    foreach(ProcessThread nativeThread in Process.GetCurrentProcess().Threads)
                    {
                        string Id = nativeThread.Id.ToString(hexa?"X":"d");
                        for(int i = 7; i > Id.Length; i--)
                            output.Append(" ");
                        output.Append(Id);
                        output.Append(" ");
                        output.Append(Enum.GetName(typeof(ThreadState), nativeThread.ThreadState));
                        if (nativeThread.ThreadState == ThreadState.Wait)
                        {
                            output.Append(" ");
                            output.Append(Enum.GetName(typeof(ThreadWaitReason), nativeThread.WaitReason));
                        }
                        output.AppendLine();
                    }
                    label1.Text = output.ToString();
                    output.Clear();
                    output.AppendLine("Managed Threads:");
                    foreach(var managedThread in Garm.Base.Helper.ThreadHelper.CurrentThreads)
                    {
                        string Id = managedThread.ManagedThreadId.ToString(hexa?"X":"d");
                        for(int i = 7; i > Id.Length; i--)
                            output.Append(" ");
                        output.Append(Id);
                        output.Append(" ");
                        output.Append(Enum.GetName(typeof(System.Threading.ThreadState), managedThread.ThreadState));
                        output.Append(" '");
                        output.Append(managedThread.Name);
                        output.AppendLine("'");
                    }
                    label2.Text = output.ToString();
                    break;
                case View.LLAssemblys:
                    var loadedAssemblys = AppDomain.CurrentDomain.GetAssemblies();
                    output = new StringBuilder();
                    foreach (var assembly in loadedAssemblys)
                    {
                        var assemblyName = assembly.GetName();
                        output.Append(assemblyName.Name);
                        output.Append(", ");
                        output.AppendLine(assemblyName.Version.ToString());
                    }
                    label1.Text = output.ToString();
                    label2.Text = "";
                    break;
            }
        }
        private void doHeaders()
        {
            switch ((View)((uint)CurrentView & 0xF))
            {
                case View.TabLowLevel:
                    categoryTabs.SelectedIndex = 0;
                    foreach (ToolStripItem item in lowLevelOpts.Items)
                        item.Font = Font;
                    switch ((View)((uint)CurrentView & 0xF0))
                    {
                        case View.LLThreads:
                            lowLevelThreadsBtn.Font = new Font(Font, FontStyle.Bold);
                            break;
                        case View.LLAssemblys:
                            lowLevelAssemblysBtn.Font = new Font(Font, FontStyle.Bold);
                            break;
                    }
                    break;
            }
        }

        private void DebugWindow_Resize(object sender, EventArgs e)
        {
            hexaDecimalCB.Location = new Point(ClientSize.Width-hexaDecimalCB.Size.Width,0);
            categoryTabs.Size = new Size(ClientSize.Width - hexaDecimalCB.Width, 57);
            panel1.Size = new Size(ClientSize.Width, ClientSize.Height - categoryTabs.Size.Height);
            label2.Location = new Point(Math.Max(panel1.ClientSize.Width/2 + 4, label2.Width + 8), 4);
        }

        private void lowLevelThreadsBtn_Click(object sender, EventArgs e)
        {
            CurrentView = (View)((uint)CurrentView & 0xFFFFFF0F) | View.LLThreads;
        }

        private void lowLevelAssemblysBtn_Click(object sender, EventArgs e)
        {
            CurrentView = (View)((uint)CurrentView & 0xFFFFFF0F) | View.LLAssemblys;
        }

        private void hexaDecimalCB_CheckedChanged(object sender, EventArgs e)
        {
            update(false);
        }
    }
}
