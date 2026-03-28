namespace Control_App
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            tableLayoutPanel1 = new TableLayoutPanel();
            webViewCAM3 = new Microsoft.Web.WebView2.WinForms.WebView2();
            webViewCAM2 = new Microsoft.Web.WebView2.WinForms.WebView2();
            panel1 = new Panel();
            tabControl_MQTT = new TabControl();
            tabPage2 = new TabPage();
            BT_Start_Stop_Preview = new Button();
            groupBox1 = new GroupBox();
            label2 = new Label();
            CAM1_ComboBox_ZoomSelect = new ComboBox();
            groupBox4 = new GroupBox();
            label3 = new Label();
            CAM2_ComboBox_ZoomSelect = new ComboBox();
            groupBox2 = new GroupBox();
            label4 = new Label();
            CAM3_ComboBox_ZoomSelect = new ComboBox();
            tabPage3 = new TabPage();
            groupBox5 = new GroupBox();
            RB_TOPMIDDLE = new RadioButton();
            RB_MIDDLEBOTTOM = new RadioButton();
            RB_TOPBOTTOM = new RadioButton();
            RB_ALL = new RadioButton();
            BT_Start_Mesh = new Button();
            groupBox3 = new GroupBox();
            TB_Obj_Name = new TextBox();
            label1 = new Label();
            CAM3_Zoom_Curr = new Label();
            CAM2_Zoom_Curr = new Label();
            CAM1_Zoom_Curr = new Label();
            BT_Start_Capture = new Button();
            webViewCAM1 = new Microsoft.Web.WebView2.WinForms.WebView2();
            RB_TOP = new RadioButton();
            RB_BOTTOM = new RadioButton();
            RB_MIDDLE = new RadioButton();
            tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)webViewCAM3).BeginInit();
            ((System.ComponentModel.ISupportInitialize)webViewCAM2).BeginInit();
            panel1.SuspendLayout();
            tabControl_MQTT.SuspendLayout();
            tabPage2.SuspendLayout();
            groupBox1.SuspendLayout();
            groupBox4.SuspendLayout();
            groupBox2.SuspendLayout();
            tabPage3.SuspendLayout();
            groupBox5.SuspendLayout();
            groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)webViewCAM1).BeginInit();
            SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 2;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.Controls.Add(webViewCAM3, 1, 1);
            tableLayoutPanel1.Controls.Add(webViewCAM2, 1, 0);
            tableLayoutPanel1.Controls.Add(panel1, 0, 1);
            tableLayoutPanel1.Controls.Add(webViewCAM1, 0, 0);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(0, 0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 2;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.Size = new Size(1390, 720);
            tableLayoutPanel1.TabIndex = 0;
            // 
            // webViewCAM3
            // 
            webViewCAM3.AllowExternalDrop = true;
            webViewCAM3.CreationProperties = null;
            webViewCAM3.DefaultBackgroundColor = Color.White;
            webViewCAM3.Dock = DockStyle.Fill;
            webViewCAM3.Location = new Point(698, 363);
            webViewCAM3.Name = "webViewCAM3";
            webViewCAM3.Size = new Size(689, 354);
            webViewCAM3.TabIndex = 3;
            webViewCAM3.ZoomFactor = 1D;
            // 
            // webViewCAM2
            // 
            webViewCAM2.AllowExternalDrop = true;
            webViewCAM2.CreationProperties = null;
            webViewCAM2.DefaultBackgroundColor = Color.White;
            webViewCAM2.Dock = DockStyle.Fill;
            webViewCAM2.Location = new Point(698, 3);
            webViewCAM2.Name = "webViewCAM2";
            webViewCAM2.Size = new Size(689, 354);
            webViewCAM2.TabIndex = 2;
            webViewCAM2.ZoomFactor = 1D;
            // 
            // panel1
            // 
            panel1.Controls.Add(tabControl_MQTT);
            panel1.Dock = DockStyle.Fill;
            panel1.Location = new Point(3, 363);
            panel1.Name = "panel1";
            panel1.Size = new Size(689, 354);
            panel1.TabIndex = 0;
            // 
            // tabControl_MQTT
            // 
            tabControl_MQTT.Controls.Add(tabPage2);
            tabControl_MQTT.Controls.Add(tabPage3);
            tabControl_MQTT.Dock = DockStyle.Fill;
            tabControl_MQTT.Location = new Point(0, 0);
            tabControl_MQTT.Name = "tabControl_MQTT";
            tabControl_MQTT.SelectedIndex = 0;
            tabControl_MQTT.Size = new Size(689, 354);
            tabControl_MQTT.TabIndex = 0;
            // 
            // tabPage2
            // 
            tabPage2.Controls.Add(BT_Start_Stop_Preview);
            tabPage2.Controls.Add(groupBox1);
            tabPage2.Controls.Add(groupBox4);
            tabPage2.Controls.Add(groupBox2);
            tabPage2.Location = new Point(4, 34);
            tabPage2.Name = "tabPage2";
            tabPage2.Padding = new Padding(3);
            tabPage2.Size = new Size(681, 316);
            tabPage2.TabIndex = 1;
            tabPage2.Text = "CAM";
            tabPage2.UseVisualStyleBackColor = true;
            // 
            // BT_Start_Stop_Preview
            // 
            BT_Start_Stop_Preview.FlatStyle = FlatStyle.System;
            BT_Start_Stop_Preview.Location = new Point(259, 172);
            BT_Start_Stop_Preview.Name = "BT_Start_Stop_Preview";
            BT_Start_Stop_Preview.Size = new Size(164, 33);
            BT_Start_Stop_Preview.TabIndex = 7;
            BT_Start_Stop_Preview.Text = "Restart Preview";
            BT_Start_Stop_Preview.UseVisualStyleBackColor = true;
            BT_Start_Stop_Preview.Click += BT_START_STOP_Preview_Click;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(label2);
            groupBox1.Controls.Add(CAM1_ComboBox_ZoomSelect);
            groupBox1.Location = new Point(6, 17);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(164, 102);
            groupBox1.TabIndex = 3;
            groupBox1.TabStop = false;
            groupBox1.Text = "CAM1 Zoom";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(7, 73);
            label2.Name = "label2";
            label2.Size = new Size(109, 25);
            label2.TabIndex = 8;
            label2.Text = "Middle View";
            // 
            // CAM1_ComboBox_ZoomSelect
            // 
            CAM1_ComboBox_ZoomSelect.FormattingEnabled = true;
            CAM1_ComboBox_ZoomSelect.ImeMode = ImeMode.Off;
            CAM1_ComboBox_ZoomSelect.Items.AddRange(new object[] { "Level 0", "Level 1", "Level 2", "Level 3", "Level 4" });
            CAM1_ComboBox_ZoomSelect.Location = new Point(7, 35);
            CAM1_ComboBox_ZoomSelect.Margin = new Padding(4, 5, 4, 5);
            CAM1_ComboBox_ZoomSelect.Name = "CAM1_ComboBox_ZoomSelect";
            CAM1_ComboBox_ZoomSelect.Size = new Size(148, 33);
            CAM1_ComboBox_ZoomSelect.TabIndex = 0;
            CAM1_ComboBox_ZoomSelect.Text = "Level 0";
            CAM1_ComboBox_ZoomSelect.SelectedIndexChanged += CAM1_ComboBox_ZoomSelect_SelectedIndexChanged;
            // 
            // groupBox4
            // 
            groupBox4.Controls.Add(label3);
            groupBox4.Controls.Add(CAM2_ComboBox_ZoomSelect);
            groupBox4.Location = new Point(259, 17);
            groupBox4.Name = "groupBox4";
            groupBox4.Size = new Size(164, 102);
            groupBox4.TabIndex = 5;
            groupBox4.TabStop = false;
            groupBox4.Text = "CAM2 Zoom";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(7, 73);
            label3.Name = "label3";
            label3.Size = new Size(135, 25);
            label3.TabIndex = 9;
            label3.Text = "Top Down View";
            // 
            // CAM2_ComboBox_ZoomSelect
            // 
            CAM2_ComboBox_ZoomSelect.FormattingEnabled = true;
            CAM2_ComboBox_ZoomSelect.Items.AddRange(new object[] { "Level 0", "Level 1", "Level 2", "Level 3", "Level 4" });
            CAM2_ComboBox_ZoomSelect.Location = new Point(7, 35);
            CAM2_ComboBox_ZoomSelect.Margin = new Padding(4, 5, 4, 5);
            CAM2_ComboBox_ZoomSelect.Name = "CAM2_ComboBox_ZoomSelect";
            CAM2_ComboBox_ZoomSelect.Size = new Size(148, 33);
            CAM2_ComboBox_ZoomSelect.TabIndex = 1;
            CAM2_ComboBox_ZoomSelect.Text = "Level 0";
            CAM2_ComboBox_ZoomSelect.SelectedIndexChanged += CAM2_ComboBox_ZoomSelect_SelectedIndexChanged;
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(label4);
            groupBox2.Controls.Add(CAM3_ComboBox_ZoomSelect);
            groupBox2.Location = new Point(510, 17);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new Size(164, 102);
            groupBox2.TabIndex = 6;
            groupBox2.TabStop = false;
            groupBox2.Text = "CAM3 Zoom";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(7, 73);
            label4.Name = "label4";
            label4.Size = new Size(142, 25);
            label4.TabIndex = 10;
            label4.Text = "Bottom Up View";
            // 
            // CAM3_ComboBox_ZoomSelect
            // 
            CAM3_ComboBox_ZoomSelect.FormattingEnabled = true;
            CAM3_ComboBox_ZoomSelect.Items.AddRange(new object[] { "Level 0", "Level 1", "Level 2", "Level 3", "Level 4" });
            CAM3_ComboBox_ZoomSelect.Location = new Point(7, 35);
            CAM3_ComboBox_ZoomSelect.Margin = new Padding(4, 5, 4, 5);
            CAM3_ComboBox_ZoomSelect.Name = "CAM3_ComboBox_ZoomSelect";
            CAM3_ComboBox_ZoomSelect.Size = new Size(148, 33);
            CAM3_ComboBox_ZoomSelect.TabIndex = 2;
            CAM3_ComboBox_ZoomSelect.Text = "Level 0";
            CAM3_ComboBox_ZoomSelect.SelectedIndexChanged += CAM3_ComboBox_ZoomSelect_SelectedIndexChanged;
            // 
            // tabPage3
            // 
            tabPage3.Controls.Add(groupBox5);
            tabPage3.Controls.Add(BT_Start_Mesh);
            tabPage3.Controls.Add(groupBox3);
            tabPage3.Controls.Add(BT_Start_Capture);
            tabPage3.Location = new Point(4, 34);
            tabPage3.Name = "tabPage3";
            tabPage3.Padding = new Padding(3);
            tabPage3.Size = new Size(681, 316);
            tabPage3.TabIndex = 2;
            tabPage3.Text = "Start Capture";
            tabPage3.UseVisualStyleBackColor = true;
            tabPage3.Enter += tabPage3_Enter;
            // 
            // groupBox5
            // 
            groupBox5.Controls.Add(RB_BOTTOM);
            groupBox5.Controls.Add(RB_MIDDLE);
            groupBox5.Controls.Add(RB_TOP);
            groupBox5.Controls.Add(RB_TOPMIDDLE);
            groupBox5.Controls.Add(RB_MIDDLEBOTTOM);
            groupBox5.Controls.Add(RB_TOPBOTTOM);
            groupBox5.Controls.Add(RB_ALL);
            groupBox5.Location = new Point(351, 7);
            groupBox5.Name = "groupBox5";
            groupBox5.Size = new Size(324, 177);
            groupBox5.TabIndex = 10;
            groupBox5.TabStop = false;
            groupBox5.Text = "Mesh Settings";
            // 
            // RB_TOPMIDDLE
            // 
            RB_TOPMIDDLE.AutoSize = true;
            RB_TOPMIDDLE.Location = new Point(6, 135);
            RB_TOPMIDDLE.Name = "RB_TOPMIDDLE";
            RB_TOPMIDDLE.Size = new Size(126, 29);
            RB_TOPMIDDLE.TabIndex = 15;
            RB_TOPMIDDLE.TabStop = true;
            RB_TOPMIDDLE.Text = "Top Middle";
            RB_TOPMIDDLE.UseVisualStyleBackColor = true;
            // 
            // RB_MIDDLEBOTTOM
            // 
            RB_MIDDLEBOTTOM.AutoSize = true;
            RB_MIDDLEBOTTOM.Location = new Point(6, 100);
            RB_MIDDLEBOTTOM.Name = "RB_MIDDLEBOTTOM";
            RB_MIDDLEBOTTOM.Size = new Size(157, 29);
            RB_MIDDLEBOTTOM.TabIndex = 14;
            RB_MIDDLEBOTTOM.TabStop = true;
            RB_MIDDLEBOTTOM.Text = "Middle Bottom";
            RB_MIDDLEBOTTOM.UseVisualStyleBackColor = true;
            // 
            // RB_TOPBOTTOM
            // 
            RB_TOPBOTTOM.AutoSize = true;
            RB_TOPBOTTOM.Location = new Point(6, 65);
            RB_TOPBOTTOM.Name = "RB_TOPBOTTOM";
            RB_TOPBOTTOM.Size = new Size(131, 29);
            RB_TOPBOTTOM.TabIndex = 13;
            RB_TOPBOTTOM.TabStop = true;
            RB_TOPBOTTOM.Text = "Top Bottom";
            RB_TOPBOTTOM.UseVisualStyleBackColor = true;
            // 
            // RB_ALL
            // 
            RB_ALL.AutoSize = true;
            RB_ALL.Location = new Point(6, 30);
            RB_ALL.Name = "RB_ALL";
            RB_ALL.Size = new Size(57, 29);
            RB_ALL.TabIndex = 12;
            RB_ALL.TabStop = true;
            RB_ALL.Text = "All";
            RB_ALL.UseVisualStyleBackColor = true;
            // 
            // BT_Start_Mesh
            // 
            BT_Start_Mesh.Location = new Point(176, 258);
            BT_Start_Mesh.Name = "BT_Start_Mesh";
            BT_Start_Mesh.Size = new Size(164, 33);
            BT_Start_Mesh.TabIndex = 10;
            BT_Start_Mesh.Text = "Start Mesh";
            BT_Start_Mesh.UseVisualStyleBackColor = true;
            BT_Start_Mesh.Click += BT_Start_Mesh_Click;
            // 
            // groupBox3
            // 
            groupBox3.Controls.Add(TB_Obj_Name);
            groupBox3.Controls.Add(label1);
            groupBox3.Controls.Add(CAM3_Zoom_Curr);
            groupBox3.Controls.Add(CAM2_Zoom_Curr);
            groupBox3.Controls.Add(CAM1_Zoom_Curr);
            groupBox3.Location = new Point(6, 7);
            groupBox3.Name = "groupBox3";
            groupBox3.Size = new Size(334, 177);
            groupBox3.TabIndex = 9;
            groupBox3.TabStop = false;
            groupBox3.Text = "Current Settings";
            // 
            // TB_Obj_Name
            // 
            TB_Obj_Name.Location = new Point(121, 107);
            TB_Obj_Name.Name = "TB_Obj_Name";
            TB_Obj_Name.Size = new Size(194, 31);
            TB_Obj_Name.TabIndex = 4;
            TB_Obj_Name.TextChanged += TB_Obj_Name_TextChanged_1;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(6, 107);
            label1.Name = "label1";
            label1.Size = new Size(120, 25);
            label1.TabIndex = 3;
            label1.Text = "Object Name:";
            // 
            // CAM3_Zoom_Curr
            // 
            CAM3_Zoom_Curr.AutoSize = true;
            CAM3_Zoom_Curr.Location = new Point(6, 77);
            CAM3_Zoom_Curr.Name = "CAM3_Zoom_Curr";
            CAM3_Zoom_Curr.Size = new Size(123, 25);
            CAM3_Zoom_Curr.TabIndex = 2;
            CAM3_Zoom_Curr.Text = "CAM3 Zoom: ";
            // 
            // CAM2_Zoom_Curr
            // 
            CAM2_Zoom_Curr.AutoSize = true;
            CAM2_Zoom_Curr.Location = new Point(6, 52);
            CAM2_Zoom_Curr.Name = "CAM2_Zoom_Curr";
            CAM2_Zoom_Curr.Size = new Size(123, 25);
            CAM2_Zoom_Curr.TabIndex = 1;
            CAM2_Zoom_Curr.Text = "CAM2 Zoom: ";
            // 
            // CAM1_Zoom_Curr
            // 
            CAM1_Zoom_Curr.AutoSize = true;
            CAM1_Zoom_Curr.Location = new Point(6, 27);
            CAM1_Zoom_Curr.Name = "CAM1_Zoom_Curr";
            CAM1_Zoom_Curr.Size = new Size(123, 25);
            CAM1_Zoom_Curr.TabIndex = 0;
            CAM1_Zoom_Curr.Text = "CAM1 Zoom: ";
            CAM1_Zoom_Curr.Click += CAM1_Zoom_Curr_Click;
            // 
            // BT_Start_Capture
            // 
            BT_Start_Capture.Location = new Point(6, 257);
            BT_Start_Capture.Name = "BT_Start_Capture";
            BT_Start_Capture.Size = new Size(164, 33);
            BT_Start_Capture.TabIndex = 8;
            BT_Start_Capture.Text = "Start Capture";
            BT_Start_Capture.UseVisualStyleBackColor = true;
            BT_Start_Capture.Click += BT_Start_Capture_Click;
            // 
            // webViewCAM1
            // 
            webViewCAM1.AllowExternalDrop = true;
            webViewCAM1.CreationProperties = null;
            webViewCAM1.DefaultBackgroundColor = Color.White;
            webViewCAM1.Dock = DockStyle.Fill;
            webViewCAM1.Location = new Point(3, 3);
            webViewCAM1.Name = "webViewCAM1";
            webViewCAM1.Size = new Size(689, 354);
            webViewCAM1.TabIndex = 1;
            webViewCAM1.ZoomFactor = 1D;
            // 
            // RB_TOP
            // 
            RB_TOP.AutoSize = true;
            RB_TOP.Location = new Point(187, 30);
            RB_TOP.Name = "RB_TOP";
            RB_TOP.Size = new Size(66, 29);
            RB_TOP.TabIndex = 16;
            RB_TOP.TabStop = true;
            RB_TOP.Text = "Top";
            RB_TOP.UseVisualStyleBackColor = true;
            // 
            // RB_BOTTOM
            // 
            RB_BOTTOM.AutoSize = true;
            RB_BOTTOM.Location = new Point(187, 100);
            RB_BOTTOM.Name = "RB_BOTTOM";
            RB_BOTTOM.Size = new Size(97, 29);
            RB_BOTTOM.TabIndex = 18;
            RB_BOTTOM.TabStop = true;
            RB_BOTTOM.Text = "Bottom";
            RB_BOTTOM.UseVisualStyleBackColor = true;
            // 
            // RB_MIDDLE
            // 
            RB_MIDDLE.AutoSize = true;
            RB_MIDDLE.Location = new Point(187, 65);
            RB_MIDDLE.Name = "RB_MIDDLE";
            RB_MIDDLE.Size = new Size(92, 29);
            RB_MIDDLE.TabIndex = 17;
            RB_MIDDLE.TabStop = true;
            RB_MIDDLE.Text = "Middle";
            RB_MIDDLE.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1390, 720);
            Controls.Add(tableLayoutPanel1);
            Margin = new Padding(4, 5, 4, 5);
            Name = "Form1";
            Text = "Form1";
            WindowState = FormWindowState.Maximized;
            FormClosing += Form1_FormClosing;
            tableLayoutPanel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)webViewCAM3).EndInit();
            ((System.ComponentModel.ISupportInitialize)webViewCAM2).EndInit();
            panel1.ResumeLayout(false);
            tabControl_MQTT.ResumeLayout(false);
            tabPage2.ResumeLayout(false);
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            groupBox4.ResumeLayout(false);
            groupBox4.PerformLayout();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            tabPage3.ResumeLayout(false);
            groupBox5.ResumeLayout(false);
            groupBox5.PerformLayout();
            groupBox3.ResumeLayout(false);
            groupBox3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)webViewCAM1).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private TableLayoutPanel tableLayoutPanel1;
        private Panel panel1;
        private Microsoft.Web.WebView2.WinForms.WebView2 webViewCAM3;
        private Microsoft.Web.WebView2.WinForms.WebView2 webViewCAM2;
        private Microsoft.Web.WebView2.WinForms.WebView2 webViewCAM1;
        private TabControl tabControl_MQTT;
        private TabPage tabPage2;
        private Button BT_Start_Stop_Preview;
        private GroupBox groupBox1;
        private GroupBox groupBox4;
        private GroupBox groupBox2;
        private TabPage tabPage3;
        private Button BT_Start_Capture;
        private ComboBox CAM1_ComboBox_ZoomSelect;
        private ComboBox CAM2_ComboBox_ZoomSelect;
        private ComboBox CAM3_ComboBox_ZoomSelect;
        private GroupBox groupBox3;
        private Label CAM3_Zoom_Curr;
        private Label CAM2_Zoom_Curr;
        private Label CAM1_Zoom_Curr;
        private TextBox TB_Obj_Name;
        private Label label1;
        private Button BT_Start_Mesh;
        private Label label2;
        private Label label3;
        private Label label4;
        private GroupBox groupBox5;
        private RadioButton RB_TOPMIDDLE;
        private RadioButton RB_MIDDLEBOTTOM;
        private RadioButton RB_TOPBOTTOM;
        private RadioButton RB_ALL;
        private RadioButton RB_BOTTOM;
        private RadioButton RB_MIDDLE;
        private RadioButton RB_TOP;
    }
}
