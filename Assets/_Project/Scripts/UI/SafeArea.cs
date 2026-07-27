using UnityEngine;

namespace TicTacToe.UI
{
    /// <summary>
    /// Keeps a full-stretch RectTransform inside the device safe area
    /// (notches, rounded corners, home indicators) in any orientation.
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(RectTransform))]
    public sealed class SafeArea : MonoBehaviour
    {
        private RectTransform _rectTransform;
        private Rect _appliedArea;

        private void OnEnable()
        {
            _rectTransform = (RectTransform)transform;
            Apply();
        }

        private void Update()
        {
            if (Screen.safeArea != _appliedArea)
            {
                Apply();
            }
        }

        private void Apply()
        {
            _appliedArea = Screen.safeArea;

            var screenSize = new Vector2(Screen.width, Screen.height);
            if (screenSize.x <= 0f || screenSize.y <= 0f)
            {
                return;
            }

            _rectTransform.anchorMin = _appliedArea.position / screenSize;
            _rectTransform.anchorMax = (_appliedArea.position + _appliedArea.size) / screenSize;
            _rectTransform.offsetMin = Vector2.zero;
            _rectTransform.offsetMax = Vector2.zero;
        }
    }
}