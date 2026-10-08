using System;

namespace _Project.Core.Drawing
{
    public class ReferenceCanvasMap
    {
        public ReferenceCanvasMap(CanvasSize size)
        {
            if (size == null)
            {
                throw new ArgumentNullException(nameof(size));
            }

            Indices = new byte[size.Width * size.Height];
        }

        public ReferenceCanvasMap(CanvasSize size, byte[] indices)
        {
            if (size == null)
            {
                throw new ArgumentNullException(nameof(size));
            }

            if (indices == null)
            {
                throw new ArgumentNullException(nameof(indices));
            }

            if (indices.Length != size.Width * size.Height)
            {
                throw new ArgumentOutOfRangeException(nameof(indices));
            }

            Indices = indices;
        }

        public byte[] Indices { get; }
    }
}
