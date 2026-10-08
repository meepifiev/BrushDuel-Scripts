using System.IO;
using UnityEditor;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace _Project.Editor
{
    public static class SteamTools
    {
        [MenuItem("BrushDuel/Steam/Build + Deploy Content (Bucket)", false, 1)]
        public static void DeployContent()
        {
            BuildTarget target = EditorUserBuildSettings.activeBuildTarget;

            if (target != BuildTarget.StandaloneWindows64 && target != BuildTarget.StandaloneOSX)
            {
                Debug.LogError("SteamTools: switch platform to StandaloneWindows64 or StandaloneOSX before building content, active is " + target);
                return;
            }

            AddressableAssetSettings.BuildPlayerContent(out AddressablesPlayerBuildResult result);

            if (string.IsNullOrEmpty(result.Error) == false)
            {
                Debug.LogError("SteamTools: addressables build failed - " + result.Error);
                return;
            }

            Debug.Log("SteamTools: addressables (" + target + ") built, uploading to bucket");
            BuildScript.RunScript("ops/deploy-steam-bucket.sh", "");
        }

        [MenuItem("BrushDuel/Steam/Build + Deploy Server", false, 10)]
        public static void BuildAndDeployServer()
        {
            BuildTarget previousTarget = EditorUserBuildSettings.activeBuildTarget;
            StandaloneBuildSubtarget previousSubtarget = EditorUserBuildSettings.standaloneBuildSubtarget;

            bool built = BuildScript.BuildServer();

            EditorUserBuildSettings.standaloneBuildSubtarget = previousSubtarget;
            EditorUserBuildSettings.SwitchActiveBuildTargetAsync(BuildPipeline.GetBuildTargetGroup(previousTarget), previousTarget);

            if (built == false)
            {
                return;
            }

            Debug.Log("SteamTools: server built, deploying to steam box (platform switches back to " + previousTarget + ")");
            BuildScript.RunScript("ops/deploy-steam-server.sh", "");
        }

        [MenuItem("BrushDuel/Steam/Deploy Server Content (paintings)", false, 11)]
        public static void DeployServerContent()
        {
            BuildScript.ExportReferences();
            BuildScript.RunScript("ops/deploy-steam-content.sh", "");
        }

        [MenuItem("BrushDuel/Steam/Delete Save File", false, 20)]
        public static void DeleteSaveFile()
        {
            string path = Path.Combine(UnityEngine.Application.persistentDataPath, "save.json");

            if (File.Exists(path))
            {
                File.Delete(path);
                Debug.Log("SteamTools: save deleted - " + path);
            }
            else
            {
                Debug.Log("SteamTools: no save file at " + path);
            }
        }
    }
}
