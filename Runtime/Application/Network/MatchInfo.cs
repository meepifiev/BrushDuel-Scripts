using _Project.Core.Match;

namespace _Project.Application.Network
{
    public readonly struct MatchInfo
    {
        public MatchInfo(string paintingId, GameMode gameMode, string opponentNickname)
        {
            PaintingId = paintingId;
            GameMode = gameMode;
            OpponentNickname = opponentNickname;
        }

        public string PaintingId { get; }
        public GameMode GameMode { get; }
        public string OpponentNickname { get; }
    }
}
