using System;
using System.Threading;
using _Project.Application.AssetManagement;
using _Project.Application.Configs;
using _Project.Application.Match;
using _Project.Application.Network;
using _Project.Application.SceneManagement;
using _Project.Presentation.Popups;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;

namespace _Project.Presentation.Menu
{
    public class MatchmakingView : MonoBehaviour
    {
        [SerializeField] private PopupNavigator _navigator;
        [SerializeField] private PopupView _foundPopup;
        [SerializeField] private PopupView _errorPopup;
        [SerializeField] private PopupView _updateGamePopup;
        [SerializeField] private PopupView _declinedPopup;
        [SerializeField] private PopupView _selfPlayPopup;
        [SerializeField] private PopupView[] _searchPopups;
        [SerializeField] private PopupView _loadingPopup;

        private IMatchmakingService _matchmaking;
        private IMatchSession _matchSession;
        private IClientConnection _connection;
        private IReferenceCatalog _catalog;
        private IContentLoader _contentLoader;
        private MatchLaunchContext _context;
        private ISceneLoader _sceneLoader;

        private bool _launching;
        private bool _loadingCancelled;

        [Inject]
        public void Construct(
            IMatchmakingService matchmaking,
            IMatchSession matchSession,
            IClientConnection connection,
            IReferenceCatalog catalog,
            IContentLoader contentLoader,
            MatchLaunchContext context,
            ISceneLoader sceneLoader)
        {
            _matchmaking = matchmaking ?? throw new ArgumentNullException(nameof(matchmaking));
            _matchSession = matchSession ?? throw new ArgumentNullException(nameof(matchSession));
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
            _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
            _contentLoader = contentLoader ?? throw new ArgumentNullException(nameof(contentLoader));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _sceneLoader = sceneLoader ?? throw new ArgumentNullException(nameof(sceneLoader));

            _matchmaking.MatchFound += OnMatchFound;
            _matchmaking.SelfPlayRejected += OnSelfPlayRejected;
            _matchSession.RoundStarted += OnRoundStarted;
            _matchSession.MatchCanceled += OnMatchCanceled;
            _connection.Disconnected += OnDisconnected;
        }

        private void OnDestroy()
        {
            _matchmaking.MatchFound -= OnMatchFound;
            _matchmaking.SelfPlayRejected -= OnSelfPlayRejected;
            _matchSession.RoundStarted -= OnRoundStarted;
            _matchSession.MatchCanceled -= OnMatchCanceled;
            _connection.Disconnected -= OnDisconnected;
        }

        private bool IsAnySearchPopupOpen()
        {
            for (int i = 0; i < _searchPopups.Length; i++)
            {
                if (_searchPopups[i].gameObject.activeSelf)
                {
                    return true;
                }
            }

            return false;
        }

        private void OnMatchFound(MatchInfo match)
        {
            if (_matchmaking.IsRoomMatch)
            {
                return;
            }

            _navigator.Replace(_foundPopup);
        }

        private void OnRoundStarted(float durationSeconds)
        {
            if (_launching)
            {
                return;
            }

            _launching = true;
            LaunchOnlineAsync(durationSeconds).Forget();
        }

        private async UniTaskVoid LaunchOnlineAsync(float durationSeconds)
        {
            _loadingCancelled = false;
            MatchInfo match = _matchmaking.CurrentMatch;

            if (_loadingPopup != null)
            {
                _navigator.Replace(_loadingPopup);
                _loadingPopup.Hidden += OnLoadingCancelled;
            }

            ReferenceConfig reference;

            try
            {
                reference = await _catalog.GetByIdAsync(match.PaintingId, CancellationToken.None);
            }
            catch (Exception)
            {
                if (this == null)
                {
                    return;
                }

                if (_loadingPopup != null)
                {
                    _loadingPopup.Hidden -= OnLoadingCancelled;
                }

                _launching = false;
                _matchmaking.LeaveMatch();
                _navigator.Replace(_updateGamePopup);
                return;
            }

            if (this == null)
            {
                return;
            }

            if (_loadingPopup != null)
            {
                _loadingPopup.Hidden -= OnLoadingCancelled;
            }

            if (_loadingCancelled)
            {
                _launching = false;
                return;
            }

            _context.SetupOnline(match.GameMode, reference, match.OpponentNickname, durationSeconds);
            await _sceneLoader.LoadAsync(SceneId.MultiplayerGameplay);
        }

        private void OnLoadingCancelled()
        {
            _loadingCancelled = true;
            _matchmaking.LeaveMatch();
        }

        private void OnMatchCanceled()
        {
            if (_launching || _matchmaking.LeftByLocalPlayer)
            {
                return;
            }

            _navigator.Replace(_declinedPopup);
        }

        private void OnSelfPlayRejected()
        {
            _navigator.Replace(_selfPlayPopup);
        }

        private void OnDisconnected()
        {
            if (_launching || IsAnySearchPopupOpen() == false)
            {
                return;
            }

            _navigator.Replace(_errorPopup);
        }
    }
}
