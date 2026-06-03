using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using VRTrainJourney.Journey;

namespace VRTrainJourney.Editor
{
    public static class JourneyAudioSystemSetup
    {
        private const string LogPrefix = "[VRTrainJourney.Audio.Setup]";
        private const string ScenePath = "Assets/Scenes/SampleScene.unity";

        [MenuItem("Tools/VR Train Journey/Configure Audio System")]
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

        public static void ConfigureScene(Scene scene)
        {
            GameObject journeySystem = GameObject.Find("JourneySystem");
            if (journeySystem == null)
            {
                throw new InvalidOperationException($"{LogPrefix} JourneySystem was not found. Configure the video system first.");
            }

            JourneySequenceController sequence = journeySystem.GetComponent<JourneySequenceController>();
            if (sequence == null)
            {
                throw new InvalidOperationException($"{LogPrefix} JourneySequenceController was not found on JourneySystem.");
            }

            JourneyAudioController audioController = GetOrAddComponent<JourneyAudioController>(journeySystem);
            AudioSource[] sources = journeySystem.GetComponents<AudioSource>();
            while (sources.Length < 3)
            {
                journeySystem.AddComponent<AudioSource>();
                sources = journeySystem.GetComponents<AudioSource>();
            }

            ConfigureAudioSource(sources[0], "BGM");
            ConfigureAudioSource(sources[1], "Ambience");
            ConfigureAudioSource(sources[2], "Voice");

            audioController.ResetToDefaultProfiles();
            audioController.Configure(sequence, sources[0], sources[1], sources[2], audioController.StationProfiles);

            EditorUtility.SetDirty(journeySystem);
            EditorUtility.SetDirty(audioController);
            EditorUtility.SetDirty(sources[0]);
            EditorUtility.SetDirty(sources[1]);
            EditorUtility.SetDirty(sources[2]);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log($"{LogPrefix} Configured JourneyAudioController with BGM, ambience, and Korean voice AudioSources. Assign AudioClips in the station profiles before final testing.");
        }

        private static void ConfigureAudioSource(AudioSource source, string role)
        {
            source.playOnAwake = false;
            source.loop = false;
            source.volume = 0f;
            source.spatialBlend = 0f;
            source.dopplerLevel = 0f;
            source.priority = role == "Voice" ? 64 : 128;
        }

        private static T GetOrAddComponent<T>(GameObject gameObject) where T : Component
        {
            T component = gameObject.GetComponent<T>();
            return component != null ? component : gameObject.AddComponent<T>();
        }
    }
}
