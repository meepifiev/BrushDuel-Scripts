using System;
using _Project.Core.Drawing;

namespace _Project.Core.Scoring
{
    public class ColorMetric
    {
        private readonly PaletteAreaCounter _counter = new PaletteAreaCounter();

        public float Measure(ImageSnapshot reference, ImageSnapshot drawing)
        {
            _counter.Count(reference, drawing);

            float weightedSum = 0f;
            float weightTotal = 0f;

            for (int color = 0; color < _counter.PaletteSize; color++)
            {
                if (color == CanvasSpec.BackgroundColorIndex)
                {
                    continue;
                }

                int referenceArea = _counter.ReferenceAreas[color];

                if (referenceArea == 0)
                {
                    continue;
                }

                int intersection = _counter.Intersections[color];
                int union = referenceArea + _counter.DrawingAreas[color] - intersection;
                float iou = (float)intersection / union;
                float weight = MathF.Sqrt(referenceArea);
                weightedSum += iou * weight;
                weightTotal += weight;
            }

            if (weightTotal <= 0f)
            {
                return 0f;
            }

            return weightedSum / weightTotal;
        }
    }
}
