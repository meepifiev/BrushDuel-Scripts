using System;
using _Project.Application.Drawing;
using _Project.Core.Drawing;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace _Project.Presentation.Drawing
{
    public class ToolCursorView : MonoBehaviour
    {
        private const int RingTextureSize = 128;
        private const float RingThickness = 7f;
        private const float RingEdgeSoftness = 2f;

        private const float FillScale = 0.68f;
        private const float FillDotDiameter = 14f;

        [SerializeField] private RectTransform _canvasRect;
        [SerializeField] private Image _brushCircle;
        [SerializeField] private Image _brushFill;
        [SerializeField] private Image _pipetteIcon;
        [SerializeField] private Image _fillIcon;
        [SerializeField] private RectTransform[] _pipetteZones;

        private DrawingSessionService _session;
        private CanvasSize _canvasSize;
        private Texture2D _ringTexture;
        private Sprite _ringSprite;
        private Texture2D _discTexture;
        private Sprite _discSprite;
        private bool _cursorHidden;

        [Inject]
        public void Construct(DrawingSessionService session, CanvasSize canvasSize)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
            _canvasSize = canvasSize ?? throw new ArgumentNullException(nameof(canvasSize));
        }

        private void Awake()
        {
            _ringSprite = CreateRingSprite();
            _brushCircle.sprite = _ringSprite;
            _discSprite = CreateDiscSprite();
            _brushFill.sprite = _discSprite;
            _brushCircle.raycastTarget = false;
            _brushFill.raycastTarget = false;
            _pipetteIcon.raycastTarget = false;
            _fillIcon.raycastTarget = false;
            HideAll();
        }

        private void OnDisable()
        {
            HideAll();
        }

        private void OnDestroy()
        {
            RestoreCursor();

            if (_ringSprite != null)
            {
                Destroy(_ringSprite);
            }

            if (_ringTexture != null)
            {
                Destroy(_ringTexture);
            }

            if (_discSprite != null)
            {
                Destroy(_discSprite);
            }

            if (_discTexture != null)
            {
                Destroy(_discTexture);
            }
        }

        private void Update()
        {
            if (Input.touchCount > 0 || _session.IsDrawingLocked)
            {
                HideAll();
                return;
            }

            Vector2 screen = Input.mousePosition;
            bool overCanvas = RectTransformUtility.RectangleContainsScreenPoint(_canvasRect, screen);
            RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)transform, screen, null, out Vector2 local);

            switch (_session.ActiveToolMode)
            {
                case ToolMode.Brush:
                case ToolMode.Eraser:
                    if (overCanvas)
                    {
                        ShowCircle(local);
                    }
                    else
                    {
                        HideAll();
                    }

                    break;
                case ToolMode.ColorPicker:
                    if (overCanvas || IsOverPipetteZone(screen))
                    {
                        ShowIcon(_pipetteIcon, local);
                    }
                    else
                    {
                        HideAll();
                    }

                    break;
                case ToolMode.Fill:
                    if (overCanvas)
                    {
                        ShowFillCursor(local);
                    }
                    else
                    {
                        HideAll();
                    }

                    break;
                default:
                    HideAll();
                    break;
            }
        }

        private bool IsOverPipetteZone(Vector2 screen)
        {
            for (int i = 0; i < _pipetteZones.Length; i++)
            {
                RectTransform zone = _pipetteZones[i];

                if (zone != null && zone.gameObject.activeInHierarchy && RectTransformUtility.RectangleContainsScreenPoint(zone, screen))
                {
                    return true;
                }
            }

            return false;
        }

        private void ShowCircle(Vector2 local)
        {
            EnsureTopmost();
            _pipetteIcon.gameObject.SetActive(false);
            _fillIcon.gameObject.SetActive(false);
            _brushCircle.gameObject.SetActive(true);
            _brushFill.gameObject.SetActive(true);

            float radiusCanvasPixels = BrushSpec.Radii[_session.ActiveSizeIndex] * CanvasSpec.MaxResolution;
            float diameter = radiusCanvasPixels * 2f * _canvasRect.rect.width / _canvasSize.Width;
            _brushCircle.rectTransform.sizeDelta = new Vector2(diameter, diameter);
            _brushCircle.rectTransform.localPosition = local;

            float fillDiameter = diameter * FillScale;
            _brushFill.rectTransform.sizeDelta = new Vector2(fillDiameter, fillDiameter);
            _brushFill.rectTransform.localPosition = local;
            PaletteColor color = _session.ActiveToolMode == ToolMode.Eraser
                ? _session.Palette[CanvasSpec.BackgroundColorIndex]
                : _session.ActiveColor;
            _brushFill.color = new Color32(color.R, color.G, color.B, 235);
            HideCursor();
        }

        private void ShowIcon(Image icon, Vector2 local)
        {
            EnsureTopmost();
            _brushCircle.gameObject.SetActive(false);
            _brushFill.gameObject.SetActive(false);
            _pipetteIcon.gameObject.SetActive(icon == _pipetteIcon);
            _fillIcon.gameObject.SetActive(icon == _fillIcon);
            icon.rectTransform.localPosition = local;
            HideCursor();
        }

        private void ShowFillCursor(Vector2 local)
        {
            EnsureTopmost();
            _brushCircle.gameObject.SetActive(false);
            _pipetteIcon.gameObject.SetActive(false);

            _brushFill.gameObject.SetActive(true);
            _brushFill.rectTransform.sizeDelta = new Vector2(FillDotDiameter, FillDotDiameter);
            _brushFill.rectTransform.localPosition = local;
            PaletteColor color = _session.ActiveColor;
            _brushFill.color = new Color32(color.R, color.G, color.B, 255);

            _fillIcon.gameObject.SetActive(true);
            Vector2 iconSize = _fillIcon.rectTransform.rect.size;
            _fillIcon.rectTransform.localPosition = local + new Vector2(iconSize.x * 0.5f, iconSize.y * 0.5f);

            HideCursor();
        }

        private void HideAll()
        {
            _brushCircle.gameObject.SetActive(false);
            _brushFill.gameObject.SetActive(false);
            _pipetteIcon.gameObject.SetActive(false);
            _fillIcon.gameObject.SetActive(false);
            RestoreCursor();
        }

        private void EnsureTopmost()
        {
            if (transform.GetSiblingIndex() != transform.parent.childCount - 1)
            {
                transform.SetAsLastSibling();
            }
        }

        private void HideCursor()
        {
            if (_cursorHidden == false)
            {
                Cursor.visible = false;
                _cursorHidden = true;
            }
        }

        private void RestoreCursor()
        {
            if (_cursorHidden)
            {
                Cursor.visible = true;
                _cursorHidden = false;
            }
        }

        private Sprite CreateRingSprite()
        {
            _ringTexture = new Texture2D(RingTextureSize, RingTextureSize, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear
            };

            float center = (RingTextureSize - 1) * 0.5f;
            float outerRadius = center - 1f;
            float innerRadius = outerRadius - RingThickness;
            Color32[] pixels = new Color32[RingTextureSize * RingTextureSize];

            for (int y = 0; y < RingTextureSize; y++)
            {
                for (int x = 0; x < RingTextureSize; x++)
                {
                    float distance = Mathf.Sqrt((x - center) * (x - center) + (y - center) * (y - center));
                    float outerAlpha = Mathf.Clamp01((outerRadius - distance) / RingEdgeSoftness);
                    float innerAlpha = Mathf.Clamp01((distance - innerRadius) / RingEdgeSoftness);
                    byte alpha = (byte)(255f * outerAlpha * innerAlpha);
                    pixels[y * RingTextureSize + x] = new Color32(255, 255, 255, alpha);
                }
            }

            _ringTexture.SetPixels32(pixels);
            _ringTexture.Apply(false);

            return Sprite.Create(_ringTexture, new Rect(0f, 0f, RingTextureSize, RingTextureSize), new Vector2(0.5f, 0.5f));
        }

        private Sprite CreateDiscSprite()
        {
            _discTexture = new Texture2D(RingTextureSize, RingTextureSize, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear
            };

            float center = (RingTextureSize - 1) * 0.5f;
            float radius = center - 1f;
            Color32[] pixels = new Color32[RingTextureSize * RingTextureSize];

            for (int y = 0; y < RingTextureSize; y++)
            {
                for (int x = 0; x < RingTextureSize; x++)
                {
                    float distance = Mathf.Sqrt((x - center) * (x - center) + (y - center) * (y - center));
                    byte alpha = (byte)(255f * Mathf.Clamp01((radius - distance) / RingEdgeSoftness));
                    pixels[y * RingTextureSize + x] = new Color32(255, 255, 255, alpha);
                }
            }

            _discTexture.SetPixels32(pixels);
            _discTexture.Apply(false);

            return Sprite.Create(_discTexture, new Rect(0f, 0f, RingTextureSize, RingTextureSize), new Vector2(0.5f, 0.5f));
        }
    }
}
