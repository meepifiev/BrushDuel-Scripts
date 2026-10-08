using System;
using _Project.Application.Configs;
using _Project.Application.Localization;
using _Project.Presentation.Popups;
using DG.Tweening;
using TMPro;
using UnityEngine;
using VContainer;

namespace _Project.Presentation.Match
{
    [RequireComponent(typeof(PopupView))]
    public class ResultPopupView : MonoBehaviour
    {
        private const string NumberColor = "#D1543D";
        private const int SecondsPerMinute = 60;

        [SerializeField] private PopupNavigator _navigator;
        [SerializeField] private TMP_Text _gradeLabel;
        [SerializeField] private TMP_Text _subtitleLabel;
        [SerializeField] private TMP_Text _similarityLabel;
        [SerializeField] private TMP_Text _timeLabel;
        [SerializeField] private TMP_Text _rewardLabel;
        [SerializeField] private GameObject _buttonsGroup;
        [SerializeField] private float _counterDuration = 0.7f;

        private PopupView _popupView;
        private ILocalizationService _localization;
        private Sequence _countersAnimation;

        public event Action RewardCounted;

        [Inject]
        public void Construct(ILocalizationService localization)
        {
            _localization = localization ?? throw new ArgumentNullException(nameof(localization));
        }

        private void Awake()
        {
            _popupView = GetComponent<PopupView>();
        }

        public void HideInstant()
        {
            KillCounters();
            _buttonsGroup.SetActive(false);
            _popupView.HideInstant();
        }

        public void ShowResults(GradingConfig.GradeEntry grade, float similarity, float elapsedSeconds, int coins)
        {
            KillCounters();
            _buttonsGroup.SetActive(false);
            _gradeLabel.text = grade.Label;
            _subtitleLabel.text = _localization.Get(grade.SubtitleKey);

            int targetPercent = Mathf.RoundToInt(similarity * 100f);
            int targetSeconds = Mathf.CeilToInt(elapsedSeconds);
            int percent = 0;
            int seconds = 0;
            int reward = 0;

            ApplySimilarity(0);
            ApplyTime(0);
            ApplyReward(0);
            _navigator.Open(_popupView);

            _countersAnimation = DOTween.Sequence()
                .Append(DOTween.To(() => percent, value => { percent = value; ApplySimilarity(value); }, targetPercent, _counterDuration))
                .Append(DOTween.To(() => seconds, value => { seconds = value; ApplyTime(value); }, targetSeconds, _counterDuration))
                .Append(DOTween.To(() => reward, value => { reward = value; ApplyReward(value); }, coins, _counterDuration))
                .AppendCallback(() =>
                {
                    _buttonsGroup.SetActive(true);
                    RewardCounted?.Invoke();
                })
                .SetLink(gameObject);
        }

        private void ApplySimilarity(int percent)
        {
            string value = "<color=" + NumberColor + ">" + percent + "%</color>";
            _similarityLabel.text = string.Format(_localization.Get("result.similarity"), value);
        }

        private void ApplyTime(int totalSeconds)
        {
            int minutes = totalSeconds / SecondsPerMinute;
            int seconds = totalSeconds % SecondsPerMinute;
            string value = "<color=" + NumberColor + ">" + minutes + ":" + seconds.ToString("00") + "</color>";
            _timeLabel.text = string.Format(_localization.Get("result.time"), value);
        }

        private void ApplyReward(int coins)
        {
            string value = "<color=" + NumberColor + ">" + coins + "</color><sprite name=\"coin\">";
            _rewardLabel.text = string.Format(_localization.Get("result.reward"), value);
        }

        private void KillCounters()
        {
            if (_countersAnimation == null)
            {
                return;
            }

            _countersAnimation.Kill();
            _countersAnimation = null;
        }
    }
}
