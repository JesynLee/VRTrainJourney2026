using System;
using System.Collections;
using UnityEngine;

namespace VRTrainJourney.Journey
{
    public sealed class JourneyAudioController : MonoBehaviour
    {
        private const string LogPrefix = "[VRTrainJourney.Audio]";

        [SerializeField] private JourneySequenceController journeySequence;
        [SerializeField] private AudioSource bgmSource;
        [SerializeField] private AudioSource ambienceSource;
        [SerializeField] private AudioSource voiceSource;
        [SerializeField] private StationAudioProfile[] stationProfiles = Array.Empty<StationAudioProfile>();
        [SerializeField, Range(0f, 1f)] private float voiceDuckingMultiplier = 0.5f;
        [SerializeField, Min(0f)] private float voiceDuckingSpeed = 2.5f;
        [SerializeField, Min(0f)] private float journeyCompleteFadeSeconds = 4f;

        private Coroutine stationRoutine;
        private Coroutine transitionFadeRoutine;
        private bool subscribed;
        private bool audioPausedByJourney;
        private bool transitionFadeStarted;
        private JourneySequenceController.JourneyPlaybackState previousState;
        private float desiredBgmVolume;
        private float desiredAmbienceVolume;

        public StationAudioProfile[] StationProfiles => stationProfiles;

        private void Awake()
        {
            journeySequence ??= GetComponent<JourneySequenceController>();
            EnsureAudioSources();
            ConfigureAudioSourceDefaults();
        }

        private void OnEnable()
        {
            Subscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
            StopRunningRoutines();
        }

        private void Update()
        {
            if (journeySequence == null)
            {
                return;
            }

            SyncPauseState(journeySequence.State);
            SyncTransitionFade(journeySequence.State);
            ApplyVoiceDucking();
            previousState = journeySequence.State;
        }

        public void Configure(
            JourneySequenceController sequence,
            AudioSource bgm,
            AudioSource ambience,
            AudioSource voice,
            StationAudioProfile[] profiles)
        {
            Unsubscribe();
            journeySequence = sequence;
            bgmSource = bgm;
            ambienceSource = ambience;
            voiceSource = voice;
            stationProfiles = profiles ?? Array.Empty<StationAudioProfile>();
            EnsureAudioSources();
            ConfigureAudioSourceDefaults();
            Subscribe();
        }

        public void ResetToDefaultProfiles()
        {
            stationProfiles = new[]
            {
                new StationAudioProfile("Station01_GoldenVillage", 0.24f, 0.18f, 0.95f, 2f, 2f, 1.2f),
                new StationAudioProfile("Station02_FjordView", 0.22f, 0.18f, 0.95f, 2.25f, 2.25f, 1.2f),
                new StationAudioProfile("Station03_AuroraFlowerField", 0.2f, 0.15f, 0.95f, 2.5f, 3f, 1.4f)
            };
        }

        private void Subscribe()
        {
            if (subscribed || journeySequence == null)
            {
                return;
            }

            journeySequence.StationStarted.AddListener(HandleStationStarted);
            journeySequence.JourneyCompleted.AddListener(HandleJourneyCompleted);
            journeySequence.PlaybackError.AddListener(HandlePlaybackError);
            subscribed = true;
        }

        private void Unsubscribe()
        {
            if (!subscribed || journeySequence == null)
            {
                subscribed = false;
                return;
            }

            journeySequence.StationStarted.RemoveListener(HandleStationStarted);
            journeySequence.JourneyCompleted.RemoveListener(HandleJourneyCompleted);
            journeySequence.PlaybackError.RemoveListener(HandlePlaybackError);
            subscribed = false;
        }

        private void HandleStationStarted(int stationIndex)
        {
            transitionFadeStarted = false;

            if (stationIndex < 0 || stationIndex >= stationProfiles.Length)
            {
                Debug.LogWarning($"{LogPrefix} No StationAudioProfile is configured for station index {stationIndex}.");
                return;
            }

            StationAudioProfile profile = stationProfiles[stationIndex];
            if (profile == null)
            {
                Debug.LogWarning($"{LogPrefix} StationAudioProfile {stationIndex + 1} is empty.");
                return;
            }

            if (stationRoutine != null)
            {
                StopCoroutine(stationRoutine);
            }

            if (transitionFadeRoutine != null)
            {
                StopCoroutine(transitionFadeRoutine);
                transitionFadeRoutine = null;
            }

            stationRoutine = StartCoroutine(PlayStationAudio(profile));
        }

