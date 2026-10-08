using System;
using _Project.Presentation.Audio;
using DG.Tweening;
using UnityEngine;

namespace _Project.Presentation.Popups
{
    [RequireComponent(typeof(CanvasGroup))]
    public class PopupView : MonoBehaviour
    {
        [Header("Animation")]
        [SerializeField] private float _showDuration = 0.25f;
        [SerializeField] private float _hideDuration = 0.15f;
        [SerializeField] private float _hiddenScale = 0.85f;
        [SerializeField] private Ease _showEase = Ease.OutBack;
        [SerializeField] private Ease _hideEase = Ease.InSine;

        [Header("Audio (optional)")]
        [SerializeField] private AudioAsset _openSound;
        [SerializeField] private AudioAsset _closeSound;

        private CanvasGroup _canvasGroup;
        private Sequence _activeAnimation;

        public event Action Shown;
        public event Action Hidden;

        public AudioAsset OpenSound => _openSound;
        public AudioAsset CloseSound => _closeSound;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        public void Show()
        {
            KillActiveAnimation();

            if (gameObject.activeSelf)
            {
                gameObject.SetActive(false);
            }

            gameObject.SetActive(true);
            _canvasGroup.blocksRaycasts = true;
            _canvasGroup.alpha = 0f;
            transform.localScale = Vector3.one * _hiddenScale;

            _activeAnimation = DOTween.Sequence()
                .Join(_canvasGroup.DOFade(1f, _showDuration))
                .Join(transform.DOScale(1f, _showDuration).SetEase(_showEase))
                .SetUpdate(true)
                .SetLink(gameObject);
            Shown?.Invoke();
        }

        public void Hide()
        {
            if (gameObject.activeSelf == false)
            {
                return;
            }

            KillActiveAnimation();
            _canvasGroup.blocksRaycasts = false;

            _activeAnimation = DOTween.Sequence()
                .Join(_canvasGroup.DOFade(0f, _hideDuration))
                .Join(transform.DOScale(_hiddenScale, _hideDuration).SetEase(_hideEase))
                .OnComplete(() => gameObject.SetActive(false))
                .SetUpdate(true)
                .SetLink(gameObject);
            Hidden?.Invoke();
        }

        public void HideInstant()
        {
            KillActiveAnimation();
            transform.localScale = Vector3.one;
            gameObject.SetActive(false);
            Hidden?.Invoke();
        }

        private void KillActiveAnimation()
        {
            if (_activeAnimation == null)
            {
                return;
            }

            _activeAnimation.Kill();
            _activeAnimation = null;
        }
    }
}
