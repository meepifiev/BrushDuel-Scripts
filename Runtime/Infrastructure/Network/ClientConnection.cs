using System;
using System.Threading;
using _Project.Application.Configs;
using _Project.Application.Network;
using Cysharp.Threading.Tasks;
using Mirror;

namespace _Project.Infrastructure.Network
{
    public class ClientConnection : IClientConnection, IDisposable
    {
        private const float TimeoutSeconds = 10f;
        private const float ShutdownTimeoutSeconds = 5f;
        private const float ShutdownSettleSeconds = 0.5f;

        private readonly SelectedRegion _selectedRegion;

        private CanvasNetworkManager _subscribedManager;
        private bool _established;

        public ClientConnection(SelectedRegion selectedRegion)
        {
            _selectedRegion = selectedRegion;
        }

        public event Action Disconnected;

        public bool IsConnected => NetworkClient.isConnected && NetworkClient.connection != null && NetworkClient.connection.isAuthenticated;

        public UniTask<bool> ConnectAsync(CancellationToken cancellationToken)
        {
            return ConnectCoreAsync(StartClientForSelectedRegion, true, cancellationToken);
        }

        public UniTask<bool> ConnectToSessionAsync(string sessionPort, CancellationToken cancellationToken)
        {
            return ConnectCoreAsync(manager => manager.StartClientAtSession(sessionPort), false, cancellationToken);
        }

        private void StartClientForSelectedRegion(CanvasNetworkManager manager)
        {
            ApplySelectedRegion(manager);
            manager.StartClient();
        }

        private async UniTask<bool> ConnectCoreAsync(Action<CanvasNetworkManager> startClient, bool allowReuse, CancellationToken cancellationToken)
        {
            if (allowReuse && IsConnected)
            {
                return true;
            }

            CanvasNetworkManager manager = ResolveManager();
            EnsureManagerSubscription(manager);
            _established = false;

            if (await WaitForShutdownAsync(manager, cancellationToken) == false)
            {
                return false;
            }

            bool connected = false;
            bool failed = false;
            Action onConnected = () => connected = true;
            Action onDisconnected = () => failed = true;
            manager.ClientConnected += onConnected;
            manager.ClientDisconnected += onDisconnected;

            try
            {
                startClient(manager);
                float elapsed = 0f;

                while (connected == false && failed == false && elapsed < TimeoutSeconds)
                {
                    await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
                    elapsed += UnityEngine.Time.unscaledDeltaTime;
                }
            }
            catch (OperationCanceledException)
            {
                manager.StopClient();
                throw;
            }
            finally
            {
                manager.ClientConnected -= onConnected;
                manager.ClientDisconnected -= onDisconnected;
            }

            if (connected == false)
            {
                manager.StopClient();
                return false;
            }

            _established = true;
            return true;
        }

        public void Disconnect()
        {
            if (NetworkClient.active == false)
            {
                return;
            }

            ResolveManager().StopClient();
        }

        public void Dispose()
        {
            if (_subscribedManager == null)
            {
                return;
            }

            _subscribedManager.ClientConnected -= OnManagerConnected;
            _subscribedManager.ClientDisconnected -= OnManagerDisconnected;
            _subscribedManager = null;
        }

        private async UniTask<bool> WaitForShutdownAsync(CanvasNetworkManager manager, CancellationToken cancellationToken)
        {
            if (NetworkClient.active == false)
            {
                return true;
            }

            manager.StopClient();
            float elapsed = 0f;

            while (NetworkClient.active && elapsed < ShutdownTimeoutSeconds)
            {
                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
                elapsed += UnityEngine.Time.unscaledDeltaTime;
            }

            if (NetworkClient.active)
            {
                return false;
            }

            float settle = 0f;

            while (settle < ShutdownSettleSeconds)
            {
                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
                settle += UnityEngine.Time.unscaledDeltaTime;
            }

            return true;
        }

        private void EnsureManagerSubscription(CanvasNetworkManager manager)
        {
            if (_subscribedManager == manager)
            {
                return;
            }

            if (_subscribedManager != null)
            {
                _subscribedManager.ClientConnected -= OnManagerConnected;
                _subscribedManager.ClientDisconnected -= OnManagerDisconnected;
            }

            _subscribedManager = manager;
            _subscribedManager.ClientConnected += OnManagerConnected;
            _subscribedManager.ClientDisconnected += OnManagerDisconnected;
        }

        private void ApplySelectedRegion(CanvasNetworkManager manager)
        {
            ServerRegion region = _selectedRegion.Current;

            if (region == null)
            {
                return;
            }

            manager.ApplyClientEndpoint(region.Address, region.ClientPort);
        }

        private CanvasNetworkManager ResolveManager()
        {
            if (NetworkManager.singleton is CanvasNetworkManager manager)
            {
                return manager;
            }

            throw new InvalidOperationException(nameof(NetworkManager.singleton));
        }

        private void OnManagerConnected()
        {
            _established = true;
        }

        private void OnManagerDisconnected()
        {
            if (_established == false)
            {
                return;
            }

            _established = false;
            Disconnected?.Invoke();
        }
    }
}
