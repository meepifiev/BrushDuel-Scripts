using System;
using _Project.Core.Drawing;

namespace _Project.Core.Scoring
{
    public class SnapshotBuilder
    {
        private const int MajorityWindowRadius = 1;
        private const int MaxPaletteSize = 32;

        private readonly PaletteQuantizer _quantizer;
        private readonly int[] _votes = new int[MaxPaletteSize];

        private int[] _rawIndices;

        public SnapshotBuilder(PaletteQuantizer quantizer)
        {
            _quantizer = quantizer ?? throw new ArgumentNullException(nameof(quantizer));
        }

        public void Build(ImageSnapshot snapshot, byte[] rgba, int sourceWidth, int sourceHeight, PaletteColor[] palette)
        {
            AnalysisGrid grid = snapshot.Grid;
            EnsureBuffers(grid.CellCount);

            for (int y = 0; y < grid.Height; y++)
            {
                int sourceY = y * sourceHeight / grid.Height;

                for (int x = 0; x < grid.Width; x++)
                {
                    int sourceX = x * sourceWidth / grid.Width;
                    int offset = (sourceY * sourceWidth + sourceX) * CanvasSpec.BytesPerPixel;
                    _rawIndices[y * grid.Width + x] = _quantizer.FindNearestIndex(rgba[offset], rgba[offset + 1], rgba[offset + 2], palette);
                }
            }

            ApplyMajorityFilter(grid, palette.Length, snapshot.PaletteIndices);
            FinishSnapshot(snapshot);
        }

        public void BuildFromIndices(ImageSnapshot snapshot, byte[] indices)
        {
            if (indices == null)
            {
                throw new ArgumentNullException(nameof(indices));
            }

            AnalysisGrid grid = snapshot.Grid;

            if (indices.Length != grid.CellCount)
            {
                throw new ArgumentOutOfRangeException(nameof(indices));
            }

            for (int i = 0; i < grid.CellCount; i++)
            {
                snapshot.PaletteIndices[i] = indices[i];
            }

            FinishSnapshot(snapshot);
        }

        private void FinishSnapshot(ImageSnapshot snapshot)
        {
            AnalysisGrid grid = snapshot.Grid;

            for (int i = 0; i < grid.CellCount; i++)
            {
                snapshot.Silhouette[i] = snapshot.PaletteIndices[i] != CanvasSpec.BackgroundColorIndex;
            }

            MarkEdges(snapshot);
            snapshot.RefreshCounters();
        }

        private void ApplyMajorityFilter(AnalysisGrid grid, int paletteSize, int[] target)
        {
            for (int y = 0; y < grid.Height; y++)
            {
                for (int x = 0; x < grid.Width; x++)
                {
                    for (int i = 0; i < paletteSize; i++)
                    {
                        _votes[i] = 0;
                    }

                    int minX = Math.Max(0, x - MajorityWindowRadius);
                    int maxX = Math.Min(grid.Width - 1, x + MajorityWindowRadius);
                    int minY = Math.Max(0, y - MajorityWindowRadius);
                    int maxY = Math.Min(grid.Height - 1, y + MajorityWindowRadius);

                    for (int ny = minY; ny <= maxY; ny++)
                    {
                        for (int nx = minX; nx <= maxX; nx++)
                        {
                            _votes[_rawIndices[ny * grid.Width + nx]]++;
                        }
                    }

                    int cell = y * grid.Width + x;
                    int ownIndex = _rawIndices[cell];
                    int bestIndex = ownIndex;
                    int bestVotes = _votes[ownIndex];

                    for (int i = 0; i < paletteSize; i++)
                    {
                        if (_votes[i] > bestVotes)
                        {
                            bestVotes = _votes[i];
                            bestIndex = i;
                        }
                    }

                    target[cell] = bestIndex;
                }
            }
        }

        private void MarkEdges(ImageSnapshot snapshot)
        {
            AnalysisGrid grid = snapshot.Grid;

            for (int y = 0; y < grid.Height; y++)
            {
                for (int x = 0; x < grid.Width; x++)
                {
                    int cell = y * grid.Width + x;
                    bool isEdge = false;

                    if (x + 1 < grid.Width && snapshot.PaletteIndices[cell] != snapshot.PaletteIndices[cell + 1])
                    {
                        isEdge = true;
                    }

                    if (y + 1 < grid.Height && snapshot.PaletteIndices[cell] != snapshot.PaletteIndices[cell + grid.Width])
                    {
                        isEdge = true;
                    }

                    snapshot.Edges[cell] = isEdge;
                }
            }
        }

        private void EnsureBuffers(int cellCount)
        {
            if (_rawIndices == null || _rawIndices.Length != cellCount)
            {
                _rawIndices = new int[cellCount];
            }
        }
    }
}
