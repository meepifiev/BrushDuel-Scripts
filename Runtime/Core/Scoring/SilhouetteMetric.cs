using System;

namespace _Project.Core.Scoring
{
    public class SilhouetteMetric
    {
        private const float DiceWeight = 0.5f;

        private readonly int _toleranceCells;
        private readonly DistanceTransform _distanceTransform = new DistanceTransform();

        private ImageSnapshot _cachedReference;
        private int _referenceBoundaryCount;
        private bool[] _referenceBoundary;
        private bool[] _drawingBoundary;
        private int[] _referenceDistances;
        private int[] _drawingDistances;

        public SilhouetteMetric(int toleranceCells)
        {
            if (toleranceCells < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(toleranceCells));
            }

            _toleranceCells = toleranceCells;
        }

        public float Measure(ImageSnapshot reference, ImageSnapshot drawing)
        {
            if (drawing.SilhouetteArea == 0 || reference.SilhouetteArea == 0)
            {
                return 0f;
            }

            int intersection = 0;

            for (int i = 0; i < reference.Grid.CellCount; i++)
            {
                if (reference.Silhouette[i] && drawing.Silhouette[i])
                {
                    intersection++;
                }
            }

            float dice = 2f * intersection / (reference.SilhouetteArea + drawing.SilhouetteArea);
            float surfaceDistance = MeasureSurfaceDistance(reference, drawing);

            return DiceWeight * dice + (1f - DiceWeight) * surfaceDistance;
        }

        private float MeasureSurfaceDistance(ImageSnapshot reference, ImageSnapshot drawing)
        {
            AnalysisGrid grid = reference.Grid;
            EnsureBuffers(grid.CellCount);
            PrepareReference(reference);
            MarkBoundary(drawing, _drawingBoundary);

            int drawingBoundaryCount = CountMarked(_drawingBoundary);
            _distanceTransform.Compute(_drawingBoundary, grid, _drawingDistances);

            int toleranceDistance = _toleranceCells * DistanceTransform.StraightCost;
            int withinTolerance = 0;

            for (int i = 0; i < grid.CellCount; i++)
            {
                if (_drawingBoundary[i] && _referenceDistances[i] <= toleranceDistance)
                {
                    withinTolerance++;
                }

                if (_referenceBoundary[i] && _drawingDistances[i] <= toleranceDistance)
                {
                    withinTolerance++;
                }
            }

            return (float)withinTolerance / (_referenceBoundaryCount + drawingBoundaryCount);
        }

        private void PrepareReference(ImageSnapshot reference)
        {
            if (ReferenceEquals(_cachedReference, reference))
            {
                return;
            }

            MarkBoundary(reference, _referenceBoundary);
            _referenceBoundaryCount = CountMarked(_referenceBoundary);
            _distanceTransform.Compute(_referenceBoundary, reference.Grid, _referenceDistances);
            _cachedReference = reference;
        }

        private void MarkBoundary(ImageSnapshot snapshot, bool[] target)
        {
            AnalysisGrid grid = snapshot.Grid;

            for (int y = 0; y < grid.Height; y++)
            {
                for (int x = 0; x < grid.Width; x++)
                {
                    int cell = y * grid.Width + x;

                    if (snapshot.Silhouette[cell] == false)
                    {
                        target[cell] = false;
                        continue;
                    }

                    bool hasOutsideNeighbour =
                        x == 0 || snapshot.Silhouette[cell - 1] == false
                        || x + 1 == grid.Width || snapshot.Silhouette[cell + 1] == false
                        || y == 0 || snapshot.Silhouette[cell - grid.Width] == false
                        || y + 1 == grid.Height || snapshot.Silhouette[cell + grid.Width] == false;

                    target[cell] = hasOutsideNeighbour;
                }
            }
        }

        private int CountMarked(bool[] cells)
        {
            int count = 0;

            for (int i = 0; i < cells.Length; i++)
            {
                if (cells[i])
                {
                    count++;
                }
            }

            return count;
        }

        private void EnsureBuffers(int cellCount)
        {
            if (_referenceBoundary != null && _referenceBoundary.Length == cellCount)
            {
                return;
            }

            _referenceBoundary = new bool[cellCount];
            _drawingBoundary = new bool[cellCount];
            _referenceDistances = new int[cellCount];
            _drawingDistances = new int[cellCount];
            _cachedReference = null;
        }
    }
}
