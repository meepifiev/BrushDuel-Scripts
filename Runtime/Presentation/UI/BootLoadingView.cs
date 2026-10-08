using System;
using _Project.Application.AssetManagement;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace _Project.Presentation.UI
{
    public class BootLoadingView : MonoBehaviour
    {
        private const float BytesPerMb = 1048576f;
        private const float SliderTweenDuration = 0.35f;

        [SerializeField] private TMP_Text _percentLabel;
        [SerializeField] private Slider _slider;

        private LoadingProgress _progress;
        private Tween _sliderTween;

        [Inject]
        public void Construct(LoadingProgress progress)
        {
            _progress = progress ?? throw new ArgumentNullException(nameof(progress));
            _progress.Changed -= OnProgressChanged;
            _progress.Changed += OnProgressChanged;

            if (_slider != null)
            {
                _slider.value = _progress.Value;
            }

            RenderLabel();
        }

        private void OnDestroy()
        {
            if (_progress != null)
            {
                _progress.Changed -= OnProgressChanged;
            }

            KillTween();
        }

        private void OnProgressChanged(float value)
        {
            RenderLabel();

            if (_slider == null)
            {
                return;
            }

            KillTween();
            _sliderTween = _slider.DOValue(value, SliderTweenDuration)
                .SetEase(Ease.OutQuad)
                .SetLink(_slider.gameObject);
        }

        private void RenderLabel()
        {
            if (_percentLabel == null)
            {
                return;
            }

            float downloadedMb = _progress.DownloadedBytes / BytesPerMb;
            float totalMb = _progress.TotalBytes / BytesPerMb;
            _percentLabel.text = downloadedMb.ToString("0.0") + " / " + totalMb.ToString("0.0") + " MB";
        }

        private void KillTween()
        {
            if (_sliderTween == null)
            {
                return;
            }

            _sliderTween.Kill();
            _sliderTween = null;
        }
    }
}
