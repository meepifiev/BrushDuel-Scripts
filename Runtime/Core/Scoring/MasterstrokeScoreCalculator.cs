using System;
using _Project.Core.Drawing;

namespace _Project.Core.Scoring
{
    public class MasterstrokeScoreCalculator : IScoreCalculator
    {
        private readonly SilhouetteMetric _silhouetteMetric;
        private readonly EdgeMetric _edgeMetric;
        private readonly AccuracyAnalyzer _analyzer;
        private readonly ScoreGates _gates;
        private readonly MasterstrokeSettings _settings;

        private AccuracyMap _map;

        public MasterstrokeScoreCalculator(MasterstrokeSettings settings, ScoreGates gates, PaletteColor[] palette)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _gates = gates ?? throw new ArgumentNullException(nameof(gates));
            _silhouetteMetric = new SilhouetteMetric(settings.SilhouetteToleranceCells);
            _edgeMetric = new EdgeMetric(settings.EdgeThetaCells);
            _analyzer = new AccuracyAnalyzer(palette);
        }

        public ScoreResult Calculate(ImageSnapshot reference, ImageSnapshot drawing)
        {
            if (_map == null || _map.Grid != reference.Grid)
            {
                _map = new AccuracyMap(reference.Grid);
            }

            _analyzer.Measure(reference, drawing, _map);

            float pixel = MeasurePixelTerm(_map);
            float edge = _edgeMetric.Measure(reference, drawing);
            float silhouette = _silhouetteMetric.Measure(reference, drawing);

            float exponentSum = _settings.PixelExponent + _settings.EdgeExponent;
            float geometricMean =
                MathF.Pow(Math.Max(pixel, ScoreGates.ComponentFloor), _settings.PixelExponent / exponentSum)
                * MathF.Pow(Math.Max(edge, ScoreGates.ComponentFloor), _settings.EdgeExponent / exponentSum);

            float coverageGate = _gates.MeasureCoverageGate(reference, drawing);
            float effortGate = _gates.MeasureEffortGate(reference, drawing);
            float total = Math.Clamp(geometricMean * coverageGate * effortGate, 0f, 1f);

            return new ScoreResult(total, silhouette, edge, pixel, _gates.MeasureCoverage(reference, drawing));
        }

        private float MeasurePixelTerm(AccuracyMap map)
        {
            float correct = map.CorrectCount;
            float weightedErrors =
                map.MissedCount * _settings.MissedPenalty
                + map.WrongColorCount * _settings.WrongColorPenalty
                + map.OverpaintCount * _settings.OverpaintPenalty;
            float denominator = correct + weightedErrors;

            if (denominator <= 0f)
            {
                return 0f;
            }

            return correct / denominator;
        }
    }
}
