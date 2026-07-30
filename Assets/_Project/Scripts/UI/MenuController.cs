using UnityEngine;

namespace TicTacToe.UI
{
    /// <summary>Wires the main-menu buttons to their popups.</summary>
    public sealed class MenuController : MonoBehaviour
    {
        [SerializeField] private Popup _themePopup;
        [SerializeField] private Popup _statsPopup;
        [SerializeField] private Popup _settingsPopup;
        [SerializeField] private Popup _exitPopup;

        public void OpenThemePopup() => _themePopup.Open();

        public void OpenStatsPopup() => _statsPopup.Open();

        public void OpenSettingsPopup() => _settingsPopup.Open();

        public void OpenExitPopup() => _exitPopup.Open();

        /// <summary>Quits the application; in the editor this stops play mode instead.</summary>
        public void QuitApplication()
        {
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }
    }
}