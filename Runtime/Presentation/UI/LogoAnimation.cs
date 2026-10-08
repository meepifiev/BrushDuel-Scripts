using DG.Tweening;
using UnityEngine;

namespace _Project.Presentation.UI
{
    public class LogoAnimation : MonoBehaviour
    {
        private const float HalfSwingDurationFactor = 0.5f;

        [Header("Appear")]
        [SerializeField] private float _appearDuration = 0.5f;
        [SerializeField] private float _appearOvershoot = 1.7f;
        [SerializeField] private Ease _appearEase = Ease.OutBack;

        [Header("Pulse")]
        [SerializeField] private float _pulseScale = 1.04f;
        [SerializeField] private float _pulseDuration = 1.2f;

        [Header("Sway")]
        [SerializeField] private float _swayAngle = 3f;
        [SerializeField] private float _swayDuration = 1.8f;

        private Tween _appear;
        private Tween _pulse;
        private Sequence _sway;

        private void OnEnable()
        {
            transform.localScale = Vector3.zero;
            transform.localRotation = Quaternion.identity;

            _appear = transform.DOScale(1f, _appearDuration)
                .SetEase(_appearEase, _appearOvershoot)
                .OnComplete(StartIdle)
                .SetLink(gameObject);
        }

        private void OnDisable()
        {
            KillAll();
            transform.localScale = Vector3.one;
            transform.localRotation = Quaternion.identity;
        }

        private void StartIdle()
        {
            _pulse = transform.DOScale(_pulseScale, _pulseDuration)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetLink(gameObject);

            float halfSwingDuration = _swayDuration * HalfSwingDurationFactor;

            _sway = DOTween.Sequence()
                .Append(transform.DOLocalRotate(new Vector3(0f, 0f, _swayAngle), halfSwingDuration).SetEase(Ease.InOutSine))
                .Append(transform.DOLocalRotate(new Vector3(0f, 0f, -_swayAngle), _swayDuration).SetEase(Ease.InOutSine))
                .Append(transform.DOLocalRotate(Vector3.zero, halfSwingDuration).SetEase(Ease.InOutSine))
                .SetLoops(-1)
                .SetLink(gameObject);
        }

        private void KillAll()
        {
            _appear?.Kill();
            _pulse?.Kill();
            _sway?.Kill();
            _appear = null;
            _pulse = null;
            _sway = null;
        }
    }
}
