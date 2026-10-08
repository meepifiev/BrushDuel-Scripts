using System.Collections.Generic;
using Mirror;

namespace _Project.Infrastructure.Network
{
    public class ReservedPlayer
    {
        public string PlayerId;
        public string Nickname;
        public NetworkConnectionToClient Connection;
    }

    public class MatchReservation
    {
        public string MatchId;
        public string PaintingId;
        public int GameMode;
        public float RoundDurationSeconds;
        public float CreatedAt;
        public ReservedPlayer First;
        public ReservedPlayer Second;

        public bool BothRedeemed => First != null && Second != null;
    }

    public class SessionReservationStore
    {
        private readonly Dictionary<string, MatchReservation> _byMatch = new Dictionary<string, MatchReservation>();

        public int Count => _byMatch.Count;

        public MatchReservation GetOrCreate(string matchId, float now)
        {
            if (_byMatch.TryGetValue(matchId, out MatchReservation reservation) == false)
            {
                reservation = new MatchReservation { MatchId = matchId, CreatedAt = now };
                _byMatch[matchId] = reservation;
            }

            return reservation;
        }

        public MatchReservation Find(string matchId)
        {
            return _byMatch.TryGetValue(matchId, out MatchReservation reservation) ? reservation : null;
        }

        public void Remove(string matchId)
        {
            _byMatch.Remove(matchId);
        }

        public void Clear()
        {
            _byMatch.Clear();
        }

        public MatchReservation FindByConnection(NetworkConnectionToClient connection)
        {
            foreach (KeyValuePair<string, MatchReservation> entry in _byMatch)
            {
                MatchReservation reservation = entry.Value;

                if (reservation.First != null && reservation.First.Connection == connection)
                {
                    return reservation;
                }

                if (reservation.Second != null && reservation.Second.Connection == connection)
                {
                    return reservation;
                }
            }

            return null;
        }

        public List<MatchReservation> DrainExpired(float now, float ttlSeconds)
        {
            List<MatchReservation> expired = new List<MatchReservation>();

            foreach (KeyValuePair<string, MatchReservation> entry in _byMatch)
            {
                if (entry.Value.BothRedeemed == false && now - entry.Value.CreatedAt > ttlSeconds)
                {
                    expired.Add(entry.Value);
                }
            }

            for (int i = 0; i < expired.Count; i++)
            {
                _byMatch.Remove(expired[i].MatchId);
            }

            return expired;
        }
    }
}
