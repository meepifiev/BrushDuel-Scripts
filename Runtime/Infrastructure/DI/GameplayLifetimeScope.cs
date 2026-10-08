using _Project.Application.Configs;
using _Project.Application.Drawing;
using _Project.Application.Match;
using _Project.Presentation.Match;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace _Project.Infrastructure.DI
{
    public class GameplayLifetimeScope : DrawingSceneLifetimeScope
    {
        [SerializeField] private MagicBrushConfig _magicBrushConfig;
        [SerializeField] private ResultPopupView _resultPopupView;
        [SerializeField] private ResultCoinBarView _resultCoinBarView;

        protected override void ConfigureMatch(IContainerBuilder builder)
        {
            RegisterResultViews(builder);
            RegisterMagicBrush(builder);
            RegisterRound(builder);
        }

        private void RegisterResultViews(IContainerBuilder builder)
        {
            builder.RegisterComponent(_resultPopupView);
            builder.RegisterComponent(_resultCoinBarView);
        }

        private void RegisterMagicBrush(IContainerBuilder builder)
        {
            builder.RegisterInstance(_magicBrushConfig);
            builder.RegisterEntryPoint<MagicBrushHandler>().AsSelf();
        }

        private void RegisterRound(IContainerBuilder builder)
        {
            builder.Register<RoundRewardService>(Lifetime.Scoped);
            builder.RegisterEntryPoint<RoundHandler>().AsSelf().As<IRoundTimeline>();
            builder.RegisterEntryPoint<RoundAnalysisController>().AsSelf();
            builder.RegisterEntryPoint<RoundResultPresenter>();
        }
    }
}
