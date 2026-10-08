using UnityEngine;

namespace _Project.Presentation.UI
{
    public class RectAspectFitter
    {
        public void FitInside(RectTransform rect, float aspect)
        {
            Vector2 maxSize = rect.sizeDelta;
            float maxAspect = maxSize.x / maxSize.y;

            if (aspect >= maxAspect)
            {
                rect.sizeDelta = new Vector2(maxSize.x, maxSize.x / aspect);
            }
            else
            {
                rect.sizeDelta = new Vector2(maxSize.y * aspect, maxSize.y);
            }
        }
    }
}
