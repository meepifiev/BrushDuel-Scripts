using System;

namespace _Project.Application.Advertising
{
    public interface IAdvertisingService
    {
        void ShowInterstitial();
        void ShowRewarded(string rewardId, Action rewarded, Action closed);
    }
}
