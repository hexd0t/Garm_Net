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
using Garm.View.Human.Render;

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
            Undefined   =    0x0,
            TabLowLevel =   0x01,
            LLThreads   = 0x0101,
            LLAssemblys = 0x0201,
            TabOutput   =   0x02,
            OutRender   = 0x0102,
            OutAudio    = 0x0202,
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

            CurrentView = runManager.Opts.Get<View>("dbg_dbgWnd_openTab", true);
            if (CurrentView == View.Undefined)
                CurrentView = View.LLThreads;
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
            _runManager.Opts.Set("dbg_dbgWnd_openTab", CurrentView, false);
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
            _runManager.WaitOnShutdown--;
        }

        private void update (bool viewChanged)
        {
            if (viewChanged)
            {
                doHeaders();
                switch (CurrentView)
                {
                    case View.LLThreads:
                        label1.Visible = true;
                        label2.Visible = true;
                        label1.Location = new Point(label1.Location.X, 7);
                        label2.Location = new Point(label2.Location.X, 7);
                        comboBox1.Visible = false;
                        button1.Visible = false;
                        break;
                    case View.LLAssemblys:
                        label1.Visible = true;
                        label2.Visible = false;
                        label1.Location = new Point(label1.Location.X, 7);
                        comboBox1.Visible = false;
                        button1.Visible = false;
                        break;
                    case View.OutRender:
                        label1.Visible = true;
                        label2.Visible = false;
                        label1.Location = new Point(label1.Location.X, 41);
                        comboBox1.Visible = true;
                        button1.Visible = true;
                        comboBox1.Items.Clear();
                        for(int i = 0; i < _runManager.Views.Count; i++)
                        {
                            var renderMember = _runManager.Views[i].GetType().GetField("Render");
                            if (renderMember == null)
                                continue;
                            var render = renderMember.GetValue(_runManager.Views[i]) as RenderManager;
                            if (render == null)
#if DEBUG
                                throw new Exception("View-Class '"+_runManager.Views[i].GetType().Name+"' contains a Field Render which is not a RenderManager!");
#else
                                continue;
#endif
                            comboBox1.Items.Add(new Garm.Gui.WinForms.ComboBoxHiddenContentContainer("View" + i + "[" + _runManager.Views[i].GetType().Name + "].Renderer", render));
                            foreach (var content in render.Content)
                                comboBox1.Items.Add(new Garm.Gui.WinForms.ComboBoxHiddenContentContainer("View" + i + "[" + _runManager.Views[i].GetType().Name + "].Renderer."+content.GetType().Name, content));
                        }
                        break;
                    case View.OutAudio:
                        label1.Visible = true;
                        label2.Visible = false;
                        comboBox1.Visible = true;
                        button1.Visible = true;
                        label1.Location = new Point(label1.Location.X, 41);
                        break;
                }
            }
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
                case View.OutRender:
                    if (comboBox1.SelectedItem == null || !(comboBox1.SelectedItem is Garm.Gui.WinForms.ComboBoxHiddenContentContainer))
                    {
                        label1.Text = "Please select statsource!";
                        break;
                    }
                    var statsource = ((Garm.Gui.WinForms.ComboBoxHiddenContentContainer)comboBox1.SelectedItem).Data;
                    output = new StringBuilder();
                    if (statsource is RenderManager)
                    {
                        var render = (RenderManager)statsource;
                        output.Append("Current FPS: ");
                        output.AppendLine(render.CurrentFps.ToString("0.0"));
                        output.Append("Camera Lookat: ");
                        output.Append(render.CameraLookAt.X.ToString("0.00"));
                        output.Append(" ");
                        output.Append(render.CameraLookAt.Y.ToString("0.00"));
                        output.Append(" ");
                        output.AppendLine(render.CameraLookAt.Z.ToString("0.00"));
                        output.Append("Camera Location: ");
                        output.Append(render.CameraLocation.X.ToString("0.00"));
                        output.Append(" ");
                        output.Append(render.CameraLocation.Y.ToString("0.00"));
                        output.Append(" ");
                        output.AppendLine(render.CameraLocation.Z.ToString("0.00"));
                        output.Append("Camera UpVector: ");
                        output.Append(render.CameraUpVector.X.ToString("0.00"));
                        output.Append(" ");
                        output.Append(render.CameraUpVector.Y.ToString("0.00"));
                        output.Append(" ");
                        output.AppendLine(render.CameraUpVector.Z.ToString("0.00"));
                    }
                    else if(statsource is Garm.View.Human.Render.Terrain.TerrainSubrender)
                    {
                        var terrain = (Garm.View.Human.Render.Terrain.TerrainSubrender)statsource;
                        
                    }
                    label1.Text = output.ToString();
                    break;
            }
        }
        private void doHeaders()
        {
            foreach (ToolStripItem item in lowLevelOpts.Items)
                item.Font = Font;
            foreach (ToolStripItem item in outputOpts.Items)
                item.Font = Font;
            switch ((View)((uint)CurrentView & 0xFF))
            {
                case View.TabLowLevel:
                    categoryTabs.SelectedIndex = 0;
                    switch (CurrentView)
                    {
                        case View.LLThreads:
                            lowLevelThreadsBtn.Font = new Font(Font, FontStyle.Bold);
                            break;
                        case View.LLAssemblys:
                            lowLevelAssemblysBtn.Font = new Font(Font, FontStyle.Bold);
                            break;
                    }
                    break;
                case View.TabOutput:
                    categoryTabs.SelectedIndex = 1;
                    switch (CurrentView)
                    {
                        case View.OutRender:
                            outputRenderBtn.Font = new Font(Font, FontStyle.Bold);
                            break;
                        case View.OutAudio:
                            outputAudioBtn.Font = new Font(Font, FontStyle.Bold);
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
            label2.Location = new Point(Math.Max(panel1.ClientSize.Width / 2 + 4, label2.Width + 8), label2.Location.Y);
        }

        private void lowLevelThreadsBtn_Click(object sender, EventArgs e)
        {
            CurrentView = View.LLThreads;
        }

        private void lowLevelAssemblysBtn_Click(object sender, EventArgs e)
        {
            CurrentView = View.LLAssemblys;
        }

        private void hexaDecimalCB_CheckedChanged(object sender, EventArgs e)
        {
            update(false);
        }

        private void outputRenderBtn_Click(object sender, EventArgs e)
        {
            CurrentView = View.OutRender;
        }

        private void outputAudioBtn_Click(object sender, EventArgs e)
        {
            CurrentView = View.OutAudio;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            update(true);
        }
    }
}
