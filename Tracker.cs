using OsuMemoryDataProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace circle_tracker
{
    class Tracker
    {
        private readonly IOsuMemoryReader osuReader;

        public Tracker()
        {
            osuReader = OsuMemoryReader.Instance.GetInstanceForWindowTitleHint("");
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