        private IEnumerator PlayStationAudio(StationAudioProfile profile)
        {
            Debug.Log($"{LogPrefix} Starting audio profile: {profile.StationName}");

            float oldFadeSeconds = profile.FadeOutSeconds;
            if (bgmSource.isPlaying || ambienceSource.isPlaying || voiceSource.isPlaying)
            {
                yield return FadeSourcesToZero(oldFadeSeconds, true);
            }

            ConfigureSourceForClip(bgmSource, profile.BgmClip, profile.LoopBgm);
            ConfigureSourceForClip(ambienceSource, profile.AmbienceClip, profile.LoopAmbience);
            ConfigureSourceForClip(voiceSource, profile.VoiceClip, false);

            desiredBgmVolume = profile.BgmClip == null ? 0f : profile.BgmVolume;
            desiredAmbienceVolume = profile.AmbienceClip == null ? 0f : profile.AmbienceVolume;

            if (profile.BgmClip != null)
            {
                bgmSource.volume = 0f;
                bgmSource.Play();
            }
            else
            {
                Debug.LogWarning($"{LogPrefix} BGM clip is not assigned for {profile.StationName}.");
            }

            if (profile.AmbienceClip != null)
            {
                ambienceSource.volume = 0f;
                ambienceSource.Play();
            }
            else
            {
                Debug.LogWarning($"{LogPrefix} Ambience clip is not assigned for {profile.StationName}.");
            }

            yield return FadeMusicAndAmbience(profile.FadeInSeconds, desiredBgmVolume, desiredAmbienceVolume);

            if (profile.VoiceClip != null)
            {
                yield return WaitForJourneySeconds(profile.VoiceDelaySeconds);
                voiceSource.volume = profile.VoiceVolume;
                voiceSource.Play();
            }
            else
            {
                Debug.LogWarning($"{LogPrefix} Korean voice clip is not assigned for {profile.StationName}.");
            }

            stationRoutine = null;
        }

        private void HandleJourneyCompleted()
        {
            Debug.Log($"{LogPrefix} Journey completed. Fading out audio.");
            StopRunningRoutines();
            transitionFadeRoutine = StartCoroutine(FadeSourcesToZero(journeyCompleteFadeSeconds, true));
        }

        private void HandlePlaybackError(string message)
        {
            Debug.LogWarning($"{LogPrefix} Playback error received. Stopping audio. {message}");
            StopRunningRoutines();
            StopAllAudioSources();
        }

        private void SyncPauseState(JourneySequenceController.JourneyPlaybackState state)
        {
            if (state == JourneySequenceController.JourneyPlaybackState.Paused && !audioPausedByJourney)
            {
                PauseAllAudioSources();
                audioPausedByJourney = true;
                Debug.Log($"{LogPrefix} Audio paused with journey.");
            }
            else if (state == JourneySequenceController.JourneyPlaybackState.Playing && audioPausedByJourney)
            {
                ResumeAllAudioSources();
                audioPausedByJourney = false;
                Debug.Log($"{LogPrefix} Audio resumed with journey.");
            }
        }

        private void SyncTransitionFade(JourneySequenceController.JourneyPlaybackState state)
        {
            if (state == JourneySequenceController.JourneyPlaybackState.Transitioning &&
                previousState != JourneySequenceController.JourneyPlaybackState.Transitioning &&
                !transitionFadeStarted)
            {
                transitionFadeStarted = true;
                float fadeSeconds = GetCurrentProfileFadeOutSeconds();
                if (transitionFadeRoutine != null)
                {
                    StopCoroutine(transitionFadeRoutine);
                }

                transitionFadeRoutine = StartCoroutine(FadeMusicAndAmbience(fadeSeconds, 0f, 0f));
            }
        }

        private void ApplyVoiceDucking()
        {
            if (voiceDuckingSpeed <= 0f)
            {
                return;
            }

            bool shouldDuck = voiceSource != null && voiceSource.isPlaying;
            float bgmTarget = shouldDuck ? desiredBgmVolume * voiceDuckingMultiplier : desiredBgmVolume;
            float ambienceTarget = shouldDuck ? desiredAmbienceVolume * voiceDuckingMultiplier : desiredAmbienceVolume;
            float step = voiceDuckingSpeed * Time.unscaledDeltaTime;

            if (bgmSource != null && bgmSource.clip != null)
            {
                bgmSource.volume = Mathf.MoveTowards(bgmSource.volume, bgmTarget, step);
            }

            if (ambienceSource != null && ambienceSource.clip != null)
            {
                ambienceSource.volume = Mathf.MoveTowards(ambienceSource.volume, ambienceTarget, step);
            }
        }

