using System;
using _Project.Application.Configs;
using _Project.Application.Platform;
using Cysharp.Threading.Tasks;
using Steamworks;
using UnityEngine;
using VContainer.Unity;

namespace _Project.Infrastructure.Steam
{
    public class SteamPlatformInitializer : IPlatformInitializer, ITickable, IDisposable
    {
        private readonly SteamConfig _config;

        private bool _initialized;

        public SteamPlatformInitializer(SteamConfig config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            _config = config;
        }

        public UniTask InitializeAsync()
        {
            if (UnityEngine.Application.isBatchMode)
            {
                return UniTask.CompletedTask;
            }

            if (UnityEngine.Application.isEditor == false && SteamClient.RestartAppIfNecessary(_config.AppId))
            {
                UnityEngine.Application.Quit();
                return UniTask.CompletedTask;
            }

            try
            {
                SteamClient.Init(_config.AppId, asyncCallbacks: false);
                _initialized = true;
            }
            catch (Exception exception)
            {
                Debug.LogWarning("Steam init failed, running without Steam - " + exception.Message);
            }

            return UniTask.CompletedTask;
        }

        public void Tick()
        {
            if (_initialized)
            {
                SteamClient.RunCallbacks();
            }
        }

        public void Dispose()
        {
            if (_initialized)
            {
                SteamClient.Shutdown();
                _initialized = false;
            }
        }
    }
}
