using System;
using _Project.Application.Advertising;
using _Project.Application.Configs;
using _Project.Application.Match;
using _Project.Application.Progress;
using _Project.Core.Drawing;
using UnityEngine;
using VContainer.Unity;

namespace _Project.Application.Drawing
{
    public class MagicBrushHandler : IStartable, ITickable, IDisposable
    {
        private const float PercentDivider = 100f;

        private readonly MagicBrushConfig _config;
        private readonly DrawingSessionService _session;
        private readonly CanvasSize _canvasSize;
        private readonly RoundHandler _roundHandler;
        private readonly IAdvertisingService _advertising;
        private readonly IProgressStorage _storage;

        private float _cooldownRemaining;
        private int _lastCooldownSecond;
        private int _remainingUses;
        private bool _roundActive;
        private bool _rewardPending;
        private bool _lastReady;

        public MagicBrushHandler(
            MagicBrushConfig config,
            DrawingSessionService session,
            CanvasSize canvasSize,
            RoundHandler roundHandler,
            IAdvertisingService advertising,
            IProgressStorage storage)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            _config = config;
            _session = session ?? throw new ArgumentNullException(nameof(session));
            _canvasSize = canvasSize ?? throw new ArgumentNullException(nameof(canvasSize));
            _roundHandler = roundHandler ?? throw new ArgumentNullException(nameof(roundHandler));
            _advertising = advertising ?? throw new ArgumentNullException(nameof(advertising));
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
        }

        public event Action<bool> ReadyChanged;
        public event Action<int> CooldownTicked;

        public bool IsReady => _roundActive && _remainingUses > 0 && _cooldownRemaining <= 0f && _rewardPending == false;

        public int CoinPrice => _config.CoinPrice;

        public bool CanAfford => _config.CoinPrice <= 0 || _storage.GetCoins() >= _config.CoinPrice;

        public void Start()
        {
            _remainingUses = _config.UsesPerRound;
            _roundHandler.IntroFinished += OnIntroFinished;
            _roundHandler.Finished += OnRoundFinished;
        }

        public void Dispose()
        {
            _roundHandler.IntroFinished -= OnIntroFinished;
            _roundHandler.Finished -= OnRoundFinished;
        }

        public void Tick()
        {
            if (_roundActive == false || _cooldownRemaining <= 0f)
            {
                return;
            }

            _cooldownRemaining -= Time.deltaTime;
            int second = Mathf.CeilToInt(Mathf.Max(0f, _cooldownRemaining));

            if (second != _lastCooldownSecond)
            {
                _lastCooldownSecond = second;
                CooldownTicked?.Invoke(second);
            }

            RaiseReadyIfChanged();
        }

        public void Request()
        {
            if (IsReady == false)
            {
                return;
            }

            if (_config.CoinPrice > 0)
            {
                if (_storage.TrySpendCoins(_config.CoinPrice))
                {
                    OnRewarded();
                }

                return;
            }

            _rewardPending = true;
            RaiseReadyIfChanged();
            _advertising.ShowRewarded(_config.RewardId, OnRewarded, OnRewardedClosed);
        }

        private void OnRewardedClosed()
        {
            _rewardPending = false;
            RaiseReadyIfChanged();
        }

        private void OnRewarded()
        {
            _rewardPending = false;

            if (_roundActive == false)
            {
                return;
            }

            int pixelBudget = (int)(_canvasSize.Width * _canvasSize.Height * _config.CoveragePercent / PercentDivider);
            _session.ApplyAssist(pixelBudget);
            _remainingUses--;
            _cooldownRemaining = _config.CooldownSeconds;
            _lastCooldownSecond = Mathf.CeilToInt(_config.CooldownSeconds);
            CooldownTicked?.Invoke(_lastCooldownSecond);
            RaiseReadyIfChanged();
        }

        private void OnIntroFinished()
        {
            _roundActive = true;
            RaiseReadyIfChanged();
        }

        private void OnRoundFinished()
        {
            _roundActive = false;
            RaiseReadyIfChanged();
        }

        private void RaiseReadyIfChanged()
        {
            bool ready = IsReady;

            if (ready == _lastReady)
            {
                return;
            }

            _lastReady = ready;
            ReadyChanged?.Invoke(ready);
        }
    }
}
