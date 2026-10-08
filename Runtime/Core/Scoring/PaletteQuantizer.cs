using _Project.Core.Drawing;

namespace _Project.Core.Scoring
{
    public class PaletteQuantizer
    {
        public int FindNearestIndex(byte r, byte g, byte b, PaletteColor[] palette)
        {
            int nearestIndex = 0;
            int nearestDistance = int.MaxValue;

            for (int i = 0; i < palette.Length; i++)
            {
                int deltaR = r - palette[i].R;
                int deltaG = g - palette[i].G;
                int deltaB = b - palette[i].B;
                int distance = deltaR * deltaR + deltaG * deltaG + deltaB * deltaB;

                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestIndex = i;
                }
            }

            return nearestIndex;
        }
    }
}
