namespace _Project.Core.Scoring
{
    public class WildstrokeSettings
    {
        public WildstrokeSettings(
            int silhouetteToleranceCells,
            float edgeThetaCells,
            float windowLow,
            float windowComfortMin,
            float windowComfortMax,
            float windowHigh,
            float overAccuracyPenalty)
        {
            SilhouetteToleranceCells = silhouetteToleranceCells;
            EdgeThetaCells = edgeThetaCells;
            WindowLow = windowLow;
            WindowComfortMin = windowComfortMin;
            WindowComfortMax = windowComfortMax;
            WindowHigh = windowHigh;
            OverAccuracyPenalty = overAccuracyPenalty;
        }

        public int SilhouetteToleranceCells { get; }
        public float EdgeThetaCells { get; }
        public float WindowLow { get; }
        public float WindowComfortMin { get; }
        public float WindowComfortMax { get; }
        public float WindowHigh { get; }
        public float OverAccuracyPenalty { get; }
    }
}
