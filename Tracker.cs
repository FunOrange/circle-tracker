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
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace circle_tracker
{
    class Tracker
    {
        private readonly IOsuMemoryReader osuReader;
        private Thread thread;
        private int updateInterval = 33;
        private bool exiting;
        private bool osuRunning;
        private OsuMemoryStatus cachedGameState;
        private MainForm form;
        private string songsFolder;
        public string SongsFolder { get => songsFolder; private set => songsFolder = value; }
        public bool SheetsApiReady { get; set; } = false;

        // game variables
        public int BeatmapID { get; set; }
        public int BeatmapSetID { get; set; }
        public string BeatmapString { get; set; }
        public string BeatmapPath { get; set; }
        public decimal BeatmapStars { get; private set; }
        public decimal BeatmapAim { get; private set; }
        public decimal BeatmapSpeed { get; private set; }
        public int TotalBeatmapHits { get; set; } = 0;
        private int cachedHits = 0;
        public int Time { get; set; } = 0;
        public OsuMemoryStatus GameState { get => cachedGameState; set => cachedGameState = value; }

        // Google Sheets API
        public string SpreadsheetId { get; set; } = "1cIBABHakKLLFHj0JqcLtsAL17FR5dC841Dow3lvzK-M";
        public string SheetName = "Raw Data";
        SheetsService GoogleSheetsService;

        public Tracker(MainForm f)
        {
            form = f;
            songsFolder = Properties.Settings.Default.SongsFolder;
            osuReader = OsuMemoryReader.Instance.GetInstanceForWindowTitleHint("");
            int _;
            cachedGameState = osuReader.GetCurrentStatus(out _);
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
            songsFolder = folder;
            Properties.Settings.Default.SongsFolder = folder;
            Properties.Settings.Default.Save();
        }

        public void OnClosing()
        {
            exiting = true;
            thread?.Abort();
            Console.WriteLine("closing");
        }

        public void TickLoop()
        {
            while (!exiting)
            {

                // beatmap
                string beatmapFilename = osuReader.GetOsuFileName();
                if (beatmapFilename != "" && beatmapFilename != Path.GetFileName(BeatmapPath))
                {
                    try
                    {
                        BeatmapPath = Path.Combine(SongsFolder, osuReader.GetMapFolderName(), osuReader.GetOsuFileName());
                    }
                    catch
                    {
                        return; 
                    }
                    BeatmapString = osuReader.GetSongString();
                    BeatmapSetID = (int)osuReader.GetMapSetId();
                    BeatmapID = osuReader.GetMapId();

                    // oppai
                    (BeatmapStars, BeatmapAim, BeatmapSpeed) = oppai(BeatmapPath);

                    form.Invoke(new MethodInvoker(form.UpdateControls));
                }

                // gameplay stuff
                int _;
                OsuMemoryStatus newGameState = osuReader.GetCurrentStatus(out _);

                if (newGameState != cachedGameState) // state transition
                {
                    if (cachedGameState == OsuMemoryStatus.Playing && newGameState != OsuMemoryStatus.Playing) // beatmap quit
                    {
                        Console.WriteLine("Beatmap Quit Detected: state transitioned from " + cachedGameState.ToString() + " to " + newGameState.ToString());
                        PostBeatmapEntryToGoogleSheets();
                        // reset game variables
                        TotalBeatmapHits = 0;
                        Time = 0;
                    }
                    cachedGameState = newGameState;
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

                        form.Invoke(new MethodInvoker(form.UpdateControls));

                    }
                }
            }
        }

        private (decimal, decimal, decimal) oppai(string beatmapPath)
        {
            if (!File.Exists(beatmapPath))
                return (0, 0, 0);
            
            Process oppai = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "oppai.exe",
                    Arguments = $"\"{beatmapPath}\" -ojson",
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

        public void InitGoogleAPI()
        {
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
            var appendResponse = appendRequest.Execute();

            SetSheetsApiReady(true);
        }
        private void PostBeatmapEntryToGoogleSheets()
        {
            if (!SheetsApiReady)
            {
                Console.WriteLine("PostBeatmapEntryToGoogleSheets: Google Sheets API has not yet been setup.");
                return;
            }
            if (TotalBeatmapHits == 0) return;

            string dateTimeFormat = "g";
            string beatmapCell = $"=HYPERLINK(\"https://osu.ppy.sh/beatmapsets/{BeatmapSetID}#osu/{BeatmapID}\", \"{BeatmapString}\")";

            var range = $"'{SheetName}'!A:G";
            var valueRange = new ValueRange();
            var writeData = new List<object>() { DateTime.Now.ToString(dateTimeFormat), beatmapCell, 0, BeatmapStars, BeatmapAim, BeatmapSpeed, TotalBeatmapHits };
            valueRange.Values = new List<IList<object>> { writeData };
            var appendRequest = GoogleSheetsService.Spreadsheets.Values.Append(valueRange, SpreadsheetId, range);
            appendRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;
            var appendResponse = appendRequest.Execute();
        }
        void SetSheetsApiReady(bool val)
        {
            SheetsApiReady = val;
            form.SetSheetsApiReady(val);
        }

    }
}
