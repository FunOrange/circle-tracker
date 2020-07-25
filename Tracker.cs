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
        private MainForm form;


        private static List<(string, string)> DataRanges = new List<(string, string)>()
        {
            ("Date and Time", "play_date"),
            ("Beatmap", "beatmap_string"),
            ("HD", "HD"),
            ("HR", "HR"),
            ("DT", "DT"),
            ("BPM", "bpm"),
            ("Aim", "aim"),
            ("Speed", "speed"),
            ("Stars", "stars"),
            ("CS", "CS"),
            ("AR", "AR"),
            ("OD", "OD"),
            ("Objects Hit", "hits"),
            ("Acc", "acc"),
            ("300s", "num300s"),
            ("100s", "num100s"),
            ("50s", "num50s"),
            ("Miss", "misses"),
            ("EZ", "EZ"),
            ("HT", "HT"),
            ("FL", "FL"),
            ("Map Complete", "complete")
        };

        // Beatmap
        public string SongsFolder { get; set; }
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
        private DateTime LastPostTime { get; set; }
        private bool GoogleSheetsAPILock { get; set; } = false;
        private bool SpreadsheetTimezoneVerified { get; set; } = false;
        public bool SheetsApiReady { get; set; } = false;
        public bool UseAltFuncSeparator { get; set; } = false;
        public string SpreadsheetId { get; set; }
        public string SheetName { get; set; }
        SheetsService GoogleSheetsService;

        public Tracker(MainForm f)
        {
            form = f;
            LoadSettings();
            osuReader = OsuMemoryReader.Instance.GetInstanceForWindowTitleHint("");
            int _;
            GameState = osuReader.GetCurrentStatus(out _);
            soundFilename = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + @"\sectionpass.wav";
            LastPostTime = DateTime.Now;
        }

        public void SaveSettings()
        {
            string[] lines = {
                SongsFolder,
                SpreadsheetId,
                SheetName,
                SubmitSoundEnabled ? "1" : "0",
                SpreadsheetTimezoneVerified ? "1" : "0"
            };
            File.WriteAllLines("user_settings.txt", lines, Encoding.UTF8);
        }
        private void LoadSettings()
        {
            // Defaults
            SongsFolder = "";
            SpreadsheetId = "";
            SheetName = "Raw Data";
            SubmitSoundEnabled = true;
            SpreadsheetTimezoneVerified = false;

            // Load Settings
            if (File.Exists("user_settings.txt"))
            {
                var lines = File.ReadAllLines("user_settings.txt");
                for (int i = 0; i < lines.Length; i++)
                {
                    switch (i)
                    {
                        case 0: SongsFolder                 = lines[0];        break;
                        case 1: SpreadsheetId               = lines[1];        break;
                        case 2: SheetName                   = lines[2];        break;
                        case 3: SubmitSoundEnabled          = lines[3] == "1"; break;
                        case 4: SpreadsheetTimezoneVerified = lines[4] == "1"; break;
                    }
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
            string beatmapPathTemp = "";
            try
            {
                beatmapPathTemp = Path.Combine(SongsFolder, osuReader.GetMapFolderName(), osuReader.GetOsuFileName());
            }
            catch
            {
                return false;
            }

            if (!File.Exists(beatmapPathTemp))
                return false;

            if (beatmapPathTemp == "")
                return false;

            string versionLine = File.ReadLines(beatmapPathTemp).First();
            Match m = Regex.Match(versionLine, @"osu file format v(\d+)");
            if (!m.Success)
                return false;
            int version = int.Parse(m.Groups[1].ToString());

            // commit to new beatmap
            BeatmapPath   = beatmapPathTemp;
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
            if (beatmapFilename != Path.GetFileName(BeatmapPath) && beatmapFilename != "")
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
                    bool beatmapCompleted = newGameState == OsuMemoryStatus.ResultsScreen;
                    PostBeatmapEntryToGoogleSheetsWrapper(beatmapCompleted);
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
                        PostBeatmapEntryToGoogleSheetsWrapper(false);
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

            // Get entire Spreadsheet
            var getSheetRequest = GoogleSheetsService.Spreadsheets.Get(SpreadsheetId);
            Spreadsheet spreadsheet;
            try
            {
                spreadsheet = getSheetRequest.Execute();
            }
            catch (GoogleApiException e)
            {
                if (!silent)
                    MessageBox.Show(e.Message, "Google Sheets API Error");
                SetSheetsApiReady(false);
                return;
            }

            // Get Raw Data sheet
            Sheet rawDataSheet = null;
            try
            {
                rawDataSheet = spreadsheet.Sheets.First((shit) => shit.Properties.Title == SheetName);
            }
            catch (Exception e)
            {
                if (!silent)
                    MessageBox.Show($"No sheet with the name {SheetName} found.");
                SetSheetsApiReady(false);
                return;
            }

            // Try to set the headers to check if we can successfully write data
            var range = $"'{SheetName}'!A1:1";
            var valueRange = new ValueRange();
            var rawDataHeaders = DataRanges.Select(x => (object)x.Item1).ToList();
            valueRange.Values = new List<IList<object>> { rawDataHeaders };
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

            // Try to add in any missing named ranges
            var namedRanges    = DataRanges.Select(x => x.Item2).ToList();
            var existingRanges = spreadsheet.NamedRanges.Select((namedRange) => namedRange.Name).ToList();
            List<Request> addMissingRangeRequests = new List<Request>();;
            for (int i = 0; i < namedRanges.Count; i++)
            {
                if (!existingRanges.Contains(namedRanges[i]))
                {
                    Console.WriteLine($"Adding range {namedRanges[i]}");
                    // update named ranges to include this data column
                    var req = new Request();
                    req.AddNamedRange = new AddNamedRangeRequest();
                    req.AddNamedRange.NamedRange = new NamedRange();
                    req.AddNamedRange.NamedRange.Name = namedRanges[i];
                    req.AddNamedRange.NamedRange.Range = new GridRange();
                    req.AddNamedRange.NamedRange.Range.SheetId = rawDataSheet.Properties.SheetId;
                    req.AddNamedRange.NamedRange.Range.StartColumnIndex = i;
                    req.AddNamedRange.NamedRange.Range.EndColumnIndex = i + 1;
                    req.AddNamedRange.NamedRange.Range.StartRowIndex = 1;
                    req.AddNamedRange.NamedRange.Range.EndRowIndex = rawDataSheet.Properties.GridProperties.RowCount;
                    addMissingRangeRequests.Add(req);
                }
            }
            var reqs = new BatchUpdateSpreadsheetRequest();
            reqs.Requests = addMissingRangeRequests;
            SpreadsheetsResource.BatchUpdateRequest batchRequest = GoogleSheetsService.Spreadsheets.BatchUpdate(reqs, SpreadsheetId);
            if (addMissingRangeRequests.Count > 0)
            {
                try
                {
                    batchRequest.Execute();
                    string message = $"The following Named Ranges have been added to your spreadsheet:{Environment.NewLine}{Environment.NewLine}";
                    message += String.Join(Environment.NewLine, addMissingRangeRequests.Select(r => $"・{r.AddNamedRange.NamedRange.Name}"));
                    MessageBox.Show(message, "Congratulations");
                }
                catch (GoogleApiException e)
                {
                    if (!silent)
                        MessageBox.Show(e.Message, "Google Sheets API Error");
                    SetSheetsApiReady(false);
                    return;
                }

            }

            // Prompt correct timezone
            if (!SpreadsheetTimezoneVerified)
            {
                var response = MessageBox.Show($"Your spreadsheet timezone is set to {spreadsheet.Properties.TimeZone}.{Environment.NewLine}{Environment.NewLine}" +
                    "Is this correct?",
                    "Confirm Timezone",
                    MessageBoxButtons.YesNo);
                if (response == DialogResult.Yes)
                {
                    SpreadsheetTimezoneVerified = true;
                    MessageBox.Show("cool", "Cool");
                }
                else
                {
                    MessageBox.Show("Please change this in your spreadsheet. Go to File > Spreadsheet Settings and then change the timezone there.");
                }
            }
            
            SetSheetsApiReady(true);
        }

        private void PostBeatmapEntryToGoogleSheetsWrapper(bool complete)
        {
            //Console.WriteLine(new System.Diagnostics.StackTrace());
            //Console.WriteLine("google sheets api access: " + DateTime.Now);
            if (GoogleSheetsAPILock)
            {
                //Console.WriteLine("duplicate call detected");
                return;
            }
            GoogleSheetsAPILock = true; // acquire lock
            try
            {
                PostBeatmapEntryToGoogleSheets(complete);
            }
            catch (NullReferenceException e)
            {
                // Game variable probably wasn't loaded or read (blame OsuMemoryDataProvider)
            }
            GoogleSheetsAPILock = false; // release lock
        }
        private void PostBeatmapEntryToGoogleSheets(bool complete)
        {
            if (!SheetsApiReady)
            {
                //Console.WriteLine("PostBeatmapEntryToGoogleSheets: Google Sheets API has not yet been setup.");
                return;
            }

            // duplicate post bug. Fix -> set a 5 second cooldown in between consecutive posts
            var timeSinceLastPost = DateTime.Now.Subtract(LastPostTime);
            if (timeSinceLastPost.TotalSeconds < 5)
                return;

            // minimum hits to submit
            if (TotalBeatmapHits < 10) return;

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
                /*U: complete   */ complete ? "1":"0",
            };
            valueRange.Values = new List<IList<object>> { writeData };
            var appendRequest = GoogleSheetsService.Spreadsheets.Values.Append(valueRange, SpreadsheetId, range);
            appendRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;
            var appendResponse = appendRequest.Execute();
            bool success = (appendResponse.Updates.UpdatedRows == 1);
            if (success)
            {
                LastPostTime = DateTime.Now;
                if (SubmitSoundEnabled)
                {
                    using (SoundPlayer player = new SoundPlayer(soundFilename))
                    {
                        player.Play();
                    }
                }
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
