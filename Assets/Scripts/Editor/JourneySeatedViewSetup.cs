using System;
using Unity.XR.CoreUtils;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using VRTrainJourney.Journey;

namespace VRTrainJourney.Editor
{
    public static class JourneySeatedViewSetup
    {
        private const string LogPrefix = "[VRTrainJourney.SeatedView.Setup]";
        private const string ScenePath = "Assets/Scenes/SampleScene.unity";
        private const string XrOriginName = "XR Origin (VR)";
        private const float FixedSeatedCameraYOffset = 0f;

        [MenuItem("Tools/VR Train Journey/Configure Seated View Diagnostics")]
        public static void ConfigureFromMenu()
        {
            Scene scene = SceneManager.GetSceneByPath(ScenePath);
            if (!scene.IsValid() || !scene.isLoaded)
            {
                if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                {
                    return;
                }

                scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            }

            ConfigureScene(scene);
        }

        public static void ConfigureSceneForBuild()
        {
            Scene scene = SceneManager.GetSceneByPath(ScenePath);
            if (!scene.IsValid() || !scene.isLoaded)
            {
                scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            }

            ConfigureScene(scene);
        }

        private static void ConfigureScene(Scene scene)
        {
            GameObject xrOriginObject = GameObject.Find(XrOriginName);
            if (xrOriginObject == null)
            {
                throw new InvalidOperationException($"{LogPrefix} {XrOriginName} was not found.");
            }

            XROrigin xrOrigin = xrOriginObject.GetComponent<XROrigin>();
            if (xrOrigin == null)
            {
                throw new InvalidOperationException($"{LogPrefix} {XrOriginName} has no XROrigin component.");
            }

            xrOrigin.CameraYOffset = FixedSeatedCameraYOffset;

            SeatedViewDiagnostics diagnostics = xrOriginObject.GetComponent<SeatedViewDiagnostics>();
            if (diagnostics == null)
            {
                diagnostics = xrOriginObject.AddComponent<SeatedViewDiagnostics>();
            }

            diagnostics.Configure(xrOrigin);

            EditorUtility.SetDirty(xrOriginObject);
            EditorUtility.SetDirty(xrOrigin);
            EditorUtility.SetDirty(diagnostics);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log($"{LogPrefix} Configured fixed seated view diagnostics on {XrOriginName} with CameraYOffset={FixedSeatedCameraYOffset:0.###}.");
        }
    }
}
