using Google.Apis.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Circle_Tracker
{
    class DifficultyCalculator
    {
        static List<(decimal, decimal)> ApproachRateTableHTEZ = new List<(decimal, decimal)> {
            (0M, -5M),
            (1M, -4.33M),
            (2M, -3.67M),
            (3M, -3M),
            (4M, -2.33M),
            (5M, -1.67M),
            (6M, -1M),
            (7M, -0.33M),
            (8M, 0.33M),
            (9M, 1M),
            (10M,1.67M)
        };
        static List<(decimal, decimal)> ApproachRateTableHT = new List<(decimal, decimal)> {
            (0M, -5M),
            (1M, -3.67M),
            (2M, -2.33M),
            (3M, -1M),
            (4M, 0.33M),
            (5M, 1.67M),
            (6M, 3.33M),
            (7M, 5M),
            (8M, 6.33M),
            (9M, 7.67M),
            (10M,9M)
        };
        static List<(decimal, decimal)> ApproachRateTableHTHR = new List<(decimal, decimal)> {
            (0M, -5M),
            (1M, -3.13M),
            (2M, -1.27M),
            (3M, 0.6M),
            (4M, 2.67M),
            (5M, 5M),
            (6M, 6.87M),
            (7M, 8.73M),
            (8M, 9M),
            (9M, 9M),
            (10M,9M)
        };
        static List<(decimal, decimal)> ApproachRateTableEZ = new List<(decimal, decimal)> {
            (0M, 0M),
            (1M, 0.5M),
            (2M, 1M),
            (3M, 1.5M),
            (4M, 2M),
            (5M, 2.5M),
            (6M, 3M),
            (7M, 3.5M),
            (8M, 4M),
            (9M, 4.5M),
            (10M,5M)
        };
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
        static List<(decimal, decimal)> ApproachRateTableDTEZ = new List<(decimal, decimal)> {
            (0M, 5M),
            (1M, 5.27M),
            (2M, 5.53M),
            (3M, 5.8M),
            (4M, 6.07M),
            (5M, 6.33M),
            (6M, 6.6M),
            (7M, 6.87M),
            (8M, 7.13M),
            (9M, 7.4M),
            (10M,7.67M)
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

        static List<(decimal, decimal)> OverallDifficultyTableHTEZ = new List<(decimal, decimal)> {
            (0M, -4.42M),
            (1M, -3.75M),
            (2M, -3.08M),
            (3M, -2.42M),
            (4M, -1.75M),
            (5M, -1.08M),
            (6M, -0.42M),
            (7M, 0.25M),
            (8M, 0.92M),
            (9M, 1.54M),
            (10M, 2.25M)
        };
        static List<(decimal, decimal)> OverallDifficultyTableHT = new List<(decimal, decimal)> {
            (0M, -4.42M),
            (1M, -3.08M),
            (2M, -1.75M),
            (3M, -0.42M),
            (4M, 0.92M),
            (5M, 2.25M),
            (6M, 3.54M),
            (7M, 4.92M),
            (8M, 6.25M),
            (9M, 7.54M),
            (10M, 8.92M)
        };
        static List<(decimal, decimal)> OverallDifficultyTableHTHR = new List<(decimal, decimal)> {
            (0M, 4.42M),
            (1M, 2.42M),
            (2M, 0.64M),
            (3M, 1.36M),
            (4M, 3.14M),
            (5M, 4.92M),
            (6M, 6.92M),
            (7M, 8.69M),
            (8M, 8.92M),
            (9M, 8.92M),
            (10M, 8.92M),
        };
        static List<(decimal, decimal)> OverallDifficultyTableEZ = new List<(decimal, decimal)> {
            (0M, 0M),
            (1M, 0.5M),
            (2M, 1M),
            (3M, 1.5M),
            (4M, 2M),
            (5M, 2.5M),
            (6M, 3M),
            (7M, 3.5M),
            (8M, 4M),
            (9M, 4.5M),
            (10M, 5M),
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
        static List<(decimal, decimal)> OverallDifficultyTableDTEZ = new List<(decimal, decimal)> {
            (0M, 4.42M),
            (1M, 4.75M),
            (2M, 5.08M),
            (3M, 5.42M),
            (4M, 5.75M),
            (5M, 6.08M),
            (6M, 6.42M),
            (7M, 6.75M),
            (8M, 7.08M),
            (9M, 7.42M),
            (10M, 7.75M)
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

        public static decimal CalculateARWithHTEZ(decimal od)   => LerpValueUsingLUT(od, ApproachRateTableHTEZ);
        public static decimal CalculateARWithHT(decimal od)   => LerpValueUsingLUT(od, ApproachRateTableHT);
        public static decimal CalculateARWithHTHR(decimal od)   => LerpValueUsingLUT(od, ApproachRateTableHTHR);
        public static decimal CalculateARWithEZ(decimal od)   => LerpValueUsingLUT(od, ApproachRateTableEZ);
        public static decimal CalculateARWithHR(decimal od)   => LerpValueUsingLUT(od, ApproachRateTableHR);
        public static decimal CalculateARWithDT(decimal od)   => LerpValueUsingLUT(od, ApproachRateTableDT);
        public static decimal CalculateARWithDTEZ(decimal od)   => LerpValueUsingLUT(od, ApproachRateTableDTEZ);
        public static decimal CalculateARWithDTHR(decimal od) => LerpValueUsingLUT(od, ApproachRateTableDTHR);

        public static decimal CalculateODWithHTEZ(decimal od)   => LerpValueUsingLUT(od, OverallDifficultyTableHTEZ);
        public static decimal CalculateODWithHT(decimal od)   => LerpValueUsingLUT(od, OverallDifficultyTableHT);
        public static decimal CalculateODWithHTHR(decimal od)   => LerpValueUsingLUT(od, OverallDifficultyTableHTHR);
        public static decimal CalculateODWithEZ(decimal od)   => LerpValueUsingLUT(od, OverallDifficultyTableEZ);
        public static decimal CalculateODWithHR(decimal od)   => LerpValueUsingLUT(od, OverallDifficultyTableHR);
        public static decimal CalculateODWithDT(decimal od)   => LerpValueUsingLUT(od, OverallDifficultyTableDT);
        public static decimal CalculateODWithDTEZ(decimal od)   => LerpValueUsingLUT(od, OverallDifficultyTableDTEZ);
        public static decimal CalculateODWithDTHR(decimal od) => LerpValueUsingLUT(od, OverallDifficultyTableDTHR);

        private static decimal LerpValueUsingLUT(decimal input, List<(decimal, decimal)> lut) {
            if (input < 0M || input > 10M)
                return 0M;

            var values = lut.Select(indexValuePair => indexValuePair.Item2);
            decimal maxValue   = values.Max();

            int lowerIndex     = (int)input;
            int upperIndex     = Math.Min(lowerIndex + 1, 10); // clamp to 10

            decimal lowerValue = lut[lowerIndex].Item2;
            decimal upperValue = lut[upperIndex].Item2;
            decimal stepDifference = upperValue - lowerValue;
            decimal stepFraction = input - (int)input; // decimal part

            if (lowerValue != maxValue && upperValue == maxValue)
            {
                // special case: piece-wise linear interval. Starts with a positive slope, then plateaus to max value.
                // Linearly interpolate based on the slope of the previous interval
                decimal prevIntervalLowerValue = lut[lowerIndex - 1].Item2;
                decimal prevIntervalUpperValue = lut[lowerIndex].Item2;
                decimal previousSlope = prevIntervalUpperValue - prevIntervalLowerValue;
                // Clamp the lerped value to maxValue
                decimal lerp = lowerValue + stepFraction * previousSlope;
                return Math.Min(lerp, maxValue);
            }
            else
                return lowerValue + stepFraction * stepDifference;
        }
    }
}
