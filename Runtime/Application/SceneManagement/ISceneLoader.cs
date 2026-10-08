using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace _Project.Application.SceneManagement
{
    public interface ISceneLoader
    {
        UniTask LoadAsync(SceneId sceneId, Action loaded = null, CancellationToken cancellationToken = default);
    }
}
