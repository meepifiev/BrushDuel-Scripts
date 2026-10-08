using System;
using _Project.Application.Match;
using _Project.Presentation.UI;
using VContainer;

namespace _Project.Presentation.Match
{
    public class FinishRoundButton : ButtonHandler
    {
        private RoundHandler _roundHandler;

        [Inject]
        public void Construct(RoundHandler roundHandler)
        {
            _roundHandler = roundHandler ?? throw new ArgumentNullException(nameof(roundHandler));
        }

        protected override void OnClick()
        {
            _roundHandler.Finish();
        }
    }
}
