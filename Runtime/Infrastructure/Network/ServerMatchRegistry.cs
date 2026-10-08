using System;
using System.Collections.Generic;
using _Project.Application.Configs;
using _Project.Core.Match;
using Mirror;

namespace _Project.Infrastructure.Network
{
    public class ServerMatchRegistry
    {
        private const float IntroDurationSeconds = 3f;
        private const float FinishGraceSeconds = 2f;

        private readonly List<ServerMatch> _matches = new List<ServerMatch>();

        public event Action<ServerMatch> RoundStarted;
        public event Action<ServerMatch> RoundFinished;
        public event Action<ServerMatch> MatchCanceled;

        public ServerMatch CreateMatch(ServerMatchPair pair, GameMode gameMode, ReferenceConfig reference, float roundDurationSeconds, float readyTimeoutSeconds)
        {
            ServerMatch match = new ServerMatch(pair.First, pair.Second, gameMode, reference, roundDurationSeconds, readyTimeoutSeconds);
            _matches.Add(match);
            return match;
        }

        public ServerMatch FindMatch(NetworkConnectionToClient connection)
        {
            for (int i = 0; i < _matches.Count; i++)
            {
                if (_matches[i].Contains(connection))
                {
                    return _matches[i];
                }
            }

            return null;
        }

        public bool Contains(NetworkConnectionToClient connection)
        {
            return FindMatch(connection) != null;
        }

        public bool HasPlayer(string playerId, NetworkConnectionToClient except)
        {
            if (string.IsNullOrEmpty(playerId))
            {
                return false;
            }

            for (int i = 0; i < _matches.Count; i++)
            {
                ServerMatch match = _matches[i];

                if (match.First.Connection != except && match.First.PlayerId == playerId)
                {
                    return true;
                }

                if (match.Second.Connection != except && match.Second.PlayerId == playerId)
                {
                    return true;
                }
            }

            return false;
        }

        public NetworkConnectionToClient FindOpponent(NetworkConnectionToClient connection)
        {
            ServerMatch match = FindMatch(connection);

            if (match == null)
            {
                return null;
            }

            if (match.First.Connection == connection)
            {
                return match.Second.Connection;
            }

            return match.First.Connection;
        }

        public void MarkReady(NetworkConnectionToClient connection)
        {
            ServerMatch match = FindMatch(connection);

            if (match == null || match.State != ServerMatchState.WaitingReady)
            {
                return;
            }

            match.MarkReady(connection);

            if (match.BothReady == false)
            {
                return;
            }

            match.StartRound(IntroDurationSeconds + match.RoundDurationSeconds + FinishGraceSeconds);
            RoundStarted?.Invoke(match);
        }

        public void MarkDone(NetworkConnectionToClient connection)
        {
            ServerMatch match = FindMatch(connection);

            if (match == null || match.State != ServerMatchState.Playing)
            {
                return;
            }

            match.MarkDone(connection);

            if (match.BothDone == false)
            {
                return;
            }

            _matches.Remove(match);
            RoundFinished?.Invoke(match);
        }

        public void RemovePlayer(NetworkConnectionToClient connection)
        {
            ServerMatch match = FindMatch(connection);

            while (match != null)
            {
                _matches.Remove(match);
                MatchCanceled?.Invoke(match);
                match = FindMatch(connection);
            }
        }

        public void Tick(float deltaSeconds)
        {
            for (int i = _matches.Count - 1; i >= 0; i--)
            {
                ServerMatch match = _matches[i];
                match.CountDown(deltaSeconds);

                if (match.TimeRemaining > 0f)
                {
                    continue;
                }

                _matches.RemoveAt(i);

                if (match.State == ServerMatchState.WaitingReady)
                {
                    MatchCanceled?.Invoke(match);
                }
                else
                {
                    RoundFinished?.Invoke(match);
                }
            }
        }

        public void Clear()
        {
            _matches.Clear();
        }
    }
}
