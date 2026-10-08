using System;

namespace _Project.Core.Scoring
{
    public class EdgeMetric
    {
        private readonly float _thetaCells;
        private readonly DistanceTransform _distanceTransform = new DistanceTransform();

        private ImageSnapshot _cachedReference;
        private int[] _referenceDistances;
        private int[] _drawingDistances;

        public EdgeMetric(float thetaCells)
        {
            if (thetaCells <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(thetaCells));
            }

            _thetaCells = thetaCells;
        }

        public float Measure(ImageSnapshot reference, ImageSnapshot drawing)
        {
            if (reference.EdgeCount == 0 && drawing.EdgeCount == 0)
            {
                return 1f;
            }

            if (reference.EdgeCount == 0 || drawing.EdgeCount == 0)
            {
                return 0f;
            }

            AnalysisGrid grid = reference.Grid;
            EnsureBuffers(grid.CellCount);
            PrepareReference(reference);
            _distanceTransform.Compute(drawing.Edges, grid, _drawingDistances);

            float precision = MeasureCredit(drawing.Edges, _referenceDistances, drawing.EdgeCount, grid);
            float recall = MeasureCredit(reference.Edges, _drawingDistances, reference.EdgeCount, grid);

            if (precision + recall <= 0f)
            {
                return 0f;
            }

            return 2f * precision * recall / (precision + recall);
        }

        private void PrepareReference(ImageSnapshot reference)
        {
            if (ReferenceEquals(_cachedReference, reference))
            {
                return;
            }

            _distanceTransform.Compute(reference.Edges, reference.Grid, _referenceDistances);
            _cachedReference = reference;
        }

        private float MeasureCredit(bool[] edges, int[] distancesToTarget, int edgeCount, AnalysisGrid grid)
        {
            float creditSum = 0f;
            float thetaDistance = _thetaCells * DistanceTransform.StraightCost;

            for (int i = 0; i < grid.CellCount; i++)
            {
                if (edges[i] == false)
                {
                    continue;
                }

                creditSum += Math.Max(0f, 1f - distancesToTarget[i] / thetaDistance);
            }

            return creditSum / edgeCount;
        }

        private void EnsureBuffers(int cellCount)
        {
            if (_referenceDistances != null && _referenceDistances.Length == cellCount)
            {
                return;
            }

            _referenceDistances = new int[cellCount];
            _drawingDistances = new int[cellCount];
            _cachedReference = null;
        }
    }
}
