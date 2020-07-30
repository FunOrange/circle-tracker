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
            ("Map Complete", "complete"),
            ("Playcount", "playcount"),
            ("Time (s)", "time_seconds")
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
        private bool TickLock { get; set; } = false;
        private bool SpreadsheetTimezoneVerified { get; set; } = false;
        public bool SheetsApiReady { get; set; } = false;
        public bool UseAltFuncSeparator { get; set; } = false;
        public string SpreadsheetId { get; set; }
        public string SheetName { get; set; }
        public int SheetRows { get; set; }
        SheetsService GoogleSheetsService;
        Spreadsheet UserSpreadsheet = null;
        Sheet RawDataSheet = null;

        public Tracker(MainForm f)
        {
            form = f;
            LoadSettings();
            osuReader = OsuMemoryReader.Instance.GetInstanceForWindowTitleHint("");
            int _;
            GameState = osuReader.GetCurrentStatus(out _);
            soundFilename = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + @"\sectionpass.wav";
            LastPostTime = DateTime.Now;

            // First time running circle tracker
            if (!File.Exists("user_settings.txt"))
            {
                MessageBox.Show($"Thank you for trying out circle tracker!{Environment.NewLine}" +
                    $"For instructions on how to get set up, watch my tutorial video on Youtube. " +
                    $"If you need help with anything, feel free to message FunOrange on osu!, twitter, discord, etc.",
                    "Welcome!");
            }
        }

        public void SaveSettings()
        {
            string[] lines = {
                SongsFolder,
                SpreadsheetId,
                SheetName,
                SubmitSoundEnabled ? "1" : "0",
                SpreadsheetTimezoneVerified ? "1" : "0",
                UseAltFuncSeparator ? "1" : "0"
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
            UseAltFuncSeparator = false;

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
                        case 5: UseAltFuncSeparator         = lines[5] == "1"; break;
                    }
                }
            }
        }

        private bool PlayDataValid (PlayContainer pc) {
            try
            {
               decimal acc = (decimal)pc.Acc;
            }
            catch (Exception)
            {
                return false;
            }            
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
                    TryPostBeatmapEntryToGoogleSheets(beatmapCompleted);
                    // reset game variables
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
                    if (newMissc > PlayMissc)
                    {
                        PlayMissc = newMissc;
                    }
                    if (newHits > TotalBeatmapHits && newHits - TotalBeatmapHits < 50) // safety measure: counters can't decrease; can't increment by more than 50
                    {
                        Play300c         = new300c;
                        Play100c         = new100c;
                        Play50c          = new50c;
                        TotalBeatmapHits = newHits;
                    }

                    // detect retry
                    if (newSongTime < Time && Time > 0)
                    {
                        //Console.WriteLine($"Beatmap retry; newSongTime {newSongTime} cachedSongTime {Time} Hits {TotalBeatmapHits}");
                        TryPostBeatmapEntryToGoogleSheets(false);
                        // reset game variables
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

        public string getFunctionSeparator()
        {
            return UseAltFuncSeparator ? ";" : ",";
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
            try
            {
                UserSpreadsheet = getSheetRequest.Execute();
            }
            catch (GoogleApiException e)
            {
                if (!silent)
                    MessageBox.Show(e.Message, "Google Sheets API Error");
                SetSheetsApiReady(false);
                return;
            }

            // Get Raw Data sheet
            try
            {
                RawDataSheet = UserSpreadsheet.Sheets.First((shit) => shit.Properties.Title == SheetName);
            }
            catch (Exception e)
            {
                if (!silent)
                    MessageBox.Show($"No sheet with the name {SheetName} found.");
                SetSheetsApiReady(false);
                return;
            }
            SheetRows = (int)RawDataSheet.Properties.GridProperties.RowCount;

            // Write headers (Row 1)
            try
            {
                WriteHeaders();
            }
            catch (GoogleApiException e)
            {
                if (!silent)
                {
                    MessageBox.Show(e.Message, "Google Sheets API Error");
                    
                    if (e.Message.Contains("Unable to parse range"))
                        MessageBox.Show("Try checking if you entered the correct thing for sheet name. Sheet name refers to the name of a 'tab' in the spreadsheet, not the name of the entire spreadsheet");
                    if (e.Message.Contains("Requested entity was not found"))
                        MessageBox.Show("Try double checking to see if the Spreadsheet ID is correct.");
                }
                SetSheetsApiReady(false);
                return;
            }

            // Try to write playcount to W2
            string range = $"'{SheetName}'!W2";
            ValueRange valueRange = new ValueRange();
            valueRange.Values = new List<IList<object>> { new List<object>() { $"=ARRAYFORMULA(IF(ISBLANK(hits) = false{getFunctionSeparator()} hits^0{getFunctionSeparator()}))" } };
            var writeRequest = GoogleSheetsService.Spreadsheets.Values.Update(valueRange, SpreadsheetId, range);
            writeRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;
            try
            {
                writeRequest.Execute();
            }
            catch (GoogleApiException e)
            {
                if (!silent)
                    MessageBox.Show(e.Message, $"Google Sheets API Error: Unable to Write Playcount to {range}");
                SetSheetsApiReady(false);
                return;
            }

            // Add in Missing Named Ranges (if any)
            try
            {
                AddMissingNamedRanges(UserSpreadsheet, RawDataSheet);
            }
            catch (GoogleApiException e)
            {
                if (!silent)
                    MessageBox.Show(e.Message, "Google Sheets API Error: Unable to Add Named Ranges");
                SetSheetsApiReady(false);
                return;
            }

            // Extend ranges if necessary
            ResizeNamedRanges(UserSpreadsheet, SheetRows);

            // Prompt Correct Timezone
            PromptTimezone(UserSpreadsheet);

            SetSheetsApiReady(true);
        }

        private void ResizeNamedRanges(Spreadsheet spreadsheet, int rows)
        {
            var definedNamedRanges = DataRanges.Select(x => x.Item2).ToList();
            var rangesToUpdate = spreadsheet.NamedRanges
                .Where(nr => definedNamedRanges.Contains(nr.Name))
                .Where(nr => nr.Range.EndRowIndex != rows);
            List<Request> rangeUpdateRequests = new List<Request>();
            foreach (NamedRange nr in rangesToUpdate)
            {
                var req = new Request();
                req.UpdateNamedRange = new UpdateNamedRangeRequest();
                req.UpdateNamedRange.NamedRange = nr;
                req.UpdateNamedRange.NamedRange.Range.EndRowIndex = rows;
                req.UpdateNamedRange.Fields = "Range";
                rangeUpdateRequests.Add(req);
            }

            if (rangeUpdateRequests.Count > 0)
            {
                var reqs = new BatchUpdateSpreadsheetRequest();
                reqs.Requests = rangeUpdateRequests;
                SpreadsheetsResource.BatchUpdateRequest batchRequest = GoogleSheetsService.Spreadsheets.BatchUpdate(reqs, SpreadsheetId);
                batchRequest.Execute();
            }
        }

        private void WriteHeaders()
        {
            string range = $"'{SheetName}'!A1:1";
            ValueRange valueRange = new ValueRange();
            var rawDataHeaders = DataRanges.Select(x => (object)x.Item1).ToList();
            valueRange.Values = new List<IList<object>> { rawDataHeaders };
            SpreadsheetsResource.ValuesResource.UpdateRequest writeRequest = GoogleSheetsService.Spreadsheets.Values.Update(valueRange, SpreadsheetId, range);
            writeRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;
            writeRequest.Execute();
        }

        private void PromptTimezone(Spreadsheet spreadsheet)
        {
            if (!SpreadsheetTimezoneVerified)
            {
                var userResponse = MessageBox.Show($"Your spreadsheet timezone is set to {spreadsheet.Properties.TimeZone}.{Environment.NewLine}{Environment.NewLine}" +
                    "Is this correct?",
                    "Confirm Timezone",
                    MessageBoxButtons.YesNo);
                if (userResponse == DialogResult.Yes)
                {
                    SpreadsheetTimezoneVerified = true;
                    MessageBox.Show("cool", "Cool");
                }
                else
                {
                    MessageBox.Show("Please change this in your spreadsheet. Go to File > Spreadsheet Settings and then change the timezone there.");
                }
            }
        }

        private void AddMissingNamedRanges(Spreadsheet spreadsheet, Sheet rawDataSheet)
        {
            var namedRanges = DataRanges.Select(x => x.Item2).ToList();
            var existingRanges = spreadsheet.NamedRanges.Select((namedRange) => namedRange.Name).ToList();
            List<Request> addMissingRangeRequests = new List<Request>();
            for (int i = 0; i < namedRanges.Count; i++)
            {
                if (!existingRanges.Contains(namedRanges[i]))
                {
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
                    req.AddNamedRange.NamedRange.Range.EndRowIndex = SheetRows;
                    addMissingRangeRequests.Add(req);
                }
            }
            if (addMissingRangeRequests.Count > 0)
            {
                var reqs = new BatchUpdateSpreadsheetRequest();
                reqs.Requests = addMissingRangeRequests;
                SpreadsheetsResource.BatchUpdateRequest batchRequest = GoogleSheetsService.Spreadsheets.BatchUpdate(reqs, SpreadsheetId);
                batchRequest.Execute();
                string message = $"The following Named Ranges have been added to your spreadsheet:{Environment.NewLine}{Environment.NewLine}";
                message += String.Join(Environment.NewLine, addMissingRangeRequests.Select(r => $"・{r.AddNamedRange.NamedRange.Name}"));
                MessageBox.Show(message, "Congratulations");
            }
        }

        public void TickWrapper()
        {
            //Console.WriteLine(new System.Diagnostics.StackTrace());
            //Console.WriteLine("google sheets api access: " + DateTime.Now);
            if (TickLock)
            {
                //Console.WriteLine("duplicate call detected");
                return;
            }
            TickLock = true; // acquire lock
            Tick();
            TickLock = false; // release lock
        }
        private void TryPostBeatmapEntryToGoogleSheets(bool complete)
        {
            try
            {
                PostBeatmapEntryToGoogleSheets(complete);
            }
            catch (NullReferenceException e)
            {
                // Game variable probably wasn't loaded or read (blame OsuMemoryDataProvider)
                MessageBox.Show("Could not detect current beatmap. " +
                    "Sorry, this part of the program is pretty much RNG. " +
                    "Just try to restart Circle Tracker and/or osu! until beatmaps start being detected in the Circle Tracker window."
                    , "oops");
            }
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
            var writeData = new List<object>() {
                /*A: Date & Time*/ DateTime.Now.ToString(dateTimeFormat, CultureInfo.InvariantCulture),
                /*B: Beatmap    */ $"=HYPERLINK(\"https://osu.ppy.sh/beatmapsets/{BeatmapSetID}#osu/{BeatmapID}\"{getFunctionSeparator()} \"{escapedName + mods}\")",
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
                /*S: EZ         */ EZ ? "1":"",
                /*T: HT         */ Halftime ? "1":"",
                /*U: FL         */ Flashlight ? "1":"",
                /*V: complete   */ complete ? "1":"0",
                /*W: playcount  */ "",                 // (this is provided by a formula in row 2)
                /*X: time       */ (Time / 1000)
            };
            valueRange.Values = new List<IList<object>> { writeData };
            var appendRequest = GoogleSheetsService.Spreadsheets.Values.Append(valueRange, SpreadsheetId, range);
            appendRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;
            var appendResponse = appendRequest.Execute();

            // play submit success
            LastPostTime = DateTime.Now;
            if (SubmitSoundEnabled)
            {
                using (SoundPlayer player = new SoundPlayer(soundFilename))
                {
                    player.Play();
                }
            }

            int updatedRow = 0;
            foreach (Match m in new Regex(@"\d+").Matches(appendResponse.Updates.UpdatedRange))
            {
                int parsedInt = int.Parse(m.Value);
                if (parsedInt > updatedRow)
                    updatedRow = parsedInt;
            }
            if (updatedRow > SheetRows)
            {
                // Add 100 more rows and update named ranges
                var req = new Request();
                req.AppendDimension = new AppendDimensionRequest();
                req.AppendDimension.Dimension = "ROWS";
                req.AppendDimension.SheetId = RawDataSheet.Properties.SheetId;
                req.AppendDimension.Length = 100;
                var b1 = new BatchUpdateSpreadsheetRequest();
                b1.Requests = new List<Request>() { req };
                var b2 = GoogleSheetsService.Spreadsheets.BatchUpdate(b1, SpreadsheetId);
                b2.Execute();

                // Update Named Ranges
                ResizeNamedRanges(UserSpreadsheet, updatedRow + 100);
                SheetRows = updatedRow + 100;
            }
        }
        void SetSheetsApiReady(bool val)
        {
            SheetsApiReady = val;
            form.SetSheetsApiReady(val);
        }

    }
}
