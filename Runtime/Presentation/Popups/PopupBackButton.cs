using _Project.Presentation.UI;
using UnityEngine;

namespace _Project.Presentation.Popups
{
    public class PopupBackButton : ButtonHandler
    {
        [SerializeField] private PopupNavigator _navigator;

        protected override void OnClick()
        {
            _navigator.Back();
        }
    }
}
