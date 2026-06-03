using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Video;

namespace VRTrainJourney.Journey
{
    [RequireComponent(typeof(VideoPlayer))]
    public sealed class JourneySequenceController : MonoBehaviour
    {
        public enum JourneyPlaybackState
        {
            Idle,
            Preparing,
            Ready,
            Playing,
            Paused,
            Transitioning,
            Completed,
            Error
        }

        private const string LogPrefix = "[VRTrainJourney.Video]";

        [SerializeField] private VideoPlayer videoPlayer;
        [SerializeField] private FadeTransitionController fadeTransition;
        [SerializeField] private VideoClip[] stationClips = Array.Empty<VideoClip>();
        [SerializeField, Min(0f)] private float fadeOutDuration = 0.75f;
        [SerializeField, Min(0f)] private float minimumBlackDuration = 0.25f;
        [SerializeField, Min(0f)] private float fadeInDuration = 0.75f;
        [SerializeField, Min(0.1f)] private float prepareTimeoutSeconds = 10f;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        [SerializeField] private bool logPlaybackDiagnostics = true;
        [SerializeField, Min(0.25f)] private float playbackDiagnosticsInterval = 1f;
#endif

        [SerializeField] private UnityEvent<int> stationStarted = new UnityEvent<int>();
        [SerializeField] private UnityEvent journeyCompleted = new UnityEvent();
        [SerializeField] private UnityEvent<string> playbackError = new UnityEvent<string>();

        private Coroutine activeTransition;
        private Coroutine prepareTimeout;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private Coroutine playbackDiagnostics;
#endif
        private bool playWhenPrepared;

        public int CurrentStationIndex { get; private set; } = -1;
        public JourneyPlaybackState State { get; private set; } = JourneyPlaybackState.Idle;
        public UnityEvent<int> StationStarted => stationStarted;
        public UnityEvent JourneyCompleted => journeyCompleted;
        public UnityEvent<string> PlaybackError => playbackError;

        private void Awake()
        {
            videoPlayer ??= GetComponent<VideoPlayer>();

            if (!ValidateConfiguration())
            {
                return;
            }

            videoPlayer.playOnAwake = false;
            videoPlayer.waitForFirstFrame = true;
            videoPlayer.isLooping = false;
            videoPlayer.audioOutputMode = VideoAudioOutputMode.None;
            if (videoPlayer.canSetSkipOnDrop)
            {
                videoPlayer.skipOnDrop = true;
            }

            if (videoPlayer.targetTexture != null)
            {
                videoPlayer.targetTexture.anisoLevel = 0;
            }

            videoPlayer.prepareCompleted += HandlePrepareCompleted;
            videoPlayer.loopPointReached += HandleLoopPointReached;
            videoPlayer.errorReceived += HandleErrorReceived;

            fadeTransition.SetImmediate(1f);
            PrepareStation(0, false);
        }

        private void OnDestroy()
        {
            if (videoPlayer == null)
            {
                return;
            }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            StopPlaybackDiagnostics();
#endif
            videoPlayer.prepareCompleted -= HandlePrepareCompleted;
            videoPlayer.loopPointReached -= HandleLoopPointReached;
            videoPlayer.errorReceived -= HandleErrorReceived;
        }

        public void Configure(
            VideoPlayer player,
            FadeTransitionController transition,
            VideoClip[] clips)
        {
            videoPlayer = player;
            fadeTransition = transition;
            stationClips = clips;
        }

        public void StartJourney()
        {
            if (State == JourneyPlaybackState.Error)
            {
                Debug.LogWarning($"{LogPrefix} Start ignored because the player is in an error state.");
                return;
            }

            if (State == JourneyPlaybackState.Completed)
            {
                PrepareStation(0, true);
                return;
            }

            if (State == JourneyPlaybackState.Preparing)
            {
                playWhenPrepared = true;
                Debug.Log($"{LogPrefix} Start requested while station {CurrentStationIndex + 1} is preparing.");
                return;
            }

            if (State == JourneyPlaybackState.Ready)
            {
                BeginPreparedPlayback();
            }
        }

