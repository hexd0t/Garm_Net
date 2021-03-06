﻿namespace Debug
{
    sealed partial class DebugWindow
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.categoryTabs = new System.Windows.Forms.TabControl();
            this.lowLevelTab = new System.Windows.Forms.TabPage();
            this.lowLevelOpts = new System.Windows.Forms.MenuStrip();
            this.lowLevelThreadsBtn = new System.Windows.Forms.ToolStripMenuItem();
            this.lowLevelAssemblysBtn = new System.Windows.Forms.ToolStripMenuItem();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.outputOpts = new System.Windows.Forms.MenuStrip();
            this.outputRenderBtn = new System.Windows.Forms.ToolStripMenuItem();
            this.outputAudioBtn = new System.Windows.Forms.ToolStripMenuItem();
            this.panel1 = new System.Windows.Forms.Panel();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.hexaDecimalCB = new System.Windows.Forms.CheckBox();
            this.button1 = new System.Windows.Forms.Button();
            this.categoryTabs.SuspendLayout();
            this.lowLevelTab.SuspendLayout();
            this.lowLevelOpts.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.outputOpts.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // categoryTabs
            // 
            this.categoryTabs.Controls.Add(this.lowLevelTab);
            this.categoryTabs.Controls.Add(this.tabPage2);
            this.categoryTabs.Location = new System.Drawing.Point(0, 0);
            this.categoryTabs.Name = "categoryTabs";
            this.categoryTabs.SelectedIndex = 0;
            this.categoryTabs.Size = new System.Drawing.Size(597, 57);
            this.categoryTabs.TabIndex = 0;
            // 
            // lowLevelTab
            // 
            this.lowLevelTab.BackColor = System.Drawing.Color.White;
            this.lowLevelTab.Controls.Add(this.lowLevelOpts);
            this.lowLevelTab.Location = new System.Drawing.Point(4, 22);
            this.lowLevelTab.Name = "lowLevelTab";
            this.lowLevelTab.Padding = new System.Windows.Forms.Padding(3);
            this.lowLevelTab.Size = new System.Drawing.Size(589, 31);
            this.lowLevelTab.TabIndex = 0;
            this.lowLevelTab.Text = "LowLevel";
            // 
            // lowLevelOpts
            // 
            this.lowLevelOpts.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lowLevelOpts.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lowLevelThreadsBtn,
            this.lowLevelAssemblysBtn});
            this.lowLevelOpts.Location = new System.Drawing.Point(3, 3);
            this.lowLevelOpts.Name = "lowLevelOpts";
            this.lowLevelOpts.Size = new System.Drawing.Size(583, 25);
            this.lowLevelOpts.TabIndex = 0;
            this.lowLevelOpts.Text = "menuStrip1";
            // 
            // lowLevelThreadsBtn
            // 
            this.lowLevelThreadsBtn.Name = "lowLevelThreadsBtn";
            this.lowLevelThreadsBtn.Size = new System.Drawing.Size(61, 21);
            this.lowLevelThreadsBtn.Text = "Threads";
            this.lowLevelThreadsBtn.Click += new System.EventHandler(this.lowLevelThreadsBtn_Click);
            // 
            // lowLevelAssemblysBtn
            // 
            this.lowLevelAssemblysBtn.Name = "lowLevelAssemblysBtn";
            this.lowLevelAssemblysBtn.Size = new System.Drawing.Size(75, 21);
            this.lowLevelAssemblysBtn.Text = "Assemblys";
            this.lowLevelAssemblysBtn.Click += new System.EventHandler(this.lowLevelAssemblysBtn_Click);
            // 
            // tabPage2
            // 
            this.tabPage2.BackColor = System.Drawing.Color.White;
            this.tabPage2.Controls.Add(this.outputOpts);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(589, 31);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Outputs";
            // 
            // outputOpts
            // 
            this.outputOpts.Dock = System.Windows.Forms.DockStyle.Fill;
            this.outputOpts.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.outputRenderBtn,
            this.outputAudioBtn});
            this.outputOpts.Location = new System.Drawing.Point(3, 3);
            this.outputOpts.Name = "outputOpts";
            this.outputOpts.Size = new System.Drawing.Size(583, 25);
            this.outputOpts.TabIndex = 1;
            this.outputOpts.Text = "menuStrip1";
            // 
            // outputRenderBtn
            // 
            this.outputRenderBtn.Name = "outputRenderBtn";
            this.outputRenderBtn.Size = new System.Drawing.Size(56, 21);
            this.outputRenderBtn.Text = "Render";
            this.outputRenderBtn.Click += new System.EventHandler(this.outputRenderBtn_Click);
            // 
            // outputAudioBtn
            // 
            this.outputAudioBtn.Name = "outputAudioBtn";
            this.outputAudioBtn.Size = new System.Drawing.Size(51, 21);
            this.outputAudioBtn.Text = "Audio";
            this.outputAudioBtn.Click += new System.EventHandler(this.outputAudioBtn_Click);
            // 
            // panel1
            // 
            this.panel1.AutoScroll = true;
            this.panel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.panel1.BackColor = System.Drawing.Color.PaleGreen;
            this.panel1.Controls.Add(this.button1);
            this.panel1.Controls.Add(this.comboBox1);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.panel1.Location = new System.Drawing.Point(0, 57);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(648, 450);
            this.panel1.TabIndex = 1;
            // 
            // comboBox1
            // 
            this.comboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(9, 7);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(549, 21);
            this.comboBox1.TabIndex = 2;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(307, 41);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(43, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "label2";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 41);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(43, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "label1";
            // 
            // hexaDecimalCB
            // 
            this.hexaDecimalCB.AutoSize = true;
            this.hexaDecimalCB.Location = new System.Drawing.Point(603, 0);
            this.hexaDecimalCB.Name = "hexaDecimalCB";
            this.hexaDecimalCB.Size = new System.Drawing.Size(45, 17);
            this.hexaDecimalCB.TabIndex = 1;
            this.hexaDecimalCB.Text = "Hex";
            this.hexaDecimalCB.UseVisualStyleBackColor = true;
            this.hexaDecimalCB.CheckedChanged += new System.EventHandler(this.hexaDecimalCB_CheckedChanged);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(565, 7);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 3;
            this.button1.Text = "Update";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // DebugWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.PowderBlue;
            this.ClientSize = new System.Drawing.Size(648, 507);
            this.Controls.Add(this.hexaDecimalCB);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.categoryTabs);
            this.DoubleBuffered = true;
            this.Name = "DebugWindow";
            this.Text = "->Debug";
            this.Resize += new System.EventHandler(this.DebugWindow_Resize);
            this.categoryTabs.ResumeLayout(false);
            this.lowLevelTab.ResumeLayout(false);
            this.lowLevelTab.PerformLayout();
            this.lowLevelOpts.ResumeLayout(false);
            this.lowLevelOpts.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.outputOpts.ResumeLayout(false);
            this.outputOpts.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TabControl categoryTabs;
        private System.Windows.Forms.TabPage lowLevelTab;
        private System.Windows.Forms.MenuStrip lowLevelOpts;
        private System.Windows.Forms.ToolStripMenuItem lowLevelThreadsBtn;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ToolStripMenuItem lowLevelAssemblysBtn;
        private System.Windows.Forms.CheckBox hexaDecimalCB;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.MenuStrip outputOpts;
        private System.Windows.Forms.ToolStripMenuItem outputRenderBtn;
        private System.Windows.Forms.ToolStripMenuItem outputAudioBtn;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.Button button1;
    }
}

