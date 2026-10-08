using _Project.Application.Drawing;
using _Project.Core.Drawing;
using UnityEngine;

namespace _Project.Presentation.Drawing.Toolbar
{
    public class BrushTypeButton : SelectionButton
    {
        [SerializeField] private BrushType _brush;

        protected override void HandleClick()
        {
            Session.SelectBrush(_brush);
        }

        protected override bool IsSelected()
        {
            return Session.ActiveToolMode == ToolMode.Brush && Session.ActiveBrush == _brush;
        }
    }
}
