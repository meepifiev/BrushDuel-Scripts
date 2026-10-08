using _Project.Application.Configs;
using _Project.Application.Drawing;
using _Project.Core.Drawing;
using _Project.Core.Scoring;
using _Project.Presentation.Drawing;
using _Project.Presentation.Match;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace _Project.Infrastructure.DI
{
    public abstract class DrawingSceneLifetimeScope : LifetimeScope
    {
        [SerializeField] private ScoringConfig _scoringConfig;
        [SerializeField] private DrawingCanvasView _drawingCanvasView;
        [SerializeField] private CanvasInputView _canvasInputView;

        protected override void Configure(IContainerBuilder builder)
        {
            RegisterViews(builder);
            RegisterCanvas(builder);
            RegisterDrawing(builder);
            RegisterScoring(builder);
            RegisterCommonEntryPoints(builder);
            ConfigureMatch(builder);
        }

        protected abstract void ConfigureMatch(IContainerBuilder builder);

        private void RegisterViews(IContainerBuilder builder)
        {
            builder.RegisterComponent(_drawingCanvasView);
            builder.RegisterComponent(_canvasInputView);
            builder.RegisterComponentInHierarchy<ReferenceImageView>();
        }

        private void RegisterCanvas(IContainerBuilder builder)
        {
            builder.Register(DrawingSceneFactory.ResolveReference, Lifetime.Scoped);
            builder.Register(DrawingSceneFactory.CreateCanvasSize, Lifetime.Scoped);
            builder.Register<AnalysisGrid>(Lifetime.Scoped);
            builder.Register(DrawingSceneFactory.CreatePalette, Lifetime.Scoped);
            builder.Register(DrawingSceneFactory.CreateReferenceCanvasMap, Lifetime.Scoped);
        }

        private void RegisterDrawing(IContainerBuilder builder)
        {
            builder.Register<DrawingDocument>(Lifetime.Scoped);
            builder.Register<StrokeRasterizer>(Lifetime.Scoped);
            builder.Register<DrawingSessionService>(Lifetime.Scoped);
        }

        private void RegisterScoring(IContainerBuilder builder)
        {
            builder.RegisterInstance(_scoringConfig.BuildMasterstroke());
            builder.RegisterInstance(_scoringConfig.BuildWildstroke());

            builder.Register<PaletteQuantizer>(Lifetime.Scoped);
            builder.Register<SnapshotBuilder>(Lifetime.Scoped);
            builder.Register<ScoreGates>(Lifetime.Scoped);
            builder.Register<MasterstrokeScoreCalculator>(Lifetime.Scoped);
            builder.Register<WildstrokeScoreCalculator>(Lifetime.Scoped);
            builder.Register(DrawingSceneFactory.ResolveScoreCalculator, Lifetime.Scoped);
        }

        private void RegisterCommonEntryPoints(IContainerBuilder builder)
        {
            builder.RegisterEntryPoint<DrawingCanvasPresenter>().AsSelf();
            builder.RegisterEntryPoint<SimilarityHandler>().AsSelf();
        }
    }
}
