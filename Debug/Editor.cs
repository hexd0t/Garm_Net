using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using Garm.Audio;
using Garm.Base.Content.Terrain;
using Garm.Base.Interfaces;
using Garm.View.Human.Render;
using Garm.View.Human.Render.Terrain;
using SlimDX;

namespace Garm.Debug
{
    public partial class Editor : Form
    {
        protected readonly EditorView View;
        protected readonly IRunManager Manager;
        protected Terrain currentTerrain;
        protected FreeflightCam Camera;

        public Editor(EditorView view, IRunManager manager)
        {
            View = view;
            Manager = manager;
            InitializeComponent();
            toolStripStatusLabel1.Text = "GarmEdit v" + Assembly.GetExecutingAssembly().GetName().Version + "/Garm v" +
                                         Assembly.GetEntryAssembly().GetName().Version;
            comboBox1.Items.AddRange(Enum.GetNames(typeof(InstrumentSynth.FadeMode)));
            comboBox2.Items.AddRange(Enum.GetNames(typeof(InstrumentSynth.FreqgenMode)));
            comboBox3.Items.AddRange(Enum.GetNames(typeof(InstrumentSynth.FreqgenMode)));
            comboBox4.Items.AddRange(Enum.GetNames(typeof(InstrumentSynth.FreqgenMode)));

            Camera = new FreeflightCam();
            RenderTarget.KeyDown += RenderTarget_KeyDown;
        }

        void RenderTarget_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.A:
                    Camera.LookAt.X += (float)Math.Sin(Camera.Rotation)*0.25f;
                    Camera.LookAt.Z += (float)Math.Cos(Camera.Rotation)*0.25f;
                    break;
                case Keys.D:
                    Camera.LookAt.X -= (float)Math.Sin(Camera.Rotation) * 0.25f;
                    Camera.LookAt.Z -= (float)Math.Cos(Camera.Rotation) * 0.25f;
                    break;
                case Keys.S:
                    Camera.LookAt.X -= (float)Math.Cos(Camera.Rotation) * 0.25f;
                    Camera.LookAt.Z += (float)Math.Sin(Camera.Rotation) * 0.25f;
                    break;
                case Keys.W:
                    Camera.LookAt.X += (float)Math.Cos(Camera.Rotation) * 0.25f;
                    Camera.LookAt.Z -= (float)Math.Sin(Camera.Rotation) * 0.25f;
                    break;
                case Keys.PageUp:
                    Camera.Height += 0.25f;
                    break;
                case Keys.PageDown:
                    Camera.Height -= 0.25f;
                    break;
                case Keys.Home:
                    Camera.Distance -= 0.25f;
                    break;
                case Keys.End:
                    Camera.Distance += 0.25f;
                    break;
                case Keys.Insert:
                    Camera.Rotation += 0.785398163397448f;
                    break;
                case Keys.Delete:
                    Camera.Rotation -= 0.785398163397448f;
                    break;
            }
            View.Render.CameraLocation = Camera.Position;
            View.Render.CameraLookAt = Camera.LookAt;
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (tabControl1.SelectedIndex)
            {
                case 0:
                    splitContainer1.FixedPanel = FixedPanel.Panel1;
                    splitContainer1.Panel2Collapsed = false;
                    splitContainer1.SplitterDistance = 300;
                    break;
                case 1:
                    splitContainer1.FixedPanel = FixedPanel.Panel2;
                    splitContainer1.Panel2Collapsed = true;
                    break;
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            View.Audio.MusicSynth = checkBox1.Checked;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (tabControl1.SelectedIndex == 1)
                MusicEditUpdate();
        }

