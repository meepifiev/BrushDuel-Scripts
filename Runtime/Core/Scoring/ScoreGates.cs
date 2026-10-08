using System;

namespace _Project.Core.Scoring
{
    public class ScoreGates
    {
        public const float ComponentFloor = 0.02f;

        private const float CoverageGateStart = 0.05f;
        private const float CoverageGateFull = 0.25f;
        private const float EffortBudgetFactor = 1.75f;
        private const float EffortOverusePenaltySlope = 0.5f;

        public float MeasureCoverage(ImageSnapshot reference, ImageSnapshot drawing)
        {
            if (reference.SilhouetteArea == 0)
            {
                return drawing.SilhouetteArea == 0 ? 1f : 0f;
            }

            return (float)drawing.SilhouetteArea / reference.SilhouetteArea;
        }

        public float MeasureCoverageGate(ImageSnapshot reference, ImageSnapshot drawing)
        {
            return Smoothstep(MeasureCoverage(reference, drawing), CoverageGateStart, CoverageGateFull);
        }

        public float MeasureEffortGate(ImageSnapshot reference, ImageSnapshot drawing)
        {
            if (reference.SilhouetteArea == 0)
            {
                return 1f;
            }

            float effort = drawing.SilhouetteArea / (EffortBudgetFactor * reference.SilhouetteArea);

            if (effort <= 1f)
            {
                return 1f;
            }

            return 1f / (1f + EffortOverusePenaltySlope * (effort - 1f));
        }

        public float Smoothstep(float value, float from, float to)
        {
            float t = Math.Clamp((value - from) / (to - from), 0f, 1f);
            return t * t * (3f - 2f * t);
        }
    }
}
