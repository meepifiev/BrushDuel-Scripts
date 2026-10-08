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
    public class RoundAnalysisController : IStartable, IDisposable
    {
        private const float SweepDuration = 2.5f;
        private const float PopupDelaySeconds = 0.4f;

        private readonly RoundHandler _roundHandler;
        private readonly SimilarityHandler _similarity;
        private readonly DrawingCanvasView _canvasView;
        private readonly ReferenceImageView _referenceView;
        private readonly AnalysisGrid _grid;
        private readonly PaletteColor[] _palette;

        private AccuracyAnalyzer _analyzer;
        private AccuracyMap _map;
        private CanvasAnalysisOverlay _canvasOverlay;
        private CanvasAnalysisOverlay _referenceOverlay;

        public RoundAnalysisController(
            RoundHandler roundHandler,
            SimilarityHandler similarity,
            DrawingCanvasView canvasView,
            ReferenceImageView referenceView,
            AnalysisGrid grid,
            PaletteColor[] palette)
        {
            _roundHandler = roundHandler ?? throw new ArgumentNullException(nameof(roundHandler));
            _similarity = similarity ?? throw new ArgumentNullException(nameof(similarity));
            _canvasView = canvasView != null ? canvasView : throw new ArgumentNullException(nameof(canvasView));
            _referenceView = referenceView != null ? referenceView : throw new ArgumentNullException(nameof(referenceView));
            _grid = grid ?? throw new ArgumentNullException(nameof(grid));
            _palette = palette ?? throw new ArgumentNullException(nameof(palette));
        }

        public event Action Completed;

        public void Start()
        {
            _analyzer = new AccuracyAnalyzer(_palette);
            _map = new AccuracyMap(_grid);
            _canvasOverlay = new CanvasAnalysisOverlay((RectTransform)_canvasView.transform, true, AnalysisOverlayStyle.Hatch);
            _referenceOverlay = new CanvasAnalysisOverlay(ResolveReferenceParent(), false, AnalysisOverlayStyle.Solid);
            _roundHandler.Finished += OnFinished;
        }

        public void Dispose()
        {
            _roundHandler.Finished -= OnFinished;
            _canvasOverlay.Dispose();
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
            _analyzer.Measure(_similarity.ReferenceSnapshot, _similarity.DrawingSnapshot, _map);

            await UniTask.WhenAll(
                _canvasOverlay.PlayAsync(_map, SweepDuration),
                _referenceOverlay.PlayAsync(_map, SweepDuration));

            await UniTask.Delay(TimeSpan.FromSeconds(PopupDelaySeconds));
            Completed?.Invoke();
        }
    }
}
