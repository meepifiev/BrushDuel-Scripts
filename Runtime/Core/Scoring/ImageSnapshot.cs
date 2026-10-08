using System;

namespace _Project.Core.Scoring
{
    public class ImageSnapshot
    {
        public ImageSnapshot(AnalysisGrid grid)
        {
            Grid = grid ?? throw new ArgumentNullException(nameof(grid));
            PaletteIndices = new int[grid.CellCount];
            Silhouette = new bool[grid.CellCount];
            Edges = new bool[grid.CellCount];
        }

        public AnalysisGrid Grid { get; }
        public int[] PaletteIndices { get; }
        public bool[] Silhouette { get; }
        public bool[] Edges { get; }
        public int SilhouetteArea { get; private set; }
        public int EdgeCount { get; private set; }

        public void RefreshCounters()
        {
            int silhouetteArea = 0;
            int edgeCount = 0;

            for (int i = 0; i < Grid.CellCount; i++)
            {
                if (Silhouette[i])
                {
                    silhouetteArea++;
                }

                if (Edges[i])
                {
                    edgeCount++;
                }
            }

            SilhouetteArea = silhouetteArea;
            EdgeCount = edgeCount;
        }
    }
}
