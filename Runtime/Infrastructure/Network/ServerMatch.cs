using _Project.Application.Configs;
using _Project.Core.Match;
using Mirror;

namespace _Project.Infrastructure.Network
{
    public class ServerMatch
    {
        private readonly ServerDrawingRecorder _firstRecorder = new ServerDrawingRecorder();
        private readonly ServerDrawingRecorder _secondRecorder = new ServerDrawingRecorder();

        private bool _firstReady;
        private bool _secondReady;
        private bool _firstDone;
        private bool _secondDone;

        public ServerMatch(ServerPlayer first, ServerPlayer second, GameMode gameMode, ReferenceConfig reference, float roundDurationSeconds, float readyTimeoutSeconds)
        {
            First = first;
            Second = second;
            GameMode = gameMode;
            Reference = reference;
            RoundDurationSeconds = roundDurationSeconds;
            TimeRemaining = readyTimeoutSeconds;
        }

        public ServerPlayer First { get; }
        public ServerPlayer Second { get; }
        public GameMode GameMode { get; }
        public ReferenceConfig Reference { get; }
        public float RoundDurationSeconds { get; }
        public ServerMatchState State { get; private set; } = ServerMatchState.WaitingReady;
        public float TimeRemaining { get; private set; }

        public bool BothReady => _firstReady && _secondReady;
        public bool BothDone => _firstDone && _secondDone;
        public ServerDrawingRecorder FirstRecorder => _firstRecorder;
        public ServerDrawingRecorder SecondRecorder => _secondRecorder;

        public bool Contains(NetworkConnectionToClient connection)
        {
            return First.Connection == connection || Second.Connection == connection;
        }

        public ServerDrawingRecorder RecorderFor(NetworkConnectionToClient connection)
        {
            if (First.Connection == connection)
            {
                return _firstRecorder;
            }

            if (Second.Connection == connection)
            {
                return _secondRecorder;
            }

            return null;
        }

        public void MarkReady(NetworkConnectionToClient connection)
        {
            if (First.Connection == connection)
            {
                _firstReady = true;
            }
            else if (Second.Connection == connection)
            {
                _secondReady = true;
            }
        }

        public void MarkDone(NetworkConnectionToClient connection)
        {
            if (First.Connection == connection)
            {
                _firstDone = true;
            }
            else if (Second.Connection == connection)
            {
                _secondDone = true;
            }
        }

        public void StartRound(float totalSeconds)
        {
            State = ServerMatchState.Playing;
            TimeRemaining = totalSeconds;
        }

        public void CountDown(float deltaSeconds)
        {
            TimeRemaining -= deltaSeconds;
        }
    }
}
