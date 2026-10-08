using System;
using _Project.Application.Match;
using _Project.Core.Drawing;
using _Project.Core.Scoring;
using _Project.Presentation.Drawing;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer.Unity;

namespace _Project.Presentation.Match
{
    public class PostRoundAnalysisPresenter : IStartable, IDisposable
    {
        private const float SweepDuration = 2.5f;
        private const float PopupDelaySeconds = 0.4f;

        private readonly MultiplayerRoundHandler _roundHandler;
        private readonly SimilarityHandler _similarity;
        private readonly OpponentCanvasPresenter _opponent;
        private readonly SnapshotBuilder _snapshotBuilder;
        private readonly AnalysisGrid _grid;
        private readonly CanvasSize _canvasSize;
        private readonly PaletteColor[] _palette;
        private readonly DrawingCanvasView _canvasView;

        private AccuracyAnalyzer _analyzer;
        private AccuracyMap _myMap;
        private AccuracyMap _opponentMap;
        private ImageSnapshot _opponentSnapshot;
        private CanvasAnalysisOverlay _myOverlay;
        private CanvasAnalysisOverlay _opponentOverlay;
        private CanvasAnalysisOverlay _referenceOverlay;
        private ReferenceImageView _referenceView;

        public PostRoundAnalysisPresenter(
            MultiplayerRoundHandler roundHandler,
            SimilarityHandler similarity,
            OpponentCanvasPresenter opponent,
            SnapshotBuilder snapshotBuilder,
            AnalysisGrid grid,
            CanvasSize canvasSize,
            PaletteColor[] palette,
            DrawingCanvasView canvasView,
            ReferenceImageView referenceView)
        {
            _roundHandler = roundHandler ?? throw new ArgumentNullException(nameof(roundHandler));
            _similarity = similarity ?? throw new ArgumentNullException(nameof(similarity));
            _opponent = opponent ?? throw new ArgumentNullException(nameof(opponent));
            _snapshotBuilder = snapshotBuilder ?? throw new ArgumentNullException(nameof(snapshotBuilder));
            _grid = grid ?? throw new ArgumentNullException(nameof(grid));
            _canvasSize = canvasSize ?? throw new ArgumentNullException(nameof(canvasSize));
            _palette = palette ?? throw new ArgumentNullException(nameof(palette));
            _canvasView = canvasView != null ? canvasView : throw new ArgumentNullException(nameof(canvasView));
            _referenceView = referenceView != null ? referenceView : throw new ArgumentNullException(nameof(referenceView));
        }

        public event Action Completed;

        public void Start()
        {
            _analyzer = new AccuracyAnalyzer(_palette);
            _myMap = new AccuracyMap(_grid);
            _opponentMap = new AccuracyMap(_grid);
            _opponentSnapshot = new ImageSnapshot(_grid);
            _myOverlay = new CanvasAnalysisOverlay((RectTransform)_canvasView.transform, true, AnalysisOverlayStyle.Hatch);
            _opponentOverlay = new CanvasAnalysisOverlay((RectTransform)_opponent.CanvasView.transform, true, AnalysisOverlayStyle.Hatch);
            _referenceOverlay = new CanvasAnalysisOverlay(ResolveReferenceParent(), false, AnalysisOverlayStyle.Solid);
            _roundHandler.Finished += OnFinished;
        }

        public void Dispose()
        {
            _roundHandler.Finished -= OnFinished;
            _myOverlay.Dispose();
            _opponentOverlay.Dispose();
            _referenceOverlay.Dispose();
        }

        private RectTransform ResolveReferenceParent()
        {
            GridOverlayView gridOverlay = _referenceView.GetComponentInChildren<GridOverlayView>(true);
            return gridOverlay != null ? (RectTransform)gridOverlay.transform : (RectTransform)_referenceView.transform;
        }

        private void OnFinished()
        {
            RunAsync().Forget();
        }

        private async UniTaskVoid RunAsync()
        {
            _analyzer.Measure(_similarity.ReferenceSnapshot, _similarity.DrawingSnapshot, _myMap);
            _snapshotBuilder.Build(_opponentSnapshot, _opponent.PixelBuffer, _canvasSize.Width, _canvasSize.Height, _palette);
            _analyzer.Measure(_similarity.ReferenceSnapshot, _opponentSnapshot, _opponentMap);

            await UniTask.WhenAll(
                _myOverlay.PlayAsync(_myMap, SweepDuration),
                _opponentOverlay.PlayAsync(_opponentMap, SweepDuration),
                _referenceOverlay.PlayAsync(_myMap, SweepDuration));

            await UniTask.Delay(TimeSpan.FromSeconds(PopupDelaySeconds));
            Completed?.Invoke();
        }
    }
}
