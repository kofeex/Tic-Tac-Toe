using TicTacToe.Audio;
using UnityEngine;
using UnityEngine.UI;

namespace TicTacToe.UI
{
    /// <summary>
    /// Binds the Settings popup's toggles to the persisted audio preferences:
    /// they show the current state whenever the popup opens, and flipping one
    /// applies and saves immediately.
    /// </summary>
    public sealed class SettingsToggles : MonoBehaviour
    {
        [SerializeField] private Toggle _musicToggle;
        [SerializeField] private Toggle _sfxToggle;

        private void Awake()
        {
            _musicToggle.onValueChanged.AddListener(value => AudioManager.MusicEnabled = value);
            _sfxToggle.onValueChanged.AddListener(value => AudioManager.SfxEnabled = value);
        }

        private void OnEnable()
        {
            // The popup root starts inactive and is activated by Open(), so this
            // runs on every open; WithoutNotify avoids re-applying or clicking.
            _musicToggle.SetIsOnWithoutNotify(AudioManager.MusicEnabled);
            _sfxToggle.SetIsOnWithoutNotify(AudioManager.SfxEnabled);
        }
    }
}