        private float GetCurrentProfileFadeOutSeconds()
        {
            int stationIndex = journeySequence != null ? journeySequence.CurrentStationIndex : -1;
            if (stationIndex >= 0 && stationIndex < stationProfiles.Length && stationProfiles[stationIndex] != null)
            {
                return stationProfiles[stationIndex].FadeOutSeconds;
            }

            return 2f;
        }

        private IEnumerator FadeSourcesToZero(float duration, bool stopAfterFade)
        {
            yield return FadeSourceVolumes(duration, 0f, 0f, 0f);

            if (stopAfterFade)
            {
                StopAllAudioSources();
            }
        }

        private IEnumerator FadeMusicAndAmbience(float duration, float targetBgm, float targetAmbience)
        {
            yield return FadeSourceVolumes(duration, targetBgm, targetAmbience, voiceSource.volume);
        }

        private IEnumerator FadeSourceVolumes(float duration, float targetBgm, float targetAmbience, float targetVoice)
        {
            float startBgm = bgmSource != null ? bgmSource.volume : 0f;
            float startAmbience = ambienceSource != null ? ambienceSource.volume : 0f;
            float startVoice = voiceSource != null ? voiceSource.volume : 0f;

            if (duration <= 0f)
            {
                SetSourceVolume(bgmSource, targetBgm);
                SetSourceVolume(ambienceSource, targetAmbience);
                SetSourceVolume(voiceSource, targetVoice);
                yield break;
            }

            float elapsed = 0f;
            while (elapsed < duration)
            {
                if (IsJourneyPaused())
                {
                    yield return null;
                    continue;
                }

                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                SetSourceVolume(bgmSource, Mathf.Lerp(startBgm, targetBgm, t));
                SetSourceVolume(ambienceSource, Mathf.Lerp(startAmbience, targetAmbience, t));
                SetSourceVolume(voiceSource, Mathf.Lerp(startVoice, targetVoice, t));
                yield return null;
            }

            SetSourceVolume(bgmSource, targetBgm);
            SetSourceVolume(ambienceSource, targetAmbience);
            SetSourceVolume(voiceSource, targetVoice);
        }

        private IEnumerator WaitForJourneySeconds(float seconds)
        {
            float elapsed = 0f;
            while (elapsed < seconds)
            {
                if (!IsJourneyPaused())
                {
                    elapsed += Time.unscaledDeltaTime;
                }

                yield return null;
            }
        }

        private bool IsJourneyPaused()
        {
            return journeySequence != null &&
                   journeySequence.State == JourneySequenceController.JourneyPlaybackState.Paused;
        }

        private void EnsureAudioSources()
        {
            bgmSource ??= gameObject.AddComponent<AudioSource>();
            ambienceSource ??= gameObject.AddComponent<AudioSource>();
            voiceSource ??= gameObject.AddComponent<AudioSource>();
        }

        private void ConfigureAudioSourceDefaults()
        {
            ConfigureSourceDefaults(bgmSource);
            ConfigureSourceDefaults(ambienceSource);
            ConfigureSourceDefaults(voiceSource);
        }

        private static void ConfigureSourceDefaults(AudioSource source)
        {
            if (source == null)
            {
                return;
            }

            source.playOnAwake = false;
            source.spatialBlend = 0f;
            source.dopplerLevel = 0f;
            source.priority = 128;
        }

        private static void ConfigureSourceForClip(AudioSource source, AudioClip clip, bool shouldLoop)
        {
            source.clip = clip;
            source.loop = shouldLoop;
            source.playOnAwake = false;
            source.spatialBlend = 0f;
            source.dopplerLevel = 0f;
        }

        private static void SetSourceVolume(AudioSource source, float volume)
        {
            if (source != null)
            {
                source.volume = Mathf.Clamp01(volume);
            }
        }

        private void StopRunningRoutines()
        {
            if (stationRoutine != null)
            {
                StopCoroutine(stationRoutine);
                stationRoutine = null;
            }

            if (transitionFadeRoutine != null)
            {
                StopCoroutine(transitionFadeRoutine);
                transitionFadeRoutine = null;
            }
        }

        private void PauseAllAudioSources()
        {
            bgmSource?.Pause();
            ambienceSource?.Pause();
            voiceSource?.Pause();
        }

        private void ResumeAllAudioSources()
        {
            bgmSource?.UnPause();
            ambienceSource?.UnPause();
            voiceSource?.UnPause();
        }

        private void StopAllAudioSources()
        {
            desiredBgmVolume = 0f;
            desiredAmbienceVolume = 0f;
            StopSource(bgmSource);
            StopSource(ambienceSource);
            StopSource(voiceSource);
        }

        private static void StopSource(AudioSource source)
        {
            if (source == null)
            {
                return;
            }

            source.Stop();
            source.clip = null;
            source.volume = 0f;
        }
    }
}
