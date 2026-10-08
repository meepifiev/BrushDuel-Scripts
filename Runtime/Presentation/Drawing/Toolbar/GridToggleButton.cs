using _Project.Presentation.UI;
using UnityEngine;

namespace _Project.Presentation.Drawing.Toolbar
{
    public class GridToggleButton : ButtonHandler
    {
        [SerializeField] private GridOverlayView _gridOverlay;
        [SerializeField] private GameObject _selectionBackground;

        protected override void OnEnable()
        {
            base.OnEnable();
            Refresh();
        }

        protected override void OnClick()
        {
            _gridOverlay.Toggle();
            Refresh();
        }

        private void Refresh()
        {
            if (_selectionBackground != null)
            {
                _selectionBackground.SetActive(_gridOverlay.gameObject.activeSelf);
            }
        }
    }
}
