using System;

namespace _Project.Application.AssetManagement
{
    public class LoadingProgress : IProgress<float>
    {
        public float Value { get; private set; }
        public long DownloadedBytes { get; private set; }
        public long TotalBytes { get; private set; }

        public event Action<float> Changed;

        public void Report(float value)
        {
            Value = value;
            Changed?.Invoke(value);
        }

        public void ReportDownload(float value, long downloadedBytes, long totalBytes)
        {
            Value = value;
            DownloadedBytes = downloadedBytes;
            TotalBytes = totalBytes;
            Changed?.Invoke(value);
        }
    }
}
