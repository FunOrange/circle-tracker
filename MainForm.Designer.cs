namespace circle_tracker
{
    partial class MainForm
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
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 175);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(23, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "hits";
            // 
            // hitsTextBox
            // 
            this.hitsTextBox.Location = new System.Drawing.Point(52, 172);
            this.hitsTextBox.Name = "hitsTextBox";
            this.hitsTextBox.ReadOnly = true;
            this.hitsTextBox.Size = new System.Drawing.Size(100, 20);
            this.hitsTextBox.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 149);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(26, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "time";
            // 
            // timeTextBox
            // 
            this.timeTextBox.Location = new System.Drawing.Point(52, 146);
            this.timeTextBox.Name = "timeTextBox";
            this.timeTextBox.ReadOnly = true;
            this.timeTextBox.Size = new System.Drawing.Size(100, 20);
            this.timeTextBox.TabIndex = 1;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 12);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(64, 13);
            this.label3.TabIndex = 0;
            this.label3.Text = "songs folder";
            // 
            // songsFolderTextBox
            // 
            this.songsFolderTextBox.Location = new System.Drawing.Point(82, 9);
            this.songsFolderTextBox.Name = "songsFolderTextBox";
            this.songsFolderTextBox.Size = new System.Drawing.Size(508, 20);
            this.songsFolderTextBox.TabIndex = 1;
            this.songsFolderTextBox.TextChanged += new System.EventHandler(this.songsFolderTextBox_TextChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 36);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(48, 13);
            this.label4.TabIndex = 0;
            this.label4.Text = "beatmap";
            // 
            // beatmapTextBox
            // 
            this.beatmapTextBox.Location = new System.Drawing.Point(82, 33);
            this.beatmapTextBox.Name = "beatmapTextBox";
            this.beatmapTextBox.ReadOnly = true;
            this.beatmapTextBox.Size = new System.Drawing.Size(508, 20);
            this.beatmapTextBox.TabIndex = 1;
            this.beatmapTextBox.TextChanged += new System.EventHandler(this.songsFolderTextBox_TextChanged);
            // 
            // aimLabel
            // 
            this.aimLabel.AutoSize = true;
            this.aimLabel.Location = new System.Drawing.Point(13, 88);
            this.aimLabel.Name = "aimLabel";
            this.aimLabel.Size = new System.Drawing.Size(23, 13);
            this.aimLabel.TabIndex = 0;
            this.aimLabel.Text = "aim";
            // 
            // starsLabel
            // 
            this.starsLabel.AutoSize = true;
            this.starsLabel.Location = new System.Drawing.Point(13, 62);
            this.starsLabel.Name = "starsLabel";
            this.starsLabel.Size = new System.Drawing.Size(29, 13);
            this.starsLabel.TabIndex = 0;
            this.starsLabel.Text = "stars";
            // 
            // aimTextBox
            // 
            this.aimTextBox.Location = new System.Drawing.Point(52, 85);
            this.aimTextBox.Name = "aimTextBox";
            this.aimTextBox.ReadOnly = true;
            this.aimTextBox.Size = new System.Drawing.Size(52, 20);
            this.aimTextBox.TabIndex = 1;
            // 
            // starsTextBox
            // 
            this.starsTextBox.Location = new System.Drawing.Point(52, 59);
            this.starsTextBox.Name = "starsTextBox";
            this.starsTextBox.ReadOnly = true;
            this.starsTextBox.Size = new System.Drawing.Size(52, 20);
            this.starsTextBox.TabIndex = 1;
            // 
            // speedLabel
            // 
            this.speedLabel.AutoSize = true;
            this.speedLabel.Location = new System.Drawing.Point(13, 113);
            this.speedLabel.Name = "speedLabel";
            this.speedLabel.Size = new System.Drawing.Size(36, 13);
            this.speedLabel.TabIndex = 0;
            this.speedLabel.Text = "speed";
            // 
            // speedTextBox
            // 
            this.speedTextBox.Location = new System.Drawing.Point(52, 110);
            this.speedTextBox.Name = "speedTextBox";
            this.speedTextBox.ReadOnly = true;
            this.speedTextBox.Size = new System.Drawing.Size(52, 20);
            this.speedTextBox.TabIndex = 1;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(646, 289);
            this.Controls.Add(this.beatmapTextBox);
            this.Controls.Add(this.songsFolderTextBox);
            this.Controls.Add(this.starsTextBox);
            this.Controls.Add(this.speedTextBox);
            this.Controls.Add(this.aimTextBox);
            this.Controls.Add(this.timeTextBox);
            this.Controls.Add(this.hitsTextBox);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.starsLabel);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.speedLabel);
            this.Controls.Add(this.aimLabel);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Name = "MainForm";
            this.Text = "Form1";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

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
    }
}

