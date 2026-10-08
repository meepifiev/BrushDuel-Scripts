using _Project.Application.Network;
using _Project.Presentation.Audio;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace _Project.Presentation.Match
{
    public class MatchResultDisplay : MonoBehaviour
    {
        [Header("Stamps")]
        [SerializeField] private Image _playerStamp;
        [SerializeField] private Image _opponentStamp;
        [SerializeField] private Sprite _winStamp;
        [SerializeField] private Sprite _loserStamp;
        [SerializeField] private Sprite _drawStamp;

        [Header("Similarity")]
        [SerializeField] private TMP_Text _playerSimilarity;
        [SerializeField] private TMP_Text _opponentSimilarity;

        [Header("Fade out")]
        [SerializeField] private CanvasGroup[] _fadeOut;
        [SerializeField] private float _fadeDuration = 0.4f;
        [SerializeField] private GameObject[] _hideOnResult;

        [Header("Disable / exit")]
        [SerializeField] private Button[] _disableButtons;
        [SerializeField] private GameObject _exitButton;
        [SerializeField] private OpponentVisibilityToggle _opponentToggle;

        [Header("Stamp effect")]
        [SerializeField] private AudioAsset _stampSound;
        [SerializeField] private RectTransform _shakeTarget;
        [SerializeField] private float _stampScale = 1.9f;
        [SerializeField] private float _stampDuration = 0.22f;
        [SerializeField] private float _shakeDuration = 0.4f;
        [SerializeField] private float _shakeStrength = 32f;

        private PostRoundAnalysisPresenter _analysis;
        private IMatchSession _matchSession;
        private IAudioService _audio;

        [Inject]
        public void Construct(
            PostRoundAnalysisPresenter analysis,
            IMatchSession matchSession,
            IAudioService audio)
        {
            _analysis = analysis;
            _matchSession = matchSession;
            _audio = audio;
        }

        private void Start()
        {
            _analysis.Completed += OnCompleted;
        }

        private void OnDestroy()
        {
            if (_analysis != null)
            {
                _analysis.Completed -= OnCompleted;
            }
        }

        private void OnCompleted()
        {
            MatchResult result = _matchSession.LastResult;

            if (_opponentToggle != null)
            {
                _opponentToggle.ForceShowOpponent();
            }

            for (int i = 0; i < _disableButtons.Length; i++)
            {
                if (_disableButtons[i] != null)
                {
                    _disableButtons[i].interactable = false;
                }
            }

            for (int i = 0; i < _fadeOut.Length; i++)
            {
                CanvasGroup group = _fadeOut[i];

                if (group == null)
                {
                    continue;
                }

                group.interactable = false;
                group.blocksRaycasts = false;
                group.DOFade(0f, _fadeDuration).SetLink(group.gameObject);
            }

            for (int i = 0; i < _hideOnResult.Length; i++)
            {
                if (_hideOnResult[i] != null)
                {
                    _hideOnResult[i].SetActive(false);
                }
            }

            SetSimilarity(_playerSimilarity, result.MyScore);
            SetSimilarity(_opponentSimilarity, result.OpponentScore);

            ShowStamp(_playerStamp, StampFor(result.Outcome, true));
            ShowStamp(_opponentStamp, StampFor(result.Outcome, false));

            if (_stampSound != null)
            {
                _audio.PlayOneShot(_stampSound);
            }

            if (_shakeTarget != null)
            {
                _shakeTarget.DOShakeAnchorPos(_shakeDuration, _shakeStrength, 22, 90f, false, true).SetLink(_shakeTarget.gameObject);
            }

            if (_exitButton != null)
            {
                _exitButton.SetActive(true);
            }
        }

        private Sprite StampFor(MatchOutcome outcome, bool self)
        {
            switch (outcome)
            {
                case MatchOutcome.Win:
                    return self ? _winStamp : _loserStamp;
                case MatchOutcome.Lose:
                    return self ? _loserStamp : _winStamp;
                default:
                    return _drawStamp;
            }
        }

        private void SetSimilarity(TMP_Text label, int score)
        {
            if (label == null)
            {
                return;
            }

            label.text = score + "%";
            label.transform.SetAsLastSibling();
            label.gameObject.SetActive(true);
        }

        private void ShowStamp(Image stamp, Sprite sprite)
        {
            if (stamp == null || sprite == null)
            {
                return;
            }

            stamp.sprite = sprite;
            stamp.transform.SetAsLastSibling();
            stamp.gameObject.SetActive(true);
            stamp.transform.localScale = Vector3.one * _stampScale;
            stamp.transform.DOScale(1f, _stampDuration).SetEase(Ease.OutBack).SetLink(stamp.gameObject);
        }
    }
}
