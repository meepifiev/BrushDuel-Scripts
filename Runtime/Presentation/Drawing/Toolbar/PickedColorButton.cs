using _Project.Core.Drawing;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Presentation.Drawing.Toolbar
{
    [RequireComponent(typeof(Image))]
    public class PickedColorButton : SelectionButton
    {
        private Image _swatch;

        protected override void Awake()
        {
            base.Awake();
            _swatch = GetComponent<Image>();
        }

        public void Select()
        {
            Session.SelectPickedColor(Session.PickedColor);
        }

        protected override void HandleClick()
        {
            Select();
        }

        protected override bool IsSelected()
        {
            return Session.UsesPickedColor;
        }

        protected override void Refresh()
        {
            base.Refresh();

            PaletteColor color = Session.PickedColor;
            _swatch.color = new Color32(color.R, color.G, color.B, byte.MaxValue);
        }
    }
}
