using System;
using System.Collections.Generic;
using _Project.Application.Network;
using _Project.Core.Drawing;
using VContainer.Unity;

namespace _Project.Presentation.Drawing
{
    public class OpponentCanvasPresenter : IStartable, IDisposable
    {
        private readonly DrawingCanvasView _canvasView;
        private readonly CanvasSize _canvasSize;
        private readonly PaletteColor[] _palette;
        private readonly StrokeRasterizer _rasterizer;
        private readonly IDrawingSyncChannel _channel;
        private readonly DrawingDocument _document = new DrawingDocument();

        private byte[] _pixelBuffer;
        private StrokeCommand _activeStroke;

        public byte[] PixelBuffer => _pixelBuffer;

        public DrawingCanvasView CanvasView => _canvasView;

        public OpponentCanvasPresenter(
            DrawingCanvasView canvasView,
            CanvasSize canvasSize,
            PaletteColor[] palette,
            StrokeRasterizer rasterizer,
            IDrawingSyncChannel channel)
        {
            _canvasView = canvasView != null ? canvasView : throw new ArgumentNullException(nameof(canvasView));
            _canvasSize = canvasSize ?? throw new ArgumentNullException(nameof(canvasSize));
            _palette = palette ?? throw new ArgumentNullException(nameof(palette));
            _rasterizer = rasterizer ?? throw new ArgumentNullException(nameof(rasterizer));
            _channel = channel ?? throw new ArgumentNullException(nameof(channel));
        }

        public void Start()
        {
            _pixelBuffer = new byte[_canvasSize.BufferLength];
            _canvasView.Initialize(_canvasSize);
            Redraw();

            _channel.StrokeBegan += OnStrokeBegan;
            _channel.StrokeContinued += OnStrokeContinued;
            _channel.StrokeEnded += OnStrokeEnded;
            _channel.FillApplied += OnFillApplied;
            _channel.UndoApplied += OnUndoApplied;
        }

        public void Dispose()
        {
            _channel.StrokeBegan -= OnStrokeBegan;
            _channel.StrokeContinued -= OnStrokeContinued;
            _channel.StrokeEnded -= OnStrokeEnded;
            _channel.FillApplied -= OnFillApplied;
            _channel.UndoApplied -= OnUndoApplied;
        }

        private void Redraw()
        {
            _rasterizer.RasterizeDocument(_document, _palette, _pixelBuffer);
            _canvasView.ApplyBuffer(_pixelBuffer);
        }

        private void AppendPoints(IReadOnlyList<CanvasPoint> points)
        {
            if (_activeStroke == null || points.Count == 0)
            {
                return;
            }

            int firstNewPointIndex = _activeStroke.Points.Count;

            for (int i = 0; i < points.Count; i++)
            {
                _activeStroke.AppendPoint(points[i]);
            }

            _rasterizer.RasterizeStrokeFromPoint(_activeStroke, firstNewPointIndex, _palette, _pixelBuffer);
            _canvasView.ApplyBuffer(_pixelBuffer);
        }

        private void OnStrokeBegan(StrokeStartData start, IReadOnlyList<CanvasPoint> points)
        {
            _activeStroke = _document.BeginStroke(start.Brush, start.SizeIndex, start.Color);
            AppendPoints(points);
        }

        private void OnStrokeContinued(IReadOnlyList<CanvasPoint> points)
        {
            AppendPoints(points);
        }

        private void OnStrokeEnded()
        {
            _activeStroke = null;
        }

        private void OnFillApplied(CanvasPoint point, PaletteColor color)
        {
            FillCommand fill = _document.ApplyFill(point, color);
            _rasterizer.RasterizeCommand(fill, _palette, _pixelBuffer);
            _canvasView.ApplyBuffer(_pixelBuffer);
        }

        private void OnUndoApplied()
        {
            _activeStroke = null;

            if (_document.UndoLastCommand() == false)
            {
                return;
            }

            Redraw();
        }
    }
}
