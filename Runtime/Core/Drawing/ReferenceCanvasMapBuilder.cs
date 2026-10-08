using System;
using _Project.Core.Scoring;

namespace _Project.Core.Drawing
{
    public class ReferenceCanvasMapBuilder
    {
        private readonly PaletteQuantizer _quantizer;
        private readonly CanvasSize _size;

        public ReferenceCanvasMapBuilder(PaletteQuantizer quantizer, CanvasSize size)
        {
            _quantizer = quantizer ?? throw new ArgumentNullException(nameof(quantizer));
            _size = size ?? throw new ArgumentNullException(nameof(size));
        }

        public void Build(ReferenceCanvasMap map, byte[] rgba, int sourceWidth, int sourceHeight, PaletteColor[] palette)
        {
            for (int y = 0; y < _size.Height; y++)
            {
                int sourceY = Math.Min(sourceHeight - 1, y * sourceHeight / _size.Height);

                for (int x = 0; x < _size.Width; x++)
                {
                    int sourceX = Math.Min(sourceWidth - 1, x * sourceWidth / _size.Width);
                    int offset = (sourceY * sourceWidth + sourceX) * CanvasSpec.BytesPerPixel;
                    map.Indices[y * _size.Width + x] = (byte)_quantizer.FindNearestIndex(
                        rgba[offset], rgba[offset + 1], rgba[offset + 2], palette);
                }
            }
        }
    }
}
