using System;

namespace _Project.Application.Configs
{
    [Serializable]
    public class ReferenceData
    {
        public string Id;
        public float RoundDurationSeconds;
        public int SourceWidth;
        public int SourceHeight;
        public byte[] GridIndices;
        public byte[] CanvasIndices;
        public RgbColor CanvasColor;
        public RgbColor[] PaletteColors;
    }

    [Serializable]
    public struct RgbColor
    {
        public byte R;
        public byte G;
        public byte B;
    }
}
