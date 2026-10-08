using System;
using _Project.Application.Configs;
using _Project.Core.Drawing;
using _Project.Core.Match;
using _Project.Core.Scoring;

namespace _Project.Application.Match
{
    public class MatchScorer
    {
        private readonly ScoringConfig _scoringConfig;

        public MatchScorer(ScoringConfig scoringConfig)
        {
            _scoringConfig = scoringConfig != null ? scoringConfig : throw new ArgumentNullException(nameof(scoringConfig));
        }

        public float Score(ReferenceConfig reference, GameMode gameMode, DrawingDocument document)
        {
            if (reference == null)
            {
                throw new ArgumentNullException(nameof(reference));
            }

            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            ReferenceBakedData baked = reference.BakedData;
            CanvasSize canvasSize = new CanvasSize(baked.SourceWidth, baked.SourceHeight);
            PaletteColor[] palette = reference.BuildPalette();

            AnalysisGrid grid = new AnalysisGrid(canvasSize);
            SnapshotBuilder snapshotBuilder = new SnapshotBuilder(new PaletteQuantizer());
            ReferenceCanvasMap referenceMap = new ReferenceCanvasMap(canvasSize, baked.CanvasIndices);
            StrokeRasterizer rasterizer = new StrokeRasterizer(canvasSize, referenceMap);

            byte[] buffer = new byte[canvasSize.BufferLength];
            rasterizer.RasterizeDocument(document, palette, buffer);

            ImageSnapshot referenceSnapshot = new ImageSnapshot(grid);
            ImageSnapshot drawingSnapshot = new ImageSnapshot(grid);
            snapshotBuilder.BuildFromIndices(referenceSnapshot, baked.GridIndices);
            snapshotBuilder.Build(drawingSnapshot, buffer, canvasSize.Width, canvasSize.Height, palette);

            IScoreCalculator calculator = ResolveCalculator(gameMode, palette);
            return calculator.Calculate(referenceSnapshot, drawingSnapshot).Total;
        }

        private IScoreCalculator ResolveCalculator(GameMode gameMode, PaletteColor[] palette)
        {
            if (gameMode == GameMode.Masterstroke)
            {
                return new MasterstrokeScoreCalculator(_scoringConfig.BuildMasterstroke(), new ScoreGates(), palette);
            }

            return new WildstrokeScoreCalculator(_scoringConfig.BuildWildstroke(), new ScoreGates());
        }
    }
}
