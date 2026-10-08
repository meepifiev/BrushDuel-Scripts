using System.Collections.Generic;

namespace _Project.Core.Drawing
{
    public class DrawingDocument
    {
        private readonly List<DrawingCommand> _commands = new List<DrawingCommand>();

        private int _nextSequence;

        public IReadOnlyList<DrawingCommand> Commands => _commands;

        public StrokeCommand BeginStroke(BrushType brush, int sizeIndex, PaletteColor color)
        {
            StrokeCommand stroke = new StrokeCommand(_nextSequence, brush, sizeIndex, color);
            _nextSequence++;
            _commands.Add(stroke);
            return stroke;
        }

        public FillCommand ApplyFill(CanvasPoint point, PaletteColor color)
        {
            FillCommand fill = new FillCommand(_nextSequence, point, color);
            _nextSequence++;
            _commands.Add(fill);
            return fill;
        }

        public AssistCommand ApplyAssist(int pixelBudget)
        {
            AssistCommand assist = new AssistCommand(_nextSequence, pixelBudget);
            _nextSequence++;
            _commands.Add(assist);
            return assist;
        }

        public bool UndoLastCommand()
        {
            if (_commands.Count == 0)
            {
                return false;
            }

            _commands.RemoveAt(_commands.Count - 1);
            return true;
        }

        public void Clear()
        {
            _commands.Clear();
            _nextSequence = 0;
        }
    }
}
