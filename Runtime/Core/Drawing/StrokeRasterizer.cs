using System;
using System.Collections.Generic;

namespace _Project.Core.Drawing
{
    public class StrokeRasterizer
    {
        private const uint SequenceSeedFactor = 73856093u;
        private const uint PointSeedFactor = 19349663u;
        private const float TwoPi = 6.2831853f;
        private const int AssistColorTolerance = 8;

        private readonly CanvasSize _size;
        private readonly ReferenceCanvasMap _referenceMap;
        private readonly Stack<int> _fillSeeds = new Stack<int>();

        private readonly List<AssistComponent> _assistComponents = new List<AssistComponent>();

        private int[] _assistVisited;
        private int[] _assistQueue;
        private int _assistVersion;

        public StrokeRasterizer(CanvasSize size, ReferenceCanvasMap referenceMap)
        {
            _size = size ?? throw new ArgumentNullException(nameof(size));
            _referenceMap = referenceMap ?? throw new ArgumentNullException(nameof(referenceMap));
        }

        public void RasterizeDocument(DrawingDocument document, PaletteColor[] palette, byte[] buffer)
        {
            FillBackground(palette[CanvasSpec.BackgroundColorIndex], buffer);

            for (int i = 0; i < document.Commands.Count; i++)
            {
                RasterizeCommand(document.Commands[i], palette, buffer);
            }
        }

        public void RasterizeCommand(DrawingCommand command, PaletteColor[] palette, byte[] buffer)
        {
            if (command is StrokeCommand stroke)
            {
                RasterizeStrokeFromPoint(stroke, 0, palette, buffer);
                return;
            }

            if (command is FillCommand fill)
            {
                RasterizeFill(fill, palette, buffer);
                return;
            }

            if (command is AssistCommand assist)
            {
                RasterizeAssist(assist, palette, buffer);
                return;
            }

            throw new ArgumentOutOfRangeException(nameof(command));
        }

        public void RasterizeAssist(AssistCommand assist, PaletteColor[] palette, byte[] buffer)
        {
            EnsureAssistBuffers();
            _assistVersion++;
            _assistComponents.Clear();
            int cellCount = _size.Width * _size.Height;

            for (int cell = 0; cell < cellCount; cell++)
            {
                if (_assistVisited[cell] == _assistVersion || IsMismatch(cell, palette, buffer) == false)
                {
                    continue;
                }

                int componentSize = TraverseMismatch(cell, palette, buffer, 0, null);
                _assistComponents.Add(new AssistComponent(cell, componentSize));
            }

            if (_assistComponents.Count == 0)
            {
                return;
            }

            _assistComponents.Sort((a, b) => b.Size.CompareTo(a.Size));

            int remaining = assist.PixelBudget;

            for (int i = 0; i < _assistComponents.Count && remaining > 0; i++)
            {
                AssistComponent component = _assistComponents[i];
                _assistVersion++;
                TraverseMismatch(component.Seed, palette, buffer, remaining, palette);
                remaining -= Math.Min(component.Size, remaining);
            }
        }

        private readonly struct AssistComponent
        {
            public AssistComponent(int seed, int size)
            {
                Seed = seed;
                Size = size;
            }

            public readonly int Seed;
            public readonly int Size;
        }

        private int TraverseMismatch(int seed, PaletteColor[] palette, byte[] buffer, int paintBudget, PaletteColor[] paintPalette)
        {
            int head = 0;
            int tail = 0;
            _assistQueue[tail] = seed;
            tail++;
            _assistVisited[seed] = _assistVersion;
            int visitedCount = 0;

            while (head < tail)
            {
                int cell = _assistQueue[head];
                head++;
                visitedCount++;

                if (paintPalette != null)
                {
                    int y = cell / _size.Width;
                    int x = cell - y * _size.Width;
                    WritePixel(buffer, x, y, paintPalette[_referenceMap.Indices[cell]]);

                    if (visitedCount >= paintBudget)
                    {
                        return visitedCount;
                    }
                }

                int cellY = cell / _size.Width;
                int cellX = cell - cellY * _size.Width;

                TryEnqueueMismatch(cellX - 1, cellY, palette, buffer, ref tail);
                TryEnqueueMismatch(cellX + 1, cellY, palette, buffer, ref tail);
                TryEnqueueMismatch(cellX, cellY - 1, palette, buffer, ref tail);
                TryEnqueueMismatch(cellX, cellY + 1, palette, buffer, ref tail);
            }

            return visitedCount;
        }

        private void TryEnqueueMismatch(int x, int y, PaletteColor[] palette, byte[] buffer, ref int tail)
        {
            if (x < 0 || y < 0 || x >= _size.Width || y >= _size.Height)
            {
                return;
            }

            int cell = y * _size.Width + x;

            if (_assistVisited[cell] == _assistVersion || IsMismatch(cell, palette, buffer) == false)
            {
                return;
            }

            _assistVisited[cell] = _assistVersion;
            _assistQueue[tail] = cell;
            tail++;
        }

