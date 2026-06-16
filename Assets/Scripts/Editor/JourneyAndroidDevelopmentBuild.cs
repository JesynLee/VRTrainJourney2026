using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.Video;

namespace VRTrainJourney.Editor
{
    public static class JourneyAndroidDevelopmentBuild
    {
        private const string LogPrefix = "[VRTrainJourney.Video.Build]";
        private const string ScenePath = "Assets/Scenes/SampleScene.unity";
        private const string RenderTexturePath = "Assets/Art/RenderTextures/RT_FrontVideo_720p.renderTexture";
        private const string OutputPath = "Builds/Android/VRTrainJourney2026_Debug.apk";

        private static readonly string[] StationClipPaths =
        {
            "Assets/Videos/Station01_GoldenVillage.mp4",
            "Assets/Videos/Station02_FjordView.mp4",
            "Assets/Videos/Station03_AuroraFlowerField.mp4"
        };

        [MenuItem("Tools/VR Train Journey/Build Quest 2 Development APK")]
        public static void BuildFromMenu()
        {
            BuildDevelopmentApk();
        }

        public static void BuildFromCommandLine()
        {
            BuildDevelopmentApk();
        }

        private static void BuildDevelopmentApk()
        {
            AssetDatabase.Refresh();
            ValidateRequiredAssets();
            JourneyVideoSystemSetup.ConfigureSceneForBuild();
            JourneySeatedViewSetup.ConfigureSceneForBuild();

            if (!EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android))
            {
                throw new InvalidOperationException($"{LogPrefix} Unable to switch the active build target to Android.");
            }

            EditorUserBuildSettings.buildAppBundle = false;
            Directory.CreateDirectory(Path.GetDirectoryName(OutputPath) ?? "Builds");

            string[] scenes = EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(scene => scene.path)
                .ToArray();

            if (scenes.Length == 0)
            {
                throw new InvalidOperationException($"{LogPrefix} No enabled scenes are configured in Build Settings.");
            }

            var options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = OutputPath,
                target = BuildTarget.Android,
                targetGroup = BuildTargetGroup.Android,
                options = BuildOptions.Development | BuildOptions.AllowDebugging
            };

            Debug.Log($"{LogPrefix} Building Android Development APK at {OutputPath}.");
            BuildReport report = BuildPipeline.BuildPlayer(options);
            BuildSummary summary = report.summary;

            if (summary.result != BuildResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"{LogPrefix} Build failed with result {summary.result}. Errors: {summary.totalErrors}, warnings: {summary.totalWarnings}.");
            }

            long apkSizeBytes = new FileInfo(OutputPath).Length;
            Debug.Log(
                $"{LogPrefix} Build succeeded: {OutputPath}, {apkSizeBytes} bytes, {summary.totalTime.TotalSeconds:0.##} seconds.");
        }

        private static void ValidateRequiredAssets()
        {
            RequireAsset<SceneAsset>(ScenePath);
            RequireAsset<RenderTexture>(RenderTexturePath);

            foreach (string stationClipPath in StationClipPaths)
            {
                RequireAsset<VideoClip>(stationClipPath);
            }
        }

        private static void RequireAsset<T>(string path) where T : UnityEngine.Object
        {
            if (AssetDatabase.LoadAssetAtPath<T>(path) == null)
            {
                throw new InvalidOperationException($"{LogPrefix} Required asset is missing: {path}");
            }
        }
    }
}