        private void MusicEditUpdate()
        {
            if (View.Audio == null)
            {
                Console.WriteLine("AudioManager not initialized yet!");
                return;
            }
            checkBox1.Checked = View.Audio.MusicSynth;
            if (View.Audio.MusicSynthGen.Instruments.ContainsKey((int)InstrumentChooser.Value))
            {
                CreateRemoveInstrumentBtn.Text = "Remove Instrument";
                button1.Enabled = numericUpDown1.Enabled = numericUpDown2.Enabled = numericUpDown3.Enabled = numericUpDown4.Enabled = numericUpDown5.Enabled = numericUpDown6.Enabled = numericUpDown7.Enabled = numericUpDown8.Enabled = numericUpDown9.Enabled = numericUpDown10.Enabled = numericUpDown11.Enabled = numericUpDown12.Enabled = comboBox1.Enabled = comboBox2.Enabled = comboBox3.Enabled = comboBox4.Enabled = checkBox2.Enabled = true;
                var inst = View.Audio.MusicSynthGen.Instruments[(int)InstrumentChooser.Value];
                numericUpDown1.Value = (decimal)inst.Amplitude;
                numericUpDown2.Value = (decimal)inst.FadeAt;
                numericUpDown3.Value = (decimal)inst.Length;
                numericUpDown4.Value = (decimal)inst.WobbleStrength;
                numericUpDown5.Value = (decimal)inst.WobbleFrequency;
                numericUpDown6.Value = (decimal)inst.AmplitudeBeatStrength;
                numericUpDown7.Value = (decimal)inst.AmplitudeBeatFrequency;
                numericUpDown8.Value = (decimal)inst.FundamentalStrength;
                numericUpDown9.Value = (decimal)inst.Overtone1Strength;
                numericUpDown10.Value = (decimal)inst.Overtone2Strength;
                numericUpDown11.Value = (decimal)inst.Overtone3Strength;
                numericUpDown12.Value = (decimal)inst.Overtone4Strength;
                numericUpDown13.Value = (decimal)inst.PassFactor;
                StrengthSumDisplay.Text = (inst.FundamentalStrength + inst.Overtone1Strength + inst.Overtone2Strength + inst.Overtone3Strength + inst.Overtone4Strength).ToString();
                checkBox2.Checked = inst.Loop;
                if (!comboBox1.Focused)
                    comboBox1.SelectedItem = Enum.GetName(typeof(InstrumentSynth.FadeMode), inst.Fade);
                if (!comboBox2.Focused)
                    comboBox2.SelectedItem = Enum.GetName(typeof(InstrumentSynth.FreqgenMode), inst.BaseFreqGen);
                if (!comboBox3.Focused)
                    comboBox3.SelectedItem = Enum.GetName(typeof(InstrumentSynth.FreqgenMode), inst.WobbleFreqGen);
                if (!comboBox4.Focused)
                    comboBox4.SelectedItem = Enum.GetName(typeof(InstrumentSynth.FreqgenMode), inst.AmplitudeBeatFreqGen);
                if (!textBox1.Focused)
                    textBox1.Text = inst.Name;

            }
            else
            {
                button1.Enabled = numericUpDown1.Enabled = numericUpDown2.Enabled = numericUpDown3.Enabled = numericUpDown4.Enabled = numericUpDown5.Enabled = numericUpDown6.Enabled = numericUpDown7.Enabled = numericUpDown8.Enabled = numericUpDown9.Enabled = numericUpDown10.Enabled = numericUpDown11.Enabled = numericUpDown12.Enabled = comboBox1.Enabled = comboBox2.Enabled = comboBox3.Enabled = comboBox4.Enabled = checkBox2.Enabled = false;
                CreateRemoveInstrumentBtn.Text = "Create Instrument";
            }
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            if (!View.Audio.MusicSynthGen.Instruments.ContainsKey((int)InstrumentChooser.Value))
                return;
            View.Audio.MusicSynthGen.Instruments[(int)InstrumentChooser.Value].Amplitude = (double)numericUpDown1.Value;
        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            if (!View.Audio.MusicSynthGen.Instruments.ContainsKey((int)InstrumentChooser.Value))
                return;
            View.Audio.MusicSynthGen.Instruments[(int)InstrumentChooser.Value].FadeAt = (double)numericUpDown2.Value;
        }

        private void numericUpDown3_ValueChanged(object sender, EventArgs e)
        {
            if (!View.Audio.MusicSynthGen.Instruments.ContainsKey((int)InstrumentChooser.Value))
                return;
            View.Audio.MusicSynthGen.Instruments[(int)InstrumentChooser.Value].Length = (double)numericUpDown3.Value;
        }

        private void numericUpDown4_ValueChanged(object sender, EventArgs e)
        {
            if (!View.Audio.MusicSynthGen.Instruments.ContainsKey((int)InstrumentChooser.Value))
                return;
            View.Audio.MusicSynthGen.Instruments[(int)InstrumentChooser.Value].WobbleStrength = (double)numericUpDown4.Value;
        }

        private void numericUpDown5_ValueChanged(object sender, EventArgs e)
        {
            if (!View.Audio.MusicSynthGen.Instruments.ContainsKey((int)InstrumentChooser.Value))
                return;
            View.Audio.MusicSynthGen.Instruments[(int)InstrumentChooser.Value].WobbleFrequency = (double)numericUpDown5.Value;
        }

        private void numericUpDown6_ValueChanged(object sender, EventArgs e)
        {
            if (!View.Audio.MusicSynthGen.Instruments.ContainsKey((int)InstrumentChooser.Value))
                return;
            View.Audio.MusicSynthGen.Instruments[(int)InstrumentChooser.Value].AmplitudeBeatStrength = (double)numericUpDown6.Value;
        }

