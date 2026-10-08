using _Project.Application.Boot;
using _Project.Application.Configs;
using _Project.Presentation.UI;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace _Project.Infrastructure.DI
{
    public class BootLifetimeScope : LifetimeScope
    {
        [SerializeField] private BootConfig _bootConfig;

        protected override void Configure(IContainerBuilder builder)
        {
#if !UNITY_SERVER
            RegisterBoot(builder);
#endif
        }

#if !UNITY_SERVER
        private void RegisterBoot(IContainerBuilder builder)
        {
            builder.RegisterInstance(_bootConfig);
            builder.RegisterComponentInHierarchy<BootLoadingView>();
            builder.RegisterComponentInHierarchy<BootIntroView>().As<IBootIntro>();
            builder.RegisterEntryPoint<Bootstrapper>();
        }
#endif
    }
}
