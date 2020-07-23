using OsuMemoryDataProvider;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using IWshRuntimeLibrary;
using System.Threading.Tasks;

namespace Circle_Tracker
{
    public partial class MainForm : Form
    {
        private readonly Tracker tracker;
        private bool MinimizeToTrayEnabled = true;
        private string ShortcutAddress;
        public MainForm()
        {
            Directory.SetCurrentDirectory(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location));
            InitializeComponent();
            //this.Icon = Properties.Resources.iconbars;
            ShortcutAddress = Environment.GetEnvironmentVariable("appdata") + @"\Microsoft\Windows\Start Menu\Programs\Startup" + @"\circle-tracker.lnk";
            startupCheckBox.Checked = ShortcutExists();

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
            SetCredentialsFound(System.IO.File.Exists("credentials.json"));
            updateGameVariablesTimer.Start();
            updateFormTimer.Start();
        }

        public void SetCredentialsFound(bool found)
        {
            credentialsLabel.Text      = found ? "Found" : "Missing";
            credentialsLabel.ForeColor = found ? Color.Green : Color.Red;
        }

        public void UpdateControls()
        {
            groupBox2.BackColor = (tracker.SheetsApiReady && tracker.GameState == OsuMemoryStatus.Playing) ? Color.FromArgb(214, 241, 216) : SystemColors.Control;
            hitsTextBox.Text    = tracker.TotalBeatmapHits.ToString() + $" ({tracker.Play300c}, {tracker.Play100c}, {tracker.Play50c}, {tracker.PlayMissc})";
            timeTextBox.Text    = tracker.Time.ToString();
            beatmapTextBox.Text = tracker.BeatmapString;
            starsTextBox.Text   = tracker.BeatmapStars.ToString("0.00");
            aimTextBox.Text     = tracker.BeatmapAim.ToString("0.00");
            speedTextBox.Text   = tracker.BeatmapSpeed.ToString("0.00");
            modsTextBox.Text    = tracker.GetModsString();
            textBoxCS.Text      = tracker.BeatmapCs.ToString("0.0");
            textBoxAR.Text      = tracker.BeatmapAr.ToString("0.0");
            textBoxOD.Text      = tracker.BeatmapOd.ToString("0.0");
            accTextBox.Text     = tracker.Accuracy.ToString("0.00") + "%";
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

        private async void updateGameVariablesTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                await Task.Run(() => tracker.Tick());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Exception occurred: {Environment.NewLine}{Environment.NewLine}" +
                    $"{ex.Message}{Environment.NewLine}{Environment.NewLine}" +
                    $"Please PLEASE tell FunOrange about this."
                    , "Error");
            }
        }

        private void startupCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (startupCheckBox.Checked)
            {
                CreateShortcut();
            }
            else
            {
                DeleteShortcut();
            }
        }

        private void notifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
             Show();
             this.WindowState = FormWindowState.Normal;
             notifyIcon.Visible = false;
        }

        private void MinimizeToTray(object sender, EventArgs e)
        {
            if (MinimizeToTrayEnabled)
            {
                Hide();
                notifyIcon.Visible = true;
            }
        }
        private void CreateShortcut()
        {
            object shDesktop = (object)"Desktop";
            WshShell shell = new WshShell();
            IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(ShortcutAddress);
            shortcut.Description = "Circle Tracker (startup shortcut)";
            shortcut.Hotkey = "";
            shortcut.TargetPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            shortcut.Save();
        }
        private void DeleteShortcut()
        {
            System.IO.File.Delete(ShortcutAddress);
        }
        private bool ShortcutExists()
        {
            return System.IO.File.Exists(ShortcutAddress);
        }

        private void updateFormTimer_Tick(object sender, EventArgs e)
        {
            UpdateControls();
        }

        private void altSepCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            tracker.UseAltFuncSeparator = altSepCheckBox.Checked;
        }
    }
}
