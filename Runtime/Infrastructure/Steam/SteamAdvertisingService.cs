using System;
using _Project.Application.Advertising;

namespace _Project.Infrastructure.Steam
{
    public class SteamAdvertisingService : IAdvertisingService
    {
        public void ShowInterstitial()
        {
        }

        public void ShowRewarded(string rewardId, Action rewarded, Action closed)
        {
            closed?.Invoke();
        }
    }
}
