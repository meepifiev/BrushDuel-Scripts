using Mirror;

namespace _Project.Infrastructure.Network
{
    public struct MatchAssignedMessage : NetworkMessage
    {
        public string SessionPort;
        public string Token;
        public string PaintingId;
        public int GameMode;
        public string OpponentNickname;
        public float RoundDurationSeconds;
    }

    public struct RedeemMatchMessage : NetworkMessage
    {
        public string Token;
    }

    public struct RedeemRejectedMessage : NetworkMessage
    {
    }
}
