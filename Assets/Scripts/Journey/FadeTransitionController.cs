using System.Collections;
using UnityEngine;

namespace VRTrainJourney.Journey
{
    public sealed class FadeTransitionController : MonoBehaviour
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorId = Shader.PropertyToID("_Color");

        [SerializeField] private Renderer overlayRenderer;

        private MaterialPropertyBlock propertyBlock;
        private float currentAlpha = 1f;

        public float CurrentAlpha => currentAlpha;

        private void Awake()
        {
            propertyBlock = new MaterialPropertyBlock();
            SetImmediate(1f);
        }

        public void Configure(Renderer renderer)
        {
            overlayRenderer = renderer;
        }

        public void SetImmediate(float alpha)
        {
            currentAlpha = Mathf.Clamp01(alpha);
            ApplyAlpha();
        }

        public IEnumerator FadeTo(float targetAlpha, float duration)
        {
            float startAlpha = currentAlpha;
            float clampedTarget = Mathf.Clamp01(targetAlpha);

            if (duration <= 0f)
            {
                SetImmediate(clampedTarget);
                yield break;
            }

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                SetImmediate(Mathf.Lerp(startAlpha, clampedTarget, elapsed / duration));
                yield return null;
            }

            SetImmediate(clampedTarget);
        }

        private void ApplyAlpha()
        {
            if (overlayRenderer == null)
            {
                return;
            }

            propertyBlock ??= new MaterialPropertyBlock();
            overlayRenderer.GetPropertyBlock(propertyBlock);
            Color color = new Color(0f, 0f, 0f, currentAlpha);
            propertyBlock.SetColor(BaseColorId, color);
            propertyBlock.SetColor(ColorId, color);
            overlayRenderer.SetPropertyBlock(propertyBlock);
        }
    }
}
