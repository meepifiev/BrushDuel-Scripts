using _Project.Application.Drawing;
using UnityEngine;

namespace _Project.Presentation.Drawing.Toolbar
{
    public class ToolModeButton : SelectionButton
    {
        [SerializeField] private ToolMode _mode;

        protected override void HandleClick()
        {
            Session.SelectToolMode(_mode);
        }

        protected override bool IsSelected()
        {
            return Session.ActiveToolMode == _mode;
        }
    }
}
