using System;
using _Project.Core.Scoring;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Presentation.Drawing
{
    public enum AnalysisOverlayStyle
    {
        Hatch,
        Solid
    }

    public class CanvasAnalysisOverlay : IDisposable
    {
        private const float LineHeight = 6f;
        private const int HatchScale = 3;
        private const int HatchPeriod = 7;
        private const int HatchThickness = 2;

        private static readonly Color32 CorrectColor = new Color32(70, 200, 100, 190);
        private static readonly Color32 WrongColor = new Color32(230, 60, 50, 210);
        private static readonly Color32 MissedColor = new Color32(120, 100, 220, 200);
        private static readonly Color32 EmptyColor = new Color32(0, 0, 0, 0);
        private static readonly Color LineColor = new Color32(235, 60, 50, 255);

        private readonly RectTransform _target;
        private readonly bool _withScanline;
        private readonly AnalysisOverlayStyle _style;

        private RectTransform _maskRect;
        private RawImage _overlayImage;
        private RectTransform _lineRect;
        private Texture2D _texture;
        private Tween _sweep;

        public CanvasAnalysisOverlay(RectTransform target, bool withScanline, AnalysisOverlayStyle style)
        {
            _target = target != null ? target : throw new ArgumentNullException(nameof(target));
            _withScanline = withScanline;
            _style = style;
        }

        public async UniTask PlayAsync(AccuracyMap map, float duration)
        {
            EnsureBuilt(map.Grid);
            FillTexture(map);

            float width = _target.rect.width;
            float height = _target.rect.height;
            _overlayImage.rectTransform.sizeDelta = new Vector2(width, height);
            _maskRect.sizeDelta = new Vector2(width, 0f);

            if (_lineRect != null)
            {
                _lineRect.gameObject.SetActive(true);
                _lineRect.anchoredPosition = Vector2.zero;
            }

            KillSweep();
            _sweep = DOTween
                .To(() => _maskRect.sizeDelta.y, value =>
                {
                    _maskRect.sizeDelta = new Vector2(width, value);

                    if (_lineRect != null)
                    {
                        _lineRect.anchoredPosition = new Vector2(0f, -value);
                    }
                }, height, duration)
                .SetEase(Ease.Linear)
                .SetLink(_maskRect.gameObject);

            while (_sweep != null && _sweep.IsActive() && _sweep.IsComplete() == false)
            {
                await UniTask.Yield();
            }

            if (_lineRect != null)
            {
                _lineRect.gameObject.SetActive(false);
            }
        }

        public void Dispose()
        {
            KillSweep();

            if (_texture != null)
            {
                UnityEngine.Object.Destroy(_texture);
                _texture = null;
            }

            if (_maskRect != null)
            {
                UnityEngine.Object.Destroy(_maskRect.gameObject);
                _maskRect = null;
            }

            if (_lineRect != null)
            {
                UnityEngine.Object.Destroy(_lineRect.gameObject);
                _lineRect = null;
            }
        }

        private void EnsureBuilt(AnalysisGrid grid)
        {
            if (_maskRect != null)
            {
                return;
            }

            GameObject maskGo = new GameObject("AnalysisReveal", typeof(RectTransform), typeof(RectMask2D));
            _maskRect = (RectTransform)maskGo.transform;
            _maskRect.SetParent(_target, false);
            _maskRect.anchorMin = new Vector2(0.5f, 1f);
            _maskRect.anchorMax = new Vector2(0.5f, 1f);
            _maskRect.pivot = new Vector2(0.5f, 1f);
            _maskRect.anchoredPosition = Vector2.zero;

            GameObject imageGo = new GameObject("AnalysisOverlay", typeof(RectTransform));
            RectTransform imageRect = (RectTransform)imageGo.transform;
            imageRect.SetParent(_maskRect, false);
            imageRect.anchorMin = new Vector2(0.5f, 1f);
            imageRect.anchorMax = new Vector2(0.5f, 1f);
            imageRect.pivot = new Vector2(0.5f, 1f);
            imageRect.anchoredPosition = Vector2.zero;
            _overlayImage = imageGo.AddComponent<RawImage>();
            _overlayImage.raycastTarget = false;

            _texture = new Texture2D(grid.Width * HatchScale, grid.Height * HatchScale, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
            _overlayImage.texture = _texture;

            if (_withScanline)
            {
                GameObject lineGo = new GameObject("AnalysisScanline", typeof(RectTransform));
                _lineRect = (RectTransform)lineGo.transform;
                _lineRect.SetParent(_target, false);
                _lineRect.anchorMin = new Vector2(0f, 1f);
                _lineRect.anchorMax = new Vector2(1f, 1f);
                _lineRect.pivot = new Vector2(0.5f, 1f);
                _lineRect.sizeDelta = new Vector2(0f, LineHeight);
                Image line = lineGo.AddComponent<Image>();
                line.color = LineColor;
                line.raycastTarget = false;
                lineGo.SetActive(false);
            }
        }

        private void FillTexture(AccuracyMap map)
        {
            AnalysisGrid grid = map.Grid;
            int width = grid.Width * HatchScale;
            int height = grid.Height * HatchScale;
            Color32[] pixels = new Color32[width * height];

            for (int y = 0; y < height; y++)
            {
                int cellRow = (y / HatchScale) * grid.Width;

                for (int x = 0; x < width; x++)
                {
                    int index = y * width + x;

                    if (_style == AnalysisOverlayStyle.Hatch && (x + y) % HatchPeriod >= HatchThickness)
                    {
                        pixels[index] = EmptyColor;
                        continue;
                    }

                    Color32 color;

                    switch (map.Verdicts[cellRow + x / HatchScale])
                    {
                        case CellVerdict.Correct:
                            color = CorrectColor;
                            break;
                        case CellVerdict.WrongColor:
                        case CellVerdict.Overpaint:
                            color = WrongColor;
                            break;
                        case CellVerdict.Missed:
                            color = MissedColor;
                            break;
                        default:
                            color = EmptyColor;
                            break;
                    }

                    if (_style == AnalysisOverlayStyle.Solid)
                    {
                        color.a = (byte)(color.a / 2);
                    }

                    pixels[index] = color;
                }
            }

            _texture.SetPixels32(pixels);
            _texture.Apply(false);
        }

        private void KillSweep()
        {
            if (_sweep == null)
            {
                return;
            }

            _sweep.Kill();
            _sweep = null;
        }
    }
}