        private bool IsMismatch(int cell, PaletteColor[] palette, byte[] buffer)
        {
            PaletteColor target = palette[_referenceMap.Indices[cell]];
            int offset = cell * CanvasSpec.BytesPerPixel;
            return Math.Abs(buffer[offset] - target.R) > AssistColorTolerance
                || Math.Abs(buffer[offset + 1] - target.G) > AssistColorTolerance
                || Math.Abs(buffer[offset + 2] - target.B) > AssistColorTolerance;
        }

        private void EnsureAssistBuffers()
        {
            if (_assistVisited != null)
            {
                return;
            }

            int cellCount = _size.Width * _size.Height;
            _assistVisited = new int[cellCount];
            _assistQueue = new int[cellCount];
        }

        public void RasterizeStrokeFromPoint(StrokeCommand stroke, int firstNewPointIndex, PaletteColor[] palette, byte[] buffer)
        {
            if (stroke.Points.Count == 0)
            {
                return;
            }

            PaletteColor color = stroke.Color;
            float radius = BrushSpec.Radii[stroke.SizeIndex] * CanvasSpec.MaxResolution;

            if (firstNewPointIndex == 0)
            {
                CanvasPoint first = stroke.Points[0];
                StampBrush(stroke, 0, ToPixelX(first.X), ToPixelY(first.Y), radius, color, buffer);
            }

            int firstSegmentEnd = Math.Max(firstNewPointIndex, 1);

            for (int i = firstSegmentEnd; i < stroke.Points.Count; i++)
            {
                StampSegment(stroke, i, radius, color, buffer);
            }
        }

        public void RasterizeFill(FillCommand fill, PaletteColor[] palette, byte[] buffer)
        {
            int startX = Math.Clamp((int)(fill.Point.X * (_size.Width - 1)), 0, _size.Width - 1);
            int startY = Math.Clamp((int)(fill.Point.Y * (_size.Height - 1)), 0, _size.Height - 1);
            int startOffset = (startY * _size.Width + startX) * CanvasSpec.BytesPerPixel;
            byte sourceR = buffer[startOffset];
            byte sourceG = buffer[startOffset + 1];
            byte sourceB = buffer[startOffset + 2];
            PaletteColor target = fill.Color;

            if (sourceR == target.R && sourceG == target.G && sourceB == target.B)
            {
                return;
            }

            _fillSeeds.Clear();
            _fillSeeds.Push(startY * _size.Width + startX);

            while (_fillSeeds.Count > 0)
            {
                int packed = _fillSeeds.Pop();
                int y = packed / _size.Width;
                int x = packed - y * _size.Width;

                if (MatchesColor(buffer, x, y, sourceR, sourceG, sourceB) == false)
                {
                    continue;
                }

                int left = x;

                while (left > 0 && MatchesColor(buffer, left - 1, y, sourceR, sourceG, sourceB))
                {
                    left--;
                }

                int right = x;

                while (right < _size.Width - 1 && MatchesColor(buffer, right + 1, y, sourceR, sourceG, sourceB))
                {
                    right++;
                }

                for (int px = left; px <= right; px++)
                {
                    WritePixel(buffer, px, y, target);
                }

                if (y > 0)
                {
                    PushRowSeeds(buffer, left, right, y - 1, sourceR, sourceG, sourceB);
                }

                if (y < _size.Height - 1)
                {
                    PushRowSeeds(buffer, left, right, y + 1, sourceR, sourceG, sourceB);
                }
            }
        }

        private void PushRowSeeds(byte[] buffer, int left, int right, int y, byte r, byte g, byte b)
        {
            bool insideRun = false;

            for (int x = left; x <= right; x++)
            {
                if (MatchesColor(buffer, x, y, r, g, b))
                {
                    if (insideRun == false)
                    {
                        _fillSeeds.Push(y * _size.Width + x);
                        insideRun = true;
                    }
                }
                else
                {
                    insideRun = false;
                }
            }
        }

        private float ResolveSpacingFactor(BrushType brush)
        {
            switch (brush)
            {
                case BrushType.Round:
                    return BrushSpec.RoundSpacingFactor;
                case BrushType.Marker:
                    return BrushSpec.MarkerSpacingFactor;
                case BrushType.Spray:
                    return BrushSpec.SpraySpacingFactor;
                default:
                    throw new ArgumentOutOfRangeException(nameof(brush));
            }
        }

