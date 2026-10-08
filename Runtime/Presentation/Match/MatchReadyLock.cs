using _Project.Application.Match;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace _Project.Presentation.Match
{
    public class MatchReadyLock : MonoBehaviour
    {
        [SerializeField] private Button[] _disableOnReady;
        [SerializeField] private OpponentVisibilityToggle _opponentToggle;

        private MultiplayerRoundHandler _roundHandler;

        [Inject]
        public void Construct(MultiplayerRoundHandler roundHandler)
        {
            _roundHandler = roundHandler;
        }

        private void Start()
        {
            _roundHandler.DoneSubmitted += OnDoneSubmitted;
        }

        private void OnDestroy()
        {
            if (_roundHandler != null)
            {
                _roundHandler.DoneSubmitted -= OnDoneSubmitted;
            }
        }

        private void OnDoneSubmitted()
        {
            if (_opponentToggle != null)
            {
                _opponentToggle.ForceShowOpponent();
            }

            for (int i = 0; i < _disableOnReady.Length; i++)
            {
                if (_disableOnReady[i] != null)
                {
                    _disableOnReady[i].interactable = false;
                }
            }
        }
    }
}
