using System;
using _Project.Application.Match;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace _Project.Presentation.Match
{
    public class TimerView : MonoBehaviour
    {
        private const int SecondsPerMinute = 60;

        [SerializeField] private Slider _slider;
        [SerializeField] private TMP_Text _label;

        private IRoundTimeline _timeline;

        [Inject]
        public void Construct(IRoundTimeline timeline)
        {
            _timeline = timeline ?? throw new ArgumentNullException(nameof(timeline));
        }

        private void OnEnable()
        {
            _timeline.Ticked += OnTicked;
        }

        private void OnDisable()
        {
            _timeline.Ticked -= OnTicked;
        }

        private void OnTicked(float remaining, float duration)
        {
            _slider.value = remaining / duration;
            int totalSeconds = Mathf.CeilToInt(remaining);
            int minutes = totalSeconds / SecondsPerMinute;
            int seconds = totalSeconds % SecondsPerMinute;
            _label.text = minutes + ":" + seconds.ToString("00");
        }
    }
}
