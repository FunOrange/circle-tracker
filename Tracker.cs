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

        // game variables
        private int beatmapID = 0;
        private string beatmapSong = "";
        private string beatmapArtist = "";
        private string beatmapDiff = "";
        private int hits = 0;
        private int cachedSongTime = 0;
        public string BeatmapPath { get; set; }
        public decimal BeatmapStars { get; private set; }
        public decimal BeatmapAim { get; private set; }
        public decimal BeatmapSpeed { get; private set; }

        public int Hits { get => hits; set => hits = value; }
        public int Time { get => cachedSongTime; set => cachedSongTime = value; }
        public OsuMemoryStatus GameState { get => cachedGameState; set => cachedGameState = value; }

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
            thread?.Join();
            Console.WriteLine("closing");
        }

        public void TickLoop()
        {
            while (!exiting)
            {

                // beatmap
                string beatmapFilename = osuReader.GetOsuFileName();
                if (beatmapFilename != "" || beatmapFilename != Path.GetFileName(BeatmapPath))
                {
                    BeatmapPath = Path.Combine(SongsFolder, osuReader.GetMapFolderName(), osuReader.GetOsuFileName());

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
                        Console.WriteLine($"beatmap exit; Hits {Hits}");
                        // reset game variables
                        Hits = 0;
                        cachedSongTime = 0;
                    }
                    form.Invoke(new MethodInvoker(form.UpdateControls));
                    cachedGameState = newGameState;
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
                        if (newSongTime < cachedSongTime && cachedSongTime > 0)
                        {
                            Console.WriteLine($"Beatmap retry; newSongTime {newSongTime} cachedSongTime {cachedSongTime} Hits {Hits}");
                        }

                        // update cached data
                        Hits = newHits;
                        cachedSongTime = newSongTime;

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
    }
}
