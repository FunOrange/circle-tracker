using OsuMemoryDataProvider;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Circle_Tracker
{
    public partial class MainForm : Form
    {
        private readonly Tracker tracker;
        public MainForm()
        {
            InitializeComponent();
            this.Icon = Properties.Resources.iconbars;

            try
            {
                Updater.CheckForUpdates();
            }
            catch (Exception e)
            {
                // exception we probably don't care about
            }

            tracker = new Tracker(this);

            songsFolderTextBox.Text = tracker.SongsFolder;
            sheetNameTextBox.Text = tracker.SheetName;
            spreadsheetIdTextBox.Text = tracker.SpreadsheetId;
            soundEnabledCheckbox.Checked = tracker.SubmitSoundEnabled;

            tracker.InitGoogleAPI(silent:true);
            SetCredentialsFound(File.Exists("credentials.json"));
        }

        public void SetCredentialsFound(bool found)
        {
            credentialsLabel.Text      = found ? "Found" : "Missing";
            credentialsLabel.ForeColor = found ? Color.Green : Color.Red;
        }

        private void OnLoad(object sender, EventArgs e)
        {
            tracker.StartUpdateThread();
        }

        public void UpdateControls()
        {
            BackColor           = (tracker.SheetsApiReady && tracker.GameState == OsuMemoryStatus.Playing) ? SystemColors.Info : SystemColors.Control;
            hitsTextBox.Text    = tracker.TotalBeatmapHits.ToString();
            timeTextBox.Text    = tracker.Time.ToString();
            beatmapTextBox.Text = tracker.BeatmapString;
            starsTextBox.Text   = tracker.BeatmapStars.ToString("0.00");
            aimTextBox.Text     = tracker.BeatmapAim.ToString("0.00");
            speedTextBox.Text   = tracker.BeatmapSpeed.ToString("0.00");
            modsTextBox.Text    = tracker.GetModsString();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            tracker.OnClosing();
        }

        private void songsFolderTextBox_TextChanged(object sender, EventArgs e)
        {
            tracker.SetSongsFolder(songsFolderTextBox.Text);
        }

        public void SetSheetsApiReady(bool val)
        {
            statusLabel.Text = val ? "Connected" : "Not connected";
            statusLabel.ForeColor = val ? System.Drawing.Color.Green : System.Drawing.Color.Red;
        }

        private void ConnectApiButton_Click(object sender, EventArgs e)
        {
            tracker.InitGoogleAPI();
        }

        private void spreadsheetIdTextBox_TextChanged(object sender, EventArgs e)
        {
            tracker.SpreadsheetId = spreadsheetIdTextBox.Text;
        }

        private void sheetNameTextBox_TextChanged(object sender, EventArgs e)
        {
            tracker.SheetName = sheetNameTextBox.Text;
        }

        private void soundEnabledCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            tracker.SubmitSoundEnabled = soundEnabledCheckbox.Checked;
        }
    }
}
