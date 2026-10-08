using System;
using _Project.Application.Configs;
using _Project.Application.Drawing;
using _Project.Core.Drawing;
using _Project.Core.Scoring;
using UnityEngine;
using VContainer.Unity;

namespace _Project.Presentation.Drawing
{
    public class SimilarityHandler : IStartable, IDisposable
    {
        private readonly ReferenceConfig _config;
        private readonly DrawingSessionService _session;
        private readonly DrawingCanvasPresenter _presenter;
        private readonly SnapshotBuilder _snapshotBuilder;
        private readonly AnalysisGrid _analysisGrid;
        private readonly IScoreCalculator _scoreCalculator;
        private readonly CanvasSize _canvasSize;

        private ImageSnapshot _referenceSnapshot;
        private ImageSnapshot _drawingSnapshot;

        public SimilarityHandler(
            ReferenceConfig config,
            DrawingSessionService session,
            DrawingCanvasPresenter presenter,
            SnapshotBuilder snapshotBuilder,
            AnalysisGrid analysisGrid,
            IScoreCalculator scoreCalculator,
            CanvasSize canvasSize)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            _config = config;
            _session = session ?? throw new ArgumentNullException(nameof(session));
            _presenter = presenter ?? throw new ArgumentNullException(nameof(presenter));
            _snapshotBuilder = snapshotBuilder ?? throw new ArgumentNullException(nameof(snapshotBuilder));
            _analysisGrid = analysisGrid ?? throw new ArgumentNullException(nameof(analysisGrid));
            _scoreCalculator = scoreCalculator ?? throw new ArgumentNullException(nameof(scoreCalculator));
            _canvasSize = canvasSize ?? throw new ArgumentNullException(nameof(canvasSize));
        }

        public event Action<float> SimilarityChanged;

        public ScoreResult CurrentScore { get; private set; }

        public ImageSnapshot ReferenceSnapshot => _referenceSnapshot;

        public ImageSnapshot DrawingSnapshot => _drawingSnapshot;

        public void Start()
        {
            _referenceSnapshot = new ImageSnapshot(_analysisGrid);
            _drawingSnapshot = new ImageSnapshot(_analysisGrid);
            _snapshotBuilder.BuildFromIndices(_referenceSnapshot, _config.BakedData.GridIndices);

            _session.CommandCompleted += Recompute;
            _session.DocumentChanged += Recompute;
            Recompute();
        }

        public void Dispose()
        {
            _session.CommandCompleted -= Recompute;
            _session.DocumentChanged -= Recompute;
        }

        private void Recompute()
        {
            _snapshotBuilder.Build(_drawingSnapshot, _presenter.PixelBuffer, _canvasSize.Width, _canvasSize.Height, _session.Palette);
            CurrentScore = _scoreCalculator.Calculate(_referenceSnapshot, _drawingSnapshot);
            SimilarityChanged?.Invoke(CurrentScore.Total);
        }
    }
}
