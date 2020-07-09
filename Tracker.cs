using FsBeatmapProcessor;
using Google;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Newtonsoft.Json.Linq;
using OsuMemoryDataProvider;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Circle_Tracker
{
    class Tracker
    {
        private readonly IOsuMemoryReader osuReader;
        private Thread thread;
        private int updateInterval = 100;
        private bool exiting;
        private MainForm form;

        // Beatmap
        private string songsFolder;
        public string SongsFolder {
            get => songsFolder;
            set
            {
                songsFolder = value;
                SaveSettings();
            }
        }
        private Beatmap beatmap;
        public string BeatmapPath { get; set; }
        public int BeatmapID { get; set; }
        public int BeatmapSetID { get; set; }
        public string BeatmapString { get; set; }
        public int BeatmapBpm { get; set; }

        // Sound
        private string soundFilename = "sectionpass.wav";
        public bool SubmitSoundEnabled { get; set; }

        // game variables
        public OsuMemoryStatus GameState { get; set; }
        public bool Hidden { get; set; } = false;
        public bool Hardrock { get; set; } = false;
        public bool Doubletime { get; set; } = false;

        public decimal BeatmapStars { get; private set; }
        public decimal BeatmapAim { get; private set; }
        public decimal BeatmapSpeed { get; private set; }
        public decimal BeatmapCs { get; private set; }
        public decimal BeatmapAr { get; private set; }
        public decimal BeatmapOd { get; private set; }
        int cachedHits = 0;
        public int TotalBeatmapHits { get; set; } = 0;
        public int Time { get; set; } = 0;

        // Google Sheets API
        public bool SheetsApiReady { get; set; } = false;
        private string spreadsheetId;
        public string SpreadsheetId {
            get => spreadsheetId;
            set
            {
                spreadsheetId = value;
                SaveSettings();
            }
        }
        private string sheetName;
        public string SheetName
        {
            get => sheetName;
            set
            {
                sheetName = value;
                SaveSettings();
            }
        }
        SheetsService GoogleSheetsService;

        public Tracker(MainForm f)
        {
            form = f;
            LoadSettings();
            osuReader = OsuMemoryReader.Instance.GetInstanceForWindowTitleHint("");
            int _;
            GameState = osuReader.GetCurrentStatus(out _);
        }

        private void SaveSettings()
        {
            string[] lines = { SongsFolder, SpreadsheetId, SheetName, SubmitSoundEnabled ? "1" : "0"};
            File.WriteAllLines("user_settings.txt", lines, Encoding.UTF8);
        }
        private void LoadSettings()
        {
            if (!File.Exists("user_settings.txt"))
            {
                SongsFolder = "";
                SpreadsheetId = "";
                SheetName = "Raw Data";
                SubmitSoundEnabled = true;
            }
            else
            {
                var lines = File.ReadAllLines("user_settings.txt");
                if (lines.Length == 4) {
                    SongsFolder = lines[0];
                    SpreadsheetId = lines[1];
                    SheetName = lines[2];
                    SubmitSoundEnabled = lines[3] == "1";
                }
                else
                {
                    SongsFolder = "";
                    SpreadsheetId = "";
                    SheetName = "Raw Data";
                    SubmitSoundEnabled = true;
                }
            }
        }

        public void StartUpdateThread()
        {
            exiting = false;
            thread = new Thread(TickLoop);
            thread.Start();
        }

        private bool PlayDataEmpty (PlayContainer pc) {
            return (
                pc.Acc      == 100 &&
                pc.C300     == 0   &&
                pc.C100     == 0   &&
                pc.C50      == 0   &&
                pc.CGeki    == 0   &&
                pc.CKatsu   == 0   &&
                pc.CMiss    == 0   &&
                pc.Combo    == 0   &&
                pc.Hp       == 200 &&
                pc.MaxCombo == 0   &&
                pc.Score    == 0
            );
        }

        internal void SetSongsFolder(string folder)
        {
            SongsFolder = folder;
        }

        public void OnClosing()
        {
            exiting = true;
            // TODO: thread.Join() gets stuck here.
            // Solution: Use mutex to wait for when thread execution is not inside form.Invoke()
            thread?.Join();
        }
        private Beatmap BeatmapConstructorWrapper(string beatmapFilename)
        {
            Beatmap newBeatmap = new Beatmap(beatmapFilename);
            if (newBeatmap.ApproachRate == -1M)
                newBeatmap.ApproachRate = newBeatmap.OverallDifficulty;　// i can't believe this is how old maps used to work...
            return newBeatmap;
        }
        public bool TrySwitchBeatmap()
        {
            try
            {
                BeatmapPath = Path.Combine(SongsFolder, osuReader.GetMapFolderName(), osuReader.GetOsuFileName());
            }
            catch
            {
                return false;
            }

            if (!File.Exists(BeatmapPath))
                return false;

            string versionLine = File.ReadLines(BeatmapPath).First();
            Match m = Regex.Match(versionLine, @"osu file format v(\d+)");
            if (!m.Success)
                return false;
            int version = int.Parse(m.Groups[1].ToString());

            beatmap       = BeatmapConstructorWrapper(BeatmapPath);
            BeatmapBpm    = (int)Math.Round(beatmap.Bpm);
            BeatmapString = osuReader.GetSongString();
            BeatmapSetID  = (int)osuReader.GetMapSetId();
            BeatmapID     = osuReader.GetMapId();

            // oppai
            (BeatmapStars, BeatmapAim, BeatmapSpeed) = oppai(BeatmapPath, GetModsString());

            return true;
        }
        public void TickLoop()
        {
            while (!exiting)
            {

                // beatmap
                string beatmapFilename = osuReader.GetOsuFileName();
                if (beatmapFilename != "" && beatmapFilename != Path.GetFileName(BeatmapPath))
                {
                    if (TrySwitchBeatmap())
                        form.Invoke(new MethodInvoker(form.UpdateControls));
                }

                // gameplay stuff
                int _;
                OsuMemoryStatus newGameState = osuReader.GetCurrentStatus(out _);

                if (newGameState != GameState) // state transition
                {
                    if (GameState == OsuMemoryStatus.Playing && newGameState != OsuMemoryStatus.Playing) // beatmap quit
                    {
                        Console.WriteLine("Beatmap Quit Detected: state transitioned from " + GameState.ToString() + " to " + newGameState.ToString());
                        PostBeatmapEntryToGoogleSheets();
                        // reset game variables
                        TotalBeatmapHits = 0;
                        Time = 0;
                    }
                    GameState = newGameState;
                    form.Invoke(new MethodInvoker(form.UpdateControls));
                }

                // read game data
                if (newGameState == OsuMemoryStatus.Playing)
                {
                    // read new game variables
                    PlayContainer playData = new PlayContainer();
                    osuReader.GetPlayData(playData);

                    if (!PlayDataEmpty(playData))
                    {
                        int newHits = playData.C300 + playData.C100 + playData.C50;
                        int newSongTime = osuReader.ReadPlayTime();

                        // detect retry
                        if (newSongTime < Time && Time > 0)
                        {
                            Console.WriteLine($"Beatmap retry; newSongTime {newSongTime} cachedSongTime {Time} Hits {TotalBeatmapHits}");
                        }

                        // update cached data
                        if (newHits > cachedHits && newHits - cachedHits < 5)
                            TotalBeatmapHits += newHits - cachedHits;
                        cachedHits = newHits;
                        Time = newSongTime;
                        var mods = osuReader.GetMods();
                        Hidden     = (mods & 0b00001000) != 0 ? true : false;
                        Hardrock   = (mods & 0b00010000) != 0 ? true : false;
                        Doubletime = (mods & 0b01000000) != 0 ? true : false;
                        (BeatmapStars, BeatmapAim, BeatmapSpeed) = oppai(BeatmapPath, GetModsString());

                        // CS AR OD
                        // FIXME: no support for HalfTime
                        if (Doubletime && Hardrock)
                        {
                            BeatmapCs = beatmap.CircleSize * 1.3M;
                            BeatmapAr = DifficultyCalculator.CalculateARWithDTHR( beatmap.ApproachRate );
                            BeatmapOd = DifficultyCalculator.CalculateODWithDTHR( beatmap.OverallDifficulty );
                        }
                        else if (Doubletime)
                        {
                            BeatmapCs = beatmap.CircleSize;
                            BeatmapAr = DifficultyCalculator.CalculateARWithDT( beatmap.ApproachRate );
                            BeatmapOd = DifficultyCalculator.CalculateODWithDT( beatmap.OverallDifficulty );
                        }
                        else if (Hardrock)
                        {
                            BeatmapCs = beatmap.CircleSize * 1.3M;
                            BeatmapAr = DifficultyCalculator.CalculateARWithHR( beatmap.ApproachRate );
                            BeatmapOd = DifficultyCalculator.CalculateODWithHR( beatmap.OverallDifficulty );
                        }
                        else // NoMod
                        {
                            BeatmapCs = beatmap.CircleSize;
                            BeatmapAr = beatmap.ApproachRate;
                            BeatmapOd = beatmap.OverallDifficulty;
                        }

                        form.Invoke(new MethodInvoker(form.UpdateControls));
                    }
                }
                Thread.Sleep(updateInterval);
            }
        }

        public string GetModsString()
        {
            string mods = "";
            if (Hidden) mods += "HD";
            if (Hardrock) mods += "HR";
            if (Doubletime) mods += "DT";
            return mods;
        }

        private (decimal, decimal, decimal) oppai(string beatmapPath, string mods)
        {
            if (!File.Exists(beatmapPath))
                return (0, 0, 0);

            if (mods != "") mods = $" +{mods}";
            Process oppai = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "oppai.exe",
                    Arguments = $"\"{beatmapPath}\" {mods} -ojson",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                    // fix this omg 
                    //StandardOutputEncoding = Encoding.UTF8
                }
            };
            oppai.Start();
            string oppaiOutput = oppai.StandardOutput.ReadToEnd();
            oppai.WaitForExit();

            // json parsing
            JObject oppaiData;
            try
            {
                oppaiData = JObject.Parse(oppaiOutput);
            }
            catch
            {
                return (0, 0, 0);
            }
            string errstr = oppaiData.GetValue("errstr").ToObject<string>();
            if (errstr != "no error")
            {
                // TODO: An error occurs when opening a non-osu!standard map (mania, taiko, etc)
                Console.WriteLine("Could not calculate difficulty");
                return (0, 0, 0);
            }
            decimal stars = oppaiData.GetValue("stars").ToObject<decimal>();
            decimal aimStars = oppaiData.GetValue("aim_stars").ToObject<decimal>();
            decimal speedStars = oppaiData.GetValue("speed_stars").ToObject<decimal>();
            return (stars, aimStars, speedStars);
        }

        public void InitGoogleAPI(bool silent = false)
        {
            bool credentialsFound = File.Exists("credentials.json");
            form.SetCredentialsFound(credentialsFound);
            if (!credentialsFound)
            {
                if (!silent)
                    MessageBox.Show("credentials.json not found.");
                SetSheetsApiReady(false);
                return;
            }
            if (SpreadsheetId == "")
            {
                if (!silent)
                    MessageBox.Show("Please enter a spreadsheet ID.");
                SetSheetsApiReady(false);
                return;
            }
            if (SheetName == "")
            {
                if (!silent)
                    MessageBox.Show("Please enter a sheet name.");
                SetSheetsApiReady(false);
                return;
            }
            string[] Scopes = { SheetsService.Scope.Spreadsheets };
            string ApplicationName = "Circle Tracker";
            GoogleCredential credential;
            using (var stream = new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
            {
                credential = GoogleCredential.FromStream(stream)
                    .CreateScoped(Scopes);
            }
            GoogleSheetsService = new SheetsService(new Google.Apis.Services.BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName
            });

            // just do an empty write to check if we can successfully write data
            var range = $"'{SheetName}'!Y:Z";
            var valueRange = new ValueRange();
            var writeData = new List<object>() { "", "" };
            valueRange.Values = new List<IList<object>> { writeData };
            var appendRequest = GoogleSheetsService.Spreadsheets.Values.Append(valueRange, SpreadsheetId, range);
            appendRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;
            try
            {
                var appendResponse = appendRequest.Execute();
            }
            catch (GoogleApiException e)
            {
                if (!silent)
                {
                    MessageBox.Show(e.Message, "Google Sheets API Error");
                    if (e.Message.Contains("Unable to parse range"))
                    {
                        MessageBox.Show("Try checking if you entered the correct thing for sheet name. Sheet name refers to the name of a 'tab' in the spreadsheet, not the name of the entire spreadsheet");
                    }
                }
                SetSheetsApiReady(false);
                return;
            }
            catch (Exception e)
            {
                if (!silent)
                {
                    MessageBox.Show(e.Message, "Error");
                }
                SetSheetsApiReady(false);
                return;
            }

            SetSheetsApiReady(true);
        }
        private void PostBeatmapEntryToGoogleSheets()
        {
            if (!SheetsApiReady)
            {
                Console.WriteLine("PostBeatmapEntryToGoogleSheets: Google Sheets API has not yet been setup.");
                return;
            }
            if (TotalBeatmapHits < 10) return;

            string dateTimeFormat = "yyyy'-'MM'-'dd h':'mm tt";
            string escapedName = BeatmapString.Replace("\"", "\"\"");
            string mods = GetModsString();
            if (mods != "") mods = $" +{mods}";

            var range = $"'{SheetName}'!A:J";
            var valueRange = new ValueRange();
            var writeData = new List<object>() {
                /*A: Date & Time*/ DateTime.Now.ToString(dateTimeFormat),
                /*B: Beatmap    */ $"=HYPERLINK(\"https://osu.ppy.sh/beatmapsets/{BeatmapSetID}#osu/{BeatmapID}\", \"{escapedName + mods}\")",
                /*C: Hidden     */ Hidden ? "1":"",
                /*D: Hardrock   */ Hardrock ? "1":"",
                /*E: Doubletime */ Doubletime ? "1":"",
                /*F: BPM        */ Doubletime ? (1.5M * BeatmapBpm) : BeatmapBpm,
                /*G: Aim        */ BeatmapAim,
                /*H: Speed      */ BeatmapSpeed,
                /*I: Stars      */ BeatmapStars,
                /*J: CS         */ BeatmapCs,
                /*K: AR         */ BeatmapAr,
                /*L: OD         */ BeatmapOd,
                /*M: Hits       */ TotalBeatmapHits
            };
            valueRange.Values = new List<IList<object>> { writeData };
            var appendRequest = GoogleSheetsService.Spreadsheets.Values.Append(valueRange, SpreadsheetId, range);
            appendRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;
            var appendResponse = appendRequest.Execute();
            bool success = (appendResponse.Updates.UpdatedRows == 1);
            if (success && SubmitSoundEnabled)
            {
                using (SoundPlayer player = new SoundPlayer(@"sectionpass.wav"))
                {
                    player.Play();
                }
            }
        }
        void SetSheetsApiReady(bool val)
        {
            SheetsApiReady = val;
            form.SetSheetsApiReady(val);
        }

    }
}
