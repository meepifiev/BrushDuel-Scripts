using System;
using _Project.Application.Localization;
using _Project.Application.Network;
using _Project.Presentation.Popups;
using TMPro;
using UnityEngine;
using VContainer;

namespace _Project.Presentation.Menu
{
    [RequireComponent(typeof(PopupView))]
    public class RoomCreatedView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _codeLabel;

        private string _code = string.Empty;

        private PopupView _popup;
        private IMatchmakingService _matchmaking;
        private IClientConnection _connection;
        private ILocalizationService _localization;

        [Inject]
        public void Construct(
            IMatchmakingService matchmaking,
            IClientConnection connection,
            ILocalizationService localization)
        {
            _matchmaking = matchmaking ?? throw new ArgumentNullException(nameof(matchmaking));
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
            _localization = localization ?? throw new ArgumentNullException(nameof(localization));
        }

        private void Awake()
        {
            _popup = GetComponent<PopupView>();
        }

        public void CopyCode()
        {
            if (string.IsNullOrEmpty(_code) == false)
            {
                GUIUtility.systemCopyBuffer = _code;
            }
        }

        private void OnEnable()
        {
            _code = string.Empty;
            _codeLabel.text = _localization.Get("common.loading");
            _popup.Hidden += OnHidden;
            _matchmaking.RoomCreated += OnRoomCreated;
        }

        private void OnDisable()
        {
            _popup.Hidden -= OnHidden;
            _matchmaking.RoomCreated -= OnRoomCreated;
        }

        private void SetCode(string code)
        {
            _codeLabel.text = string.Format(_localization.Get("net.room.code"), code);
        }

        private void OnRoomCreated(string code)
        {
            _code = code;
            SetCode(code);
        }

        private void OnHidden()
        {
            if (_matchmaking.HasMatch)
            {
                return;
            }

            _matchmaking.Cancel();
        }
    }
}
