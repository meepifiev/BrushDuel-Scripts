using System;
using _Project.Application.Purchasing;

namespace _Project.Infrastructure.Steam
{
    public class SteamPurchaseService : IPurchaseService
    {
        public event Action Changed
        {
            add
            {
            }
            remove
            {
            }
        }

        public bool AdsDisabled => true;

        public string RemoveAdsPrice => string.Empty;

        public void BuyRemoveAds()
        {
        }
    }
}
