using System;
using _Project.Application.Localization;
using _Project.Application.Purchasing;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace _Project.Presentation.Shop
{
    public class NoAdsOrderView : MonoBehaviour
    {
        private const string ActivatedKey = "shop.noads.activated";

        [SerializeField] private Button _buyButton;
        [SerializeField] private TMP_Text _buttonLabel;

        private IPurchaseService _purchases;
        private ILocalizationService _localization;
        private bool _started;

        [Inject]
        public void Construct(IPurchaseService purchases, ILocalizationService localization)
        {
            _purchases = purchases ?? throw new ArgumentNullException(nameof(purchases));
            _localization = localization ?? throw new ArgumentNullException(nameof(localization));
        }

        private void Start()
        {
            _started = true;
            _purchases.Changed += Refresh;
            Refresh();
        }

        private void OnEnable()
        {
            if (_started)
            {
                Refresh();
            }
        }

        private void OnDestroy()
        {
            _purchases.Changed -= Refresh;
        }

        private void Refresh()
        {
            bool owned = _purchases.AdsDisabled;

            if (_buyButton != null)
            {
                _buyButton.interactable = owned == false;
            }

            if (_buttonLabel != null)
            {
                _buttonLabel.text = owned ? _localization.Get(ActivatedKey) : _purchases.RemoveAdsPrice;
            }
        }
    }
}
