using _Project.Application.Advertising;
using _Project.Application.Configs;
using _Project.Application.AssetManagement;
using _Project.Infrastructure.AssetManagement;
using _Project.Application.Factories;
using _Project.Application.Localization;
using _Project.Application.Match;
using _Project.Application.Network;
using _Project.Application.Platform;
using _Project.Application.Profile;
using _Project.Application.Progress;
using _Project.Application.Purchasing;
using _Project.Application.SceneManagement;
using _Project.Application.Settings;
using _Project.Infrastructure.Factories;
using _Project.Infrastructure.Network;
using _Project.Infrastructure.SceneManagement;
using _Project.Infrastructure.Steam;
using _Project.Presentation.Audio;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace _Project.Infrastructure.DI
{
    public class RootLifetimeScope : LifetimeScope
    {
        [SerializeField] private GradingConfig _gradingConfig;
        [SerializeField] private LocalizationConfig _localizationConfig;
        [SerializeField] private ServerConfig _serverConfig;
        [SerializeField] private ServerRegionsConfig _serverRegionsConfig;
        [SerializeField] private EconomyConfig _economyConfig;
        [SerializeField] private SteamConfig _steamConfig;
        [SerializeField] private VersionConfig _versionConfig;

        protected override void Configure(IContainerBuilder builder)
        {
#if !UNITY_SERVER
            RegisterConfigs(builder);
            RegisterContent(builder);
            RegisterPlatform(builder);
            RegisterNetwork(builder);
            RegisterProgress(builder);
            RegisterScenes(builder);
            RegisterFactories(builder);
            RegisterMatch(builder);
#endif
        }

        private void RegisterConfigs(IContainerBuilder builder)
        {
            builder.RegisterInstance(_gradingConfig);
            builder.RegisterInstance(_localizationConfig);
            builder.RegisterInstance(_serverConfig);
            builder.RegisterInstance(_serverRegionsConfig);
            builder.RegisterInstance(_economyConfig);
            builder.RegisterInstance(_steamConfig);
            builder.RegisterInstance(_versionConfig);
        }

        private void RegisterContent(IContainerBuilder builder)
        {
            builder.Register<AddressableReferenceCatalog>(Lifetime.Singleton).As<IReferenceCatalog>();
            builder.Register<AddressableContentLoader>(Lifetime.Singleton).As<IContentLoader>();
            builder.Register<LoadingProgress>(Lifetime.Singleton).AsSelf();
        }

        private void RegisterPlatform(IContainerBuilder builder)
        {
            builder.Register<SteamLocalizationService>(Lifetime.Singleton).As<ILocalizationService>().As<ILanguageSettings>();
            builder.Register<SteamPurchaseService>(Lifetime.Singleton).As<IPurchaseService>();
            builder.Register<SteamAdvertisingService>(Lifetime.Singleton).As<IAdvertisingService>();
            builder.Register<SteamPlayerProfileService>(Lifetime.Singleton).As<IPlayerProfileService>();
            
            builder.RegisterEntryPoint<SteamPlatformInitializer>().As<IPlatformInitializer>();
        }

        private void RegisterNetwork(IContainerBuilder builder)
        {
            builder.Register<SelectedRegion>(Lifetime.Singleton);
            builder.Register<UdpServerPingService>(Lifetime.Singleton).As<IServerPingService>();
            builder.Register<ClientConnection>(Lifetime.Singleton).As<IClientConnection>();
            builder.Register<MatchClient>(Lifetime.Singleton).As<IMatchmakingService>().As<IMatchSession>();
        }

        private void RegisterProgress(IContainerBuilder builder)
        {
            builder.Register<LocalSaveFile>(Lifetime.Singleton);
            builder.Register<FileSettingsStorage>(Lifetime.Singleton).As<ISettingsStorage>();
            builder.Register<FileProgressStorage>(Lifetime.Singleton).As<IProgressStorage>();
            builder.RegisterEntryPoint<DisplaySettingsService>(Lifetime.Singleton).As<IDisplaySettings>();
        }

        private void RegisterScenes(IContainerBuilder builder)
        {
            builder.Register<SceneLoader>(Lifetime.Singleton).As<ISceneLoader>();
            builder.Register<AudioService>(Lifetime.Singleton).As<IAudioService>();
        }

        private void RegisterFactories(IContainerBuilder builder)
        {
            builder.Register<ObjectFactory>(Lifetime.Singleton).As<IObjectFactory>();
            builder.Register<InjectObjectFactory>(Lifetime.Scoped).As<IInjectObjectFactory>();
        }

        private void RegisterMatch(IContainerBuilder builder)
        {
            builder.Register<MatchLaunchContext>(Lifetime.Singleton);
        }
    }
}
