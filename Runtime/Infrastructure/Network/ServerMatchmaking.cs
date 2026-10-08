using System;
using System.Collections.Generic;
using System.Text;
using _Project.Core.Match;
using Mirror;

namespace _Project.Infrastructure.Network
{
    public class ServerMatchmaking
    {
        private const string RoomCodeAlphabet = "ABCDEFGHJKMNPQRSTUVWXYZ23456789";
        private const int RoomCodeLength = 5;
        private const int MaxNicknameLength = 24;
        private const int MaxFailedJoinsPerWindow = 5;
        private const double FailedJoinWindowSeconds = 60d;

        private readonly List<ServerPlayer> _queue = new List<ServerPlayer>();
        private readonly Dictionary<string, ServerRoom> _rooms = new Dictionary<string, ServerRoom>();
        private readonly Dictionary<int, JoinAttempts> _failedJoins = new Dictionary<int, JoinAttempts>();
        private readonly Random _random = new Random();

        public ServerMatchPair? EnqueueForQuickMatch(NetworkConnectionToClient connection, string nickname, string playerId)
        {
            RemovePlayer(connection);
            ServerPlayer player = new ServerPlayer(connection, ClampNickname(nickname), playerId);

            int opponentIndex = FindQueuedOpponentIndex(playerId);

            if (opponentIndex >= 0)
            {
                ServerPlayer opponent = _queue[opponentIndex];
                _queue.RemoveAt(opponentIndex);
                return new ServerMatchPair(opponent, player, GameMode.Masterstroke);
            }

            _queue.Add(player);
            return null;
        }

        private int FindQueuedOpponentIndex(string playerId)
        {
            for (int i = 0; i < _queue.Count; i++)
            {
                if (string.IsNullOrEmpty(playerId) || _queue[i].PlayerId != playerId)
                {
                    return i;
                }
            }

            return -1;
        }

        public string CreateRoom(NetworkConnectionToClient connection, string nickname, string playerId, GameMode gameMode, string paintingId)
        {
            RemovePlayer(connection);
            ServerPlayer player = new ServerPlayer(connection, ClampNickname(nickname), playerId);
            string code = GenerateRoomCode();
            _rooms[code] = new ServerRoom(player, gameMode, paintingId);
            return code;
        }

        public bool TryJoinRoom(string code, NetworkConnectionToClient connection, string nickname, string playerId, out ServerMatchPair pair)
        {
            pair = default;

            if (IsJoinBudgetExceeded(connection))
            {
                return false;
            }

            RemovePlayer(connection);
            string normalizedCode = NormalizeCode(code);

            if (normalizedCode == null || _rooms.TryGetValue(normalizedCode, out ServerRoom room) == false)
            {
                RegisterFailedJoin(connection);
                return false;
            }

            _rooms.Remove(normalizedCode);
            _failedJoins.Remove(connection.connectionId);
            ServerPlayer guest = new ServerPlayer(connection, ClampNickname(nickname), playerId);
            pair = new ServerMatchPair(room.Host, guest, room.GameMode, room.PaintingId);
            return true;
        }

        public bool HasPlayer(string playerId, NetworkConnectionToClient except)
        {
            if (string.IsNullOrEmpty(playerId))
            {
                return false;
            }

            foreach (ServerPlayer queued in _queue)
            {
                if (queued.Connection != except && queued.PlayerId == playerId)
                {
                    return true;
                }
            }

            foreach (KeyValuePair<string, ServerRoom> room in _rooms)
            {
                if (room.Value.Host.Connection != except && room.Value.Host.PlayerId == playerId)
                {
                    return true;
                }
            }

            return false;
        }

        public void RemovePlayer(NetworkConnectionToClient connection)
        {
            for (int i = _queue.Count - 1; i >= 0; i--)
            {
                if (_queue[i].Connection == connection)
                {
                    _queue.RemoveAt(i);
                }
            }

            string ownedRoomCode = null;

            foreach (KeyValuePair<string, ServerRoom> room in _rooms)
            {
                if (room.Value.Host.Connection == connection)
                {
                    ownedRoomCode = room.Key;
                    break;
                }
            }

            if (ownedRoomCode != null)
            {
                _rooms.Remove(ownedRoomCode);
            }
        }

        public void Forget(NetworkConnectionToClient connection)
        {
            RemovePlayer(connection);
            _failedJoins.Remove(connection.connectionId);
        }

        public void Clear()
        {
            _queue.Clear();
            _rooms.Clear();
            _failedJoins.Clear();
        }

        private bool IsJoinBudgetExceeded(NetworkConnectionToClient connection)
        {
            if (_failedJoins.TryGetValue(connection.connectionId, out JoinAttempts attempts) == false)
            {
                return false;
            }

            double elapsed = NetworkTime.localTime - attempts.WindowStart;

            if (elapsed > FailedJoinWindowSeconds)
            {
                _failedJoins.Remove(connection.connectionId);
                return false;
            }

            return attempts.Count >= MaxFailedJoinsPerWindow;
        }

        private void RegisterFailedJoin(NetworkConnectionToClient connection)
        {
            double now = NetworkTime.localTime;

            if (_failedJoins.TryGetValue(connection.connectionId, out JoinAttempts attempts) == false || now - attempts.WindowStart > FailedJoinWindowSeconds)
            {
                _failedJoins[connection.connectionId] = new JoinAttempts(now, 1);
                return;
            }

            _failedJoins[connection.connectionId] = new JoinAttempts(attempts.WindowStart, attempts.Count + 1);
        }

        private string GenerateRoomCode()
        {
            string code = BuildRoomCode();

            while (_rooms.ContainsKey(code))
            {
                code = BuildRoomCode();
            }

            return code;
        }

        private string BuildRoomCode()
        {
            StringBuilder builder = new StringBuilder(RoomCodeLength);

            for (int i = 0; i < RoomCodeLength; i++)
            {
                builder.Append(RoomCodeAlphabet[_random.Next(RoomCodeAlphabet.Length)]);
            }

            return builder.ToString();
        }

        private string NormalizeCode(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return null;
            }

            return code.Trim().ToUpperInvariant();
        }

        private string ClampNickname(string nickname)
        {
            if (string.IsNullOrWhiteSpace(nickname))
            {
                return string.Empty;
            }

            StringBuilder builder = new StringBuilder(nickname.Length);

            foreach (char symbol in nickname.Trim())
            {
                if (symbol == '<' || symbol == '>')
                {
                    continue;
                }

                builder.Append(symbol);

                if (builder.Length >= MaxNicknameLength)
                {
                    break;
                }
            }

            return builder.ToString();
        }

        private readonly struct JoinAttempts
        {
            public JoinAttempts(double windowStart, int count)
            {
                WindowStart = windowStart;
                Count = count;
            }

            public double WindowStart { get; }
            public int Count { get; }
        }
    }
}
