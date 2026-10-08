using System;
using System.Threading;
using _Project.Application.Network;
using _Project.Presentation.Popups;
using _Project.Presentation.UI;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;

namespace _Project.Presentation.Menu
{
    public abstract class NetworkFlowButton : ButtonHandler
    {
        [SerializeField] private PopupNavigator _navigator;
        [SerializeField] private PopupView _searchingPopup;
        [SerializeField] private PopupView _errorPopup;
        [SerializeField] private bool _replacesCurrent;

        private IClientConnection _connection;
        private IMatchmakingService _matchmaking;
        private CancellationTokenSource _flowCancellation;
        private bool _inFlight;

        protected IMatchmakingService Matchmaking => _matchmaking;

        [Inject]
        public void Construct(IClientConnection connection, IMatchmakingService matchmaking)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
            _matchmaking = matchmaking ?? throw new ArgumentNullException(nameof(matchmaking));
        }

        private void OnDestroy()
        {
            UnsubscribeSearching();
            CancelFlow();
        }

        protected override void OnClick()
        {
            if (_inFlight || ValidateRequest() == false)
            {
                return;
            }

            OnFlowStart();
            _inFlight = true;
            _flowCancellation = CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken);
            RunFlow(_flowCancellation.Token).Forget();
        }

        protected abstract void RequestMatch();

        protected virtual bool ValidateRequest()
        {
            return true;
        }

        protected virtual void OnFlowStart()
        {
        }

        private async UniTaskVoid RunFlow(CancellationToken cancellationToken)
        {
            if (_searchingPopup != null)
            {
                if (_replacesCurrent)
                {
                    _navigator.Replace(_searchingPopup);
                }
                else
                {
                    _navigator.Open(_searchingPopup);
                }

                _searchingPopup.Hidden += OnSearchingHidden;
            }

            bool connected;

            try
            {
                connected = await _connection.ConnectAsync(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                _inFlight = false;
                return;
            }

            if (cancellationToken.IsCancellationRequested)
            {
                _inFlight = false;
                return;
            }

            if (connected == false)
            {
                UnsubscribeSearching();
                _navigator.Replace(_errorPopup);
                _inFlight = false;
                return;
            }

            try
            {
                RequestMatch();
            }
            catch (Exception)
            {
                UnsubscribeSearching();
                _navigator.Replace(_errorPopup);
                _connection.Disconnect();
            }
            finally
            {
                _inFlight = false;
            }
        }

        private void UnsubscribeSearching()
        {
            if (_searchingPopup == null)
            {
                return;
            }

            _searchingPopup.Hidden -= OnSearchingHidden;
        }

        private void CancelFlow()
        {
            if (_flowCancellation == null)
            {
                return;
            }

            _flowCancellation.Cancel();
            _flowCancellation.Dispose();
            _flowCancellation = null;
        }

        private void OnSearchingHidden()
        {
            UnsubscribeSearching();
            CancelFlow();
            _inFlight = false;

            if (_matchmaking.HasMatch)
            {
                return;
            }

            _matchmaking.Cancel();
        }
    }
}
