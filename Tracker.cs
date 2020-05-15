using OsuMemoryDataProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace circle_tracker
{
    class Tracker
    {
        private readonly IOsuMemoryReader osuReader;
        private Thread thread;
        private int updateInterval = 33;
        private bool running;

        // osu temporal variables variables
        private string cachedBeatmapFilename;

        public Tracker()
        {
            osuReader = OsuMemoryReader.Instance.GetInstanceForWindowTitleHint("");
            running = true;
            thread = new Thread(TickLoop);
            thread.Start();
        }

        public void OnClosing()
        {
            running = false;
            thread?.Join();
            Console.WriteLine("closing");
        }

        public void TickLoop()
        {
            while (running)
            {
                int status;
                osuReader.GetCurrentStatus(out status);
                string currentBeatmapFilename = osuReader.GetOsuFileName();
                if (currentBeatmapFilename != cachedBeatmapFilename)
                {
                    Console.WriteLine(currentBeatmapFilename);
                }
                cachedBeatmapFilename = currentBeatmapFilename;
            }
        }

        private decimal oppaiStars(string beatmapPath)
        {
            (decimal stars, decimal aim, decimal speed) = oppai(beatmapPath);
            return stars;
        }
        private (decimal, decimal, decimal) oppai(string beatmapPath)
        {
            return (0, 0, 0);
        }
    }
}
