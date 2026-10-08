using System.Threading;
using _Project.Application.Configs;
using Cysharp.Threading.Tasks;

namespace _Project.Application.AssetManagement
{
    public interface IReferenceCatalog
    {
        int Count { get; }

        UniTask InitAsync(CancellationToken cancellationToken);

        UniTask<ReferenceConfig> GetAtAsync(int index, CancellationToken cancellationToken);

        UniTask<ReferenceConfig> GetByIdAsync(string id, CancellationToken cancellationToken);

        bool TryGetById(string id, out ReferenceConfig reference);
    }
}
