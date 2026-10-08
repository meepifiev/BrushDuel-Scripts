using System;
using _Project.Application.Localization;
using _Project.Application.Network;
using _Project.Application.Profile;
using _Project.Core.Match;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace _Project.Presentation.Match
{
    public class RoundResultView : MonoBehaviour
    {
        [Header("Text")]
        [SerializeField] private TMP_Text _outcomeLabel;
        [SerializeField] private TMP_Text _modeLabel;
        [SerializeField] private TMP_Text _playerNickname;
        [SerializeField] private TMP_Text _opponentNickname;
        [SerializeField] private TMP_Text _playerScore;
        [SerializeField] private TMP_Text _opponentScore;

        [Header("Thumbnails")]
        [SerializeField] private RawImage _playerDrawing;
        [SerializeField] private RawImage _opponentDrawing;
        [SerializeField] private RawImage _referenceImage;
        [SerializeField] private RawImage _playerSource;
        [SerializeField] private RawImage _opponentSource;
        [SerializeField] private RawImage _referenceSource;

        [Header("Winner")]
        [SerializeField] private GameObject _playerDefaultFrame;
        [SerializeField] private GameObject _opponentDefaultFrame;
        [SerializeField] private GameObject _playerWinnerFrame;
        [SerializeField] private GameObject _opponentWinnerFrame;

        private IMatchmakingService _matchmaking;
        private IMatchSession _matchSession;
        private IPlayerProfileService _profile;
        private ILocalizationService _localization;

        [Inject]
        public void Construct(
            IMatchmakingService matchmaking,
            IMatchSession matchSession,
            IPlayerProfileService profile,
            ILocalizationService localization)
        {
            _matchmaking = matchmaking ?? throw new ArgumentNullException(nameof(matchmaking));
            _matchSession = matchSession ?? throw new ArgumentNullException(nameof(matchSession));
            _profile = profile ?? throw new ArgumentNullException(nameof(profile));
            _localization = localization ?? throw new ArgumentNullException(nameof(localization));
        }

        private void OnEnable()
        {
            MatchResult result = _matchSession.LastResult;

            _outcomeLabel.text = _localization.Get(OutcomeKey(result.Outcome));

            if (_modeLabel != null)
            {
                _modeLabel.text = _localization.Get(ModeKey(_matchmaking.CurrentMatch.GameMode));
            }

            _playerNickname.text = string.Format(_localization.Get("net.you"), FormatNickname(_profile.Nickname));
            _opponentNickname.text = FormatNickname(_matchmaking.CurrentMatch.OpponentNickname);
            _playerScore.text = result.MyScore + "%";
            _opponentScore.text = result.OpponentScore + "%";

            CopyTexture(_playerSource, _playerDrawing);
            CopyTexture(_opponentSource, _opponentDrawing);
            CopyTexture(_referenceSource, _referenceImage);

            ShowWinner(_playerDefaultFrame, _playerWinnerFrame, result.Outcome == MatchOutcome.Win);
            ShowWinner(_opponentDefaultFrame, _opponentWinnerFrame, result.Outcome == MatchOutcome.Lose);
        }

        private void ShowWinner(GameObject defaultFrame, GameObject winnerFrame, bool isWinner)
        {
            if (winnerFrame != null)
            {
                winnerFrame.SetActive(isWinner);
            }

            if (defaultFrame != null)
            {
                defaultFrame.SetActive(isWinner == false);
            }
        }

        private void CopyTexture(RawImage source, RawImage target)
        {
            if (source == null || target == null)
            {
                return;
            }

            target.texture = source.texture;
            target.uvRect = source.uvRect;
        }

        private string FormatNickname(string nickname)
        {
            return string.IsNullOrEmpty(nickname) ? _localization.Get("net.anonymous") : nickname;
        }

        private string OutcomeKey(MatchOutcome outcome)
        {
            switch (outcome)
            {
                case MatchOutcome.Win:
                    return "net.result.win";
                case MatchOutcome.Lose:
                    return "net.result.lose";
                default:
                    return "net.result.draw";
            }
        }

        private string ModeKey(GameMode gameMode)
        {
            return gameMode == GameMode.Masterstroke ? "mode.master" : "mode.wild";
        }
    }
}
