using System;
using _Project.Application.Localization;
using _Project.Application.Network;
using _Project.Presentation.UI;
using TMPro;
using UnityEngine;
using VContainer;

namespace _Project.Presentation.Menu
{
    public class MatchReadyButton : ButtonHandler
    {
        [SerializeField] private TMP_Text _label;

        private IMatchmakingService _matchmaking;
        private IMatchSession _matchSession;
        private IClientConnection _connection;
        private ILocalizationService _localization;

        [Inject]
        public void Construct(
            IMatchmakingService matchmaking,
            IMatchSession matchSession,
            IClientConnection connection,
            ILocalizationService localization)
        {
            _matchmaking = matchmaking ?? throw new ArgumentNullException(nameof(matchmaking));
            _matchSession = matchSession ?? throw new ArgumentNullException(nameof(matchSession));
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
            _localization = localization ?? throw new ArgumentNullException(nameof(localization));
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            ResetState();
        }

        protected override void OnClick()
        {
            if (_matchmaking.HasMatch == false || _connection.IsConnected == false)
            {
                return;
            }

            _matchSession.SendReady();
            Button.interactable = false;
            _label.text = _localization.Get("net.youready");
        }

        private void ResetState()
        {
            Button.interactable = true;
            _label.text = _localization.Get("net.ready");
        }
    }
}
