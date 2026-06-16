using UnityEngine;
using UnityEngine.XR;

namespace VRTrainJourney.Journey
{
    public sealed class JourneyHandControllerInput : MonoBehaviour
    {
        private const string LogPrefix = "[VRTrainJourney.Controls]";

        [SerializeField] private JourneySequenceController journeySequence;
        [SerializeField, Min(0.1f)] private float restartHoldSeconds = 1f;
        [SerializeField, Range(0f, 1f)] private float hapticAmplitude = 0.25f;
        [SerializeField, Min(0f)] private float hapticDurationSeconds = 0.08f;

        private InputDevice rightHandDevice;
        private bool previousPrimaryButton;
        private bool previousSecondaryButton;
        private bool previousPrimaryAxisClick;
        private bool restartTriggeredDuringCurrentPress;
        private float restartHeldSeconds;

        public void Configure(JourneySequenceController sequence)
        {
            journeySequence = sequence;
        }

        private void Awake()
        {
            journeySequence ??= GetComponent<JourneySequenceController>();
        }

        private void Update()
        {
            if (journeySequence == null)
            {
                return;
            }

            RefreshRightHandDevice();

            bool primaryButton = ReadButton(CommonUsages.primaryButton);
            bool secondaryButton = ReadButton(CommonUsages.secondaryButton);
            bool primaryAxisClick = ReadButton(CommonUsages.primary2DAxisClick);

            if (primaryButton && !previousPrimaryButton)
            {
                HandlePlayPause();
            }

            if (secondaryButton && !previousSecondaryButton)
            {
                HandleNextStation();
            }

            HandleRestartHold(primaryAxisClick);

            previousPrimaryButton = primaryButton;
            previousSecondaryButton = secondaryButton;
            previousPrimaryAxisClick = primaryAxisClick;
        }

        private void RefreshRightHandDevice()
        {
            if (rightHandDevice.isValid)
            {
                return;
            }

            rightHandDevice = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
        }

        private bool ReadButton(InputFeatureUsage<bool> usage)
        {
            return rightHandDevice.isValid &&
                   rightHandDevice.TryGetFeatureValue(usage, out bool isPressed) &&
                   isPressed;
        }

        private void HandlePlayPause()
        {
            switch (journeySequence.State)
            {
                case JourneySequenceController.JourneyPlaybackState.Playing:
                case JourneySequenceController.JourneyPlaybackState.Paused:
                    journeySequence.TogglePause();
                    LogAndPulse("A pressed: toggled pause.");
                    break;
                case JourneySequenceController.JourneyPlaybackState.Preparing:
                case JourneySequenceController.JourneyPlaybackState.Ready:
                case JourneySequenceController.JourneyPlaybackState.Completed:
                    journeySequence.StartJourney();
                    LogAndPulse("A pressed: requested playback start.");
                    break;
                default:
                    Debug.Log($"{LogPrefix} A press ignored in state {journeySequence.State}.");
                    break;
            }
        }

        private void HandleNextStation()
        {
            if (journeySequence.State == JourneySequenceController.JourneyPlaybackState.Error ||
                journeySequence.State == JourneySequenceController.JourneyPlaybackState.Completed ||
                journeySequence.State == JourneySequenceController.JourneyPlaybackState.Transitioning ||
                journeySequence.CurrentStationIndex < 0)
            {
                Debug.Log($"{LogPrefix} B press ignored in state {journeySequence.State}.");
                return;
            }

            journeySequence.SkipToNextStation();
            LogAndPulse("B pressed: skipped to next station.");
        }

        private void HandleRestartHold(bool primaryAxisClick)
        {
            if (!primaryAxisClick)
            {
                if (previousPrimaryAxisClick && !restartTriggeredDuringCurrentPress)
                {
                    Debug.Log($"{LogPrefix} Right stick restart hold cancelled.");
                }

                restartHeldSeconds = 0f;
                restartTriggeredDuringCurrentPress = false;
                return;
            }

            if (restartTriggeredDuringCurrentPress)
            {
                return;
            }

            restartHeldSeconds += Time.unscaledDeltaTime;
            if (restartHeldSeconds < restartHoldSeconds)
            {
                return;
            }

            if (journeySequence.State == JourneySequenceController.JourneyPlaybackState.Error)
            {
                Debug.Log($"{LogPrefix} Restart hold ignored in state {journeySequence.State}.");
                restartTriggeredDuringCurrentPress = true;
                return;
            }

            journeySequence.RestartJourney();
            restartTriggeredDuringCurrentPress = true;
            LogAndPulse("Right stick held: restarted journey from station 1.");
        }

        private void LogAndPulse(string message)
        {
            Debug.Log($"{LogPrefix} {message}");
            TryPulseRightHand();
        }

        private void TryPulseRightHand()
        {
            if (!rightHandDevice.isValid || hapticAmplitude <= 0f || hapticDurationSeconds <= 0f)
            {
                return;
            }

            if (rightHandDevice.TryGetHapticCapabilities(out HapticCapabilities capabilities) &&
                capabilities.supportsImpulse)
            {
                rightHandDevice.SendHapticImpulse(0u, hapticAmplitude, hapticDurationSeconds);
            }
        }
    }
}
