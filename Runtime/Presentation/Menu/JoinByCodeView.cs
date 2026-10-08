using System;
using _Project.Application.Localization;
using _Project.Application.Network;
using _Project.Presentation.Popups;
using DG.Tweening;
using TMPro;
using UnityEngine;
using VContainer;

namespace _Project.Presentation.Menu
{
    [RequireComponent(typeof(PopupView))]
    public class JoinByCodeView : MonoBehaviour
    {
        private const float PopScale = 1.4f;
        private const float PopDuration = 0.35f;
        private const float HoldDuration = 2.5f;
        private const float FadeDuration = 0.4f;

        [SerializeField] private TMP_Text _errorLabel;

        private PopupView _popup;
        private IMatchmakingService _matchmaking;
        private IClientConnection _connection;
        private ILocalizationService _localization;
        private Sequence _errorAnimation;

        [Inject]
        public void Construct(IMatchmakingService matchmaking, IClientConnection connection, ILocalizationService localization)
        {
            _matchmaking = matchmaking ?? throw new ArgumentNullException(nameof(matchmaking));
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
            _localization = localization ?? throw new ArgumentNullException(nameof(localization));
        }

        private void Awake()
        {
            _popup = GetComponent<PopupView>();
        }

        public void ShowRoomNotFound()
        {
            ShowError("net.room.notfound");
        }

        public void ShowInvalidCode()
        {
            ShowError("net.room.badcode");
        }

        public void ClearError()
        {
            KillErrorAnimation();
            _errorLabel.gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            ClearError();
            _popup.Hidden += OnHidden;
            _matchmaking.JoinRoomFailed += OnJoinRoomFailed;
        }

        private void OnDisable()
        {
            KillErrorAnimation();
            _popup.Hidden -= OnHidden;
            _matchmaking.JoinRoomFailed -= OnJoinRoomFailed;
        }

        private void ShowError(string key)
        {
            KillErrorAnimation();
            _errorLabel.text = _localization.Get(key);
            _errorLabel.gameObject.SetActive(true);
            _errorLabel.alpha = 1f;
            _errorLabel.transform.localScale = Vector3.one * PopScale;

            _errorAnimation = DOTween.Sequence()
                .Append(_errorLabel.transform.DOScale(1f, PopDuration).SetEase(Ease.OutBack))
                .AppendInterval(HoldDuration)
                .Append(DOTween.To(() => _errorLabel.alpha, alpha => _errorLabel.alpha = alpha, 0f, FadeDuration))
                .OnComplete(() => _errorLabel.gameObject.SetActive(false))
                .SetLink(_errorLabel.gameObject);
        }

        private void KillErrorAnimation()
        {
            if (_errorAnimation == null)
            {
                return;
            }

            _errorAnimation.Kill();
            _errorAnimation = null;
        }

        private void OnJoinRoomFailed()
        {
            ShowRoomNotFound();
        }

        private void OnHidden()
        {
            KillErrorAnimation();

            if (_matchmaking.HasMatch)
            {
                return;
            }

            _matchmaking.Cancel();
        }
    }
}
