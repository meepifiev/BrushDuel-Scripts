using _Project.Core.Drawing;
using _Project.Presentation.UI;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Presentation.Drawing
{
    [RequireComponent(typeof(RawImage))]
    public class DrawingCanvasView : MonoBehaviour
    {
        private RawImage _canvasImage;
        private Texture2D _texture;
        private float _aspect = 1f;

        private void Awake()
        {
            _canvasImage = GetComponent<RawImage>();
            _canvasImage.raycastTarget = false;
        }

        public void Initialize(CanvasSize size)
        {
            _texture = new Texture2D(size.Width, size.Height, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };

            _canvasImage.texture = _texture;
            _aspect = size.Aspect;
            new RectAspectFitter().FitInside((RectTransform)transform, _aspect);
        }

        public void FitInto(Vector2 box)
        {
            RectTransform rect = (RectTransform)transform;
            rect.sizeDelta = box;
            new RectAspectFitter().FitInside(rect, _aspect);
        }

        public void ApplyBuffer(byte[] pixelBuffer)
        {
            _texture.LoadRawTextureData(pixelBuffer);
            _texture.Apply(false);
        }

        private void OnDestroy()
        {
            if (_texture != null)
            {
                Destroy(_texture);
            }
        }
    }
}
