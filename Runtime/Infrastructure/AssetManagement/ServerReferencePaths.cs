using System.IO;
using UnityEngine;

namespace _Project.Infrastructure.AssetManagement
{
    public static class ServerReferencePaths
    {
        private const string FolderName = "references";

        public static string Folder
        {
            get
            {
                string root = Directory.GetParent(UnityEngine.Application.dataPath).FullName;
                return Path.Combine(root, FolderName);
            }
        }
    }
}
