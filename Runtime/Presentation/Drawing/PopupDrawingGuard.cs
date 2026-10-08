using _Project.Application.Drawing;
using _Project.Presentation.Popups;
using UnityEngine;
using VContainer;

namespace _Project.Presentation.Drawing
{
    [RequireComponent(typeof(PopupNavigator))]
    public class PopupDrawingGuard : MonoBehaviour
    {
        private PopupNavigator _navigator;
        private DrawingSessionService _session;

        [Inject]
        public void Construct(DrawingSessionService session)
        {
            _session = session;
        }

        private void Awake()
        {
            _navigator = GetComponent<PopupNavigator>();
        }

        private void OnEnable()
        {
            _navigator.OpenStateChanged += OnOpenStateChanged;
        }

        private void Start()
        {
            _session.SetInputBlocked(_navigator.HasOpenPopup);
        }

        private void OnDisable()
        {
            _navigator.OpenStateChanged -= OnOpenStateChanged;

            if (_session != null)
            {
                _session.SetInputBlocked(false);
            }
        }

        private void OnOpenStateChanged(bool open)
        {
            _session.SetInputBlocked(open);
        }
    }
}
