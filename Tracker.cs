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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Media;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Circle_Tracker
{
    class Tracker
    {
        private readonly IOsuMemoryReader osuReader;
        private int updateInterval = 100;
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
        private string soundFilename;
        public bool SubmitSoundEnabled { get; set; }

        // game variables
        public OsuMemoryStatus GameState { get; set; }
        public int RawMods { get; set; } = 0;
        public bool Hidden { get; set; } = false;
        public bool Hardrock { get; set; } = false;
        public bool Doubletime { get; set; } = false;
        public bool EZ { get; set; } = false;
        public bool Halftime { get; set; } = false;
        public bool Flashlight { get; set; } = false;
        public bool Auto { get; set; } = false;

        public decimal BeatmapStars { get; private set; }
        public decimal BeatmapAim { get; private set; }
        public decimal BeatmapSpeed { get; private set; }
        public decimal BeatmapCs { get; private set; }
        public decimal BeatmapAr { get; private set; }
        public decimal BeatmapOd { get; private set; }
        int cached300c = 0;
        int cached100c = 0;
        int cached50c = 0;
        int cachedMissc = 0;
        int cachedHits = 0;
        public int Play300c { get; set; } = 0;
        public int Play100c { get; set; } = 0;
        public int Play50c { get; set; } = 0;
        public int PlayMissc { get; set; } = 0;
        public int TotalBeatmapHits { get; set; } = 0;
        public decimal LastPostedAcc { get; set; } = 0;
        public decimal Accuracy { get; set; } = 0;
        public int Time { get; set; } = 0;

        // Google Sheets API
        Stopwatch stopwatch;
        public bool SheetsApiReady { get; set; } = false;
        public bool UseAltFuncSeparator { get; set; } = false;
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
            stopwatch = new Stopwatch();
            soundFilename = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + @"\sectionpass.wav";

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

        private bool PlayDataValid (PlayContainer pc) {
            return !(
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
        public void Tick()
        {
            // beatmap
            string beatmapFilename = osuReader.GetOsuFileName();
            if (beatmapFilename != "" && beatmapFilename != Path.GetFileName(BeatmapPath))
            {
                TrySwitchBeatmap();
            }

            // gameplay stuff
            int _;
            OsuMemoryStatus newGameState = osuReader.GetCurrentStatus(out _);

            if (newGameState != GameState) // state transition
            {
                if (GameState == OsuMemoryStatus.Playing && newGameState != OsuMemoryStatus.Playing) // beatmap quit
                {
                    //Console.WriteLine("Beatmap Quit Detected: state transitioned from " + GameState.ToString() + " to " + newGameState.ToString());
                    PostBeatmapEntryToGoogleSheets();
                    // reset game variables
                    cached300c = 0;
                    cached100c = 0;
                    cached50c = 0;
                    cachedMissc = 0;
                    Play300c = 0;
                    Play100c = 0;
                    Play50c = 0;
                    PlayMissc = 0;
                    Accuracy = 0;
                    TotalBeatmapHits = 0;
                    Time = 0;
                }
                GameState = newGameState;
            }

            // update mods
            bool songSelectGameState =
                newGameState == OsuMemoryStatus.SongSelect
                || newGameState == OsuMemoryStatus.MultiplayerRoom
                || newGameState == OsuMemoryStatus.MultiplayerSongSelect;
            if (songSelectGameState && beatmap != null)
            {
                RawMods = osuReader.GetMods();
                if (RawMods != -1) // invalid
                {
                    //Console.WriteLine(RawMods);
                    Hidden     = (RawMods & 8) != 0 ? true : false;
                    Hardrock   = (RawMods & 16) != 0 ? true : false;
                    Doubletime = ((RawMods & 64) != 0 || (RawMods & 0b001001000000) != 0) ? true : false;
                    EZ         = (RawMods & 2) != 0 ? true : false;
                    Halftime   = (RawMods & 256) != 0 ? true : false;
                    Flashlight = (RawMods & 1024) != 0 ? true : false;
                    Auto       = (RawMods & 2048) != 0 ? true : false;
                    (BeatmapStars, BeatmapAim, BeatmapSpeed) = oppai(BeatmapPath, GetModsString());

                    // CS AR OD
                    // FIXME: no support for HalfTime
                    BeatmapCs = beatmap.CircleSize * (Hardrock ? 1.3M : EZ ? 0.5M : 1);
                    if (Halftime && !Doubletime && !Hardrock && EZ) // HTEZ
                    {
                        BeatmapAr = DifficultyCalculator.CalculateARWithHTEZ(beatmap.ApproachRate);
                        BeatmapOd = DifficultyCalculator.CalculateODWithHTEZ(beatmap.OverallDifficulty);
                    }
                    else if (Halftime && !Doubletime && !Hardrock && !EZ) // HT
                    {
                        BeatmapAr = DifficultyCalculator.CalculateARWithHT(beatmap.ApproachRate);
                        BeatmapOd = DifficultyCalculator.CalculateODWithHT(beatmap.OverallDifficulty);
                    }
                    else if (Halftime && !Doubletime && Hardrock && !EZ) // HTHR
                    {
                        BeatmapAr = DifficultyCalculator.CalculateARWithHTHR(beatmap.ApproachRate);
                        BeatmapOd = DifficultyCalculator.CalculateODWithHTHR(beatmap.OverallDifficulty);
                    }
                    else if (!Halftime && !Doubletime && !Hardrock && EZ) // EZ
                    {
                        BeatmapAr = DifficultyCalculator.CalculateARWithEZ(beatmap.ApproachRate);
                        BeatmapOd = DifficultyCalculator.CalculateODWithEZ(beatmap.OverallDifficulty);
                    }
                    else if (!Halftime && !Doubletime && Hardrock && !EZ) // HR
                    {
                        BeatmapAr = DifficultyCalculator.CalculateARWithHR(beatmap.ApproachRate);
                        BeatmapOd = DifficultyCalculator.CalculateODWithHR(beatmap.OverallDifficulty);
                    }
                    else if (!Halftime && Doubletime && !Hardrock && EZ) // DTEZ
                    {
                        BeatmapAr = DifficultyCalculator.CalculateARWithDTEZ(beatmap.ApproachRate);
                        BeatmapOd = DifficultyCalculator.CalculateODWithDTEZ(beatmap.OverallDifficulty);
                    }
                    else if (!Halftime && Doubletime && !Hardrock && !EZ) // DT
                    {
                        BeatmapAr = DifficultyCalculator.CalculateARWithDT(beatmap.ApproachRate);
                        BeatmapOd = DifficultyCalculator.CalculateODWithDT(beatmap.OverallDifficulty);
                    }
                    else if (!Halftime && Doubletime && Hardrock && !EZ) // DTHR
                    {
                        BeatmapAr = DifficultyCalculator.CalculateARWithDTHR(beatmap.ApproachRate);
                        BeatmapOd = DifficultyCalculator.CalculateODWithDTHR(beatmap.OverallDifficulty);
                    }
                    else // NoMod
                    {
                        BeatmapCs = beatmap.CircleSize;
                        BeatmapAr = beatmap.ApproachRate;
                        BeatmapOd = beatmap.OverallDifficulty;
                    }
                }
            }

            // read gameplay data
            if (newGameState == OsuMemoryStatus.Playing)
            {
                // read new game variables
                PlayContainer playData = new PlayContainer();
                osuReader.GetPlayData(playData);

                if (PlayDataValid(playData))
                {
                    Accuracy = (decimal)playData.Acc;
                    int new300c = playData.C300;
                    int new100c = playData.C100;
                    int new50c = playData.C50;
                    int newMissc = playData.CMiss;
                    int newHits = playData.C300 + playData.C100 + playData.C50;
                    int newSongTime = osuReader.ReadPlayTime();

                    // update hits
                    if (newMissc > cachedMissc)
                    {
                        PlayMissc += newMissc - cachedMissc;
                    }
                    if (newHits > cachedHits && newHits - cachedHits < 5)
                    {
                        Play300c         += new300c - cached300c;
                        Play100c         += new100c - cached100c;
                        Play50c          += new50c - cached50c;
                        TotalBeatmapHits += newHits - cachedHits;
                    }
                    cached300c = new300c;
                    cached100c = new100c;
                    cached50c = new50c;
                    cachedMissc = newMissc;
                    cachedHits = newHits;

                    // detect retry
                    if (newSongTime < Time && Time > 0)
                    {
                        //Console.WriteLine($"Beatmap retry; newSongTime {newSongTime} cachedSongTime {Time} Hits {TotalBeatmapHits}");
                        PostBeatmapEntryToGoogleSheets();
                        // reset game variables
                        cached300c = 0;
                        cached100c = 0;
                        cached50c = 0;
                        cachedMissc = 0;
                        Play300c = 0;
                        Play100c = 0;
                        Play50c = 0;
                        PlayMissc = 0;
                        TotalBeatmapHits = 0;
                        Time = 0;
                    }
                    else
                    {
                        // update time
                        Time = newSongTime;
                    }
                }
            }
        }

        public string GetModsString()
        {
            string mods = "";
            if (Auto) mods += "AT";
            if (EZ) mods += "EZ";
            if (Halftime) mods += "HT";
            if (Hidden) mods += "HD";
            if (Hardrock) mods += "HR";
            if (Doubletime) mods += "DT";
            if (Flashlight) mods += "FL";
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
                //Console.WriteLine("Could not calculate difficulty");
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

            // Try to set the headers to check if we can successfully write data
            var range = $"'{SheetName}'!A1:1";
            var valueRange = new ValueRange();
            var writeData = new List<object>() { "Date and Time", "Beatmap", "HD", "HR", "DT", "BPM", "Aim", "Speed", "Stars", "CS", "AR", "OD", "Objects Hit", "Acc", "300s", "100s", "50s", "Miss", "EZ", "HT", "FL"};
            valueRange.Values = new List<IList<object>> { writeData };
            var writeRequest = GoogleSheetsService.Spreadsheets.Values.Update(valueRange, SpreadsheetId, range);
            writeRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;
            try
            {
                var appendResponse = writeRequest.Execute();
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
                    if (e.Message.Contains("Requested entity was not found"))
                    {
                        MessageBox.Show("Try double checking to see if the Spreadsheet ID is correct.");
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
                //Console.WriteLine("PostBeatmapEntryToGoogleSheets: Google Sheets API has not yet been setup.");
                return;
            }

            // duplicate post bug. Fix -> set a 5 second cooldown in between consecutive posts
            stopwatch.Stop();
            if (stopwatch.ElapsedMilliseconds < 5000 && stopwatch.ElapsedMilliseconds != 0)
            {
                Console.WriteLine($"Time elapsed between consecutive posts: {stopwatch.Elapsed}");
                Console.WriteLine($"Duplicate post detected");
                stopwatch.Reset();
                return;
            }
            stopwatch.Reset();

            // minimum hits to submit
            if (TotalBeatmapHits < 20) return;


            string dateTimeFormat = "yyyy'-'MM'-'dd h':'mm tt";
            string escapedName = BeatmapString.Replace("\"", "\"\"");
            string mods = GetModsString();
            if (mods != "") mods = $" +{mods}";

            var range = $"'{SheetName}'!A:J";
            var valueRange = new ValueRange();
            string functionSeparator = UseAltFuncSeparator ? ";" : ",";
            var writeData = new List<object>() {
                /*A: Date & Time*/ DateTime.Now.ToString(dateTimeFormat, CultureInfo.InvariantCulture),
                /*B: Beatmap    */ $"=HYPERLINK(\"https://osu.ppy.sh/beatmapsets/{BeatmapSetID}#osu/{BeatmapID}\", \"{escapedName + mods}\")",
                /*C: Hidden     */ Hidden ? "1":"",
                /*D: Hardrock   */ Hardrock ? "1":"",
                /*E: Doubletime */ Doubletime ? "1":"",
                /*F: BPM        */ BeatmapBpm * (Doubletime ? 1.5M : Halftime ? 0.75M : 1),
                /*G: Aim        */ BeatmapAim,
                /*H: Speed      */ BeatmapSpeed,
                /*I: Stars      */ BeatmapStars,
                /*J: CS         */ BeatmapCs,
                /*K: AR         */ BeatmapAr,
                /*L: OD         */ BeatmapOd,
                /*M: Hits       */ TotalBeatmapHits,
                /*N: Acc        */ Accuracy,
                /*O: 300c       */ Play300c,
                /*P: 100c       */ Play100c,
                /*Q: 50c        */ Play50c,
                /*R: Missc      */ PlayMissc,
                /*R: EZ         */ EZ ? "1":"",
                /*S: HT         */ Halftime ? "1":"",
                /*T: FL         */ Flashlight ? "1":"",
            };
            valueRange.Values = new List<IList<object>> { writeData };
            var appendRequest = GoogleSheetsService.Spreadsheets.Values.Append(valueRange, SpreadsheetId, range);
            appendRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;
            var appendResponse = appendRequest.Execute();
            bool success = (appendResponse.Updates.UpdatedRows == 1);
            if (success && SubmitSoundEnabled)
            {
                Console.WriteLine(soundFilename);
                using (SoundPlayer player = new SoundPlayer(soundFilename))
                {
                    player.Play();
                }
                stopwatch.Start();
            }
            else
            {
                Console.WriteLine("submit error");
            }
        }
        void SetSheetsApiReady(bool val)
        {
            SheetsApiReady = val;
            form.SetSheetsApiReady(val);
        }

    }
}
