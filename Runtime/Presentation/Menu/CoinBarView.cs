using System;
using _Project.Application.Progress;
using _Project.Presentation.Popups;
using DG.Tweening;
using TMPro;
using UnityEngine;
using VContainer;

namespace _Project.Presentation.Menu
{
    public class CoinBarView : MonoBehaviour
    {
        private const float CountDuration = 0.6f;
        private const float CountDelay = 0.25f;

        [SerializeField] private PopupView _shopPopup;
        [SerializeField] private TMP_Text _amountLabel;

        private IProgressStorage _progressStorage;
        private Tween _countAnimation;
        private int _displayed;

        [Inject]
        public void Construct(IProgressStorage progressStorage)
        {
            _progressStorage = progressStorage ?? throw new ArgumentNullException(nameof(progressStorage));
            _progressStorage.CoinsChanged += OnCoinsChanged;

            if (_shopPopup != null)
            {
                _shopPopup.Shown += OnShopShown;
                _shopPopup.Hidden += OnShopHidden;
                return;
            }

            _displayed = _progressStorage.GetCoins();
            _amountLabel.text = _displayed.ToString();
        }

        private void Awake()
        {
            if (_shopPopup != null)
            {
                gameObject.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            _progressStorage.CoinsChanged -= OnCoinsChanged;

            if (_shopPopup != null)
            {
                _shopPopup.Shown -= OnShopShown;
                _shopPopup.Hidden -= OnShopHidden;
            }
        }

        private void OnCoinsChanged()
        {
            int target = _progressStorage.GetCoins();

            if (gameObject.activeInHierarchy == false)
            {
                _displayed = target;
                _amountLabel.text = target.ToString();
                return;
            }

            KillCounter();
            _countAnimation = DOTween
                .To(() => _displayed, ApplyValue, target, CountDuration)
                .SetLink(gameObject);
        }

        private void OnShopShown()
        {
            gameObject.SetActive(true);
            KillCounter();
            _displayed = 0;
            _amountLabel.text = "0";
            _countAnimation = DOTween
                .To(() => _displayed, ApplyValue, _progressStorage.GetCoins(), CountDuration)
                .SetDelay(CountDelay)
                .SetLink(gameObject);
        }

        private void OnShopHidden()
        {
            KillCounter();
            gameObject.SetActive(false);
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
