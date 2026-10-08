using System.Collections.Generic;
using _Project.Core.Drawing;
using UnityEngine;

namespace _Project.Infrastructure.Network
{
    public static class DrawingWireCodec
    {
        public static ushort ToWire(float value)
        {
            return (ushort)Mathf.RoundToInt(Mathf.Clamp01(value) * ushort.MaxValue);
        }

        public static float FromWire(ushort value)
        {
            return value / (float)ushort.MaxValue;
        }

        public static ushort[] Pack(IReadOnlyList<CanvasPoint> points)
        {
            ushort[] packed = new ushort[points.Count * 2];

            for (int i = 0; i < points.Count; i++)
            {
                packed[i * 2] = ToWire(points[i].X);
                packed[i * 2 + 1] = ToWire(points[i].Y);
            }

            return packed;
        }

        public static void Unpack(ushort[] packed, List<CanvasPoint> destination)
        {
            destination.Clear();

            if (packed == null)
            {
                return;
            }

            for (int i = 0; i + 1 < packed.Length; i += 2)
            {
                destination.Add(new CanvasPoint(FromWire(packed[i]), FromWire(packed[i + 1])));
            }
        }
    }
}