        public void TogglePause()
        {
            if (State == JourneyPlaybackState.Playing)
            {
                videoPlayer.Pause();
                State = JourneyPlaybackState.Paused;
                Debug.Log($"{LogPrefix} Paused station {CurrentStationIndex + 1}.");
            }
            else if (State == JourneyPlaybackState.Paused)
            {
                videoPlayer.Play();
                State = JourneyPlaybackState.Playing;
                Debug.Log($"{LogPrefix} Resumed station {CurrentStationIndex + 1}.");
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                StartPlaybackDiagnostics();
#endif
            }
        }

        public void SkipToNextStation()
        {
            if (State == JourneyPlaybackState.Error ||
                State == JourneyPlaybackState.Completed ||
                State == JourneyPlaybackState.Transitioning ||
                CurrentStationIndex < 0)
            {
                return;
            }

            StartTransitionToNextStation();
        }

        private bool ValidateConfiguration()
        {
            if (videoPlayer == null)
            {
                EnterError("VideoPlayer reference is missing.");
                return false;
            }

            if (fadeTransition == null)
            {
                EnterError("FadeTransitionController reference is missing.");
                return false;
            }

            if (stationClips == null || stationClips.Length == 0)
            {
                EnterError("No station VideoClips are configured.");
                return false;
            }

            for (int index = 0; index < stationClips.Length; index++)
            {
                if (stationClips[index] == null)
                {
                    EnterError($"Station VideoClip {index + 1} is missing.");
                    return false;
                }
            }

            return true;
        }

        private void PrepareStation(int stationIndex, bool shouldPlayWhenPrepared)
        {
            if (stationIndex < 0 || stationIndex >= stationClips.Length)
            {
                EnterError($"Station index {stationIndex} is outside the configured clip range.");
                return;
            }

            StopPrepareTimeout();
            videoPlayer.Stop();
            CurrentStationIndex = stationIndex;
            playWhenPrepared = shouldPlayWhenPrepared;
            State = JourneyPlaybackState.Preparing;
            videoPlayer.clip = stationClips[stationIndex];
            videoPlayer.Prepare();
            prepareTimeout = StartCoroutine(WaitForPrepareTimeout(stationIndex));
            Debug.Log($"{LogPrefix} Preparing station {stationIndex + 1}: {stationClips[stationIndex].name}");
        }

        private void HandlePrepareCompleted(VideoPlayer source)
        {
            if (State != JourneyPlaybackState.Preparing ||
                CurrentStationIndex < 0 ||
                CurrentStationIndex >= stationClips.Length ||
                source.clip != stationClips[CurrentStationIndex])
            {
                Debug.LogWarning($"{LogPrefix} Ignored a stale prepare callback.");
                return;
            }

            StopPrepareTimeout();
            State = JourneyPlaybackState.Ready;
            Debug.Log($"{LogPrefix} Prepared station {CurrentStationIndex + 1}: {source.clip.name}");

            if (playWhenPrepared)
            {
                BeginPreparedPlayback();
            }
        }

        private void BeginPreparedPlayback()
        {
            if (State != JourneyPlaybackState.Ready)
            {
                return;
            }

            playWhenPrepared = false;
            videoPlayer.Play();
            State = JourneyPlaybackState.Playing;
            stationStarted.Invoke(CurrentStationIndex);
            Debug.Log($"{LogPrefix} Started station {CurrentStationIndex + 1}: {videoPlayer.clip.name}");
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            StartPlaybackDiagnostics();
#endif

            if (activeTransition != null)
            {
                StopCoroutine(activeTransition);
            }

            activeTransition = StartCoroutine(FadeInAfterPlaybackStarts());
        }

