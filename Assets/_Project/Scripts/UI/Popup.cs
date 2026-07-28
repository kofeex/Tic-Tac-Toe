using System;
using System.Collections;
using UnityEngine;

namespace TicTacToe.UI
{
    /// <summary>
    /// Base popup behaviour: a full-screen dimmer plus a centered panel,
    /// animated on open (fade in + scale overshoot) and close (fade out + shrink).
    /// The root starts inactive with CanvasGroup alpha 0; Open()/Close() drive everything.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class Popup : MonoBehaviour
    {
        private const float ClosedScale = 0.85f;
        private const float Overshoot = 1.70158f;

        [SerializeField] private RectTransform _panel;
        [SerializeField, Min(0.01f)] private float _openDuration = 0.25f;
        [SerializeField, Min(0.01f)] private float _closeDuration = 0.15f;

        private CanvasGroup _canvasGroup;
        private Coroutine _transition;

        /// <summary>Raised when the open animation finishes.</summary>
        public event Action Opened;

        /// <summary>Raised when the close animation finishes and the popup is deactivated.</summary>
        public event Action Closed;

        public bool IsOpen { get; private set; }

        protected virtual void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        public void Open()
        {
            if (IsOpen)
            {
                return;
            }

            IsOpen = true;
            gameObject.SetActive(true);
            Restart(OpenRoutine());
        }

        public void Close()
        {
            if (!IsOpen)
            {
                return;
            }

            IsOpen = false;
            Restart(CloseRoutine());
        }

        private void Restart(IEnumerator routine)
        {
            if (_transition != null)
            {
                StopCoroutine(_transition);
            }

            _transition = StartCoroutine(routine);
        }

        private IEnumerator OpenRoutine()
        {
            _canvasGroup.blocksRaycasts = true;
            float fromAlpha = _canvasGroup.alpha;
            for (float t = 0f; t < 1f; t += Time.unscaledDeltaTime / _openDuration)
            {
                _canvasGroup.alpha = Mathf.Lerp(fromAlpha, 1f, t);
                _panel.localScale = Vector3.one * Mathf.LerpUnclamped(ClosedScale, 1f, EaseOutBack(t));
                yield return null;
            }

            _canvasGroup.alpha = 1f;
            _panel.localScale = Vector3.one;
            _transition = null;
            Opened?.Invoke();
        }

        private IEnumerator CloseRoutine()
        {
            _canvasGroup.blocksRaycasts = false;
            float fromAlpha = _canvasGroup.alpha;
            float fromScale = _panel.localScale.x;
            for (float t = 0f; t < 1f; t += Time.unscaledDeltaTime / _closeDuration)
            {
                _canvasGroup.alpha = Mathf.Lerp(fromAlpha, 0f, t);
                _panel.localScale = Vector3.one * Mathf.Lerp(fromScale, ClosedScale, t * t);
                yield return null;
            }

            _canvasGroup.alpha = 0f;
            _panel.localScale = Vector3.one * ClosedScale;
            gameObject.SetActive(false);
            _transition = null;
            Closed?.Invoke();
        }

        private static float EaseOutBack(float t)
        {
            float u = t - 1f;
            return 1f + (Overshoot + 1f) * u * u * u + Overshoot * u * u;
        }
    }
}