using Mirror;

namespace _Project.Infrastructure.Network
{
    public struct JoinQueueMessage : NetworkMessage
    {
        public string Nickname;
        public string PlayerId;
    }

    public struct LeaveQueueMessage : NetworkMessage
    {
    }

    public struct CreateRoomMessage : NetworkMessage
    {
        public string Nickname;
        public string PlayerId;
        public int GameMode;
        public string PaintingId;
    }

    public struct LeaveMatchMessage : NetworkMessage
    {
    }

    public struct RoomCreatedMessage : NetworkMessage
    {
        public string Code;
    }

    public struct JoinRoomMessage : NetworkMessage
    {
        public string Code;
        public string Nickname;
        public string PlayerId;
    }

    public struct JoinRoomFailedMessage : NetworkMessage
    {
    }

    public struct SelfPlayRejectedMessage : NetworkMessage
    {
    }

    public struct MatchFoundMessage : NetworkMessage
    {
        public string PaintingId;
        public int GameMode;
        public string OpponentNickname;
    }
}
