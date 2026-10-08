using System;

namespace _Project.Core.Scoring
{
    public class DistanceTransform
    {
        public const int StraightCost = 3;
        public const int DiagonalCost = 4;
        public const int Infinity = int.MaxValue / 2;

        public void Compute(bool[] sources, AnalysisGrid grid, int[] distances)
        {
            int width = grid.Width;
            int height = grid.Height;

            for (int i = 0; i < grid.CellCount; i++)
            {
                distances[i] = sources[i] ? 0 : Infinity;
            }

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int cell = y * width + x;
                    int best = distances[cell];

                    if (x > 0)
                    {
                        best = Math.Min(best, distances[cell - 1] + StraightCost);
                    }

                    if (y > 0)
                    {
                        best = Math.Min(best, distances[cell - width] + StraightCost);

                        if (x > 0)
                        {
                            best = Math.Min(best, distances[cell - width - 1] + DiagonalCost);
                        }

                        if (x + 1 < width)
                        {
                            best = Math.Min(best, distances[cell - width + 1] + DiagonalCost);
                        }
                    }

                    distances[cell] = best;
                }
            }

            for (int y = height - 1; y >= 0; y--)
            {
                for (int x = width - 1; x >= 0; x--)
                {
                    int cell = y * width + x;
                    int best = distances[cell];

                    if (x + 1 < width)
                    {
                        best = Math.Min(best, distances[cell + 1] + StraightCost);
                    }

                    if (y + 1 < height)
                    {
                        best = Math.Min(best, distances[cell + width] + StraightCost);

                        if (x + 1 < width)
                        {
                            best = Math.Min(best, distances[cell + width + 1] + DiagonalCost);
                        }

                        if (x > 0)
                        {
                            best = Math.Min(best, distances[cell + width - 1] + DiagonalCost);
                        }
                    }

                    distances[cell] = best;
                }
            }
        }
    }
}
