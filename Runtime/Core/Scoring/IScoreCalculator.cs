namespace _Project.Core.Scoring
{
    public interface IScoreCalculator
    {
        ScoreResult Calculate(ImageSnapshot reference, ImageSnapshot drawing);
    }
}
