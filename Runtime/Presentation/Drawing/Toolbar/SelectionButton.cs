using System;
using _Project.Application.Drawing;
using _Project.Presentation.UI;
using UnityEngine;
using VContainer;

namespace _Project.Presentation.Drawing.Toolbar
{
    public abstract class SelectionButton : ButtonHandler
    {
        private const float SelectedScale = 1.15f;

        [SerializeField] private GameObject _selectionBackground;
        [SerializeField] private float _selectedLift;

        private Vector2 _basePosition;
        private bool _baseCaptured;

        protected DrawingSessionService Session { get; private set; }

        [Inject]
        public void Construct(DrawingSessionService session)
        {
            Session = session ?? throw new ArgumentNullException(nameof(session));
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            Session.ToolStateChanged += Refresh;
            Refresh();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            Session.ToolStateChanged -= Refresh;
        }

        protected override void OnClick()
        {
            HandleClick();
        }

        protected abstract void HandleClick();

        protected abstract bool IsSelected();

        protected virtual void Refresh()
        {
            bool selected = IsSelected();
            transform.localScale = selected ? Vector3.one * SelectedScale : Vector3.one;

            if (_selectionBackground != null)
            {
                _selectionBackground.SetActive(selected);
            }

            if (_selectedLift > 0f)
            {
                RectTransform rect = (RectTransform)transform;

                if (_baseCaptured == false)
                {
                    _basePosition = rect.anchoredPosition;
                    _baseCaptured = true;
                }

                rect.anchoredPosition = selected ? _basePosition + new Vector2(0f, _selectedLift) : _basePosition;
            }
        }
    }
}
