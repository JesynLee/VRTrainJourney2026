using UnityEngine;
using UnityEngine.InputSystem;

namespace VRTrainJourney.Journey
{
    public sealed class JourneyDebugInput : MonoBehaviour
    {
        [SerializeField] private JourneySequenceController journeySequence;
        [SerializeField] private bool autoStartInDevelopmentBuild = true;

        public void Configure(JourneySequenceController sequence, bool shouldAutoStartInDevelopmentBuild)
        {
            journeySequence = sequence;
            autoStartInDevelopmentBuild = shouldAutoStartInDevelopmentBuild;
        }

        private void Start()
        {
#if DEVELOPMENT_BUILD && !UNITY_EDITOR
            TryAutoStartDevelopmentBuild();
#endif
        }

        private void TryAutoStartDevelopmentBuild()
        {
            if (autoStartInDevelopmentBuild)
            {
                journeySequence?.StartJourney();
            }
        }

        private void Update()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (journeySequence == null)
            {
                return;
            }

            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return;
            }

            if (keyboard.spaceKey.wasPressedThisFrame)
            {
                journeySequence.StartJourney();
            }

            if (keyboard.pKey.wasPressedThisFrame)
            {
                journeySequence.TogglePause();
            }

            if (keyboard.nKey.wasPressedThisFrame)
            {
                journeySequence.SkipToNextStation();
            }
#endif
        }
    }
}
