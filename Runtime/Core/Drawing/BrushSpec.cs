namespace _Project.Core.Drawing
{
    public static class BrushSpec
    {
        public const float RoundSpacingFactor = 0.4f;
        public const float MarkerSpacingFactor = 0.35f;
        public const float SpraySpacingFactor = 0.9f;
        public const int SprayDotsPerStamp = 14;
        public const float SprayDotRadius = 1.6f;

        public const float MinRadius = 0.008f;
        public const float MaxRadius = 0.03f;
        public const int SizeSteps = 16;
        public const int DefaultSizeIndex = 7;

        public static readonly float[] Radii = BuildRadii();

        private static float[] BuildRadii()
        {
            float[] radii = new float[SizeSteps];

            for (int i = 0; i < SizeSteps; i++)
            {
                float t = (float)i / (SizeSteps - 1);
                radii[i] = MinRadius + (MaxRadius - MinRadius) * t;
            }

            return radii;
        }
    }
}
