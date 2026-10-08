using System;
using _Project.Application.Match;
using _Project.Application.Network;
using _Project.Application.SceneManagement;
using _Project.Presentation.UI;
using Cysharp.Threading.Tasks;
using VContainer;

namespace _Project.Presentation.Match
{
    public class MultiplayerExitButton : ButtonHandler
    {
        private MultiplayerRoundHandler _roundHandler;
        private IMatchmakingService _matchmaking;
        private ISceneLoader _sceneLoader;

        [Inject]
        public void Construct(
            MultiplayerRoundHandler roundHandler,
            IMatchmakingService matchmaking,
            ISceneLoader sceneLoader)
        {
            _roundHandler = roundHandler ?? throw new ArgumentNullException(nameof(roundHandler));
            _matchmaking = matchmaking ?? throw new ArgumentNullException(nameof(matchmaking));
            _sceneLoader = sceneLoader ?? throw new ArgumentNullException(nameof(sceneLoader));
        }

        protected override void OnClick()
        {
            Button.interactable = false;
            _roundHandler.StopLocal();
            _matchmaking.LeaveMatch();
            _sceneLoader.LoadAsync(SceneId.MainMenu).Forget();
        }
    }
}
