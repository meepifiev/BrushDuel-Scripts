using System;
using _Project.Application.Purchasing;
using _Project.Presentation.UI;
using VContainer;

namespace _Project.Presentation.Shop
{
    public class RemoveAdsButton : ButtonHandler
    {
        private IPurchaseService _purchases;

        [Inject]
        public void Construct(IPurchaseService purchases)
        {
            _purchases = purchases ?? throw new ArgumentNullException(nameof(purchases));
        }

        protected override void OnClick()
        {
            _purchases.BuyRemoveAds();
        }
    }
}
