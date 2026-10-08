using _Project.Core.Drawing;

namespace _Project.Core.Scoring
{
    public class PixelAccuracyMetric
    {
        public float Measure(ImageSnapshot reference, ImageSnapshot drawing)
        {
            int meaningful = 0;
            int matched = 0;

            for (int i = 0; i < reference.Grid.CellCount; i++)
            {
                bool referenceBackground = reference.PaletteIndices[i] == CanvasSpec.BackgroundColorIndex;
                bool drawingBackground = drawing.PaletteIndices[i] == CanvasSpec.BackgroundColorIndex;

                if (referenceBackground && drawingBackground)
                {
                    continue;
                }

                meaningful++;

                if (reference.PaletteIndices[i] == drawing.PaletteIndices[i])
                {
                    matched++;
                }
            }

            if (meaningful == 0)
            {
                return 0f;
            }

            return (float)matched / meaningful;
        }
    }
}
