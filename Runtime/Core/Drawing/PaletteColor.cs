namespace _Project.Core.Drawing
{
    public readonly struct PaletteColor
    {
        public readonly byte R;
        public readonly byte G;
        public readonly byte B;

        public PaletteColor(byte r, byte g, byte b)
        {
            R = r;
            G = g;
            B = b;
        }
    }
}
