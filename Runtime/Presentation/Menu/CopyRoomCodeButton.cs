using _Project.Presentation.UI;
using UnityEngine;

namespace _Project.Presentation.Menu
{
    public class CopyRoomCodeButton : ButtonHandler
    {
        [SerializeField] private RoomCreatedView _view;

        protected override void OnClick()
        {
            _view.CopyCode();
        }
    }
}
