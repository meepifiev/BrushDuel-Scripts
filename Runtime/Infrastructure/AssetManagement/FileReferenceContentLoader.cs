using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading;
using _Project.Application.AssetManagement;
using _Project.Application.Configs;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;

namespace _Project.Infrastructure.AssetManagement
{
    public class FileReferenceContentLoader : IContentLoader
    {
        private const string SearchPattern = "*.json.gz";

        private readonly RuntimeReferenceCatalog _catalog;

        public FileReferenceContentLoader(RuntimeReferenceCatalog catalog)
        {
            _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
        }

        public UniTask LoadAsync(IProgress<float> progress, CancellationToken cancellationToken)
        {
            string folder = ServerReferencePaths.Folder;

            if (Directory.Exists(folder) == false)
            {
                throw new InvalidOperationException("Reference folder not found: " + folder);
            }

            string[] files = Directory.GetFiles(folder, SearchPattern);
            List<ReferenceConfig> references = new List<ReferenceConfig>(files.Length);

            for (int i = 0; i < files.Length; i++)
            {
                string json = ReadCompressed(files[i]);
                ReferenceData data = JsonConvert.DeserializeObject<ReferenceData>(json);

                if (data != null)
                {
                    references.Add(ReferenceConfig.FromData(data));
                }

                progress?.Report(files.Length == 0 ? 1f : (float)(i + 1) / files.Length);
            }

            _catalog.Set(references);
            progress?.Report(1f);
            return UniTask.CompletedTask;
        }

        private static string ReadCompressed(string path)
        {
            using FileStream fileStream = File.OpenRead(path);
            using GZipStream gzipStream = new GZipStream(fileStream, CompressionMode.Decompress);
            using MemoryStream memoryStream = new MemoryStream();
            gzipStream.CopyTo(memoryStream);
            return Encoding.UTF8.GetString(memoryStream.ToArray());
        }
    }
}
