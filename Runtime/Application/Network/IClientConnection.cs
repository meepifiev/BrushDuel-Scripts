using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace _Project.Application.Network
{
    public interface IClientConnection
    {
        event Action Disconnected;

        bool IsConnected { get; }
        UniTask<bool> ConnectAsync(CancellationToken cancellationToken);
        UniTask<bool> ConnectToSessionAsync(string sessionPort, CancellationToken cancellationToken);
        void Disconnect();
    }
}
