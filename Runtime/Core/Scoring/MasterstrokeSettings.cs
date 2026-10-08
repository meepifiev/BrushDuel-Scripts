using System;

namespace _Project.Core.Scoring
{
    public class MasterstrokeSettings
    {
        public MasterstrokeSettings(
            float pixelExponent,
            float edgeExponent,
            int silhouetteToleranceCells,
            float edgeThetaCells,
            float wrongColorPenalty,
            float overpaintPenalty,
            float missedPenalty)
        {
            if (pixelExponent + edgeExponent <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(pixelExponent));
            }

            PixelExponent = pixelExponent;
            EdgeExponent = edgeExponent;
            SilhouetteToleranceCells = silhouetteToleranceCells;
            EdgeThetaCells = edgeThetaCells;
            WrongColorPenalty = wrongColorPenalty;
            OverpaintPenalty = overpaintPenalty;
            MissedPenalty = missedPenalty;
        }

        public float PixelExponent { get; }
        public float EdgeExponent { get; }
        public int SilhouetteToleranceCells { get; }
        public float EdgeThetaCells { get; }
        public float WrongColorPenalty { get; }
        public float OverpaintPenalty { get; }
        public float MissedPenalty { get; }
    }
}
