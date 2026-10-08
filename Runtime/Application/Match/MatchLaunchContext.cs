using _Project.Application.Configs;
using _Project.Core.Match;

namespace _Project.Application.Match
{
    public class MatchLaunchContext
    {
        public GameMode GameMode { get; private set; } = GameMode.Masterstroke;
        public PaintingCategory Category { get; private set; } = PaintingCategory.Famous;
        public MatchType MatchType { get; private set; } = MatchType.Offline;
        public ReferenceConfig Reference { get; private set; }
        public string OpponentNickname { get; private set; } = string.Empty;
        public float RoundDurationSeconds { get; private set; }

        public void Setup(GameMode gameMode, MatchType matchType)
        {
            GameMode = gameMode;
            MatchType = matchType;
            OpponentNickname = string.Empty;
        }

        public void SetupOnline(GameMode gameMode, ReferenceConfig reference, string opponentNickname, float roundDurationSeconds)
        {
            GameMode = gameMode;
            MatchType = MatchType.Online;
            Reference = reference;
            OpponentNickname = opponentNickname ?? string.Empty;
            RoundDurationSeconds = roundDurationSeconds;
        }

        public void SetCategory(PaintingCategory category)
        {
            Category = category;
        }

        public void SelectReference(ReferenceConfig reference)
        {
            Reference = reference;
        }
    }
}
