using Mirror;

namespace _Project.Infrastructure.Network
{
    public struct PlayerReadyMessage : NetworkMessage
    {
    }

    public struct RoundStartMessage : NetworkMessage
    {
        public float DurationSeconds;
    }

    public struct PlayerDoneMessage : NetworkMessage
    {
    }

    public struct RoundResultMessage : NetworkMessage
    {
        public int MyScore;
        public int OpponentScore;
        public int Outcome;
    }

    public struct MatchCanceledMessage : NetworkMessage
    {
    }
}
