using UnityEngine;

namespace _Project.Presentation.Audio
{
    public class DecibelConverter
    {
        private const float DecibelMultiplier = 20f;
        private const float MinLinearVolume = 0.0001f;

        public float ConvertLinearToDecibels(float linear)
        {
            return Mathf.Log10(Mathf.Max(linear, MinLinearVolume)) * DecibelMultiplier;
        }

        public float ConvertDecibelsToLinear(float decibels)
        {
            return Mathf.Pow(10f, decibels / DecibelMultiplier);
        }
    }
}
