using System;
using _Project.Application.Configs;
using _Project.Application.Drawing;
using _Project.Core.Match;
using UnityEngine;
using VContainer.Unity;

namespace _Project.Application.Match
{
    public class RoundHandler : IStartable, ITickable, IRoundTimeline
    {
        private const float IntroDurationSeconds = 3f;

        private readonly ReferenceConfig _config;
        private readonly DrawingSessionService _session;
        private readonly MatchLaunchContext _context;

        private RoundTimer _timer;
        private float _introRemaining;
        private int _lastIntroSecond;
        private bool _finishRaised;

        public RoundHandler(
            ReferenceConfig config,
            DrawingSessionService session,
            MatchLaunchContext context)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            _config = config;
            _session = session ?? throw new ArgumentNullException(nameof(session));
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public event Action<int> IntroTicked;
        public event Action IntroFinished;
        public event Action<float, float> Ticked;
        public event Action Finished;

        event Action IRoundTimeline.Aborted
        {
            add { }
            remove { }
        }

        public float ElapsedSeconds => _timer == null ? 0f : _timer.Duration - _timer.Remaining;

        public void Start()
        {
            _timer = new RoundTimer(_config.RoundDurationSeconds);
            _introRemaining = IntroDurationSeconds;
            _lastIntroSecond = Mathf.CeilToInt(IntroDurationSeconds);
            _session.LockDrawing();
            Ticked?.Invoke(_timer.Remaining, _timer.Duration);
            IntroTicked?.Invoke(_lastIntroSecond);
        }

        public void Tick()
        {
            if (_timer == null || _finishRaised)
            {
                return;
            }

            if (_introRemaining > 0f)
            {
                TickIntro();
                return;
            }

            _timer.Tick(Time.deltaTime);
            Ticked?.Invoke(_timer.Remaining, _timer.Duration);

            if (_timer.IsFinished)
            {
                Finish();
            }
        }

        public void Finish()
        {
            if (_finishRaised || _introRemaining > 0f)
            {
                return;
            }

            _finishRaised = true;
            _session.LockDrawing();
            Finished?.Invoke();
        }

        private void TickIntro()
        {
            _introRemaining -= Time.deltaTime;

            if (_introRemaining <= 0f)
            {
                _session.UnlockDrawing();
                IntroFinished?.Invoke();
                Ticked?.Invoke(_timer.Remaining, _timer.Duration);
                return;
            }

            int second = Mathf.CeilToInt(_introRemaining);

            if (second != _lastIntroSecond)
            {
                _lastIntroSecond = second;
                IntroTicked?.Invoke(second);
            }
        }
    }
}