        private void StampSegment(StrokeCommand stroke, int pointIndex, float radius, PaletteColor color, byte[] buffer)
        {
            CanvasPoint from = stroke.Points[pointIndex - 1];
            CanvasPoint to = stroke.Points[pointIndex];
            float fromX = ToPixelX(from.X);
            float fromY = ToPixelY(from.Y);
            float deltaX = ToPixelX(to.X) - fromX;
            float deltaY = ToPixelY(to.Y) - fromY;
            float distance = MathF.Sqrt(deltaX * deltaX + deltaY * deltaY);
            float spacing = radius * ResolveSpacingFactor(stroke.Brush);
            int steps = Math.Max(1, (int)MathF.Ceiling(distance / spacing));

            for (int step = 1; step <= steps; step++)
            {
                float t = (float)step / steps;
                StampBrush(stroke, pointIndex, fromX + deltaX * t, fromY + deltaY * t, radius, color, buffer);
            }
        }

        private void StampBrush(StrokeCommand stroke, int pointIndex, float centerX, float centerY, float radius, PaletteColor color, byte[] buffer)
        {
            switch (stroke.Brush)
            {
                case BrushType.Round:
                    StampCircle(centerX, centerY, radius, color, buffer);
                    break;
                case BrushType.Marker:
                    StampSquare(centerX, centerY, radius, color, buffer);
                    break;
                case BrushType.Spray:
                    StampSpray(stroke, pointIndex, centerX, centerY, radius, color, buffer);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(stroke));
            }
        }

        private void StampSpray(StrokeCommand stroke, int pointIndex, float centerX, float centerY, float radius, PaletteColor color, byte[] buffer)
        {
            uint seed = (uint)stroke.Sequence * SequenceSeedFactor ^ (uint)pointIndex * PointSeedFactor;
            DeterministicRandom random = new DeterministicRandom(seed);

            for (int dot = 0; dot < BrushSpec.SprayDotsPerStamp; dot++)
            {
                float angle = random.NextFloat01() * TwoPi;
                float distance = MathF.Sqrt(random.NextFloat01()) * radius;
                float dotX = centerX + MathF.Cos(angle) * distance;
                float dotY = centerY + MathF.Sin(angle) * distance;
                StampCircle(dotX, dotY, BrushSpec.SprayDotRadius, color, buffer);
            }
        }

        private void StampCircle(float centerX, float centerY, float radius, PaletteColor color, byte[] buffer)
        {
            int minX = Math.Max(0, (int)MathF.Floor(centerX - radius));
            int maxX = Math.Min(_size.Width - 1, (int)MathF.Ceiling(centerX + radius));
            int minY = Math.Max(0, (int)MathF.Floor(centerY - radius));
            int maxY = Math.Min(_size.Height - 1, (int)MathF.Ceiling(centerY + radius));
            float radiusSquared = radius * radius;

            for (int y = minY; y <= maxY; y++)
            {
                float offsetY = y - centerY;

                for (int x = minX; x <= maxX; x++)
                {
                    float offsetX = x - centerX;

                    if (offsetX * offsetX + offsetY * offsetY > radiusSquared)
                    {
                        continue;
                    }

                    WritePixel(buffer, x, y, color);
                }
            }
        }

        private void StampSquare(float centerX, float centerY, float halfSize, PaletteColor color, byte[] buffer)
        {
            int minX = Math.Max(0, (int)MathF.Floor(centerX - halfSize));
            int maxX = Math.Min(_size.Width - 1, (int)MathF.Ceiling(centerX + halfSize));
            int minY = Math.Max(0, (int)MathF.Floor(centerY - halfSize));
            int maxY = Math.Min(_size.Height - 1, (int)MathF.Ceiling(centerY + halfSize));

            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    WritePixel(buffer, x, y, color);
                }
            }
        }

        private void FillBackground(PaletteColor color, byte[] buffer)
        {
            for (int i = 0; i < buffer.Length; i += CanvasSpec.BytesPerPixel)
            {
                buffer[i] = color.R;
                buffer[i + 1] = color.G;
                buffer[i + 2] = color.B;
                buffer[i + 3] = byte.MaxValue;
            }
        }

        private bool MatchesColor(byte[] buffer, int x, int y, byte r, byte g, byte b)
        {
            int offset = (y * _size.Width + x) * CanvasSpec.BytesPerPixel;
            return buffer[offset] == r && buffer[offset + 1] == g && buffer[offset + 2] == b;
        }

        private void WritePixel(byte[] buffer, int x, int y, PaletteColor color)
        {
            int offset = (y * _size.Width + x) * CanvasSpec.BytesPerPixel;
            buffer[offset] = color.R;
            buffer[offset + 1] = color.G;
            buffer[offset + 2] = color.B;
            buffer[offset + 3] = byte.MaxValue;
        }

        private float ToPixelX(float normalized)
        {
            return normalized * (_size.Width - 1);
        }

        private float ToPixelY(float normalized)
        {
            return normalized * (_size.Height - 1);
        }
    }
}
