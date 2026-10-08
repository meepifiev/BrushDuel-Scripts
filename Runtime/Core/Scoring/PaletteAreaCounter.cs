namespace _Project.Core.Scoring
{
    public class PaletteAreaCounter
    {
        public int PaletteSize { get; private set; }
        public int[] ReferenceAreas { get; private set; }
        public int[] DrawingAreas { get; private set; }
        public int[] Intersections { get; private set; }

        public void Count(ImageSnapshot reference, ImageSnapshot drawing)
        {
            PaletteSize = FindPaletteSize(reference, drawing);
            EnsureBuffers(PaletteSize);

            for (int color = 0; color < PaletteSize; color++)
            {
                ReferenceAreas[color] = 0;
                DrawingAreas[color] = 0;
                Intersections[color] = 0;
            }

            for (int i = 0; i < reference.Grid.CellCount; i++)
            {
                int referenceColor = reference.PaletteIndices[i];
                int drawingColor = drawing.PaletteIndices[i];
                ReferenceAreas[referenceColor]++;
                DrawingAreas[drawingColor]++;

                if (referenceColor == drawingColor)
                {
                    Intersections[referenceColor]++;
                }
            }
        }

        private int FindPaletteSize(ImageSnapshot reference, ImageSnapshot drawing)
        {
            int maxIndex = 0;

            for (int i = 0; i < reference.Grid.CellCount; i++)
            {
                if (reference.PaletteIndices[i] > maxIndex)
                {
                    maxIndex = reference.PaletteIndices[i];
                }

                if (drawing.PaletteIndices[i] > maxIndex)
                {
                    maxIndex = drawing.PaletteIndices[i];
                }
            }

            return maxIndex + 1;
        }

        private void EnsureBuffers(int paletteSize)
        {
            if (ReferenceAreas != null && ReferenceAreas.Length >= paletteSize)
            {
                return;
            }

            ReferenceAreas = new int[paletteSize];
            DrawingAreas = new int[paletteSize];
            Intersections = new int[paletteSize];
        }
    }
}
