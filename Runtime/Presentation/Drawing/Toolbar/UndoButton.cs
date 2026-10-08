using System;
using _Project.Application.Drawing;
using _Project.Presentation.UI;
using UnityEngine;
using VContainer;

namespace _Project.Presentation.Drawing.Toolbar
{
    public class UndoButton : ButtonHandler
    {
        private DrawingSessionService _session;

        [Inject]
        public void Construct(DrawingSessionService session)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
        }

        protected override void OnClick()
        {
            _session.UndoLastCommand();
        }

        private void Update()
        {
            bool modifier = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)
                || Input.GetKey(KeyCode.LeftCommand) || Input.GetKey(KeyCode.RightCommand);

            if (modifier && Input.GetKeyDown(KeyCode.Z))
            {
                _session.UndoLastCommand();
            }
        }
    }
}
