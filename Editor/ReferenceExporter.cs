using System.IO;
using System.IO.Compression;
using System.Text;
using _Project.Application.Configs;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace _Project.Editor
{
    public class ReferenceExporter
    {
        private const string SearchPattern = "*.json.gz";

        public static int ExportTo(string folder)
        {
            Directory.CreateDirectory(folder);

            foreach (string existing in Directory.GetFiles(folder, SearchPattern))
            {
                File.Delete(existing);
            }

            string[] guids = AssetDatabase.FindAssets("t:ReferenceConfig");
            int exported = 0;

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                ReferenceConfig config = AssetDatabase.LoadAssetAtPath<ReferenceConfig>(path);

                if (config.BakedData == null)
                {
                    Debug.LogWarning(nameof(ReferenceExporter) + ": skip '" + config.name + "' - not baked (run Bake Reference Data first)");
                    continue;
                }

                ReferenceData data = config.ToData();
                string json = JsonConvert.SerializeObject(data);
                WriteCompressed(Path.Combine(folder, data.Id + ".json.gz"), json);
                exported++;
            }

            Debug.Log(nameof(ReferenceExporter) + ": exported " + exported + " reference(s) to " + folder);
            return exported;
        }

        private static void WriteCompressed(string path, string json)
        {
            byte[] raw = Encoding.UTF8.GetBytes(json);

            using FileStream fileStream = File.Create(path);
            using GZipStream gzipStream = new GZipStream(fileStream, System.IO.Compression.CompressionLevel.Optimal);
            gzipStream.Write(raw, 0, raw.Length);
        }
    }
}
