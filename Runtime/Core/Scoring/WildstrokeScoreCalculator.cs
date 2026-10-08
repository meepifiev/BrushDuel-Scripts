using System;

namespace _Project.Core.Scoring
{
    public class WildstrokeScoreCalculator : IScoreCalculator
    {
        private const float EdgeWeight = 0.5f;

        private readonly SilhouetteMetric _silhouetteMetric;
        private readonly EdgeMetric _edgeMetric;
        private readonly ColorHistogramMetric _histogramMetric;
        private readonly PixelAccuracyMetric _pixelAccuracyMetric;
        private readonly ScoreGates _gates;
        private readonly WildstrokeSettings _settings;

        public WildstrokeScoreCalculator(WildstrokeSettings settings, ScoreGates gates)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _gates = gates ?? throw new ArgumentNullException(nameof(gates));
            _silhouetteMetric = new SilhouetteMetric(settings.SilhouetteToleranceCells);
            _edgeMetric = new EdgeMetric(settings.EdgeThetaCells);
            _histogramMetric = new ColorHistogramMetric();
            _pixelAccuracyMetric = new PixelAccuracyMetric();
        }

        public ScoreResult Calculate(ImageSnapshot reference, ImageSnapshot drawing)
        {
            float silhouette = _silhouetteMetric.Measure(reference, drawing);
            float color = _histogramMetric.Measure(reference, drawing);
            float edge = _edgeMetric.Measure(reference, drawing);
            float pixelAccuracy = _pixelAccuracyMetric.Measure(reference, drawing);

            float recognizability = MathF.Sqrt(
                Math.Max(silhouette, ScoreGates.ComponentFloor) * Math.Max(color, ScoreGates.ComponentFloor));
            float precisionFine = EdgeWeight * edge + (1f - EdgeWeight) * pixelAccuracy;
            float window = MeasureWindow(precisionFine);

            float coverageGate = _gates.MeasureCoverageGate(reference, drawing);
            float effortGate = _gates.MeasureEffortGate(reference, drawing);
            float total = Math.Clamp(recognizability * window * coverageGate * effortGate, 0f, 1f);

            return new ScoreResult(total, silhouette, precisionFine, color, _gates.MeasureCoverage(reference, drawing));
        }

        private float MeasureWindow(float precision)
        {
            if (precision < _settings.WindowComfortMin)
            {
                return _gates.Smoothstep(precision, _settings.WindowLow, _settings.WindowComfortMin);
            }

            if (precision <= _settings.WindowComfortMax)
            {
                return 1f;
            }

            return 1f - _settings.OverAccuracyPenalty * _gates.Smoothstep(precision, _settings.WindowComfortMax, _settings.WindowHigh);
        }
    }
}
