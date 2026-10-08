using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace _Project.Presentation.Menu
{
    public class PaintingsAppearAnimation : MonoBehaviour
    {
        [SerializeField] private float _minDelay;
        [SerializeField] private float _maxDelay = 0.7f;
        [SerializeField] private float _duration = 0.45f;
        [SerializeField] private float _startScale = 0.6f;
        [SerializeField] private float _rotationRange = 12f;
        [SerializeField] private Ease _ease = Ease.OutBack;

        private readonly List<CanvasGroup> _groups = new List<CanvasGroup>();
        private readonly List<Vector3> _targetRotations = new List<Vector3>();
        private readonly List<Tween> _animations = new List<Tween>();

        private void Awake()
        {
            foreach (Transform child in transform)
            {
                CanvasGroup group = child.GetComponent<CanvasGroup>();

                if (group == null)
                {
                    group = child.gameObject.AddComponent<CanvasGroup>();
                }

                _groups.Add(group);
                _targetRotations.Add(child.localEulerAngles);
            }
        }

        private void OnEnable()
        {
            KillAnimations();

            for (int i = 0; i < _groups.Count; i++)
            {
                CanvasGroup group = _groups[i];
                Transform painting = group.transform;
                Vector3 targetRotation = _targetRotations[i];

                group.alpha = 0f;
                painting.localScale = Vector3.one * _startScale;
                painting.localEulerAngles = targetRotation + new Vector3(0f, 0f, Random.Range(-_rotationRange, _rotationRange));

                float delay = Random.Range(_minDelay, _maxDelay);

                _animations.Add(group.DOFade(1f, _duration).SetDelay(delay).SetLink(gameObject));
                _animations.Add(painting.DOScale(1f, _duration).SetDelay(delay).SetEase(_ease).SetLink(gameObject));
                _animations.Add(painting.DOLocalRotate(targetRotation, _duration).SetDelay(delay).SetEase(_ease).SetLink(gameObject));
            }
        }

        private void OnDisable()
        {
            KillAnimations();

            for (int i = 0; i < _groups.Count; i++)
            {
                _groups[i].alpha = 1f;
                _groups[i].transform.localScale = Vector3.one;
                _groups[i].transform.localEulerAngles = _targetRotations[i];
            }
        }

        private void KillAnimations()
        {
            for (int i = 0; i < _animations.Count; i++)
            {
                _animations[i].Kill();
            }

            _animations.Clear();
        }
    }
}
