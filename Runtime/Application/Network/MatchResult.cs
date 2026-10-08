namespace _Project.Application.Network
{
    public readonly struct MatchResult
    {
        public MatchResult(int myScore, int opponentScore, MatchOutcome outcome)
        {
            MyScore = myScore;
            OpponentScore = opponentScore;
            Outcome = outcome;
        }

        public int MyScore { get; }
        public int OpponentScore { get; }
        public MatchOutcome Outcome { get; }
    }
}
