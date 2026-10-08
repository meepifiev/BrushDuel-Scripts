using DG.Tweening;
using UnityEngine;

namespace _Project.Presentation.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class UiAppearAnimation : MonoBehaviour
    {
        [SerializeField] private float _delay;
        [SerializeField] private float _duration = 0.25f;
        [SerializeField] private float _startScale = 0.8f;
        [SerializeField] private Ease _ease = Ease.OutBack;
        [SerializeField] private float _popOvershoot = 1.4f;

        private CanvasGroup _canvasGroup;
        private Sequence _animation;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        private void OnEnable()
        {
            _canvasGroup.alpha = 0f;
            _canvasGroup.blocksRaycasts = false;
            transform.localScale = Vector3.one * _startScale;

            _animation = DOTween.Sequence()
                .SetDelay(_delay)
                .Join(_canvasGroup.DOFade(1f, _duration))
                .Join(transform.DOScale(1f, _duration).SetEase(_ease, _popOvershoot))
                .OnComplete(() => _canvasGroup.blocksRaycasts = true)
                .SetLink(gameObject);
        }

        private void OnDisable()
        {
            if (_animation != null)
            {
                _animation.Kill();
                _animation = null;
            }

            _canvasGroup.alpha = 1f;
            _canvasGroup.blocksRaycasts = true;
            transform.localScale = Vector3.one;
        }
    }
}
