using System;

namespace TicTacToe.Themes
{
    /// <summary>
    /// Both players' theme choices, persisted between runs via JSON. Player 1 always plays X
    /// and Player 2 always plays O, so the ids are stored per mark rather than per player
    /// number and stay correct wherever the marks are handed out.
    /// </summary>
    [Serializable]
    public sealed class ThemePreferences
    {
        /// <summary>Id of the theme chosen for the X mark (Player 1); empty means "use the default".</summary>
        public string XThemeId = string.Empty;

        /// <summary>Id of the theme chosen for the O mark (Player 2); empty means "use the default".</summary>
        public string OThemeId = string.Empty;
    }
}
