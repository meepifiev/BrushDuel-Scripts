using System;
using _Project.Application.Drawing;
using _Project.Application.Localization;
using _Project.Presentation.UI;
using TMPro;
using UnityEngine;
using VContainer;

namespace _Project.Presentation.Match
{
    public class MagicBrushRewardButton : ButtonHandler
    {
        [SerializeField] private Animation _idleAnimation;
        [SerializeField] private TMP_Text _timerLabel;
        [SerializeField] private TMP_Text _priceLabel;

        private MagicBrushHandler _handler;
        private ILocalizationService _localization;

        [Inject]
        public void Construct(MagicBrushHandler handler, ILocalizationService localization)
        {
            _handler = handler ?? throw new ArgumentNullException(nameof(handler));
            _localization = localization ?? throw new ArgumentNullException(nameof(localization));
            _handler.ReadyChanged += OnReadyChanged;
            _handler.CooldownTicked += OnCooldownTicked;
        }

        private void Start()
        {
            _priceLabel.text = $"{_handler.CoinPrice} <sprite name=\"coin\">";
            ShowPrice();
            OnReadyChanged(_handler.IsReady);
        }

        private void OnDestroy()
        {
            _handler.ReadyChanged -= OnReadyChanged;
            _handler.CooldownTicked -= OnCooldownTicked;
        }

        protected override void OnClick()
        {
            _handler.Request();
        }

        private void OnReadyChanged(bool ready)
        {
            bool available = ready && _handler.CanAfford;
            Button.interactable = available;
            ApplyAnimation(available);

            if (ready)
            {
                ShowPrice();
            }
        }

        private void OnCooldownTicked(int seconds)
        {
            if (seconds <= 0)
            {
                ShowPrice();
                return;
            }

            _timerLabel.gameObject.SetActive(true);
            _priceLabel.gameObject.SetActive(false);
            _timerLabel.text = string.Format(_localization.Get("assist.cooldown"), seconds);
        }

        private void ShowPrice()
        {
            _timerLabel.gameObject.SetActive(false);
            _priceLabel.gameObject.SetActive(true);
        }

        private void ApplyAnimation(bool active)
        {
            AnimationState state = _idleAnimation[_idleAnimation.clip.name];

            if (active)
            {
                state.wrapMode = WrapMode.Loop;

                if (_idleAnimation.isPlaying == false)
                {
                    _idleAnimation.Play();
                }

                return;
            }

            state.wrapMode = WrapMode.Once;
        }
    }
}
