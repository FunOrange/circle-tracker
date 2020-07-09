using Google.Apis.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace Circle_Tracker
{
    class DifficultyCalculator
    {
        static List<(decimal, decimal)> ApproachRateTableHR = new List<(decimal, decimal)> {
            (0M, 0M),
            (1M, 1.4M),
            (2M, 2.8M),
            (3M, 4.2M),
            (4M, 5.6M),
            (5M, 7M),
            (6M, 8.4M),
            (7M, 9.8M),
            (8M, 10M),
            (9M, 10M),
            (10M, 10M)
        };
        static List<(decimal, decimal)> ApproachRateTableDT = new List<(decimal, decimal)> {
            (0M, 5M),
            (1M, 5.4M),
            (2M, 6.07M),
            (3M, 6.6M),
            (4M, 7.13M),
            (5M, 7.67M),
            (6M, 8.33M),
            (7M, 9M),
            (8M, 9.67M),
            (9M, 10.33M),
            (10M, 11M)
        };
        static List<(decimal, decimal)> ApproachRateTableDTHR = new List<(decimal, decimal)> {
            (0M, 5M),
            (1M, 5.747M),
            (2M, 6.493M),
            (3M, 7.24M),
            (4M, 8.07M),
            (5M, 9M),
            (6M, 9.93M),
            (7M, 10.87M),
            (8M, 11M),
            (9M, 11M),
            (10M, 11M)
        };

        static List<(decimal, decimal)> OverallDifficultyTableHR = new List<(decimal, decimal)> {
            (0M, 0M),
            (1M, 1.4M),
            (2M, 2.8M),
            (3M, 4.2M),
            (4M, 5.6M),
            (5M, 7M),
            (6M, 8.4M),
            (7M, 9.8M),
            (8M, 10M),
            (9M, 10M),
            (10M,10M)
        };
        static List<(decimal, decimal)> OverallDifficultyTableDT = new List<(decimal, decimal)> {
            (0M, 4.42M),
            (1M, 5.08M),
            (2M, 5.75M),
            (3M, 6.42M),
            (4M, 7.08M),
            (5M, 7.75M),
            (6M, 8.42M),
            (7M, 9.08M),
            (8M, 9.75M),
            (9M, 10.42M),
            (10M,11.08M)
        };
        static List<(decimal, decimal)> OverallDifficultyTableDTHR = new List<(decimal, decimal)> {
            (0M, 4.42M),
            (1M, 5.42M),
            (2M, 6.31M),
            (3M, 7.31M),
            (4M, 8.19M),
            (5M, 9.08M),
            (6M, 10.08M),
            (7M, 10.97M),
            (8M, 11.08M),
            (9M, 11.08M),
            (10M,11.08M)
        };

        public static decimal CalculateARWithHR(decimal od)   => LerpValueUsingLUT(od, ApproachRateTableHR);
        public static decimal CalculateARWithDT(decimal od)   => LerpValueUsingLUT(od, ApproachRateTableDT);
        public static decimal CalculateARWithDTHR(decimal od) => LerpValueUsingLUT(od, ApproachRateTableDTHR);

        public static decimal CalculateODWithHR(decimal od)   => LerpValueUsingLUT(od, OverallDifficultyTableHR);
        public static decimal CalculateODWithDT(decimal od)   => LerpValueUsingLUT(od, OverallDifficultyTableDT);
        public static decimal CalculateODWithDTHR(decimal od) => LerpValueUsingLUT(od, OverallDifficultyTableDTHR);

        private static decimal LerpValueUsingLUT(decimal input, List<(decimal, decimal)> lut) {
            if (input < 0M || input > 10M)
                return 0M;

            int lowerIndex = (int)input;
            int upperIndex = Math.Min(lowerIndex + 1, 10); // clamp to 10

            decimal lowerValue = lut[lowerIndex].Item2;
            decimal upperValue = lut[upperIndex].Item2;
            decimal stepDifference = upperValue - lowerValue;
            decimal stepFraction = input - (int)input; // decimal part
            Console.WriteLine($"stepFraction: {stepFraction}");


            return lowerValue + stepFraction * stepDifference;
        }
    }
}
