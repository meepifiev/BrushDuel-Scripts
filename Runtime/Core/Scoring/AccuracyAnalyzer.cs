using System;
using _Project.Core.Drawing;

namespace _Project.Core.Scoring
{
    public class AccuracyAnalyzer
    {
        private const float SimilarityThreshold = 55f;
        private const int SpatialToleranceCells = 2;

        private readonly bool[,] _similar;

        public AccuracyAnalyzer(PaletteColor[] palette)
        {
            if (palette == null)
            {
                throw new ArgumentNullException(nameof(palette));
            }

            _similar = BuildSimilarityMatrix(palette);
        }

        public void Measure(ImageSnapshot reference, ImageSnapshot drawing, AccuracyMap map)
        {
            if (reference == null)
            {
                throw new ArgumentNullException(nameof(reference));
            }

            if (drawing == null)
            {
                throw new ArgumentNullException(nameof(drawing));
            }

            if (map == null)
            {
                throw new ArgumentNullException(nameof(map));
            }

            AnalysisGrid grid = map.Grid;

            for (int y = 0; y < grid.Height; y++)
            {
                for (int x = 0; x < grid.Width; x++)
                {
                    int cell = y * grid.Width + x;
                    map.Verdicts[cell] = Judge(reference, drawing, grid, x, y, cell);
                }
            }

            map.RefreshCounters();
        }

        private static bool[,] BuildSimilarityMatrix(PaletteColor[] palette)
        {
            bool[,] similar = new bool[palette.Length, palette.Length];

            for (int a = 0; a < palette.Length; a++)
            {
                for (int b = 0; b < palette.Length; b++)
                {
                    similar[a, b] = a == b || Distance(palette[a], palette[b]) <= SimilarityThreshold;
                }
            }

            return similar;
        }

        private static float Distance(PaletteColor a, PaletteColor b)
        {
            float deltaR = a.R - b.R;
            float deltaG = a.G - b.G;
            float deltaB = a.B - b.B;
            return MathF.Sqrt(2f * deltaR * deltaR + 4f * deltaG * deltaG + 3f * deltaB * deltaB);
        }

        private bool ColorsMatch(int a, int b)
        {
            return _similar[a, b];
        }

        private CellVerdict Judge(ImageSnapshot reference, ImageSnapshot drawing, AnalysisGrid grid, int x, int y, int cell)
        {
            bool referencePainted = reference.Silhouette[cell];
            bool drawingPainted = drawing.Silhouette[cell];

            if (referencePainted == false && drawingPainted == false)
            {
                return CellVerdict.Empty;
            }

            if (drawingPainted == false)
            {
                if (HasMatchingColorNearby(drawing, grid, x, y, reference.PaletteIndices[cell]))
                {
                    return CellVerdict.Correct;
                }

                return CellVerdict.Missed;
            }

            int drawnColor = drawing.PaletteIndices[cell];

            if (referencePainted && ColorsMatch(reference.PaletteIndices[cell], drawnColor))
            {
                return CellVerdict.Correct;
            }

            if (HasMatchingColorNearby(reference, grid, x, y, drawnColor))
            {
                return CellVerdict.Correct;
            }

            if (referencePainted || HasPaintNearby(reference, grid, x, y))
            {
                return CellVerdict.WrongColor;
            }

            return CellVerdict.Overpaint;
        }

        private bool HasMatchingColorNearby(ImageSnapshot snapshot, AnalysisGrid grid, int x, int y, int color)
        {
            int minX = Math.Max(0, x - SpatialToleranceCells);
            int maxX = Math.Min(grid.Width - 1, x + SpatialToleranceCells);
            int minY = Math.Max(0, y - SpatialToleranceCells);
            int maxY = Math.Min(grid.Height - 1, y + SpatialToleranceCells);

            for (int ny = minY; ny <= maxY; ny++)
            {
                for (int nx = minX; nx <= maxX; nx++)
                {
                    int cell = ny * grid.Width + nx;

                    if (snapshot.Silhouette[cell] && snapshot.PaletteIndices[cell] == color)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private bool HasPaintNearby(ImageSnapshot snapshot, AnalysisGrid grid, int x, int y)
        {
            int minX = Math.Max(0, x - SpatialToleranceCells);
            int maxX = Math.Min(grid.Width - 1, x + SpatialToleranceCells);
            int minY = Math.Max(0, y - SpatialToleranceCells);
            int maxY = Math.Min(grid.Height - 1, y + SpatialToleranceCells);

            for (int ny = minY; ny <= maxY; ny++)
            {
                for (int nx = minX; nx <= maxX; nx++)
                {
                    if (snapshot.Silhouette[ny * grid.Width + nx])
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
