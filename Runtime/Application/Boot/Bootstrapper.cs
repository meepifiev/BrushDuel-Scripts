using System;
using System.Threading;
using _Project.Application.AssetManagement;
using _Project.Application.Platform;
using _Project.Application.SceneManagement;
using Cysharp.Threading.Tasks;
using VContainer.Unity;

namespace _Project.Application.Boot
{
    public class Bootstrapper : IStartable
    {
        private readonly IPlatformInitializer _platformInitializer;
        private readonly ISceneLoader _sceneLoader;
        private readonly IContentLoader _contentLoader;
        private readonly LoadingProgress _loadingProgress;
        private readonly IBootIntro _bootIntro;

        public Bootstrapper(
            IPlatformInitializer platformInitializer,
            ISceneLoader sceneLoader,
            IContentLoader contentLoader,
            LoadingProgress loadingProgress,
            IBootIntro bootIntro)
        {
            _platformInitializer = platformInitializer ?? throw new ArgumentNullException(nameof(platformInitializer));
            _sceneLoader = sceneLoader ?? throw new ArgumentNullException(nameof(sceneLoader));
            _contentLoader = contentLoader ?? throw new ArgumentNullException(nameof(contentLoader));
            _loadingProgress = loadingProgress ?? throw new ArgumentNullException(nameof(loadingProgress));
            _bootIntro = bootIntro ?? throw new ArgumentNullException(nameof(bootIntro));
        }

        public void Start()
        {
            Run().Forget();
        }

        private async UniTaskVoid Run()
        {
            await _platformInitializer.InitializeAsync();
            UniTask loading = _contentLoader.LoadAsync(_loadingProgress, CancellationToken.None);
            await _bootIntro.PlayAsync(CancellationToken.None);
            await loading;
            await _sceneLoader.LoadAsync(SceneId.MainMenu);
        }
    }
}
