using System;
using System.Collections.Generic;
using _Project.Core.Drawing;

namespace _Project.Application.Network
{
    public interface IDrawingSyncChannel
    {
        event Action<StrokeStartData, IReadOnlyList<CanvasPoint>> StrokeBegan;
        event Action<IReadOnlyList<CanvasPoint>> StrokeContinued;
        event Action StrokeEnded;
        event Action<CanvasPoint, PaletteColor> FillApplied;
        event Action UndoApplied;

        void SendStrokeBegan(StrokeStartData start, IReadOnlyList<CanvasPoint> points);
        void SendStrokeContinued(IReadOnlyList<CanvasPoint> points);
        void SendStrokeEnded();
        void SendFill(CanvasPoint point, PaletteColor color);
        void SendUndo();
    }
}
