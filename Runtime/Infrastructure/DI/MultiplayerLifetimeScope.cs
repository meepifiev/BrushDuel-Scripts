using _Project.Application.Drawing;
using _Project.Application.Match;
using _Project.Infrastructure.Network;
using _Project.Presentation.Drawing;
using _Project.Presentation.Match;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace _Project.Infrastructure.DI
{
    public class MultiplayerLifetimeScope : DrawingSceneLifetimeScope
    {
        [SerializeField] private DrawingCanvasView _opponentCanvasView;

        protected override void ConfigureMatch(IContainerBuilder builder)
        {
            RegisterRound(builder);
            RegisterDrawingSync(builder);
        }

        private void RegisterRound(IContainerBuilder builder)
        {
            builder.RegisterEntryPoint<MultiplayerRoundHandler>(Lifetime.Scoped).AsSelf().As<IRoundTimeline>();
            builder.RegisterEntryPoint<PostRoundAnalysisPresenter>(Lifetime.Scoped).AsSelf();
        }

        private void RegisterDrawingSync(IContainerBuilder builder)
        {
            builder.RegisterEntryPoint<DrawingSyncChannel>(Lifetime.Scoped);
            builder.RegisterEntryPoint<DrawingSyncSender>(Lifetime.Scoped);
            builder.RegisterEntryPoint<OpponentCanvasPresenter>(Lifetime.Scoped).AsSelf().WithParameter(_opponentCanvasView);
        }
    }
}
