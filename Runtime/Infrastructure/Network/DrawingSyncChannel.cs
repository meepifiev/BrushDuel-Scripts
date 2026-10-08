using System;
using System.Collections.Generic;
using _Project.Application.Network;
using _Project.Core.Drawing;
using Mirror;
using VContainer.Unity;

namespace _Project.Infrastructure.Network
{
    public class DrawingSyncChannel : IDrawingSyncChannel, IStartable, IDisposable
    {
        private readonly List<CanvasPoint> _received = new List<CanvasPoint>();

        public event Action<StrokeStartData, IReadOnlyList<CanvasPoint>> StrokeBegan;
        public event Action<IReadOnlyList<CanvasPoint>> StrokeContinued;
        public event Action StrokeEnded;
        public event Action<CanvasPoint, PaletteColor> FillApplied;
        public event Action UndoApplied;

        public void Start()
        {
            NetworkClient.ReplaceHandler<StrokeBeganMessage>(OnStrokeBegan);
            NetworkClient.ReplaceHandler<StrokeContinuedMessage>(OnStrokeContinued);
            NetworkClient.ReplaceHandler<StrokeEndedMessage>(OnStrokeEnded);
            NetworkClient.ReplaceHandler<FillAppliedMessage>(OnFillApplied);
            NetworkClient.ReplaceHandler<UndoAppliedMessage>(OnUndoApplied);
        }

        public void Dispose()
        {
            NetworkClient.UnregisterHandler<StrokeBeganMessage>();
            NetworkClient.UnregisterHandler<StrokeContinuedMessage>();
            NetworkClient.UnregisterHandler<StrokeEndedMessage>();
            NetworkClient.UnregisterHandler<FillAppliedMessage>();
            NetworkClient.UnregisterHandler<UndoAppliedMessage>();
        }

        public void SendStrokeBegan(StrokeStartData start, IReadOnlyList<CanvasPoint> points)
        {
            if (NetworkClient.isConnected == false)
            {
                return;
            }

            NetworkClient.Send(new StrokeBeganMessage
            {
                Brush = (byte)start.Brush,
                SizeIndex = (byte)start.SizeIndex,
                ColorR = start.Color.R,
                ColorG = start.Color.G,
                ColorB = start.Color.B,
                Points = DrawingWireCodec.Pack(points),
            });
        }

        public void SendStrokeContinued(IReadOnlyList<CanvasPoint> points)
        {
            if (NetworkClient.isConnected == false)
            {
                return;
            }

            NetworkClient.Send(new StrokeContinuedMessage { Points = DrawingWireCodec.Pack(points) });
        }

        public void SendStrokeEnded()
        {
            if (NetworkClient.isConnected == false)
            {
                return;
            }

            NetworkClient.Send(new StrokeEndedMessage());
        }

        public void SendFill(CanvasPoint point, PaletteColor color)
        {
            if (NetworkClient.isConnected == false)
            {
                return;
            }

            NetworkClient.Send(new FillAppliedMessage
            {
                X = DrawingWireCodec.ToWire(point.X),
                Y = DrawingWireCodec.ToWire(point.Y),
                ColorR = color.R,
                ColorG = color.G,
                ColorB = color.B,
            });
        }

        public void SendUndo()
        {
            if (NetworkClient.isConnected == false)
            {
                return;
            }

            NetworkClient.Send(new UndoAppliedMessage());
        }

        private IReadOnlyList<CanvasPoint> Unpack(ushort[] packed)
        {
            DrawingWireCodec.Unpack(packed, _received);
            return _received;
        }

        private void OnStrokeBegan(StrokeBeganMessage message)
        {
            PaletteColor color = new PaletteColor(message.ColorR, message.ColorG, message.ColorB);
            StrokeStartData start = new StrokeStartData((BrushType)message.Brush, message.SizeIndex, color);
            StrokeBegan?.Invoke(start, Unpack(message.Points));
        }

        private void OnStrokeContinued(StrokeContinuedMessage message)
        {
            StrokeContinued?.Invoke(Unpack(message.Points));
        }

        private void OnStrokeEnded(StrokeEndedMessage message)
        {
            StrokeEnded?.Invoke();
        }

        private void OnFillApplied(FillAppliedMessage message)
        {
            PaletteColor color = new PaletteColor(message.ColorR, message.ColorG, message.ColorB);
            FillApplied?.Invoke(new CanvasPoint(DrawingWireCodec.FromWire(message.X), DrawingWireCodec.FromWire(message.Y)), color);
        }

        private void OnUndoApplied(UndoAppliedMessage message)
        {
            UndoApplied?.Invoke();
        }
    }
}
