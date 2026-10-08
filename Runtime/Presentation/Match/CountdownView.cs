using System;
using _Project.Application.Match;
using _Project.Presentation.Popups;
using DG.Tweening;
using TMPro;
using UnityEngine;
using VContainer;

namespace _Project.Presentation.Match
{
    public class CountdownView : MonoBehaviour
    {
        private const float PopScale = 1.4f;
        private const float PopDuration = 0.3f;

        [SerializeField] private TMP_Text _label;
        [SerializeField] private PopupNavigator _navigator;

        private IRoundTimeline _timeline;
        private bool _dimShown;

        [Inject]
        public void Construct(IRoundTimeline timeline)
        {
            _timeline = timeline ?? throw new ArgumentNullException(nameof(timeline));
            _timeline.IntroTicked += OnIntroTicked;
            _timeline.IntroFinished += OnIntroFinished;
            _timeline.Aborted += OnAborted;
        }

        private void OnDestroy()
        {
            if (_timeline == null)
            {
                return;
            }

            _timeline.IntroTicked -= OnIntroTicked;
            _timeline.IntroFinished -= OnIntroFinished;
            _timeline.Aborted -= OnAborted;
        }

        private void OnIntroTicked(int second)
        {
            if (_dimShown == false)
            {
                _dimShown = true;
                _navigator.ShowDim();
            }

            _label.text = second.ToString();
            _label.transform.localScale = Vector3.one * PopScale;
            _label.transform.DOScale(1f, PopDuration).SetEase(Ease.OutBack).SetLink(_label.gameObject);
        }

        private void OnIntroFinished()
        {
            _navigator.HideDim();
            gameObject.SetActive(false);
        }

        private void OnAborted()
        {
            if (_dimShown)
            {
                _navigator.HideDim();
            }

            gameObject.SetActive(false);
        }
    }
}
