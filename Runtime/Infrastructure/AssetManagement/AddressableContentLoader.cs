using System;
using System.Threading;
using _Project.Application.AssetManagement;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace _Project.Infrastructure.AssetManagement
{
    public class AddressableContentLoader : IContentLoader
    {
        private const string ReferenceLabel = "reference";

        private readonly IReferenceCatalog _catalog;

        private bool _downloaded;

        public AddressableContentLoader(IReferenceCatalog catalog)
        {
            _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
        }

        public async UniTask LoadAsync(IProgress<float> progress, CancellationToken cancellationToken)
        {
            await _catalog.InitAsync(cancellationToken);

            if (_downloaded)
            {
                progress?.Report(1f);
                return;
            }

            AsyncOperationHandle handle = Addressables.DownloadDependenciesAsync(ReferenceLabel);
            LoadingProgress loadingProgress = progress as LoadingProgress;

            while (handle.IsDone == false)
            {
                DownloadStatus status = handle.GetDownloadStatus();

                if (loadingProgress != null)
                {
                    loadingProgress.ReportDownload(status.Percent, status.DownloadedBytes, status.TotalBytes);
                }
                else
                {
                    progress?.Report(status.Percent);
                }

                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
            }

            bool succeeded = handle.Status == AsyncOperationStatus.Succeeded;
            Addressables.Release(handle);

            if (succeeded == false)
            {
                throw new InvalidOperationException("Failed to download reference content");
            }

            for (int i = 0; i < _catalog.Count; i++)
            {
                await _catalog.GetAtAsync(i, cancellationToken);
            }

            _downloaded = true;
            progress?.Report(1f);
        }
    }
}
