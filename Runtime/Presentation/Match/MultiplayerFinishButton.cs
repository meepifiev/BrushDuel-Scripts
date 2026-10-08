using System;
using _Project.Application.Match;
using _Project.Presentation.UI;
using VContainer;

namespace _Project.Presentation.Match
{
    public class MultiplayerFinishButton : ButtonHandler
    {
        private MultiplayerRoundHandler _roundHandler;

        [Inject]
        public void Construct(MultiplayerRoundHandler roundHandler)
        {
            _roundHandler = roundHandler ?? throw new ArgumentNullException(nameof(roundHandler));
        }

        protected override void OnClick()
        {
            _roundHandler.SubmitDone();
        }
    }
}
