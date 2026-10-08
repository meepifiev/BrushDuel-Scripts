using System;
using _Project.Application.Configs;
using _Project.Application.Localization;
using _Project.Application.Network;
using _Project.Application.Profile;
using _Project.Presentation.Popups;
using TMPro;
using UnityEngine;
using VContainer;

namespace _Project.Presentation.Menu
{
    [RequireComponent(typeof(PopupView))]
    public class MatchFoundView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _myNicknameLabel;
        [SerializeField] private TMP_Text _opponentNicknameLabel;
        [SerializeField] private TMP_Text _timerLabel;

        private PopupView _popup;
        private IMatchmakingService _matchmaking;
        private IPlayerProfileService _profile;
        private ILocalizationService _localization;
        private ServerConfig _serverConfig;

        private float _acceptRemaining;
        private int _lastShownSecond;

        [Inject]
        public void Construct(
            IMatchmakingService matchmaking,
            IPlayerProfileService profile,
            ILocalizationService localization,
            ServerConfig serverConfig)
        {
            _matchmaking = matchmaking ?? throw new ArgumentNullException(nameof(matchmaking));
            _profile = profile ?? throw new ArgumentNullException(nameof(profile));
            _localization = localization ?? throw new ArgumentNullException(nameof(localization));

            if (serverConfig == null)
            {
                throw new ArgumentNullException(nameof(serverConfig));
            }

            _serverConfig = serverConfig;
        }

        private void Awake()
        {
            _popup = GetComponent<PopupView>();
        }

        private void OnEnable()
        {
            _acceptRemaining = _serverConfig.ReadyTimeoutSeconds;
            _lastShownSecond = -1;
            RefreshLabels();
            RefreshTimerLabel();
            _popup.Hidden += OnHidden;
        }

        private void OnDisable()
        {
            _popup.Hidden -= OnHidden;
        }

        private void Update()
        {
            if (_acceptRemaining <= 0f)
            {
                return;
            }

            _acceptRemaining -= Time.unscaledDeltaTime;
            RefreshTimerLabel();
        }

        private void RefreshLabels()
        {
            string myNickname = FormatNickname(_profile.Nickname);
            _myNicknameLabel.text = string.Format(_localization.Get("net.you"), myNickname);
            _opponentNicknameLabel.text = FormatNickname(_matchmaking.CurrentMatch.OpponentNickname);
        }

        private void RefreshTimerLabel()
        {
            int second = Mathf.Max(0, Mathf.CeilToInt(_acceptRemaining));

            if (second == _lastShownSecond)
            {
                return;
            }

            _lastShownSecond = second;
            _timerLabel.text = string.Format(_localization.Get("net.ready.timer"), second);
        }

        private string FormatNickname(string nickname)
        {
            return string.IsNullOrEmpty(nickname) ? _localization.Get("net.anonymous") : nickname;
        }

        private void OnHidden()
        {
            if (_matchmaking.IsRoundActive || _matchmaking.HasMatch == false)
            {
                return;
            }

            _matchmaking.Cancel();
        }
    }
}
