using UnityEngine;

namespace _Project.Presentation.Popups
{
    public class EscapePopupToggle : MonoBehaviour
    {
        [SerializeField] private PopupNavigator _navigator;
        [SerializeField] private PopupView _target;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape) == false)
            {
                return;
            }

            if (_navigator.HasOpenPopup)
            {
                _navigator.Back();
                return;
            }

            _navigator.Open(_target);
        }
    }
}
