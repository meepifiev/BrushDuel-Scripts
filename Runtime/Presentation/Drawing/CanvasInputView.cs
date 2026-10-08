using System;
using _Project.Core.Drawing;
using UnityEngine;

namespace _Project.Presentation.Drawing
{
    public class CanvasInputView : MonoBehaviour
    {
        [SerializeField] private RectTransform _canvasRect;

        private bool _isPointerDown;
        private bool _wasInside;
        private Vector2 _lastRaw;

        public event Action<CanvasPoint> PointerPressed;
        public event Action<CanvasPoint> PointerMoved;
        public event Action PointerReleased;

        private void Update()
        {
            if (Input.touchCount > 0)
            {
                HandleTouch(Input.GetTouch(0));
            }
            else
            {
                HandleMouse();
            }
        }

        private void HandleTouch(Touch touch)
        {
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    TryPress(touch.position);
                    break;
                case TouchPhase.Moved:
                case TouchPhase.Stationary:
                    TryMove(touch.position);
                    break;
                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    Release();
                    break;
            }
        }

        private void HandleMouse()
        {
            if (Input.GetMouseButtonDown(0))
            {
                TryPress(Input.mousePosition);
            }
            else if (Input.GetMouseButton(0))
            {
                TryMove(Input.mousePosition);
            }
            else if (_isPointerDown)
            {
                Release();
            }
        }

        private void TryPress(Vector2 screenPosition)
        {
            Vector2 raw = ToNormalized(screenPosition);

            if (IsInside(raw) == false)
            {
                return;
            }

            _isPointerDown = true;
            _wasInside = true;
            _lastRaw = raw;
            PointerPressed?.Invoke(new CanvasPoint(raw.x, raw.y));
        }

        private void TryMove(Vector2 screenPosition)
        {
            if (_isPointerDown == false)
            {
                return;
            }

            Vector2 raw = ToNormalized(screenPosition);
            bool inside = IsInside(raw);

            if (inside)
            {
                if (_wasInside == false)
                {
                    Vector2 entry = ClipToEdge(raw, _lastRaw);
                    PointerMoved?.Invoke(new CanvasPoint(entry.x, entry.y));
                }

                PointerMoved?.Invoke(new CanvasPoint(raw.x, raw.y));
            }
            else if (_wasInside)
            {
                Vector2 exit = ClipToEdge(_lastRaw, raw);
                PointerMoved?.Invoke(new CanvasPoint(exit.x, exit.y));
            }

            _wasInside = inside;
            _lastRaw = raw;
        }

        private void Release()
        {
            if (_isPointerDown == false)
            {
                return;
            }

            _isPointerDown = false;
            _wasInside = false;
            PointerReleased?.Invoke();
        }

        private Vector2 ToNormalized(Vector2 screenPosition)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_canvasRect, screenPosition, null, out Vector2 local);
            Rect rect = _canvasRect.rect;
            return new Vector2((local.x - rect.xMin) / rect.width, (local.y - rect.yMin) / rect.height);
        }

        private static bool IsInside(Vector2 p)
        {
            return p.x >= 0f && p.x <= 1f && p.y >= 0f && p.y <= 1f;
        }

        private static Vector2 ClipToEdge(Vector2 inside, Vector2 outside)
        {
            Vector2 delta = outside - inside;
            float t = 1f;

            if (delta.x > 0f) t = Mathf.Min(t, (1f - inside.x) / delta.x);
            else if (delta.x < 0f) t = Mathf.Min(t, -inside.x / delta.x);

            if (delta.y > 0f) t = Mathf.Min(t, (1f - inside.y) / delta.y);
            else if (delta.y < 0f) t = Mathf.Min(t, -inside.y / delta.y);

            Vector2 hit = inside + delta * Mathf.Clamp01(t);
            return new Vector2(Mathf.Clamp01(hit.x), Mathf.Clamp01(hit.y));
        }
    }
}
