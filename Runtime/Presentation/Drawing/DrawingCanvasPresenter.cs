using System;
using _Project.Application.Drawing;
using _Project.Core.Drawing;
using VContainer.Unity;

namespace _Project.Presentation.Drawing
{
    public class DrawingCanvasPresenter : IStartable, IDisposable
    {
        private readonly CanvasInputView _inputView;
        private readonly DrawingCanvasView _canvasView;
        private readonly DrawingSessionService _session;
        private readonly DrawingDocument _document;
        private readonly StrokeRasterizer _rasterizer;
        private readonly CanvasSize _canvasSize;

        private byte[] _pixelBuffer;

        public byte[] PixelBuffer => _pixelBuffer;

        public DrawingCanvasPresenter(
            CanvasInputView inputView,
            DrawingCanvasView canvasView,
            DrawingSessionService session,
            DrawingDocument document,
            StrokeRasterizer rasterizer,
            CanvasSize canvasSize)
        {
            _inputView = inputView;
            _canvasView = canvasView;
            _session = session;
            _document = document;
            _rasterizer = rasterizer;
            _canvasSize = canvasSize;
        }

        public void Start()
        {
            _pixelBuffer = new byte[_canvasSize.BufferLength];
            _canvasView.Initialize(_canvasSize);
            RedrawDocument();

            _inputView.PointerPressed += OnPointerPressed;
            _inputView.PointerMoved += OnPointerMoved;
            _inputView.PointerReleased += OnPointerReleased;
            _session.StrokePointsAppended += OnStrokePointsAppended;
            _session.CommandApplied += OnCommandApplied;
            _session.DocumentChanged += OnDocumentChanged;
        }

        public void Dispose()
        {
            _inputView.PointerPressed -= OnPointerPressed;
            _inputView.PointerMoved -= OnPointerMoved;
            _inputView.PointerReleased -= OnPointerReleased;
            _session.StrokePointsAppended -= OnStrokePointsAppended;
            _session.CommandApplied -= OnCommandApplied;
            _session.DocumentChanged -= OnDocumentChanged;
        }

        private void RedrawDocument()
        {
            _rasterizer.RasterizeDocument(_document, _session.Palette, _pixelBuffer);
            _canvasView.ApplyBuffer(_pixelBuffer);
        }

        private void OnPointerPressed(CanvasPoint point)
        {
            if (_session.IsDrawingLocked)
            {
                return;
            }

            switch (_session.ActiveToolMode)
            {
                case ToolMode.Brush:
                case ToolMode.Eraser:
                    _session.BeginStroke(point);
                    break;
                case ToolMode.Fill:
                    _session.ApplyFill(point);
                    break;
                case ToolMode.ColorPicker:
                    PickColor(point);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(_session.ActiveToolMode));
            }
        }

        private void OnPointerMoved(CanvasPoint point)
        {
            _session.ContinueStroke(point);
        }

        private void OnPointerReleased()
        {
            _session.EndStroke();
        }

        private void OnStrokePointsAppended(StrokeCommand stroke, int firstNewPointIndex)
        {
            _rasterizer.RasterizeStrokeFromPoint(stroke, firstNewPointIndex, _session.Palette, _pixelBuffer);
            _canvasView.ApplyBuffer(_pixelBuffer);
        }

        private void OnCommandApplied(DrawingCommand command)
        {
            _rasterizer.RasterizeCommand(command, _session.Palette, _pixelBuffer);
            _canvasView.ApplyBuffer(_pixelBuffer);
        }

        private void OnDocumentChanged()
        {
            RedrawDocument();
        }

        private void PickColor(CanvasPoint point)
        {
            int x = (int)(point.X * (_canvasSize.Width - 1));
            int y = (int)(point.Y * (_canvasSize.Height - 1));
            int offset = (y * _canvasSize.Width + x) * CanvasSpec.BytesPerPixel;
            _session.SelectPickedColor(new PaletteColor(_pixelBuffer[offset], _pixelBuffer[offset + 1], _pixelBuffer[offset + 2]));
        }
    }
}
