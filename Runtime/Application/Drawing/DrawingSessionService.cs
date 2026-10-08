using System;
using _Project.Core.Drawing;

namespace _Project.Application.Drawing
{
    public class DrawingSessionService
    {
        private const float MinPointDistance = 0.004f;
        private const float MinPointDistanceSquared = MinPointDistance * MinPointDistance;

        private readonly DrawingDocument _document;
        private readonly PaletteColor[] _palette;

        private StrokeCommand _activeStroke;
        private CanvasPoint _lastAcceptedPoint;
        private bool _drawingLocked;
        private bool _inputBlocked;
        private PaletteColor _pickedColor;
        private bool _usesPickedColor;

        public DrawingSessionService(DrawingDocument document, PaletteColor[] palette)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
            _palette = palette ?? throw new ArgumentNullException(nameof(palette));
            _pickedColor = _palette[0];
        }

        public event Action<StrokeCommand, int> StrokePointsAppended;
        public event Action<DrawingCommand> CommandApplied;
        public event Action CommandCompleted;
        public event Action DocumentChanged;
        public event Action ToolStateChanged;

        public ToolMode ActiveToolMode { get; private set; } = ToolMode.Brush;
        public BrushType ActiveBrush { get; private set; } = BrushType.Round;
        public int ActiveColorIndex { get; private set; }
        public int ActiveSizeIndex { get; private set; } = BrushSpec.DefaultSizeIndex;
        public PaletteColor[] Palette => _palette;
        public PaletteColor ActiveColor => _usesPickedColor ? _pickedColor : _palette[ActiveColorIndex];
        public PaletteColor PickedColor => _pickedColor;
        public bool UsesPickedColor => _usesPickedColor;
        public bool IsDrawingLocked => _drawingLocked || _inputBlocked;

        public void SetInputBlocked(bool blocked)
        {
            if (blocked)
            {
                EndStroke();
            }

            _inputBlocked = blocked;
        }

        public void BeginStroke(CanvasPoint point)
        {
            if (IsDrawingLocked || _activeStroke != null)
            {
                return;
            }

            if (ActiveToolMode != ToolMode.Brush && ActiveToolMode != ToolMode.Eraser)
            {
                return;
            }

            bool isEraser = ActiveToolMode == ToolMode.Eraser;
            BrushType brush = isEraser ? BrushType.Round : ActiveBrush;
            PaletteColor color = isEraser ? _palette[CanvasSpec.BackgroundColorIndex] : ActiveColor;
            _activeStroke = _document.BeginStroke(brush, ActiveSizeIndex, color);
            _activeStroke.AppendPoint(point);
            _lastAcceptedPoint = point;
            StrokePointsAppended?.Invoke(_activeStroke, 0);
        }

        public void ContinueStroke(CanvasPoint point)
        {
            if (_activeStroke == null)
            {
                return;
            }

            float deltaX = point.X - _lastAcceptedPoint.X;
            float deltaY = point.Y - _lastAcceptedPoint.Y;

            if (deltaX * deltaX + deltaY * deltaY < MinPointDistanceSquared)
            {
                return;
            }

            int firstNewPointIndex = _activeStroke.Points.Count;
            _activeStroke.AppendPoint(point);
            _lastAcceptedPoint = point;
            StrokePointsAppended?.Invoke(_activeStroke, firstNewPointIndex);
        }

        public void EndStroke()
        {
            if (_activeStroke == null)
            {
                return;
            }

            _activeStroke = null;
            CommandCompleted?.Invoke();
        }

        public void ApplyFill(CanvasPoint point)
        {
            if (IsDrawingLocked || ActiveToolMode != ToolMode.Fill)
            {
                return;
            }

            FillCommand fill = _document.ApplyFill(point, ActiveColor);
            CommandApplied?.Invoke(fill);
            CommandCompleted?.Invoke();
        }

        public void ApplyAssist(int pixelBudget)
        {
            if (IsDrawingLocked)
            {
                return;
            }

            EndStroke();
            AssistCommand assist = _document.ApplyAssist(pixelBudget);
            CommandApplied?.Invoke(assist);
            CommandCompleted?.Invoke();
        }

        public void LockDrawing()
        {
            EndStroke();
            _drawingLocked = true;
        }

        public void UnlockDrawing()
        {
            _drawingLocked = false;
        }

        public void SelectToolMode(ToolMode mode)
        {
            ActiveToolMode = mode;
            ToolStateChanged?.Invoke();
        }

        public void SelectBrush(BrushType brush)
        {
            ActiveBrush = brush;
            ActiveToolMode = ToolMode.Brush;
            ToolStateChanged?.Invoke();
        }

        public void SelectColor(int colorIndex)
        {
            ActiveColorIndex = colorIndex;
            _usesPickedColor = false;
            ToolStateChanged?.Invoke();
        }

        public void SelectPickedColor(PaletteColor color)
        {
            _pickedColor = color;
            _usesPickedColor = true;
            ActiveToolMode = ToolMode.Brush;
            ToolStateChanged?.Invoke();
        }

        public void SelectSize(int sizeIndex)
        {
            ActiveSizeIndex = sizeIndex;
            ToolStateChanged?.Invoke();
        }

        public bool UndoLastCommand()
        {
            if (_activeStroke != null)
            {
                return false;
            }

            if (_document.UndoLastCommand() == false)
            {
                return false;
            }

            DocumentChanged?.Invoke();
            return true;
        }
    }
}
