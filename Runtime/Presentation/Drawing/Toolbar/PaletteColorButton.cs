using _Project.Core.Drawing;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Presentation.Drawing.Toolbar
{
    [RequireComponent(typeof(Image))]
    public class PaletteColorButton : SelectionButton
    {
        [SerializeField] private int _colorIndex;

        private bool HasColor => _colorIndex >= 0 && _colorIndex < Session.Palette.Length;

        private void Start()
        {
            if (HasColor == false)
            {
                gameObject.SetActive(false);
                return;
            }

            PaletteColor color = Session.Palette[_colorIndex];
            GetComponent<Image>().color = new Color32(color.R, color.G, color.B, byte.MaxValue);
        }

        public void Select()
        {
            if (HasColor == false)
            {
                return;
            }

            Session.SelectColor(_colorIndex);
        }

        protected override void HandleClick()
        {
            Select();
        }

        protected override bool IsSelected()
        {
            return HasColor && Session.UsesPickedColor == false && Session.ActiveColorIndex == _colorIndex;
        }
    }
}
