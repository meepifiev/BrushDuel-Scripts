using System;
using _Project.Application.SceneManagement;
using _Project.Presentation.UI;
using Cysharp.Threading.Tasks;
using VContainer;

namespace _Project.Presentation.Menu
{
    public class ExitToMenuButton : ButtonHandler
    {
        private ISceneLoader _sceneLoader;

        [Inject]
        public void Construct(ISceneLoader sceneLoader)
        {
            _sceneLoader = sceneLoader ?? throw new ArgumentNullException(nameof(sceneLoader));
        }

        protected override void OnClick()
        {
            _sceneLoader.LoadAsync(SceneId.MainMenu).Forget();
        }
    }
}
