using System;

namespace _Project.Application.Purchasing
{
    public interface IPurchaseService
    {
        bool AdsDisabled { get; }

        string RemoveAdsPrice { get; }

        event Action Changed;

        void BuyRemoveAds();
    }
}
