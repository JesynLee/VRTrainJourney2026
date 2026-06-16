using System.Collections;
using System.Collections.Generic;
using System.Text;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR;

namespace VRTrainJourney.Journey
{
    public sealed class SeatedViewDiagnostics : MonoBehaviour
    {
        private const string LogPrefix = "[VRTrainJourney.SeatedView]";

        [SerializeField] private XROrigin xrOrigin;
        [SerializeField] private Transform cameraOffset;
        [SerializeField] private Camera mainCamera;
        [SerializeField, Min(0f)] private float delayedSnapshotSeconds = 2f;

        public void Configure(XROrigin origin)
        {
            xrOrigin = origin;
            cameraOffset = origin != null && origin.CameraFloorOffsetObject != null
                ? origin.CameraFloorOffsetObject.transform
                : null;
            mainCamera = origin != null ? origin.Camera : null;
        }

        private void Reset()
        {
            Configure(GetComponent<XROrigin>());
        }

        private void Awake()
        {
            if (xrOrigin == null)
            {
                xrOrigin = GetComponent<XROrigin>();
            }

            if (xrOrigin != null)
            {
                if (cameraOffset == null && xrOrigin.CameraFloorOffsetObject != null)
                {
                    cameraOffset = xrOrigin.CameraFloorOffsetObject.transform;
                }

                if (mainCamera == null)
                {
                    mainCamera = xrOrigin.Camera;
                }
            }

            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }
        }

        private void Start()
        {
            LogSnapshot("Start");
            StartCoroutine(LogDelayedSnapshot());
        }

        private IEnumerator LogDelayedSnapshot()
        {
            if (delayedSnapshotSeconds > 0f)
            {
                yield return new WaitForSecondsRealtime(delayedSnapshotSeconds);
            }

            LogSnapshot("Delayed");
        }

        private void LogSnapshot(string label)
        {
            var builder = new StringBuilder();
            builder.Append(LogPrefix)
                .Append(' ')
                .Append(label)
                .Append(": ");

            if (xrOrigin == null)
            {
                builder.Append("XROrigin=None");
            }
            else
            {
                builder.Append("RequestedTrackingOriginMode=")
                    .Append(xrOrigin.RequestedTrackingOriginMode)
                    .Append(", CurrentTrackingOriginMode=")
                    .Append(xrOrigin.CurrentTrackingOriginMode)
                    .Append(", CameraYOffset=")
                    .Append(FormatFloat(xrOrigin.CameraYOffset));
            }

            builder.Append(", CameraOffset.localPosition=")
                .Append(FormatVector(cameraOffset != null ? cameraOffset.localPosition : (Vector3?)null))
                .Append(", MainCamera.localPosition=")
                .Append(FormatVector(mainCamera != null ? mainCamera.transform.localPosition : (Vector3?)null))
                .Append(", MainCamera.worldPosition=")
                .Append(FormatVector(mainCamera != null ? mainCamera.transform.position : (Vector3?)null))
                .Append(", XRInputSubsystems=")
                .Append(DescribeInputSubsystems());

            Debug.Log(builder.ToString());
        }

        private static string DescribeInputSubsystems()
        {
            var subsystems = new List<XRInputSubsystem>();
            SubsystemManager.GetSubsystems(subsystems);

            if (subsystems.Count == 0)
            {
                return "None";
            }

            var builder = new StringBuilder();
            for (int index = 0; index < subsystems.Count; index++)
            {
                XRInputSubsystem subsystem = subsystems[index];
                if (index > 0)
                {
                    builder.Append(" | ");
                }

                builder.Append(subsystem.subsystemDescriptor.id)
                    .Append("(running=")
                    .Append(subsystem.running)
                    .Append(", current=")
                    .Append(subsystem.GetTrackingOriginMode())
                    .Append(", supported=")
                    .Append(subsystem.GetSupportedTrackingOriginModes())
                    .Append(')');
            }

            return builder.ToString();
        }

        private static string FormatVector(Vector3? value)
        {
            if (!value.HasValue)
            {
                return "None";
            }

            Vector3 vector = value.Value;
            return $"({FormatFloat(vector.x)}, {FormatFloat(vector.y)}, {FormatFloat(vector.z)})";
        }

        private static string FormatFloat(float value)
        {
            return value.ToString("0.###");
        }
    }
}
