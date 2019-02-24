namespace Black_Midi_Render
{
    partial class SettingsForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.wavPath = new System.Windows.Forms.TextBox();
            this.browseWavButton = new System.Windows.Forms.Button();
            this.useAudioCheck = new System.Windows.Forms.CheckBox();
            this.browseButton = new System.Windows.Forms.Button();
            this.width_nud = new System.Windows.Forms.NumericUpDown();
            this.loadButton = new System.Windows.Forms.Button();
            this.label11 = new System.Windows.Forms.Label();
            this.height_nud = new System.Windows.Forms.NumericUpDown();
            this.maxBuffer_nud = new System.Windows.Forms.NumericUpDown();
            this.unloadButton = new System.Windows.Forms.Button();
            this.bitrate_nud = new System.Windows.Forms.NumericUpDown();
            this.fps_nud = new System.Windows.Forms.NumericUpDown();
            this.label10 = new System.Windows.Forms.Label();
            this.MidiBox = new System.Windows.Forms.TextBox();
            this.imgpath = new System.Windows.Forms.TextBox();
            this.ffpath = new System.Windows.Forms.TextBox();
            this.browseImgButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.browseFFButton = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.noneVsync_radio = new System.Windows.Forms.RadioButton();
            this.label3 = new System.Windows.Forms.Label();
            this.none_radio = new System.Windows.Forms.RadioButton();
            this.img_radio = new System.Windows.Forms.RadioButton();
            this.ff_radio = new System.Windows.Forms.RadioButton();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.noteBrightness_nud = new System.Windows.Forms.NumericUpDown();
            this.label15 = new System.Windows.Forms.Label();
            this.noteRenderBox = new System.Windows.Forms.ComboBox();
            this.label14 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.keyboardRenderBox = new System.Windows.Forms.ComboBox();
            this.label13 = new System.Windows.Forms.Label();
            this.glowRad_nud = new System.Windows.Forms.NumericUpDown();
            this.glowCheck = new System.Windows.Forms.CheckBox();
            this.minNote_nud = new System.Windows.Forms.NumericUpDown();
            this.maxNote_nud = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.pianoHeight_nud = new System.Windows.Forms.NumericUpDown();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.noteDT_nud = new System.Windows.Forms.NumericUpDown();
            this.stopButton = new System.Windows.Forms.Button();
            this.startButton = new System.Windows.Forms.Button();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.groupBox1.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.width_nud)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.height_nud)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.maxBuffer_nud)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.bitrate_nud)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.fps_nud)).BeginInit();
            this.tabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.noteBrightness_nud)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.glowRad_nud)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.minNote_nud)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.maxNote_nud)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pianoHeight_nud)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.noteDT_nud)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.tabControl1);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(715, 394);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Settings";
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.tabControl1.Location = new System.Drawing.Point(6, 19);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(703, 369);
            this.tabControl1.TabIndex = 17;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.wavPath);
            this.tabPage1.Controls.Add(this.browseWavButton);
            this.tabPage1.Controls.Add(this.useAudioCheck);
            this.tabPage1.Controls.Add(this.browseButton);
            this.tabPage1.Controls.Add(this.width_nud);
            this.tabPage1.Controls.Add(this.loadButton);
            this.tabPage1.Controls.Add(this.label11);
            this.tabPage1.Controls.Add(this.height_nud);
            this.tabPage1.Controls.Add(this.maxBuffer_nud);
            this.tabPage1.Controls.Add(this.unloadButton);
            this.tabPage1.Controls.Add(this.bitrate_nud);
            this.tabPage1.Controls.Add(this.fps_nud);
            this.tabPage1.Controls.Add(this.label10);
            this.tabPage1.Controls.Add(this.MidiBox);
            this.tabPage1.Controls.Add(this.imgpath);
            this.tabPage1.Controls.Add(this.ffpath);
            this.tabPage1.Controls.Add(this.browseImgButton);
            this.tabPage1.Controls.Add(this.label1);
            this.tabPage1.Controls.Add(this.browseFFButton);
            this.tabPage1.Controls.Add(this.label2);
            this.tabPage1.Controls.Add(this.noneVsync_radio);
            this.tabPage1.Controls.Add(this.label3);
            this.tabPage1.Controls.Add(this.none_radio);
            this.tabPage1.Controls.Add(this.img_radio);
            this.tabPage1.Controls.Add(this.ff_radio);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(695, 343);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "I/O";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // wavPath
            // 
            this.wavPath.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.wavPath.Enabled = false;
            this.wavPath.Location = new System.Drawing.Point(192, 185);
            this.wavPath.Name = "wavPath";
            this.wavPath.ReadOnly = true;
            this.wavPath.Size = new System.Drawing.Size(497, 20);
            this.wavPath.TabIndex = 31;
            // 
            // browseWavButton
            // 
            this.browseWavButton.Enabled = false;
            this.browseWavButton.Location = new System.Drawing.Point(99, 183);
            this.browseWavButton.Name = "browseWavButton";
            this.browseWavButton.Size = new System.Drawing.Size(87, 23);
            this.browseWavButton.TabIndex = 30;
            this.browseWavButton.Text = "Browse File";
            this.browseWavButton.UseVisualStyleBackColor = true;
            this.browseWavButton.Click += new System.EventHandler(this.browseWavButton_Click);
            // 
            // useAudioCheck
            // 
            this.useAudioCheck.AutoSize = true;
            this.useAudioCheck.Enabled = false;
            this.useAudioCheck.Location = new System.Drawing.Point(6, 187);
            this.useAudioCheck.Name = "useAudioCheck";
            this.useAudioCheck.Size = new System.Drawing.Size(91, 17);
            this.useAudioCheck.TabIndex = 29;
            this.useAudioCheck.Text = "Include Audio";
            this.useAudioCheck.UseVisualStyleBackColor = true;
            this.useAudioCheck.CheckedChanged += new System.EventHandler(this.useAudioCheck_CheckedChanged);
            // 
            // browseButton
            // 
            this.browseButton.Location = new System.Drawing.Point(6, 6);
            this.browseButton.Name = "browseButton";
            this.browseButton.Size = new System.Drawing.Size(87, 23);
            this.browseButton.TabIndex = 0;
            this.browseButton.Text = "Browse";
            this.browseButton.UseVisualStyleBackColor = true;
            this.browseButton.Click += new System.EventHandler(this.browseButton_Click);
            // 
            // width_nud
            // 
            this.width_nud.Location = new System.Drawing.Point(47, 103);
            this.width_nud.Maximum = new decimal(new int[] {
            7680,
            0,
            0,
            0});
            this.width_nud.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.width_nud.Name = "width_nud";
            this.width_nud.Size = new System.Drawing.Size(120, 20);
            this.width_nud.TabIndex = 3;
            this.width_nud.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.width_nud.ValueChanged += new System.EventHandler(this.width_nud_ValueChanged);
            // 
            // loadButton
            // 
            this.loadButton.Enabled = false;
            this.loadButton.Location = new System.Drawing.Point(6, 65);
            this.loadButton.Name = "loadButton";
            this.loadButton.Size = new System.Drawing.Size(56, 23);
            this.loadButton.TabIndex = 1;
            this.loadButton.Text = "Load";
            this.loadButton.UseVisualStyleBackColor = true;
            this.loadButton.Click += new System.EventHandler(this.loadButton_Click);
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(96, 41);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(192, 13);
            this.label11.TabIndex = 28;
            this.label11.Text = "Maximum buffer size per track (in bytes)";
            // 
            // height_nud
            // 
            this.height_nud.Location = new System.Drawing.Point(217, 103);
            this.height_nud.Maximum = new decimal(new int[] {
            4320,
            0,
            0,
            0});
            this.height_nud.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.height_nud.Name = "height_nud";
            this.height_nud.Size = new System.Drawing.Size(120, 20);
            this.height_nud.TabIndex = 4;
            this.height_nud.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.height_nud.ValueChanged += new System.EventHandler(this.height_nud_ValueChanged);
            // 
            // maxBuffer_nud
            // 
            this.maxBuffer_nud.Location = new System.Drawing.Point(6, 39);
            this.maxBuffer_nud.Margin = new System.Windows.Forms.Padding(3, 7, 3, 3);
            this.maxBuffer_nud.Maximum = new decimal(new int[] {
            -2147483648,
            0,
            0,
            0});
            this.maxBuffer_nud.Name = "maxBuffer_nud";
            this.maxBuffer_nud.Size = new System.Drawing.Size(84, 20);
            this.maxBuffer_nud.TabIndex = 10;
            this.maxBuffer_nud.ValueChanged += new System.EventHandler(this.maxBuffer_nud_ValueChanged);
            // 
            // unloadButton
            // 
            this.unloadButton.Enabled = false;
            this.unloadButton.Location = new System.Drawing.Point(68, 65);
            this.unloadButton.Name = "unloadButton";
            this.unloadButton.Size = new System.Drawing.Size(56, 23);
            this.unloadButton.TabIndex = 2;
            this.unloadButton.Text = "Unload";
            this.unloadButton.UseVisualStyleBackColor = true;
            this.unloadButton.Click += new System.EventHandler(this.unloadButton_Click);
            // 
            // bitrate_nud
            // 
            this.bitrate_nud.Enabled = false;
            this.bitrate_nud.Location = new System.Drawing.Point(92, 161);
            this.bitrate_nud.Margin = new System.Windows.Forms.Padding(3, 7, 3, 3);
            this.bitrate_nud.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.bitrate_nud.Name = "bitrate_nud";
            this.bitrate_nud.Size = new System.Drawing.Size(94, 20);
            this.bitrate_nud.TabIndex = 12;
            this.bitrate_nud.ValueChanged += new System.EventHandler(this.bitrate_nud_ValueChanged);
            // 
            // fps_nud
            // 
            this.fps_nud.Location = new System.Drawing.Point(376, 103);
            this.fps_nud.Maximum = new decimal(new int[] {
            2147483647,
            0,
            0,
            0});
            this.fps_nud.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.fps_nud.Name = "fps_nud";
            this.fps_nud.Size = new System.Drawing.Size(50, 20);
            this.fps_nud.TabIndex = 5;
            this.fps_nud.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.fps_nud.ValueChanged += new System.EventHandler(this.fps_nud_ValueChanged);
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(49, 163);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(37, 13);
            this.label10.TabIndex = 24;
            this.label10.Text = "Bitrate";
            // 
            // MidiBox
            // 
            this.MidiBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.MidiBox.Location = new System.Drawing.Point(99, 8);
            this.MidiBox.Name = "MidiBox";
            this.MidiBox.ReadOnly = true;
            this.MidiBox.Size = new System.Drawing.Size(590, 20);
            this.MidiBox.TabIndex = 3;
            // 
            // imgpath
            // 
            this.imgpath.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.imgpath.Enabled = false;
            this.imgpath.Location = new System.Drawing.Point(218, 211);
            this.imgpath.Name = "imgpath";
            this.imgpath.ReadOnly = true;
            this.imgpath.Size = new System.Drawing.Size(471, 20);
            this.imgpath.TabIndex = 23;
            // 
            // ffpath
            // 
            this.ffpath.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ffpath.Enabled = false;
            this.ffpath.Location = new System.Drawing.Point(192, 137);
            this.ffpath.Name = "ffpath";
            this.ffpath.ReadOnly = true;
            this.ffpath.Size = new System.Drawing.Size(497, 20);
            this.ffpath.TabIndex = 22;
            // 
            // browseImgButton
            // 
            this.browseImgButton.Enabled = false;
            this.browseImgButton.Location = new System.Drawing.Point(115, 209);
            this.browseImgButton.Name = "browseImgButton";
            this.browseImgButton.Size = new System.Drawing.Size(94, 23);
            this.browseImgButton.TabIndex = 14;
            this.browseImgButton.Text = "Browse Folder";
            this.browseImgButton.UseVisualStyleBackColor = true;
            this.browseImgButton.Click += new System.EventHandler(this.browseImgButton_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 105);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(35, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "Width";
            // 
            // browseFFButton
            // 
            this.browseFFButton.Enabled = false;
            this.browseFFButton.Location = new System.Drawing.Point(99, 135);
            this.browseFFButton.Name = "browseFFButton";
            this.browseFFButton.Size = new System.Drawing.Size(87, 23);
            this.browseFFButton.TabIndex = 13;
            this.browseFFButton.Text = "Browse File";
            this.browseFFButton.UseVisualStyleBackColor = true;
            this.browseFFButton.Click += new System.EventHandler(this.browseFFButton_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(173, 105);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(38, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "Height";
            // 
            // noneVsync_radio
            // 
            this.noneVsync_radio.AutoSize = true;
            this.noneVsync_radio.Checked = true;
            this.noneVsync_radio.Location = new System.Drawing.Point(6, 258);
            this.noneVsync_radio.Name = "noneVsync_radio";
            this.noneVsync_radio.Size = new System.Drawing.Size(105, 17);
            this.noneVsync_radio.TabIndex = 19;
            this.noneVsync_radio.TabStop = true;
            this.noneVsync_radio.Text = "None with Vsync";
            this.noneVsync_radio.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(343, 107);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(27, 13);
            this.label3.TabIndex = 7;
            this.label3.Text = "FPS";
            // 
            // none_radio
            // 
            this.none_radio.AutoSize = true;
            this.none_radio.Location = new System.Drawing.Point(6, 235);
            this.none_radio.Name = "none_radio";
            this.none_radio.Size = new System.Drawing.Size(51, 17);
            this.none_radio.TabIndex = 18;
            this.none_radio.Text = "None";
            this.none_radio.UseVisualStyleBackColor = true;
            // 
            // img_radio
            // 
            this.img_radio.AutoSize = true;
            this.img_radio.Location = new System.Drawing.Point(6, 212);
            this.img_radio.Name = "img_radio";
            this.img_radio.Size = new System.Drawing.Size(104, 17);
            this.img_radio.TabIndex = 17;
            this.img_radio.Text = "Image sequence";
            this.img_radio.UseVisualStyleBackColor = true;
            this.img_radio.CheckedChanged += new System.EventHandler(this.img_radio_CheckedChanged);
            // 
            // ff_radio
            // 
            this.ff_radio.AutoSize = true;
            this.ff_radio.Location = new System.Drawing.Point(6, 138);
            this.ff_radio.Name = "ff_radio";
            this.ff_radio.Size = new System.Drawing.Size(80, 17);
            this.ff_radio.TabIndex = 16;
            this.ff_radio.Text = "ffmpeg mp4";
            this.ff_radio.UseVisualStyleBackColor = true;
            this.ff_radio.CheckedChanged += new System.EventHandler(this.ff_radio_CheckedChanged);
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.noteBrightness_nud);
            this.tabPage2.Controls.Add(this.label15);
            this.tabPage2.Controls.Add(this.noteRenderBox);
            this.tabPage2.Controls.Add(this.label14);
            this.tabPage2.Controls.Add(this.label12);
            this.tabPage2.Controls.Add(this.keyboardRenderBox);
            this.tabPage2.Controls.Add(this.label13);
            this.tabPage2.Controls.Add(this.glowRad_nud);
            this.tabPage2.Controls.Add(this.glowCheck);
            this.tabPage2.Controls.Add(this.minNote_nud);
            this.tabPage2.Controls.Add(this.maxNote_nud);
            this.tabPage2.Controls.Add(this.label4);
            this.tabPage2.Controls.Add(this.label5);
            this.tabPage2.Controls.Add(this.label6);
            this.tabPage2.Controls.Add(this.label9);
            this.tabPage2.Controls.Add(this.pianoHeight_nud);
            this.tabPage2.Controls.Add(this.label8);
            this.tabPage2.Controls.Add(this.label7);
            this.tabPage2.Controls.Add(this.noteDT_nud);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(695, 343);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Graphics";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // noteBrightness_nud
            // 
            this.noteBrightness_nud.DecimalPlaces = 2;
            this.noteBrightness_nud.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.noteBrightness_nud.Location = new System.Drawing.Point(89, 181);
            this.noteBrightness_nud.Maximum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.noteBrightness_nud.Name = "noteBrightness_nud";
            this.noteBrightness_nud.Size = new System.Drawing.Size(79, 20);
            this.noteBrightness_nud.TabIndex = 50;
            this.noteBrightness_nud.ValueChanged += new System.EventHandler(this.noteBrightness_nud_ValueChanged);
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(6, 183);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(82, 13);
            this.label15.TabIndex = 49;
            this.label15.Text = "Note Brightness";
            // 
            // noteRenderBox
            // 
            this.noteRenderBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.noteRenderBox.FormattingEnabled = true;
            this.noteRenderBox.Items.AddRange(new object[] {
            "Shaded",
            "Flat"});
            this.noteRenderBox.Location = new System.Drawing.Point(111, 124);
            this.noteRenderBox.Name = "noteRenderBox";
            this.noteRenderBox.Size = new System.Drawing.Size(121, 21);
            this.noteRenderBox.TabIndex = 48;
            this.noteRenderBox.SelectedIndexChanged += new System.EventHandler(this.noteRenderBox_SelectedIndexChanged);
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(6, 127);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(77, 13);
            this.label14.TabIndex = 47;
            this.label14.Text = "Note Renderer";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(6, 100);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(99, 13);
            this.label12.TabIndex = 46;
            this.label12.Text = "Keyboard Renderer";
            // 
            // keyboardRenderBox
            // 
            this.keyboardRenderBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.keyboardRenderBox.FormattingEnabled = true;
            this.keyboardRenderBox.Items.AddRange(new object[] {
            "Legacy",
            "New",
            "Flat"});
            this.keyboardRenderBox.Location = new System.Drawing.Point(111, 97);
            this.keyboardRenderBox.Name = "keyboardRenderBox";
            this.keyboardRenderBox.Size = new System.Drawing.Size(121, 21);
            this.keyboardRenderBox.TabIndex = 45;
            this.keyboardRenderBox.SelectedIndexChanged += new System.EventHandler(this.keyboardRenderBox_SelectedIndexChanged);
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(195, 155);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(67, 13);
            this.label13.TabIndex = 44;
            this.label13.Text = "Glow Radius";
            // 
            // glowRad_nud
            // 
            this.glowRad_nud.Location = new System.Drawing.Point(104, 153);
            this.glowRad_nud.Margin = new System.Windows.Forms.Padding(3, 7, 3, 3);
            this.glowRad_nud.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.glowRad_nud.Name = "glowRad_nud";
            this.glowRad_nud.Size = new System.Drawing.Size(84, 20);
            this.glowRad_nud.TabIndex = 39;
            this.glowRad_nud.ValueChanged += new System.EventHandler(this.glowRad_nud_ValueChanged);
            // 
            // glowCheck
            // 
            this.glowCheck.AutoSize = true;
            this.glowCheck.Location = new System.Drawing.Point(6, 154);
            this.glowCheck.Name = "glowCheck";
            this.glowCheck.Size = new System.Drawing.Size(92, 17);
            this.glowCheck.TabIndex = 43;
            this.glowCheck.Text = "Glow Enabled";
            this.glowCheck.UseVisualStyleBackColor = true;
            this.glowCheck.CheckedChanged += new System.EventHandler(this.glowCheck_CheckedChanged);
            // 
            // minNote_nud
            // 
            this.minNote_nud.Location = new System.Drawing.Point(68, 11);
            this.minNote_nud.Margin = new System.Windows.Forms.Padding(3, 15, 3, 3);
            this.minNote_nud.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.minNote_nud.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.minNote_nud.Name = "minNote_nud";
            this.minNote_nud.Size = new System.Drawing.Size(68, 20);
            this.minNote_nud.TabIndex = 32;
            this.minNote_nud.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.minNote_nud.ValueChanged += new System.EventHandler(this.minNote_nud_ValueChanged);
            // 
            // maxNote_nud
            // 
            this.maxNote_nud.Location = new System.Drawing.Point(201, 11);
            this.maxNote_nud.Maximum = new decimal(new int[] {
            256,
            0,
            0,
            0});
            this.maxNote_nud.Minimum = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.maxNote_nud.Name = "maxNote_nud";
            this.maxNote_nud.Size = new System.Drawing.Size(71, 20);
            this.maxNote_nud.TabIndex = 33;
            this.maxNote_nud.Value = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.maxNote_nud.ValueChanged += new System.EventHandler(this.maxNote_nud_ValueChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 13);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(52, 13);
            this.label4.TabIndex = 34;
            this.label4.Text = "First Note";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(142, 13);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(53, 13);
            this.label5.TabIndex = 36;
            this.label5.Text = "Last Note";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(6, 43);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(68, 13);
            this.label6.TabIndex = 38;
            this.label6.Text = "Piano Height";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(160, 43);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(91, 13);
            this.label9.TabIndex = 42;
            this.label9.Text = "% from the bottom";
            // 
            // pianoHeight_nud
            // 
            this.pianoHeight_nud.Location = new System.Drawing.Point(80, 41);
            this.pianoHeight_nud.Margin = new System.Windows.Forms.Padding(3, 15, 3, 3);
            this.pianoHeight_nud.Name = "pianoHeight_nud";
            this.pianoHeight_nud.Size = new System.Drawing.Size(74, 20);
            this.pianoHeight_nud.TabIndex = 35;
            this.pianoHeight_nud.ValueChanged += new System.EventHandler(this.pianoHeight_nud_ValueChanged);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(215, 73);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(74, 13);
            this.label8.TabIndex = 41;
            this.label8.Text = "Lower = faster";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(6, 73);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(113, 13);
            this.label7.TabIndex = 40;
            this.label7.Text = "Note delta screen time";
            // 
            // noteDT_nud
            // 
            this.noteDT_nud.Location = new System.Drawing.Point(125, 71);
            this.noteDT_nud.Margin = new System.Windows.Forms.Padding(3, 7, 3, 3);
            this.noteDT_nud.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.noteDT_nud.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.noteDT_nud.Name = "noteDT_nud";
            this.noteDT_nud.Size = new System.Drawing.Size(84, 20);
            this.noteDT_nud.TabIndex = 37;
            this.noteDT_nud.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.noteDT_nud.ValueChanged += new System.EventHandler(this.noteDT_nud_ValueChanged);
            // 
            // stopButton
            // 
            this.stopButton.Enabled = false;
            this.stopButton.Location = new System.Drawing.Point(91, 412);
            this.stopButton.Name = "stopButton";
            this.stopButton.Size = new System.Drawing.Size(75, 23);
            this.stopButton.TabIndex = 16;
            this.stopButton.Text = "Stop";
            this.stopButton.UseVisualStyleBackColor = true;
            this.stopButton.Click += new System.EventHandler(this.stopButton_Click);
            // 
            // startButton
            // 
            this.startButton.Enabled = false;
            this.startButton.Location = new System.Drawing.Point(12, 412);
            this.startButton.Name = "startButton";
            this.startButton.Size = new System.Drawing.Size(75, 23);
            this.startButton.TabIndex = 15;
            this.startButton.Text = "Start";
            this.startButton.UseVisualStyleBackColor = true;
            this.startButton.Click += new System.EventHandler(this.startButton_Click);
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // SettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(739, 442);
            this.Controls.Add(this.stopButton);
            this.Controls.Add(this.startButton);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "SettingsForm";
            this.Text = "7";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SettingsForm_FormClosing);
            this.groupBox1.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.width_nud)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.height_nud)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.maxBuffer_nud)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.bitrate_nud)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.fps_nud)).EndInit();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.noteBrightness_nud)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.glowRad_nud)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.minNote_nud)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.maxNote_nud)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pianoHeight_nud)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.noteDT_nud)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox MidiBox;
        private System.Windows.Forms.Button unloadButton;
        private System.Windows.Forms.Button loadButton;
        private System.Windows.Forms.Button browseButton;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown fps_nud;
        private System.Windows.Forms.NumericUpDown height_nud;
        private System.Windows.Forms.NumericUpDown width_nud;
        private System.Windows.Forms.RadioButton noneVsync_radio;
        private System.Windows.Forms.RadioButton none_radio;
        private System.Windows.Forms.RadioButton img_radio;
        private System.Windows.Forms.RadioButton ff_radio;
        private System.Windows.Forms.Button startButton;
        private System.Windows.Forms.Button stopButton;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.NumericUpDown maxBuffer_nud;
        private System.Windows.Forms.NumericUpDown bitrate_nud;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox imgpath;
        private System.Windows.Forms.TextBox ffpath;
        private System.Windows.Forms.Button browseImgButton;
        private System.Windows.Forms.Button browseFFButton;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TextBox wavPath;
        private System.Windows.Forms.Button browseWavButton;
        private System.Windows.Forms.CheckBox useAudioCheck;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.NumericUpDown glowRad_nud;
        private System.Windows.Forms.CheckBox glowCheck;
        private System.Windows.Forms.NumericUpDown minNote_nud;
        private System.Windows.Forms.NumericUpDown maxNote_nud;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.NumericUpDown pianoHeight_nud;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.NumericUpDown noteDT_nud;
        private System.Windows.Forms.ComboBox keyboardRenderBox;
        private System.Windows.Forms.ComboBox noteRenderBox;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.NumericUpDown noteBrightness_nud;
        private System.Windows.Forms.Label label15;
    }
}