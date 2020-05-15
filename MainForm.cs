using OsuMemoryDataProvider;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace circle_tracker
{
    public partial class MainForm : Form
    {
        private readonly Tracker tracker;
        public MainForm()
        {
            InitializeComponent();
            tracker = new Tracker(this);
            songsFolderTextBox.Text = tracker.SongsFolder;
        }

        public void UpdateControls()
        {
            BackColor = tracker.GameState == OsuMemoryStatus.Playing ? SystemColors.Info : SystemColors.Control;
            hitsTextBox.Text = tracker.Hits.ToString();
            timeTextBox.Text = tracker.Time.ToString();
            beatmapTextBox.Text = tracker.BeatmapPath;
            starsTextBox.Text = tracker.BeatmapStars.ToString("0.00");
            aimTextBox.Text = tracker.BeatmapAim.ToString("0.00");
            speedTextBox.Text = tracker.BeatmapSpeed.ToString("0.00");
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            tracker.OnClosing();
        }

        public void SetBgColor(Color col)
        {
            BackColor = col;
        }

        private void songsFolderTextBox_TextChanged(object sender, EventArgs e)
        {
            tracker.SetSongsFolder(songsFolderTextBox.Text);
        }
    }
}
