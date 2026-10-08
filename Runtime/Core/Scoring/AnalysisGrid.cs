using System;
using _Project.Core.Drawing;

namespace _Project.Core.Scoring
{
    public class AnalysisGrid
    {
        public const int MaxResolution = 256;

        public AnalysisGrid(CanvasSize canvasSize)
        {
            if (canvasSize == null)
            {
                throw new ArgumentNullException(nameof(canvasSize));
            }

            if (canvasSize.Width >= canvasSize.Height)
            {
                Width = MaxResolution;
                Height = Math.Max(1, canvasSize.Height * MaxResolution / canvasSize.Width);
            }
            else
            {
                Height = MaxResolution;
                Width = Math.Max(1, canvasSize.Width * MaxResolution / canvasSize.Height);
            }
        }

        public int Width { get; }
        public int Height { get; }
        public int CellCount => Width * Height;
    }
}
