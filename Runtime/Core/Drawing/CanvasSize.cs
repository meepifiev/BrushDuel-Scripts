using System;

namespace _Project.Core.Drawing
{
    public class CanvasSize
    {
        public CanvasSize(int sourceWidth, int sourceHeight)
        {
            if (sourceWidth <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(sourceWidth));
            }

            if (sourceHeight <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(sourceHeight));
            }

            if (sourceWidth >= sourceHeight)
            {
                Width = CanvasSpec.MaxResolution;
                Height = Math.Max(1, (int)Math.Round((double)sourceHeight * CanvasSpec.MaxResolution / sourceWidth));
            }
            else
            {
                Height = CanvasSpec.MaxResolution;
                Width = Math.Max(1, (int)Math.Round((double)sourceWidth * CanvasSpec.MaxResolution / sourceHeight));
            }
        }

        public int Width { get; }
        public int Height { get; }
        public float Aspect => (float)Width / Height;
        public int BufferLength => Width * Height * CanvasSpec.BytesPerPixel;
    }
}
