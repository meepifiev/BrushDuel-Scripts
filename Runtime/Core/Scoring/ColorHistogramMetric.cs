using System;
using _Project.Core.Drawing;

namespace _Project.Core.Scoring
{
    public class ColorHistogramMetric
    {
        private readonly PaletteAreaCounter _counter = new PaletteAreaCounter();

        public float Measure(ImageSnapshot reference, ImageSnapshot drawing)
        {
            _counter.Count(reference, drawing);

            int intersection = 0;
            int referenceTotal = 0;

            for (int color = 0; color < _counter.PaletteSize; color++)
            {
                if (color == CanvasSpec.BackgroundColorIndex)
                {
                    continue;
                }

                intersection += Math.Min(_counter.ReferenceAreas[color], _counter.DrawingAreas[color]);
                referenceTotal += _counter.ReferenceAreas[color];
            }

            if (referenceTotal == 0)
            {
                return 0f;
            }

            return (float)intersection / referenceTotal;
        }
    }
}
