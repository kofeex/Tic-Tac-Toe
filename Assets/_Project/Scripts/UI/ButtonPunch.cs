using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace TicTacToe.UI
{
    /// <summary>
    /// Drop-on component for a Button: a quick scale "punch" on click, so a press registers
    /// instantly rather than waiting for whatever happens next (a popup animation, a scene fade).
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public sealed class ButtonPunch : MonoBehaviour
    {
        private const float PunchScale = 1.12f;
        private const float PunchDuration = 0.12f;

        private RectTransform _rectTransform;
        private Coroutine _punch;

        private void Awake()
        {
            _rectTransform = (RectTransform)transform;

            if (TryGetComponent(out Button button))
            {
                button.onClick.AddListener(Punch);
            }
        }

        private void OnDisable()
        {
            // A popup can close mid-punch and deactivate this object; without this, the button
            // would stay visually scaled up the next time it becomes active.
            if (_punch != null)
            {
                StopCoroutine(_punch);
                _punch = null;
            }

            _rectTransform.localScale = Vector3.one;
        }

        private void Punch()
        {
            if (_punch != null)
            {
                StopCoroutine(_punch);
            }

            _punch = StartCoroutine(PunchRoutine());
        }

        private IEnumerator PunchRoutine()
        {
            // Unscaled time, accumulated exactly like Popup's coroutines: a button press should
            // feel responsive even if anything is ever paused.
            for (float t = 0f; t < 1f; t += Time.unscaledDeltaTime / PunchDuration)
            {
                float eased = t * t;
                _rectTransform.localScale = Vector3.one * Mathf.Lerp(PunchScale, 1f, eased);
                yield return null;
            }

            _rectTransform.localScale = Vector3.one;
            _punch = null;
        }
    }
}