        private IEnumerator FadeInAfterPlaybackStarts()
        {
            float deadline = Time.realtimeSinceStartup + prepareTimeoutSeconds;
            while (videoPlayer.frame < 0 && Time.realtimeSinceStartup < deadline)
            {
                yield return null;
            }

            if (videoPlayer.frame < 0)
            {
                EnterError($"Station {CurrentStationIndex + 1} did not produce a first frame within {prepareTimeoutSeconds:0.##} seconds.");
                yield break;
            }

            yield return fadeTransition.FadeTo(0f, fadeInDuration);
            activeTransition = null;
        }

        private void HandleLoopPointReached(VideoPlayer source)
        {
            if (State == JourneyPlaybackState.Playing || State == JourneyPlaybackState.Paused)
            {
                StartTransitionToNextStation();
            }
        }

        private void StartTransitionToNextStation()
        {
            StopPrepareTimeout();
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            StopPlaybackDiagnostics();
#endif
            playWhenPrepared = false;
            if (activeTransition != null)
            {
                StopCoroutine(activeTransition);
            }

            activeTransition = StartCoroutine(TransitionToNextStation());
        }

        private IEnumerator TransitionToNextStation()
        {
            State = JourneyPlaybackState.Transitioning;
            yield return fadeTransition.FadeTo(1f, fadeOutDuration);
            yield return new WaitForSecondsRealtime(minimumBlackDuration);

            videoPlayer.Stop();
            int nextStationIndex = CurrentStationIndex + 1;
            if (nextStationIndex >= stationClips.Length)
            {
                State = JourneyPlaybackState.Completed;
                activeTransition = null;
                journeyCompleted.Invoke();
                Debug.Log($"{LogPrefix} Journey completed.");
                yield break;
            }

            activeTransition = null;
            PrepareStation(nextStationIndex, true);
        }

        private IEnumerator WaitForPrepareTimeout(int stationIndex)
        {
            yield return new WaitForSecondsRealtime(prepareTimeoutSeconds);

            if (State == JourneyPlaybackState.Preparing && stationIndex == CurrentStationIndex)
            {
                EnterError($"Preparing station {stationIndex + 1} exceeded {prepareTimeoutSeconds:0.##} seconds.");
            }
        }

        private void HandleErrorReceived(VideoPlayer source, string message)
        {
            EnterError($"Station {CurrentStationIndex + 1} failed: {message}");
        }

        private void EnterError(string message)
        {
            StopPrepareTimeout();
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            StopPlaybackDiagnostics();
#endif
            if (activeTransition != null)
            {
                StopCoroutine(activeTransition);
                activeTransition = null;
            }

            State = JourneyPlaybackState.Error;
            playWhenPrepared = false;
            videoPlayer?.Stop();
            fadeTransition?.SetImmediate(1f);
            Debug.LogError($"{LogPrefix} {message}");
            playbackError.Invoke(message);
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private void StartPlaybackDiagnostics()
        {
            if (!logPlaybackDiagnostics || playbackDiagnostics != null)
            {
                return;
            }

            playbackDiagnostics = StartCoroutine(LogPlaybackDiagnostics());
        }

        private void StopPlaybackDiagnostics()
        {
            if (playbackDiagnostics == null)
            {
                return;
            }

            StopCoroutine(playbackDiagnostics);
            playbackDiagnostics = null;
        }

        private IEnumerator LogPlaybackDiagnostics()
        {
            while (State == JourneyPlaybackState.Playing || State == JourneyPlaybackState.Paused)
            {
                Debug.Log(
                    $"{LogPrefix} Diagnostics station={CurrentStationIndex + 1}, " +
                    $"state={State}, isPlaying={videoPlayer.isPlaying}, " +
                    $"time={videoPlayer.time:0.00}, frame={videoPlayer.frame}, " +
                    $"targetTexture={(videoPlayer.targetTexture != null ? videoPlayer.targetTexture.name : "None")}");

                yield return new WaitForSecondsRealtime(playbackDiagnosticsInterval);
            }

            playbackDiagnostics = null;
        }
#endif

        private void StopPrepareTimeout()
        {
            if (prepareTimeout == null)
            {
                return;
            }

            StopCoroutine(prepareTimeout);
            prepareTimeout = null;
        }
    }
}