        private void numericUpDown7_ValueChanged(object sender, EventArgs e)
        {
            if (!View.Audio.MusicSynthGen.Instruments.ContainsKey((int)InstrumentChooser.Value))
                return;
            View.Audio.MusicSynthGen.Instruments[(int)InstrumentChooser.Value].AmplitudeBeatFrequency = (double)numericUpDown7.Value;
        }

        private void numericUpDown8_ValueChanged(object sender, EventArgs e)
        {
            if (!View.Audio.MusicSynthGen.Instruments.ContainsKey((int)InstrumentChooser.Value))
                return;
            View.Audio.MusicSynthGen.Instruments[(int)InstrumentChooser.Value].FundamentalStrength = (double)numericUpDown8.Value;
        }

        private void numericUpDown9_ValueChanged(object sender, EventArgs e)
        {
            if (!View.Audio.MusicSynthGen.Instruments.ContainsKey((int)InstrumentChooser.Value))
                return;
            View.Audio.MusicSynthGen.Instruments[(int)InstrumentChooser.Value].Overtone1Strength = (double)numericUpDown9.Value;
        }

        private void numericUpDown10_ValueChanged(object sender, EventArgs e)
        {
            if (!View.Audio.MusicSynthGen.Instruments.ContainsKey((int)InstrumentChooser.Value))
                return;
            View.Audio.MusicSynthGen.Instruments[(int)InstrumentChooser.Value].Overtone2Strength = (double)numericUpDown10.Value;
        }

        private void numericUpDown11_ValueChanged(object sender, EventArgs e)
        {
            if (!View.Audio.MusicSynthGen.Instruments.ContainsKey((int)InstrumentChooser.Value))
                return;
            View.Audio.MusicSynthGen.Instruments[(int)InstrumentChooser.Value].Overtone3Strength = (double)numericUpDown11.Value;
        }

