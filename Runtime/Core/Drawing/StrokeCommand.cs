using System.Collections.Generic;

namespace _Project.Core.Drawing
{
    public class StrokeCommand : DrawingCommand
    {
        private readonly List<CanvasPoint> _points = new List<CanvasPoint>();

        public StrokeCommand(int sequence, BrushType brush, int sizeIndex, PaletteColor color)
            : base(sequence)
        {
            Brush = brush;
            SizeIndex = sizeIndex;
            Color = color;
        }

        public BrushType Brush { get; }
        public int SizeIndex { get; }
        public PaletteColor Color { get; }
        public IReadOnlyList<CanvasPoint> Points => _points;

        public void AppendPoint(CanvasPoint point)
        {
            _points.Add(point);
        }
    }
}
