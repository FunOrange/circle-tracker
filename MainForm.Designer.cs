namespace Circle_Tracker
{
    partial class MainForm
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.label1 = new System.Windows.Forms.Label();
            this.hitsTextBox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.timeTextBox = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.songsFolderTextBox = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.beatmapTextBox = new System.Windows.Forms.TextBox();
            this.aimLabel = new System.Windows.Forms.Label();
            this.starsLabel = new System.Windows.Forms.Label();
            this.aimTextBox = new System.Windows.Forms.TextBox();
            this.starsTextBox = new System.Windows.Forms.TextBox();
            this.speedLabel = new System.Windows.Forms.Label();
            this.speedTextBox = new System.Windows.Forms.TextBox();
            this.ConnectApiButton = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.credentialsLabel = new System.Windows.Forms.Label();
            this.soundEnabledCheckbox = new System.Windows.Forms.CheckBox();
            this.label8 = new System.Windows.Forms.Label();
            this.statusLabel = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.sheetNameTextBox = new System.Windows.Forms.TextBox();
            this.spreadsheetIdTextBox = new System.Windows.Forms.TextBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.modsTextBox = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.panel1.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(15, 160);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(23, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "hits";
            // 
            // hitsTextBox
            // 
            this.hitsTextBox.Location = new System.Drawing.Point(54, 157);
            this.hitsTextBox.Name = "hitsTextBox";
            this.hitsTextBox.ReadOnly = true;
            this.hitsTextBox.Size = new System.Drawing.Size(100, 20);
            this.hitsTextBox.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(15, 134);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(26, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "time";
            // 
            // timeTextBox
            // 
            this.timeTextBox.Location = new System.Drawing.Point(54, 131);
            this.timeTextBox.Name = "timeTextBox";
            this.timeTextBox.ReadOnly = true;
            this.timeTextBox.Size = new System.Drawing.Size(100, 20);
            this.timeTextBox.TabIndex = 1;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 9);
            this.label3.Margin = new System.Windows.Forms.Padding(3, 6, 3, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(64, 13);
            this.label3.TabIndex = 0;
            this.label3.Text = "songs folder";
            // 
            // songsFolderTextBox
            // 
            this.songsFolderTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.songsFolderTextBox.Location = new System.Drawing.Point(76, 6);
            this.songsFolderTextBox.Name = "songsFolderTextBox";
            this.songsFolderTextBox.Size = new System.Drawing.Size(419, 20);
            this.songsFolderTextBox.TabIndex = 1;
            this.songsFolderTextBox.TextChanged += new System.EventHandler(this.songsFolderTextBox_TextChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 34);
            this.label4.Margin = new System.Windows.Forms.Padding(3, 6, 3, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(48, 13);
            this.label4.TabIndex = 0;
            this.label4.Text = "beatmap";
            // 
            // beatmapTextBox
            // 
            this.beatmapTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.beatmapTextBox.Location = new System.Drawing.Point(76, 31);
            this.beatmapTextBox.Name = "beatmapTextBox";
            this.beatmapTextBox.ReadOnly = true;
            this.beatmapTextBox.Size = new System.Drawing.Size(419, 20);
            this.beatmapTextBox.TabIndex = 1;
            this.beatmapTextBox.TextChanged += new System.EventHandler(this.songsFolderTextBox_TextChanged);
            // 
            // aimLabel
            // 
            this.aimLabel.AutoSize = true;
            this.aimLabel.Location = new System.Drawing.Point(15, 73);
            this.aimLabel.Name = "aimLabel";
            this.aimLabel.Size = new System.Drawing.Size(23, 13);
            this.aimLabel.TabIndex = 0;
            this.aimLabel.Text = "aim";
            // 
            // starsLabel
            // 
            this.starsLabel.AutoSize = true;
            this.starsLabel.Location = new System.Drawing.Point(15, 47);
            this.starsLabel.Name = "starsLabel";
            this.starsLabel.Size = new System.Drawing.Size(29, 13);
            this.starsLabel.TabIndex = 0;
            this.starsLabel.Text = "stars";
            // 
            // aimTextBox
            // 
            this.aimTextBox.Location = new System.Drawing.Point(54, 70);
            this.aimTextBox.Name = "aimTextBox";
            this.aimTextBox.ReadOnly = true;
            this.aimTextBox.Size = new System.Drawing.Size(52, 20);
            this.aimTextBox.TabIndex = 1;
            // 
            // starsTextBox
            // 
            this.starsTextBox.Location = new System.Drawing.Point(54, 44);
            this.starsTextBox.Name = "starsTextBox";
            this.starsTextBox.ReadOnly = true;
            this.starsTextBox.Size = new System.Drawing.Size(52, 20);
            this.starsTextBox.TabIndex = 1;
            // 
            // speedLabel
            // 
            this.speedLabel.AutoSize = true;
            this.speedLabel.Location = new System.Drawing.Point(15, 98);
            this.speedLabel.Name = "speedLabel";
            this.speedLabel.Size = new System.Drawing.Size(36, 13);
            this.speedLabel.TabIndex = 0;
            this.speedLabel.Text = "speed";
            // 
            // speedTextBox
            // 
            this.speedTextBox.Location = new System.Drawing.Point(54, 95);
            this.speedTextBox.Name = "speedTextBox";
            this.speedTextBox.ReadOnly = true;
            this.speedTextBox.Size = new System.Drawing.Size(52, 20);
            this.speedTextBox.TabIndex = 1;
            // 
            // ConnectApiButton
            // 
            this.ConnectApiButton.Location = new System.Drawing.Point(6, 165);
            this.ConnectApiButton.Name = "ConnectApiButton";
            this.ConnectApiButton.Size = new System.Drawing.Size(284, 46);
            this.ConnectApiButton.TabIndex = 2;
            this.ConnectApiButton.Text = "Connect Google Sheets API";
            this.ConnectApiButton.UseVisualStyleBackColor = true;
            this.ConnectApiButton.Click += new System.EventHandler(this.ConnectApiButton_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.credentialsLabel);
            this.groupBox1.Controls.Add(this.soundEnabledCheckbox);
            this.groupBox1.Controls.Add(this.label8);
            this.groupBox1.Controls.Add(this.statusLabel);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.label7);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.sheetNameTextBox);
            this.groupBox1.Controls.Add(this.spreadsheetIdTextBox);
            this.groupBox1.Controls.Add(this.ConnectApiButton);
            this.groupBox1.Location = new System.Drawing.Point(9, 62);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(297, 216);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Google Sheets Integration";
            // 
            // credentialsLabel
            // 
            this.credentialsLabel.AutoSize = true;
            this.credentialsLabel.ForeColor = System.Drawing.Color.Red;
            this.credentialsLabel.Location = new System.Drawing.Point(245, 25);
            this.credentialsLabel.Name = "credentialsLabel";
            this.credentialsLabel.Size = new System.Drawing.Size(42, 13);
            this.credentialsLabel.TabIndex = 6;
            this.credentialsLabel.Text = "Missing";
            // 
            // soundEnabledCheckbox
            // 
            this.soundEnabledCheckbox.AutoSize = true;
            this.soundEnabledCheckbox.Location = new System.Drawing.Point(6, 138);
            this.soundEnabledCheckbox.Name = "soundEnabledCheckbox";
            this.soundEnabledCheckbox.Size = new System.Drawing.Size(198, 17);
            this.soundEnabledCheckbox.TabIndex = 6;
            this.soundEnabledCheckbox.Text = "Play sound when writing a new entry";
            this.soundEnabledCheckbox.UseVisualStyleBackColor = true;
            this.soundEnabledCheckbox.CheckedChanged += new System.EventHandler(this.soundEnabledCheckbox_CheckedChanged);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(162, 25);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(83, 13);
            this.label8.TabIndex = 5;
            this.label8.Text = "credentials.json:";
            // 
            // statusLabel
            // 
            this.statusLabel.AutoSize = true;
            this.statusLabel.ForeColor = System.Drawing.Color.Red;
            this.statusLabel.Location = new System.Drawing.Point(45, 25);
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(78, 13);
            this.statusLabel.TabIndex = 6;
            this.statusLabel.Text = "Not connected";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(6, 25);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(43, 13);
            this.label6.TabIndex = 5;
            this.label6.Text = "Status: ";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(6, 90);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(66, 13);
            this.label7.TabIndex = 4;
            this.label7.Text = "Sheet Name";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 47);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(81, 13);
            this.label5.TabIndex = 4;
            this.label5.Text = "Spreadsheet ID";
            // 
            // sheetNameTextBox
            // 
            this.sheetNameTextBox.Location = new System.Drawing.Point(6, 107);
            this.sheetNameTextBox.Name = "sheetNameTextBox";
            this.sheetNameTextBox.Size = new System.Drawing.Size(284, 20);
            this.sheetNameTextBox.TabIndex = 3;
            this.sheetNameTextBox.TextChanged += new System.EventHandler(this.sheetNameTextBox_TextChanged);
            // 
            // spreadsheetIdTextBox
            // 
            this.spreadsheetIdTextBox.Location = new System.Drawing.Point(6, 64);
            this.spreadsheetIdTextBox.Name = "spreadsheetIdTextBox";
            this.spreadsheetIdTextBox.Size = new System.Drawing.Size(284, 20);
            this.spreadsheetIdTextBox.TabIndex = 3;
            this.spreadsheetIdTextBox.TextChanged += new System.EventHandler(this.spreadsheetIdTextBox_TextChanged);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.modsTextBox);
            this.groupBox2.Controls.Add(this.starsTextBox);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.aimLabel);
            this.groupBox2.Controls.Add(this.speedLabel);
            this.groupBox2.Controls.Add(this.speedTextBox);
            this.groupBox2.Controls.Add(this.label9);
            this.groupBox2.Controls.Add(this.starsLabel);
            this.groupBox2.Controls.Add(this.aimTextBox);
            this.groupBox2.Controls.Add(this.hitsTextBox);
            this.groupBox2.Controls.Add(this.timeTextBox);
            this.groupBox2.Location = new System.Drawing.Point(321, 62);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(172, 186);
            this.groupBox2.TabIndex = 4;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Beatmap Info";
            // 
            // modsTextBox
            // 
            this.modsTextBox.Location = new System.Drawing.Point(54, 18);
            this.modsTextBox.Name = "modsTextBox";
            this.modsTextBox.ReadOnly = true;
            this.modsTextBox.Size = new System.Drawing.Size(86, 20);
            this.modsTextBox.TabIndex = 1;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(15, 21);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(32, 13);
            this.label9.TabIndex = 0;
            this.label9.Text = "mods";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.tableLayoutPanel1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(501, 56);
            this.panel1.TabIndex = 5;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 70F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.songsFolderTextBox, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.label3, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.beatmapTextBox, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.label4, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.Padding = new System.Windows.Forms.Padding(3);
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(501, 56);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(501, 289);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainForm";
            this.Text = "Circle Tracker";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Load += new System.EventHandler(this.OnLoad);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox hitsTextBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox timeTextBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox songsFolderTextBox;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox beatmapTextBox;
        private System.Windows.Forms.Label aimLabel;
        private System.Windows.Forms.Label starsLabel;
        private System.Windows.Forms.TextBox aimTextBox;
        private System.Windows.Forms.TextBox starsTextBox;
        private System.Windows.Forms.Label speedLabel;
        private System.Windows.Forms.TextBox speedTextBox;
        private System.Windows.Forms.Button ConnectApiButton;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label statusLabel;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox spreadsheetIdTextBox;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox sheetNameTextBox;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label credentialsLabel;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox modsTextBox;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.CheckBox soundEnabledCheckbox;
    }
}

