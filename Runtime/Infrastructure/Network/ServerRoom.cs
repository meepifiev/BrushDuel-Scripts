using _Project.Core.Match;

namespace _Project.Infrastructure.Network
{
    public readonly struct ServerRoom
    {
        public ServerRoom(ServerPlayer host, GameMode gameMode, string paintingId)
        {
            Host = host;
            GameMode = gameMode;
            PaintingId = paintingId;
        }

        public ServerPlayer Host { get; }
        public GameMode GameMode { get; }
        public string PaintingId { get; }
    }
}
