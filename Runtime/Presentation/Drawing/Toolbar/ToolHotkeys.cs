using System;
using System.Collections.Generic;
using _Project.Application.Drawing;
using _Project.Core.Drawing;
using UnityEngine;
using VContainer;

namespace _Project.Presentation.Drawing.Toolbar
{
    public class ToolHotkeys : MonoBehaviour
    {
        private const float WheelStepThreshold = 0.01f;

        private static readonly KeyCode[] DigitKeys =
        {
            KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4, KeyCode.Alpha5,
            KeyCode.Alpha6, KeyCode.Alpha7, KeyCode.Alpha8, KeyCode.Alpha9,
        };

        private const string HintPrefix = "Hint_";

        [SerializeField] private GridOverlayView _gridOverlay;
        [SerializeField] private Transform _paletteRoot;

        private readonly List<Action> _paletteSelectors = new List<Action>();
        private readonly List<GameObject> _hintBadges = new List<GameObject>();

        private DrawingSessionService _session;
        private bool _altFromEvents;
        private bool _hintsVisible;

        [Inject]
        public void Construct(DrawingSessionService session)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
        }

        private void Start()
        {
            CollectPaletteButtons();
            CollectHintBadges(transform);

            if (_paletteRoot != null)
            {
                CollectHintBadges(_paletteRoot);
            }

            ApplyHintsVisible(false);
        }

        private void OnGUI()
        {
            Event current = Event.current;

            if (current == null)
            {
                return;
            }

            if (current.type == EventType.KeyDown || current.type == EventType.KeyUp || current.type == EventType.Repaint || current.type == EventType.Layout)
            {
                _altFromEvents = current.alt;
            }
        }

        private void Update()
        {
            UpdateHints();

            if (_session.IsDrawingLocked)
            {
                return;
            }

            if (IsCommandHeld())
            {
                HandleSizeWheel();
                return;
            }

            if (IsAltHeld())
            {
                return;
            }

            HandleTools();
            HandleBrushes();
            HandleSize();
            HandlePalette();
            HandleGrid();
        }

        private void UpdateHints()
        {
            bool visible = IsAltHeld();

            if (visible != _hintsVisible)
            {
                ApplyHintsVisible(visible);
            }
        }

        private void ApplyHintsVisible(bool visible)
        {
            _hintsVisible = visible;

            for (int i = 0; i < _hintBadges.Count; i++)
            {
                _hintBadges[i].SetActive(visible);
            }
        }

        private void CollectHintBadges(Transform root)
        {
            foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
            {
                if (child.name.StartsWith(HintPrefix, StringComparison.Ordinal))
                {
                    _hintBadges.Add(child.gameObject);
                }
            }
        }

        private void HandleSizeWheel()
        {
            float scroll = Input.mouseScrollDelta.y;

            if (scroll > WheelStepThreshold)
            {
                ChangeSize(1);
            }
            else if (scroll < -WheelStepThreshold)
            {
                ChangeSize(-1);
            }
        }

        private void HandleTools()
        {
            if (Input.GetKeyDown(KeyCode.B))
            {
                _session.SelectToolMode(ToolMode.Brush);
            }
            else if (Input.GetKeyDown(KeyCode.E))
            {
                _session.SelectToolMode(ToolMode.Eraser);
            }
            else if (Input.GetKeyDown(KeyCode.G))
            {
                _session.SelectToolMode(ToolMode.Fill);
            }
            else if (Input.GetKeyDown(KeyCode.I))
            {
                _session.SelectToolMode(ToolMode.ColorPicker);
            }
        }

        private void HandleBrushes()
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                _session.SelectBrush(BrushType.Round);
            }
            else if (Input.GetKeyDown(KeyCode.W))
            {
                _session.SelectBrush(BrushType.Marker);
            }
            else if (Input.GetKeyDown(KeyCode.R))
            {
                _session.SelectBrush(BrushType.Spray);
            }
        }

        private void HandleSize()
        {
            if (Input.GetKeyDown(KeyCode.LeftBracket))
            {
                ChangeSize(-1);
            }
            else if (Input.GetKeyDown(KeyCode.RightBracket))
            {
                ChangeSize(1);
            }
        }

        private void ChangeSize(int delta)
        {
            int target = Mathf.Clamp(_session.ActiveSizeIndex + delta, 0, BrushSpec.SizeSteps - 1);

            if (target != _session.ActiveSizeIndex)
            {
                _session.SelectSize(target);
            }
        }

        private void HandlePalette()
        {
            int count = Mathf.Min(DigitKeys.Length, _paletteSelectors.Count);

            for (int i = 0; i < count; i++)
            {
                if (Input.GetKeyDown(DigitKeys[i]))
                {
                    _paletteSelectors[i]();
                    return;
                }
            }
        }

        private void HandleGrid()
        {
            if (_gridOverlay != null && Input.GetKeyDown(KeyCode.H))
            {
                _gridOverlay.Toggle();
            }
        }

        private void CollectPaletteButtons()
        {
            _paletteSelectors.Clear();

            if (_paletteRoot == null)
            {
                return;
            }

            foreach (Transform child in _paletteRoot)
            {
                if (child.gameObject.activeSelf == false)
                {
                    continue;
                }

                PaletteColorButton palette = child.GetComponent<PaletteColorButton>();

                if (palette != null)
                {
                    _paletteSelectors.Add(palette.Select);
                    continue;
                }

                PickedColorButton picked = child.GetComponent<PickedColorButton>();

                if (picked != null)
                {
                    _paletteSelectors.Add(picked.Select);
                }
            }
        }

        private static bool IsCommandHeld()
        {
            return Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)
                || Input.GetKey(KeyCode.LeftCommand) || Input.GetKey(KeyCode.RightCommand);
        }

        private bool IsAltHeld()
        {
            return _altFromEvents || Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt) || Input.GetKey(KeyCode.AltGr);
        }
    }
}
