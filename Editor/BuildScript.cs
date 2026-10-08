using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.Build.Reporting;
using Debug = UnityEngine.Debug;

namespace _Project.Editor
{
    public static class BuildScript
    {
        private const string ServerScene = "Assets/_Project/Scenes/DedicatedServer.unity";

        [MenuItem("BrushDuel/Server/Export References", false, 20)]
        public static void ExportReferences()
        {
            int count = ReferenceExporter.ExportTo(ReferencesDir());
            Debug.Log("BrushDuel: exported " + count + " reference(s) -> " + ReferencesDir());
        }

        public static bool BuildServer()
        {
            BuildPlayerOptions options = new BuildPlayerOptions
            {
                scenes = new[] { ServerScene },
                locationPathName = Path.Combine(BuildDir(), "Server.x86_64"),
                target = BuildTarget.StandaloneLinux64,
                subtarget = (int)StandaloneBuildSubtarget.Server,
                options = BuildOptions.None,
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);

            if (report.summary.result != BuildResult.Succeeded)
            {
                Debug.LogError("BrushDuel: server build FAILED (" + report.summary.result + ")");
                Exit(1);
                return false;
            }

            int references = ReferenceExporter.ExportTo(ReferencesDir());
            Debug.Log("BrushDuel: server build OK, references=" + references + " -> " + BuildDir());
            Exit(0);
            return true;
        }

        private static string ProjectRoot()
        {
            return Directory.GetParent(UnityEngine.Application.dataPath).FullName;
        }

        private static string BuildDir()
        {
            return Path.Combine(ProjectRoot(), "Builds", "Server");
        }

        private static string ReferencesDir()
        {
            return Path.Combine(BuildDir(), "references");
        }

        internal static void RunScript(string scriptRelPath, string args)
        {
            string root = ProjectRoot();
            string script = Path.Combine(root, scriptRelPath);

            if (File.Exists(script) == false)
            {
                Debug.LogError("BrushDuel: not found " + script);
                return;
            }

            ProcessStartInfo info = new ProcessStartInfo
            {
                FileName = "/bin/bash",
                Arguments = "\"" + script + "\" " + args,
                WorkingDirectory = root,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
            };

            Debug.Log("BrushDuel: run " + scriptRelPath + " " + args);

            using Process process = Process.Start(info);
            string stdout = process.StandardOutput.ReadToEnd();
            string stderr = process.StandardError.ReadToEnd();
            process.WaitForExit();

            if (string.IsNullOrEmpty(stdout) == false)
            {
                Debug.Log(stdout);
            }

            if (process.ExitCode != 0)
            {
                Debug.LogError("BrushDuel: script failed (exit " + process.ExitCode + ")\n" + stderr);
            }
        }

        private static void Exit(int code)
        {
            if (UnityEngine.Application.isBatchMode)
            {
                EditorApplication.Exit(code);
            }
        }
    }
}
