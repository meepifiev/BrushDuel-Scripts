using _Project.Core.Match;

namespace _Project.Infrastructure.Network
{
    public readonly struct ServerMatchPair
    {
        public ServerMatchPair(ServerPlayer first, ServerPlayer second, GameMode gameMode, string paintingId = null)
        {
            First = first;
            Second = second;
            GameMode = gameMode;
            PaintingId = paintingId;
        }

        public ServerPlayer First { get; }
        public ServerPlayer Second { get; }
        public GameMode GameMode { get; }
        public string PaintingId { get; }
    }
}
