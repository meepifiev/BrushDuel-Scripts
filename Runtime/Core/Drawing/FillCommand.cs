namespace _Project.Core.Drawing
{
    public class FillCommand : DrawingCommand
    {
        public FillCommand(int sequence, CanvasPoint point, PaletteColor color)
            : base(sequence)
        {
            Point = point;
            Color = color;
        }

        public CanvasPoint Point { get; }
        public PaletteColor Color { get; }
    }
}
