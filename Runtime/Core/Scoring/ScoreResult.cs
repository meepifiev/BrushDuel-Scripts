namespace _Project.Core.Scoring
{
    public readonly struct ScoreResult
    {
        public ScoreResult(float total, float silhouette, float edge, float color, float coverage)
        {
            Total = total;
            Silhouette = silhouette;
            Edge = edge;
            Color = color;
            Coverage = coverage;
        }

        public readonly float Total;
        public readonly float Silhouette;
        public readonly float Edge;
        public readonly float Color;
        public readonly float Coverage;
    }
}
