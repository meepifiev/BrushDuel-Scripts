using UnityEngine;
using UnityEngine.UI;

namespace _Project.Presentation.Drawing
{
    [RequireComponent(typeof(RawImage))]
    public class GridOverlayView : MonoBehaviour
    {
        private const int DashPairPixels = 16;
        private const float LineThickness = 2f;
        private const float DashesPerLine = 48f;

        private static readonly Color32 DashLight = new Color32(255, 255, 255, 150);
        private static readonly Color32 DashDark = new Color32(20, 20, 24, 150);

        [SerializeField] private int _cellCount = 8;

        private Texture2D _verticalDashTexture;
        private Texture2D _horizontalDashTexture;

        private void Awake()
        {
            RawImage baseImage = GetComponent<RawImage>();
            baseImage.raycastTarget = false;
            baseImage.enabled = false;

            _verticalDashTexture = CreateDashTexture(1, DashPairPixels);
            _horizontalDashTexture = CreateDashTexture(DashPairPixels, 1);
            BuildLines();
        }

        private void OnDestroy()
        {
            if (_verticalDashTexture != null)
            {
                Destroy(_verticalDashTexture);
            }

            if (_horizontalDashTexture != null)
            {
                Destroy(_horizontalDashTexture);
            }
        }

        public void Toggle()
        {
            gameObject.SetActive(gameObject.activeSelf == false);
        }

        private void BuildLines()
        {
            RectTransform root = (RectTransform)transform;

            for (int line = 1; line < _cellCount; line++)
            {
                float fraction = (float)line / _cellCount;

                RawImage vertical = CreateLine(root, "GridLineV" + line);
                RectTransform verticalRect = vertical.rectTransform;
                verticalRect.anchorMin = new Vector2(fraction, 0f);
                verticalRect.anchorMax = new Vector2(fraction, 1f);
                verticalRect.sizeDelta = new Vector2(LineThickness, 0f);
                vertical.texture = _verticalDashTexture;
                vertical.uvRect = new Rect(0f, 0f, 1f, DashesPerLine);

                RawImage horizontal = CreateLine(root, "GridLineH" + line);
                RectTransform horizontalRect = horizontal.rectTransform;
                horizontalRect.anchorMin = new Vector2(0f, fraction);
                horizontalRect.anchorMax = new Vector2(1f, fraction);
                horizontalRect.sizeDelta = new Vector2(0f, LineThickness);
                horizontal.texture = _horizontalDashTexture;
                horizontal.uvRect = new Rect(0f, 0f, DashesPerLine, 1f);
            }
        }

        private static RawImage CreateLine(RectTransform root, string name)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            RectTransform rect = (RectTransform)go.transform;
            rect.SetParent(root, false);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            RawImage image = go.AddComponent<RawImage>();
            image.raycastTarget = false;
            return image;
        }

        private Texture2D CreateDashTexture(int width, int height)
        {
            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Repeat
            };

            Color32[] pixels = new Color32[width * height];
            int length = Mathf.Max(width, height);

            for (int i = 0; i < length; i++)
            {
                pixels[i] = i < length / 2 ? DashLight : DashDark;
            }

            texture.SetPixels32(pixels);
            texture.Apply(false);
            return texture;
        }
    }
}
