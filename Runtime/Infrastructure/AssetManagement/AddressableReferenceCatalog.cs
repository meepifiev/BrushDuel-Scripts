using System;
using System.Collections.Generic;
using System.Threading;
using _Project.Application.AssetManagement;
using _Project.Application.Configs;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace _Project.Infrastructure.AssetManagement
{
    public class AddressableReferenceCatalog : IReferenceCatalog
    {
        private const string ReferenceLabel = "reference";

        private readonly Dictionary<string, ReferenceConfig> _cache = new Dictionary<string, ReferenceConfig>();

        private List<IResourceLocation> _locations;
        private bool _initialized;

        public int Count => _locations?.Count ?? 0;

        public async UniTask InitAsync(CancellationToken cancellationToken)
        {
            if (_initialized)
            {
                return;
            }

            AsyncOperationHandle<IList<IResourceLocation>> handle =
                Addressables.LoadResourceLocationsAsync(ReferenceLabel, typeof(ReferenceConfig));

            while (handle.IsDone == false)
            {
                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
            }

            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                throw new InvalidOperationException("Failed to load reference index: " + handle.OperationException);
            }

            List<IResourceLocation> sorted = new List<IResourceLocation>(handle.Result);
            sorted.Sort((a, b) => string.CompareOrdinal(a.PrimaryKey, b.PrimaryKey));
            _locations = sorted;
            _initialized = true;
        }

        public UniTask<ReferenceConfig> GetAtAsync(int index, CancellationToken cancellationToken)
        {
            if (_cache.TryGetValue(_locations[index].PrimaryKey, out ReferenceConfig cached))
            {
                return UniTask.FromResult(cached);
            }

            return LoadHandleAsync(Addressables.LoadAssetAsync<ReferenceConfig>(_locations[index]), cancellationToken);
        }

        public UniTask<ReferenceConfig> GetByIdAsync(string id, CancellationToken cancellationToken)
        {
            if (_cache.TryGetValue(id, out ReferenceConfig cached))
            {
                return UniTask.FromResult(cached);
            }

            return LoadHandleAsync(Addressables.LoadAssetAsync<ReferenceConfig>(id), cancellationToken);
        }

        public bool TryGetById(string id, out ReferenceConfig reference)
        {
            return _cache.TryGetValue(id, out reference);
        }

        private async UniTask<ReferenceConfig> LoadHandleAsync(AsyncOperationHandle<ReferenceConfig> handle, CancellationToken cancellationToken)
        {
            while (handle.IsDone == false)
            {
                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
            }

            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                throw new InvalidOperationException("Failed to load reference: " + handle.OperationException);
            }

            _cache[handle.Result.Id] = handle.Result;
            return handle.Result;
        }
    }
}
