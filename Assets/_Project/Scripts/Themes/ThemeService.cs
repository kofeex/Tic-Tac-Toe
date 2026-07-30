using TicTacToe.Core;
using TicTacToe.Persistence;
using UnityEngine;

namespace TicTacToe.Themes
{
    /// <summary>
    /// Static access to the theme catalogues and to each player's own choice, which is
    /// remembered between runs. Player 1 picks the look of the X and Player 2 the look of the
    /// O, so choices are keyed by <see cref="Mark"/> from end to end — the saved id, the lookup
    /// and the catalogue searched are all per mark. The library is loaded from Resources so the
    /// Game scene resolves the saved themes without depending on the menu that set them.
    /// </summary>
    public static class ThemeService
    {
        private const string PreferencesFile = "theme.json";
        private const string LibraryResourcePath = "ThemeLibrary";

        private static ThemeLibrary _library;
        private static ThemePreferences _preferences;

        /// <summary>Both marks' catalogues of selectable looks.</summary>
        public static ThemeLibrary Library
        {
            get
            {
                if (_library == null)
                {
                    _library = Resources.Load<ThemeLibrary>(LibraryResourcePath);
                    if (_library == null)
                    {
                        Debug.LogError($"ThemeLibrary missing from Resources/{LibraryResourcePath}.");
                    }
                }

                return _library;
            }
        }

        /// <summary>
        /// Both players' current choices as one object; this is what gameplay draws with.
        /// A fresh snapshot is built per call, so a match keeps the selection it was handed.
        /// </summary>
        public static ThemeSelection CurrentSelection =>
            new ThemeSelection(GetSelected(Mark.X), GetSelected(Mark.O));

        private static ThemePreferences Preferences =>
            _preferences ??= JsonFileStore.Load<ThemePreferences>(PreferencesFile);

        /// <summary>
        /// The theme chosen for <paramref name="mark"/>; falls back to that mark's default look
        /// when the player has not chosen yet or the saved id no longer exists in its catalogue.
        /// </summary>
        public static MarkTheme GetSelected(Mark mark) =>
            Library == null ? null : Library.Find(mark, GetSelectedId(mark));

        /// <summary>
        /// Chooses <paramref name="theme"/> for the player who owns <paramref name="mark"/>
        /// and saves the choice immediately; the other player's choice is left untouched.
        /// </summary>
        public static void Select(Mark mark, MarkTheme theme)
        {
            if (theme == null)
            {
                return;
            }

            if (mark == Mark.X)
            {
                Preferences.XThemeId = theme.Id;
            }
            else
            {
                Preferences.OThemeId = theme.Id;
            }

            JsonFileStore.Save(PreferencesFile, Preferences);
        }

        private static string GetSelectedId(Mark mark) =>
            mark == Mark.X ? Preferences.XThemeId : Preferences.OThemeId;
    }
}
