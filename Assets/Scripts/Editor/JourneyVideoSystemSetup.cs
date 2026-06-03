using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using VRTrainJourney.Journey;

namespace VRTrainJourney.Editor
{
    [InitializeOnLoad]
    public static class JourneyVideoSystemSetup
    {
        private const string LogPrefix = "[VRTrainJourney.Video.Setup]";
        private const string ScenePath = "Assets/Scenes/SampleScene.unity";
        private const string RenderTexturePath = "Assets/Art/RenderTextures/RT_FrontVideo_720p.renderTexture";
        private const string VideoMaterialPath = "Assets/Art/Materials/Mat_FrontVideo_Unlit.mat";
        private const string FadeMaterialPath = "Assets/Art/Materials/Mat_FrontVideoFade_Unlit.mat";

        private static readonly string[] StationClipPaths =
        {
            "Assets/Videos/Station01_GoldenVillage.mp4",
            "Assets/Videos/Station02_FjordView.mp4",
            "Assets/Videos/Station03_AuroraFlowerField.mp4"
        };

        static JourneyVideoSystemSetup()
        {
            EditorApplication.delayCall += ConfigureLoadedSampleSceneOnce;
        }

        [MenuItem("Tools/VR Train Journey/Configure Video System")]
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

        private static void ConfigureLoadedSampleSceneOnce()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorApplication.delayCall += ConfigureLoadedSampleSceneOnce;
                return;
            }

            Scene scene = SceneManager.GetSceneByPath(ScenePath);
            if (!scene.IsValid() || !scene.isLoaded)
            {
                Debug.LogWarning($"{LogPrefix} SampleScene is not loaded. Use Tools > VR Train Journey > Configure Video System.");
                return;
            }

            if (!RequiresAutomaticConfiguration())
            {
                return;
            }

