using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace _Project.Application.AssetManagement
{
    public interface IContentLoader
    {
        UniTask LoadAsync(IProgress<float> progress, CancellationToken cancellationToken);
    }
}
