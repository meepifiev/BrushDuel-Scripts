using System;
using _Project.Application.Localization;
using TMPro;
using UnityEngine;
using VContainer;

namespace _Project.Presentation.UI
{
    public class LoadingDots : MonoBehaviour
    {
        [SerializeField] private TMP_Text _label;
        [SerializeField] private string _key = "common.loading";
        [SerializeField] private float _interval = 0.4f;
        [SerializeField] private int _maxDots = 3;

        private ILocalizationService _localization;
        private string _baseText = "Загрузка";
        private float _timer;
        private int _dots;

        [Inject]
        public void Construct(ILocalizationService localization)
        {
            _localization = localization ?? throw new ArgumentNullException(nameof(localization));
        }

        private void OnEnable()
        {
            _baseText = _localization.Get(_key);
            _timer = 0f;
            _dots = 0;
            Apply();
        }

        private void Update()
        {
            _timer += Time.unscaledDeltaTime;

            if (_timer < _interval)
            {
                return;
            }

            _timer = 0f;
            _dots = (_dots + 1) % (_maxDots + 1);
            Apply();
        }

        private void Apply()
        {
            if (_label != null)
            {
                _label.text = _baseText + new string('.', _dots);
            }
        }
    }
}
