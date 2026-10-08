using System;
using _Project.Application.Configs;
using _Project.Application.Localization;
using _Project.Presentation.UI;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace _Project.Presentation.Menu
{
    [RequireComponent(typeof(Button))]
    [RequireComponent(typeof(CanvasGroup))]
    public class PaintingCardView : MonoBehaviour
    {
        private const float AppearDuration = 0.25f;
        private const float HiddenScale = 0.7f;

        private readonly RectAspectFitter _aspectFitter = new RectAspectFitter();

        [SerializeField] private RawImage _preview;
        [SerializeField] private TMP_Text _nameLabel;
        [SerializeField] private GameObject _gradeBadge;
        [SerializeField] private TMP_Text _gradeLabel;

        private Button _button;
        private CanvasGroup _canvasGroup;
        private ILocalizationService _localization;
        private Sequence _appearAnimation;
        private Vector2 _previewMaxSize;
        private ReferenceConfig _reference;

        public event Action<ReferenceConfig> Clicked;

        [Inject]
        public void Construct(ILocalizationService localization)
        {
            _localization = localization ?? throw new ArgumentNullException(nameof(localization));
        }

        private void Awake()
        {
            _button = GetComponent<Button>();
            _canvasGroup = GetComponent<CanvasGroup>();
            _previewMaxSize = _preview.rectTransform.sizeDelta;
        }

        private void OnEnable()
        {
            _button.onClick.AddListener(OnClick);
        }

        private void OnDisable()
        {
            _button.onClick.RemoveListener(OnClick);
        }

        public void Show(ReferenceConfig reference, string gradeLabel)
        {
            _reference = reference;
            gameObject.SetActive(true);
            _preview.texture = reference.Picture;
            _preview.rectTransform.sizeDelta = _previewMaxSize;
            _aspectFitter.FitInside(_preview.rectTransform, (float)reference.Picture.width / reference.Picture.height);
            _nameLabel.text = _localization.Localize(reference.NameRussian, reference.NameEnglish);

            bool hasGrade = string.IsNullOrEmpty(gradeLabel) == false;
            _gradeBadge.SetActive(hasGrade);

            if (hasGrade)
            {
                _gradeLabel.text = gradeLabel;
            }
        }

        public void ShowEmpty()
        {
            _reference = null;
            gameObject.SetActive(false);
        }

        public void PlayAppear(float delay)
        {
            if (_appearAnimation != null)
            {
                _appearAnimation.Kill();
            }

            _canvasGroup.alpha = 0f;
            transform.localScale = Vector3.one * HiddenScale;

            _appearAnimation = DOTween.Sequence()
                .AppendInterval(delay)
                .Append(_canvasGroup.DOFade(1f, AppearDuration))
                .Join(transform.DOScale(1f, AppearDuration).SetEase(Ease.OutBack))
                .SetLink(gameObject);
        }

        private void OnClick()
        {
            if (_reference == null)
            {
                return;
            }

            Clicked?.Invoke(_reference);
        }
    }
}