        private void numericUpDown12_ValueChanged(object sender, EventArgs e)
        {
            if (!View.Audio.MusicSynthGen.Instruments.ContainsKey((int)InstrumentChooser.Value))
                return;
            View.Audio.MusicSynthGen.Instruments[(int)InstrumentChooser.Value].Overtone4Strength = (double)numericUpDown12.Value;
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (!View.Audio.MusicSynthGen.Instruments.ContainsKey((int)InstrumentChooser.Value))
                return;
            View.Audio.MusicSynthGen.Instruments[(int)InstrumentChooser.Value].Loop = checkBox2.Checked;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!View.Audio.MusicSynthGen.Instruments.ContainsKey((int)InstrumentChooser.Value))
                return;
            View.Audio.MusicSynthGen.Instruments[(int)InstrumentChooser.Value].Fade = (InstrumentSynth.FadeMode)Enum.Parse(typeof(InstrumentSynth.FadeMode), comboBox1.SelectedItem.ToString());
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!View.Audio.MusicSynthGen.Instruments.ContainsKey((int)InstrumentChooser.Value))
                return;
            View.Audio.MusicSynthGen.Instruments[(int)InstrumentChooser.Value].BaseFreqGen = (InstrumentSynth.FreqgenMode)Enum.Parse(typeof(InstrumentSynth.FreqgenMode), comboBox2.SelectedItem.ToString());
        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!View.Audio.MusicSynthGen.Instruments.ContainsKey((int)InstrumentChooser.Value))
                return;
            View.Audio.MusicSynthGen.Instruments[(int)InstrumentChooser.Value].WobbleFreqGen = (InstrumentSynth.FreqgenMode)Enum.Parse(typeof(InstrumentSynth.FreqgenMode), comboBox3.SelectedItem.ToString());
        }

        private void comboBox4_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!View.Audio.MusicSynthGen.Instruments.ContainsKey((int)InstrumentChooser.Value))
                return;
            View.Audio.MusicSynthGen.Instruments[(int)InstrumentChooser.Value].AmplitudeBeatFreqGen = (InstrumentSynth.FreqgenMode)Enum.Parse(typeof(InstrumentSynth.FreqgenMode), comboBox4.SelectedItem.ToString());
        }

        private void CreateRemoveInstrumentBtn_Click(object sender, EventArgs e)
        {
            if (View.Audio.MusicSynthGen.Instruments.ContainsKey((int)InstrumentChooser.Value))
                View.Audio.MusicSynthGen.Instruments.Remove((int)InstrumentChooser.Value);
            else
                View.Audio.MusicSynthGen.Instruments.Add((int)InstrumentChooser.Value, new InstrumentSynth());
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            PitchDisplay.Text = Enum.GetName(typeof(Pitch), (Pitch)trackBar1.Value);
        }

        InstrumentNoteInstance ini = null;
        private void button1_Click(object sender, EventArgs e)
        {
            if (ini == null)
            {
                ini = new InstrumentNoteInstance();
                ini.Instrument = (int)InstrumentChooser.Value;
                ini.time = 0;
                ini.freq = InstrumentSynth.PitchToFrequency(trackBar1.Value);
                View.Audio.MusicSynthGen.AddNote(ini);
                button1.Text = "Stop";
            }
            else
            {
                View.Audio.MusicSynthGen.RemoveNote(ini);
                ini = null;
                button1.Text = "Test";
            }
        }

        private void numericUpDown13_ValueChanged(object sender, EventArgs e)
        {
            if (!View.Audio.MusicSynthGen.Instruments.ContainsKey((int)InstrumentChooser.Value))
                return;
            View.Audio.MusicSynthGen.Instruments[(int)InstrumentChooser.Value].PassFactor = (double)numericUpDown13.Value;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (!View.Audio.MusicSynthGen.Instruments.ContainsKey((int)InstrumentChooser.Value))
                return;
            View.Audio.MusicSynthGen.Instruments[(int)InstrumentChooser.Value].Name = textBox1.Text;
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            var sizeRequest = new Form() { Size = new Size(220, 130),ControlBox = false, Text = Manager.Opts.Get<string>("loc_mapProperties"), FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow};
            sizeRequest.Controls.Add(new NumericUpDown() { Maximum = 100000, Minimum = 1, Location = new Point(70, 10), Value = 102.4m, Size = new Size(70,25),TextAlign = HorizontalAlignment.Right, DecimalPlaces = 1});
            sizeRequest.Controls.Add(new NumericUpDown() { Maximum = 100000, Minimum = 1, Location = new Point(70, 40), Value = 102.4m, Size = new Size(70, 25), TextAlign = HorizontalAlignment.Right, DecimalPlaces = 1 });
            sizeRequest.Controls.Add(new Button() { Text = Manager.Opts.Get<string>("loc_ok"), Location = new Point(10, 70)});
            sizeRequest.Controls.Add(new Label() { Text = Manager.Opts.Get<string>("loc_mapWidth")+@":", Location = new Point(10, 12) });
            sizeRequest.Controls.Add(new Label() { Text = Manager.Opts.Get<string>("loc_mapDepth")+@":", Location = new Point(10, 42) });
            sizeRequest.Controls.Add(new Label() { Text = Manager.Opts.Get<string>("loc_meter"), Location = new Point(150, 12) });
            sizeRequest.Controls.Add(new Label() { Text = Manager.Opts.Get<string>("loc_meter"), Location = new Point(150, 42) });
            sizeRequest.Controls[2].Click += delegate { sizeRequest.Close(); };
            sizeRequest.ShowDialog(this);
            var oldTerrains = View.Render.Content.Where(renderable => renderable is TerrainSubrender).Select(rendereable => View.Render.Content.IndexOf(rendereable)).Reverse();
            foreach (var oldTerrain in oldTerrains)
            {
                View.Render.Content[oldTerrain].Dispose();
                View.Render.Content.RemoveAt(oldTerrain);
            }
            if(currentTerrain != null)
                currentTerrain.Dispose();
            currentTerrain = new Terrain((float)((NumericUpDown)sizeRequest.Controls[0]).Value, (float)((NumericUpDown)sizeRequest.Controls[1]).Value,Manager);
            
            View.Render.Content.Add(new TerrainSubrender(currentTerrain, View.Render, Manager));
        }

        private void RenderTarget_MouseDown(object sender, MouseEventArgs e)
        {
            RenderTarget.Focus();
        }

        private void Editor_FormClosing(object sender, FormClosingEventArgs e)
        {
            var renderables = new IRenderable[View.Render.Content.Count];
            View.Render.Content.CopyTo(renderables, 0);
            View.Render.Content.Clear();
            foreach (IRenderable renderable in renderables)
            {
                renderable.Dispose();
            }
        }

        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            var oldTerrains = View.Render.Content.Where(renderable => renderable is TerrainSubrender).Select(rendereable => View.Render.Content.IndexOf(rendereable)).Reverse();
            foreach (var oldTerrain in oldTerrains)
                View.Render.Content.RemoveAt(oldTerrain);
            if (currentTerrain != null)
                currentTerrain.Dispose();
            currentTerrain = Terrain.GetRandomTerrain(Manager);

            View.Render.Content.Add(new TerrainSubrender(currentTerrain, View.Render, Manager));
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {

        }
    }
}
