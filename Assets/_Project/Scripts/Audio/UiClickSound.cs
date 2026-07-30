using UnityEngine;
using UnityEngine.UI;

namespace TicTacToe.Audio
{
    /// <summary>
    /// Plays the shared click sound when the Button or Toggle on this object is used.
    /// Popup dimmers deliberately do not get this component — closing a popup by
    /// clicking outside plays the popup sound only.
    /// </summary>
    public sealed class UiClickSound : MonoBehaviour
    {
        private void Awake()
        {
            if (TryGetComponent(out Button button))
            {
                button.onClick.AddListener(AudioManager.PlayButtonClick);
            }
            else if (TryGetComponent(out Toggle toggle))
            {
                toggle.onValueChanged.AddListener(_ => AudioManager.PlayButtonClick());
            }
        }
    }
}
