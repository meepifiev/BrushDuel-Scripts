using _Project.Core.Drawing;

namespace _Project.Infrastructure.Network
{
    public class ServerDrawingRecorder
    {
        private readonly DrawingDocument _document = new DrawingDocument();

        private StrokeCommand _activeStroke;

        public DrawingDocument Document => _document;

        public void ApplyStrokeBegan(StrokeBeganMessage message)
        {
            PaletteColor color = new PaletteColor(message.ColorR, message.ColorG, message.ColorB);
            _activeStroke = _document.BeginStroke((BrushType)message.Brush, message.SizeIndex, color);
            AppendPoints(message.Points);
        }

        public void ApplyStrokeContinued(StrokeContinuedMessage message)
        {
            AppendPoints(message.Points);
        }

        public void ApplyStrokeEnded()
        {
            _activeStroke = null;
        }

        public void ApplyFill(FillAppliedMessage message)
        {
            PaletteColor color = new PaletteColor(message.ColorR, message.ColorG, message.ColorB);
            CanvasPoint point = new CanvasPoint(DrawingWireCodec.FromWire(message.X), DrawingWireCodec.FromWire(message.Y));
            _document.ApplyFill(point, color);
        }

        public void ApplyUndo()
        {
            _activeStroke = null;
            _document.UndoLastCommand();
        }

        private void AppendPoints(ushort[] packed)
        {
            if (_activeStroke == null || packed == null)
            {
                return;
            }

            for (int i = 0; i + 1 < packed.Length; i += 2)
            {
                _activeStroke.AppendPoint(new CanvasPoint(DrawingWireCodec.FromWire(packed[i]), DrawingWireCodec.FromWire(packed[i + 1])));
            }
        }
    }
}
