using System;
using System.Collections.Generic;
using _Project.Presentation.Audio;
using DG.Tweening;
using UnityEngine;
using VContainer;

namespace _Project.Presentation.Popups
{
    public class PopupNavigator : MonoBehaviour
    {
        private readonly Stack<PopupView> _history = new Stack<PopupView>();

        [SerializeField] private CanvasGroup _dimPanel;
        [SerializeField] private float _dimFadeDuration = 0.2f;

        private PopupView _current;
        private IAudioService _audioService;
        private Tween _dimAnimation;
        private bool _dimForced;
        private bool _openState;

        public bool HasOpenPopup => _current != null;

        public event Action<bool> OpenStateChanged;

        [Inject]
        public void Construct(IAudioService audioService)
        {
            _audioService = audioService ?? throw new ArgumentNullException(nameof(audioService));
        }

        private void Start()
        {
            foreach (PopupView popup in GetComponentsInChildren<PopupView>(true))
            {
                popup.HideInstant();
            }

            if (_dimForced)
            {
                return;
            }

            _dimPanel.alpha = 0f;
            _dimPanel.gameObject.SetActive(false);
        }

        public void ShowDim()
        {
            _dimForced = true;
            SetDimVisible(true);
        }

        public void HideDim()
        {
            _dimForced = false;
            SetDimVisible(_current != null);
        }

        public void Open(PopupView popup)
        {
            if (popup == null || popup == _current)
            {
                return;
            }

            if (_current != null)
            {
                _history.Push(_current);
                _current.Hide();
            }

            _current = popup;
            popup.Show();
            PlaySound(popup.OpenSound);
            SetDimVisible(true);
        }

        public void Replace(PopupView popup)
        {
            if (popup == null || popup == _current)
            {
                return;
            }

            if (_current != null)
            {
                _current.Hide();
            }

            _current = popup;
            popup.Show();
            PlaySound(popup.OpenSound);
            SetDimVisible(true);
        }

        public void Back()
        {
            if (_current == null)
            {
                return;
            }

            PlaySound(_current.CloseSound);
            _current.Hide();
            _current = _history.Count > 0 ? _history.Pop() : null;

            if (_current != null)
            {
                _current.Show();
            }

            SetDimVisible(_current != null);
        }

        public void CloseAll()
        {
            if (_current == null)
            {
                return;
            }

            PlaySound(_current.CloseSound);
            _current.Hide();
            _current = null;
            _history.Clear();
            SetDimVisible(false);
        }

        private void PlaySound(AudioAsset asset)
        {
            if (asset == null)
            {
                return;
            }

            _audioService.PlayOneShot(asset);
        }

        private void RaiseOpenState()
        {
            bool open = _current != null;

            if (open == _openState)
            {
                return;
            }

            _openState = open;
            OpenStateChanged?.Invoke(open);
        }

        private void SetDimVisible(bool visible)
        {
            RaiseOpenState();

            if (_dimAnimation != null)
            {
                _dimAnimation.Kill();
            }

            if (visible && _dimPanel.gameObject.activeSelf == false)
            {
                _dimPanel.gameObject.SetActive(true);
            }

            _dimAnimation = _dimPanel.DOFade(visible ? 1f : 0f, _dimFadeDuration)
                .OnComplete(() =>
                {
                    if (visible == false)
                    {
                        _dimPanel.gameObject.SetActive(false);
                    }
                })
                .SetUpdate(true)
                .SetLink(_dimPanel.gameObject);
        }
    }
}
