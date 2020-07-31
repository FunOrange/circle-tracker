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
using System.Runtime.InteropServices;

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
            bool shortcutExists = ShortcutExists();
            startupCheckBox.Checked = shortcutExists;
            if (shortcutExists)
            {
                // overwrite the existing shortcut just in it's an older version
                TryDeleteShortcut();
                CreateShortcut();
            }

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
            altSepCheckBox.Checked = tracker.UseAltFuncSeparator;

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
            bpmTextBox.Text     = tracker.BeatmapBpm.ToString();

            if (tracker.BeatmapString == null || tracker.BeatmapString == "")
                beatmapTextBox.BackColor = Color.Pink;
            else
                beatmapTextBox.BackColor = SystemColors.Control;

            bool valuesAreBad = (tracker.BeatmapStars == 0
                           && tracker.BeatmapAim == 0
                           && tracker.BeatmapSpeed == 0
                           && tracker.BeatmapCs == 0
                           && tracker.BeatmapAr == 0
                           && tracker.BeatmapOd == 0);
            starsTextBox.BackColor = valuesAreBad ? Color.Pink : SystemColors.Control;
            aimTextBox.BackColor   = valuesAreBad ? Color.Pink : SystemColors.Control;
            speedTextBox.BackColor = valuesAreBad ? Color.Pink : SystemColors.Control;
            textBoxCS.BackColor    = valuesAreBad ? Color.Pink : SystemColors.Control;
            textBoxAR.BackColor    = valuesAreBad ? Color.Pink : SystemColors.Control;
            textBoxOD.BackColor    = valuesAreBad ? Color.Pink : SystemColors.Control;

            BackColor = tracker.MemoryReadError ? Color.Pink : SystemColors.Control;
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
#if DEBUG
            await Task.Run(() => tracker.TickWrapper());
#else
            try
            {
                await Task.Run(() => tracker.Tick());
            }
            catch (TaskCanceledException)
            {
                // do nothing? idk why this occurs. it's probably okay to ignore it.
            }
            catch (Exception ex)
            {
                updateGameVariablesTimer.Stop();
                using (StreamWriter outputFile = new StreamWriter(Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "errorlog.txt")))
                {
                    outputFile.WriteLine("-------------------");
                    outputFile.WriteLine(DateTime.Now);
                    outputFile.WriteLine("-------------------");
                    outputFile.WriteLine(ex.ToString());
                    outputFile.WriteLine("");
                    outputFile.WriteLine("");
                }
                MessageBox.Show($"Exception occurred: {Environment.NewLine}{Environment.NewLine}" +
                    $"{ex.ToString()}{Environment.NewLine}{Environment.NewLine}"
                    , "Error");
                MessageBox.Show($"Please send errorlog.txt to FunOrange. This file is located inside the circle tracker folder.");
                updateGameVariablesTimer.Start();
            }
#endif
        }
        public void StopUpdateTimer()
        {
            updateGameVariablesTimer.Stop();
        }

        private void startupCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (startupCheckBox.Checked)
            {
                CreateShortcut();
            }
            else
            {
                TryDeleteShortcut();
            }
        }

        private void notifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
             Show();
             WindowState = FormWindowState.Normal;
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
        private void TryDeleteShortcut()
        {
            try
            {
                System.IO.File.Delete(ShortcutAddress);
            }
            catch
            {
                // couldn't delete (eg. shortcut in use...)
            }
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

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            tracker.SaveSettings();
        }
    }
}
