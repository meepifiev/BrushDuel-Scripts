using System;

namespace _Project.Core.Scoring
{
    public class AccuracyMap
    {
        public AccuracyMap(AnalysisGrid grid)
        {
            Grid = grid ?? throw new ArgumentNullException(nameof(grid));
            Verdicts = new CellVerdict[grid.CellCount];
        }

        public AnalysisGrid Grid { get; }
        public CellVerdict[] Verdicts { get; }
        public int CorrectCount { get; private set; }
        public int WrongColorCount { get; private set; }
        public int OverpaintCount { get; private set; }
        public int MissedCount { get; private set; }

        public int PaintedCount => CorrectCount + WrongColorCount + OverpaintCount;

        public void RefreshCounters()
        {
            CorrectCount = 0;
            WrongColorCount = 0;
            OverpaintCount = 0;
            MissedCount = 0;

            for (int i = 0; i < Verdicts.Length; i++)
            {
                switch (Verdicts[i])
                {
                    case CellVerdict.Correct:
                        CorrectCount++;
                        break;
                    case CellVerdict.WrongColor:
                        WrongColorCount++;
                        break;
                    case CellVerdict.Overpaint:
                        OverpaintCount++;
                        break;
                    case CellVerdict.Missed:
                        MissedCount++;
                        break;
                }
            }
        }
    }
}
