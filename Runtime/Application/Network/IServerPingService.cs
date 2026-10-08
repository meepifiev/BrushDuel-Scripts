using System.Threading;
using _Project.Application.Configs;
using Cysharp.Threading.Tasks;

namespace _Project.Application.Network
{
    public interface IServerPingService
    {
        UniTask<int> PingAsync(ServerRegion region, CancellationToken cancellationToken);
    }
}
