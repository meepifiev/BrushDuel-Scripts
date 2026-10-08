using System;
using _Project.Application.Advertising;
using VContainer;

namespace _Project.Presentation.UI
{
    public class InterstitialAdButton : ButtonHandler
    {
        private IAdvertisingService _advertising;

        [Inject]
        public void Construct(IAdvertisingService advertising)
        {
            _advertising = advertising ?? throw new ArgumentNullException(nameof(advertising));
        }

        protected override void OnClick()
        {
            _advertising.ShowInterstitial();
        }
    }
}
