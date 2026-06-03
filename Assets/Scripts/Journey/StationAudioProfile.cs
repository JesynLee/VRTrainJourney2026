using System;
using UnityEngine;

namespace VRTrainJourney.Journey
{
    [Serializable]
    public sealed class StationAudioProfile
    {
        [SerializeField] private string stationName = "Station";
        [SerializeField] private AudioClip bgmClip;
        [SerializeField] private AudioClip ambienceClip;
        [SerializeField] private AudioClip voiceClip;
        [SerializeField, Range(0f, 1f)] private float bgmVolume = 0.24f;
        [SerializeField, Range(0f, 1f)] private float ambienceVolume = 0.18f;
        [SerializeField, Range(0f, 1f)] private float voiceVolume = 0.95f;
        [SerializeField, Min(0f)] private float fadeInSeconds = 2f;
        [SerializeField, Min(0f)] private float fadeOutSeconds = 2f;
        [SerializeField, Min(0f)] private float voiceDelaySeconds = 1.25f;
        [SerializeField] private bool loopBgm = true;
        [SerializeField] private bool loopAmbience = true;

        public string StationName => stationName;
        public AudioClip BgmClip => bgmClip;
        public AudioClip AmbienceClip => ambienceClip;
        public AudioClip VoiceClip => voiceClip;
        public float BgmVolume => bgmVolume;
        public float AmbienceVolume => ambienceVolume;
        public float VoiceVolume => voiceVolume;
        public float FadeInSeconds => fadeInSeconds;
        public float FadeOutSeconds => fadeOutSeconds;
        public float VoiceDelaySeconds => voiceDelaySeconds;
        public bool LoopBgm => loopBgm;
        public bool LoopAmbience => loopAmbience;

        public StationAudioProfile()
        {
        }

        public StationAudioProfile(
            string stationName,
            float bgmVolume,
            float ambienceVolume,
            float voiceVolume,
            float fadeInSeconds,
            float fadeOutSeconds,
            float voiceDelaySeconds)
        {
            this.stationName = stationName;
            this.bgmVolume = Mathf.Clamp01(bgmVolume);
            this.ambienceVolume = Mathf.Clamp01(ambienceVolume);
            this.voiceVolume = Mathf.Clamp01(voiceVolume);
            this.fadeInSeconds = Mathf.Max(0f, fadeInSeconds);
            this.fadeOutSeconds = Mathf.Max(0f, fadeOutSeconds);
            this.voiceDelaySeconds = Mathf.Max(0f, voiceDelaySeconds);
        }
    }
}
