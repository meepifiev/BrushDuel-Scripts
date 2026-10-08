using _Project.Presentation.UI;
using UnityEngine;

namespace _Project.Presentation.Popups
{
    public class PopupOpenButton : ButtonHandler
    {
        [SerializeField] private PopupNavigator _navigator;
        [SerializeField] private PopupView _target;

        protected override void OnClick()
        {
            _navigator.Open(_target);
        }
    }
}