            ConfigureScene(scene);
        }

        private static bool RequiresAutomaticConfiguration()
        {
            GameObject frontScreen = GameObject.Find("FrontVideoScreen");
            GameObject journeySystem = GameObject.Find("JourneySystem");
            return frontScreen == null ||
                   GameObject.Find("FrontVideoFadeOverlay") == null ||
                   journeySystem == null ||
                   journeySystem.GetComponent<VideoPlayer>() == null ||
                   journeySystem.GetComponent<FadeTransitionController>() == null ||
                   journeySystem.GetComponent<JourneySequenceController>() == null ||
                   journeySystem.GetComponent<JourneyDebugInput>() == null ||
                   AssetDatabase.LoadAssetAtPath<RenderTexture>(RenderTexturePath) == null ||
                   AssetDatabase.LoadAssetAtPath<Material>(VideoMaterialPath) == null ||
                   AssetDatabase.LoadAssetAtPath<Material>(FadeMaterialPath) == null;
        }

        private static void ConfigureScene(Scene scene)
        {
            EnsureFolder("Assets/Art/RenderTextures");

            RenderTexture renderTexture = GetOrCreateRenderTexture();
            Material videoMaterial = GetOrCreateVideoMaterial(renderTexture);
            Material fadeMaterial = GetOrCreateFadeMaterial();

            GameObject frontScreen = GameObject.Find("FrontVideoScreen") ?? GameObject.Find("FrontVideoScreen_Placeholder");
            if (frontScreen == null)
            {
                throw new InvalidOperationException($"{LogPrefix} FrontVideoScreen_Placeholder was not found.");
            }

            frontScreen.name = "FrontVideoScreen";
            ConfigureScreenRenderer(frontScreen, videoMaterial);
            GameObject overlay = GetOrCreateFadeOverlay(frontScreen, fadeMaterial);

            GameObject journeySystem = GameObject.Find("JourneySystem") ?? new GameObject("JourneySystem");
            VideoPlayer videoPlayer = GetOrAddComponent<VideoPlayer>(journeySystem);
            FadeTransitionController fade = GetOrAddComponent<FadeTransitionController>(journeySystem);
            JourneySequenceController sequence = GetOrAddComponent<JourneySequenceController>(journeySystem);
            JourneyDebugInput debugInput = GetOrAddComponent<JourneyDebugInput>(journeySystem);

            VideoClip[] clips = LoadStationClips();
            ConfigureVideoPlayer(videoPlayer, renderTexture);
            fade.Configure(overlay.GetComponent<Renderer>());
            sequence.Configure(videoPlayer, fade, clips);
            debugInput.Configure(sequence, true);

            EditorUtility.SetDirty(renderTexture);
            EditorUtility.SetDirty(videoMaterial);
            EditorUtility.SetDirty(fadeMaterial);
            EditorUtility.SetDirty(frontScreen);
            EditorUtility.SetDirty(overlay);
            EditorUtility.SetDirty(journeySystem);
            EditorUtility.SetDirty(videoPlayer);
            EditorUtility.SetDirty(fade);
            EditorUtility.SetDirty(sequence);
            EditorUtility.SetDirty(debugInput);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log($"{LogPrefix} Configured local VideoClip playback, RenderTexture output, fade overlay, and JourneySystem.");
        }

        private static RenderTexture GetOrCreateRenderTexture()
        {
            RenderTexture renderTexture = AssetDatabase.LoadAssetAtPath<RenderTexture>(RenderTexturePath);
            if (renderTexture == null)
            {
                renderTexture = new RenderTexture(1280, 720, 0, RenderTextureFormat.ARGB32)
                {
                    name = "RT_FrontVideo_720p"
                };
                AssetDatabase.CreateAsset(renderTexture, RenderTexturePath);
            }

            renderTexture.Release();
            renderTexture.width = 1280;
            renderTexture.height = 720;
            renderTexture.depth = 0;
            renderTexture.antiAliasing = 1;
            renderTexture.useMipMap = false;
            renderTexture.autoGenerateMips = false;
            renderTexture.wrapMode = TextureWrapMode.Clamp;
            renderTexture.filterMode = FilterMode.Point;
            renderTexture.anisoLevel = 0;
            return renderTexture;
        }

        private static Material GetOrCreateVideoMaterial(RenderTexture renderTexture)
        {
            Material material = GetOrCreateMaterial(VideoMaterialPath, "Mat_FrontVideo_Unlit");
            material.SetTexture("_BaseMap", renderTexture);
            material.SetTexture("_MainTex", renderTexture);
            material.SetColor("_BaseColor", Color.white);
            material.SetColor("_Color", Color.white);
            material.renderQueue = (int)RenderQueue.Geometry;
            return material;
        }

        private static Material GetOrCreateFadeMaterial()
        {
            Material material = GetOrCreateMaterial(FadeMaterialPath, "Mat_FrontVideoFade_Unlit");
            material.SetColor("_BaseColor", Color.black);
            material.SetColor("_Color", Color.black);
            material.SetFloat("_Surface", 1f);
            material.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            material.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
            material.SetFloat("_ZWrite", 0f);
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.SetOverrideTag("RenderType", "Transparent");
            material.renderQueue = (int)RenderQueue.Transparent;
            return material;
        }

        private static Material GetOrCreateMaterial(string path, string materialName)
        {
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null)
            {
                return material;
            }

            Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null)
            {
                throw new InvalidOperationException($"{LogPrefix} URP Unlit shader was not found.");
            }

            material = new Material(shader)
            {
                name = materialName
            };
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        private static void ConfigureScreenRenderer(GameObject frontScreen, Material videoMaterial)
        {
            Renderer renderer = frontScreen.GetComponent<Renderer>();
            if (renderer == null)
            {
                throw new InvalidOperationException($"{LogPrefix} FrontVideoScreen has no Renderer.");
            }

            renderer.sharedMaterial = videoMaterial;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
        }

        private static GameObject GetOrCreateFadeOverlay(GameObject frontScreen, Material fadeMaterial)
        {
            GameObject overlay = GameObject.Find("FrontVideoFadeOverlay");
            if (overlay == null)
            {
                overlay = GameObject.CreatePrimitive(PrimitiveType.Cube);
                overlay.name = "FrontVideoFadeOverlay";
                UnityEngine.Object.DestroyImmediate(overlay.GetComponent<Collider>());
            }

            overlay.transform.SetParent(frontScreen.transform.parent, false);
            overlay.transform.localRotation = frontScreen.transform.localRotation;
            overlay.transform.localPosition =
                frontScreen.transform.localPosition +
                frontScreen.transform.localRotation * new Vector3(0f, 0f, -0.031f);
            overlay.transform.localScale = new Vector3(
                frontScreen.transform.localScale.x,
                frontScreen.transform.localScale.y,
                0.01f);

            Renderer renderer = overlay.GetComponent<Renderer>();
            renderer.sharedMaterial = fadeMaterial;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
            return overlay;
        }

        private static void ConfigureVideoPlayer(VideoPlayer videoPlayer, RenderTexture renderTexture)
        {
            videoPlayer.playOnAwake = false;
            videoPlayer.waitForFirstFrame = true;
            videoPlayer.isLooping = false;
            videoPlayer.renderMode = VideoRenderMode.RenderTexture;
            videoPlayer.targetTexture = renderTexture;
            videoPlayer.audioOutputMode = VideoAudioOutputMode.None;
        }

        private static VideoClip[] LoadStationClips()
        {
            var clips = new VideoClip[StationClipPaths.Length];
            for (int index = 0; index < StationClipPaths.Length; index++)
            {
                clips[index] = AssetDatabase.LoadAssetAtPath<VideoClip>(StationClipPaths[index]);
                if (clips[index] == null)
                {
                    throw new InvalidOperationException($"{LogPrefix} Missing VideoClip: {StationClipPaths[index]}");
                }
            }

            return clips;
        }

        private static T GetOrAddComponent<T>(GameObject gameObject) where T : Component
        {
            T component = gameObject.GetComponent<T>();
            return component != null ? component : gameObject.AddComponent<T>();
        }

        private static void EnsureFolder(string path)
        {
            string[] parts = path.Split('/');
            string current = parts[0];
            for (int index = 1; index < parts.Length; index++)
            {
                string next = $"{current}/{parts[index]}";
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[index]);
                }

                current = next;
            }
        }
    }
}
