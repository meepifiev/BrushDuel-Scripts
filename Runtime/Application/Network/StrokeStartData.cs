using _Project.Core.Drawing;

namespace _Project.Application.Network
{
    public readonly struct StrokeStartData
    {
        public StrokeStartData(BrushType brush, int sizeIndex, PaletteColor color)
        {
            Brush = brush;
            SizeIndex = sizeIndex;
            Color = color;
        }

        public BrushType Brush { get; }
        public int SizeIndex { get; }
        public PaletteColor Color { get; }
    }
}
