using System;
using AssetKits.ParticleImage;
using _Project.Application.Progress;
using _Project.Presentation.Audio;
using DG.Tweening;
using TMPro;
using UnityEngine;
using VContainer;

namespace _Project.Presentation.Match
{
    public class ResultCoinBarView : MonoBehaviour
    {
        private const float CountDuration = 0.5f;

        [SerializeField] private ResultPopupView _resultPopup;
        [SerializeField] private ParticleImage _coinParticle;
        [SerializeField] private TMP_Text _amountLabel;
        [SerializeField] private AudioAsset _coinFlyingStartSound;
        [SerializeField] private AudioAsset _coinPouchSound;

        private IAudioService _audioService;
        private IProgressStorage _progressStorage;
        private Tween _countAnimation;
        private int _displayed;
        private int _fromAmount;
        private int _toAmount;
        private bool _resultPending;

        [Inject]
        public void Construct(IAudioService audioService, IProgressStorage progressStorage)
        {
            _audioService = audioService ?? throw new ArgumentNullException(nameof(audioService));
            _progressStorage = progressStorage ?? throw new ArgumentNullException(nameof(progressStorage));
            _resultPopup.RewardCounted -= OnRewardCounted;
            _resultPopup.RewardCounted += OnRewardCounted;
            _coinParticle.onLastParticleFinished.RemoveListener(OnCoinsArrived);
            _coinParticle.onLastParticleFinished.AddListener(OnCoinsArrived);
            _progressStorage.CoinsChanged -= OnCoinsChanged;
            _progressStorage.CoinsChanged += OnCoinsChanged;
            ApplyValue(_progressStorage.GetCoins());
        }

        private void OnDestroy()
        {
            _progressStorage.CoinsChanged -= OnCoinsChanged;

            if (_resultPopup != null)
            {
                _resultPopup.RewardCounted -= OnRewardCounted;
            }

            if (_coinParticle != null)
            {
                _coinParticle.onLastParticleFinished.RemoveListener(OnCoinsArrived);
            }
        }

        public void Prepare(int fromAmount, int toAmount)
        {
            _fromAmount = fromAmount;
            _toAmount = toAmount;
            _resultPending = true;
            transform.SetSiblingIndex(1);
            KillCounter();
            ApplyValue(fromAmount);
        }

        private void OnCoinsChanged()
        {
            if (_resultPending)
            {
                return;
            }

            KillCounter();
            _countAnimation = DOTween
                .To(() => _displayed, ApplyValue, _progressStorage.GetCoins(), CountDuration)
                .SetLink(gameObject);
        }

        private void OnRewardCounted()
        {
            if (_toAmount <= _fromAmount)
            {
                _resultPending = false;
                return;
            }

            _audioService.PlayOneShot(_coinFlyingStartSound);
            _coinParticle.Play();
        }

        private void OnCoinsArrived()
        {
            KillCounter();
            _countAnimation = DOTween
                .To(() => _displayed, ApplyValue, _toAmount, CountDuration)
                .OnComplete(() =>
                {
                    _resultPending = false;
                    _audioService.PlayOneShot(_coinPouchSound);
                })
                .SetLink(gameObject);
        }

        private void ApplyValue(int value)
        {
            _displayed = value;
            _amountLabel.text = value.ToString();
        }

        private void KillCounter()
        {
            if (_countAnimation == null)
            {
                return;
            }

            _countAnimation.Kill();
            _countAnimation = null;
        }
    }
}
