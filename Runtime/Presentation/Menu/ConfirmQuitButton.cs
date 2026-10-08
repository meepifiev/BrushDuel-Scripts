using _Project.Presentation.UI;

namespace _Project.Presentation.Menu
{
    public class ConfirmQuitButton : ButtonHandler
    {
        protected override void OnClick()
        {
            UnityEngine.Application.Quit();
        }
    }
}
