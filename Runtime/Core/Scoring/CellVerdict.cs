namespace _Project.Core.Scoring
{
    public enum CellVerdict : byte
    {
        Empty = 0,
        Correct = 1,
        WrongColor = 2,
        Overpaint = 3,
        Missed = 4
    }
}
