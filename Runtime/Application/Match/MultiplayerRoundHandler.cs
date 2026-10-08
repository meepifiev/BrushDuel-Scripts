using System;
using _Project.Application.Configs;
using _Project.Application.Drawing;
using _Project.Application.Network;
using _Project.Core.Match;
using UnityEngine;
using VContainer.Unity;

namespace _Project.Application.Match
{
    public class MultiplayerRoundHandler : IStartable, ITickable, IDisposable, IRoundTimeline
    {
        private const float IntroDurationSeconds = 3f;

        private readonly ReferenceConfig _config;
        private readonly DrawingSessionService _session;
        private readonly MatchLaunchContext _context;
        private readonly IMatchSession _matchSession;
        private readonly IClientConnection _connection;

        private RoundTimer _timer;
        private float _introRemaining;
        private float _lastRealtime;
        private int _lastIntroSecond;
        private bool _started;
        private bool _finishRaised;
        private bool _aborted;
        private bool _doneSent;

        public MultiplayerRoundHandler(
            ReferenceConfig config,
            DrawingSessionService session,
            MatchLaunchContext context,
            IMatchSession matchSession,
            IClientConnection connection)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            _config = config;
            _session = session ?? throw new ArgumentNullException(nameof(session));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _matchSession = matchSession ?? throw new ArgumentNullException(nameof(matchSession));
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        }

        public event Action<int> IntroTicked;
        public event Action IntroFinished;
        public event Action<float, float> Ticked;
        public event Action Finished;
        public event Action Aborted;
        public event Action OpponentLeft;
        public event Action ConnectionLost;
        public event Action DoneSubmitted;

        public void Start()
        {
            _session.LockDrawing();
            _matchSession.RoundFinished += OnRoundFinished;
            _matchSession.MatchCanceled += OnMatchCanceled;

            if (_connection.IsConnected == false)
            {
                Ticked?.Invoke(_context.RoundDurationSeconds, _context.RoundDurationSeconds);
                RaiseAbort(ConnectionLost);
                return;
            }

            BeginRound();
        }

        public void Tick()
        {
            if (_finishRaised || _aborted)
            {
                return;
            }

            if (_connection.IsConnected == false)
            {
                RaiseAbort(ConnectionLost);
                return;
            }

            if (_started == false)
            {
                return;
            }

            float delta = Time.realtimeSinceStartup - _lastRealtime;
            _lastRealtime = Time.realtimeSinceStartup;

            if (_introRemaining > 0f)
            {
                TickIntro(delta);
                return;
            }

            if (_timer.IsFinished)
            {
                return;
            }

            _timer.Tick(delta);
            Ticked?.Invoke(_timer.Remaining, _timer.Duration);

            if (_timer.IsFinished)
            {
                _session.LockDrawing();
            }
        }

        public void Dispose()
        {
            _matchSession.RoundFinished -= OnRoundFinished;
            _matchSession.MatchCanceled -= OnMatchCanceled;
        }

        public bool CanSubmitDone => _started && _introRemaining <= 0f && _finishRaised == false && _aborted == false && _doneSent == false;

        public void SubmitDone()
        {
            if (CanSubmitDone == false)
            {
                return;
            }

            _doneSent = true;
            _session.LockDrawing();
            _matchSession.SendDone();
            DoneSubmitted?.Invoke();
        }

        public void StopLocal()
        {
            if (_aborted || _finishRaised)
            {
                return;
            }

            _aborted = true;
            _session.LockDrawing();
        }

        private void BeginRound()
        {
            _started = true;
            _timer = new RoundTimer(_context.RoundDurationSeconds);
            _introRemaining = IntroDurationSeconds;
            _lastRealtime = Time.realtimeSinceStartup;
            _lastIntroSecond = Mathf.CeilToInt(IntroDurationSeconds);
            Ticked?.Invoke(_timer.Remaining, _timer.Duration);
            IntroTicked?.Invoke(_lastIntroSecond);
        }

        private void TickIntro(float delta)
        {
            _introRemaining -= delta;

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

        private void RaiseAbort(Action specificEvent)
        {
            _aborted = true;
            _session.LockDrawing();

            Aborted?.Invoke();
            specificEvent?.Invoke();
        }

        private void OnRoundFinished()
        {
            if (_finishRaised || _aborted)
            {
                return;
            }

            _finishRaised = true;
            _session.LockDrawing();
            Finished?.Invoke();
        }

        private void OnMatchCanceled()
        {
            if (_finishRaised || _aborted)
            {
                return;
            }

            RaiseAbort(OpponentLeft);
        }
    }
}
