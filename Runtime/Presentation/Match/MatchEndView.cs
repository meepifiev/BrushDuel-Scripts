using System;
using _Project.Application.Match;
using _Project.Presentation.Popups;
using UnityEngine;
using VContainer;

namespace _Project.Presentation.Match
{
    public class MatchEndView : MonoBehaviour
    {
        [SerializeField] private PopupNavigator _navigator;
        [SerializeField] private PopupView _roundOverPopup;
        [SerializeField] private PopupView _opponentLeftPopup;
        [SerializeField] private PopupView _connectionErrorPopup;

        private MultiplayerRoundHandler _roundHandler;
        private PostRoundAnalysisPresenter _analysis;

        [Inject]
        public void Construct(MultiplayerRoundHandler roundHandler, PostRoundAnalysisPresenter analysis)
        {
            _roundHandler = roundHandler ?? throw new ArgumentNullException(nameof(roundHandler));
            _analysis = analysis ?? throw new ArgumentNullException(nameof(analysis));
            _roundHandler.OpponentLeft += OnOpponentLeft;
            _roundHandler.ConnectionLost += OnConnectionLost;
        }

        private void OnDestroy()
        {
            _roundHandler.OpponentLeft -= OnOpponentLeft;
            _roundHandler.ConnectionLost -= OnConnectionLost;
        }

        private void OnOpponentLeft()
        {
            _navigator.Open(_opponentLeftPopup);
        }

        private void OnConnectionLost()
        {
            _navigator.Open(_connectionErrorPopup);
        }
    }
}
