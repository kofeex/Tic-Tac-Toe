using System.Collections.Generic;
using TicTacToe.Audio;
using TicTacToe.Core;
using TicTacToe.Themes;
using TMPro;
using UnityEngine;

namespace TicTacToe.UI
{
    /// <summary>
    /// The theme selection popup. Each player picks the look of their own mark: the first
    /// dropdown chooses Player 1's X, the second Player 2's O, and the two are completely
    /// independent. Each dropdown is filled at runtime from its own mark's catalogue in the
    /// <see cref="ThemeLibrary"/>, so the two may offer different numbers of looks and adding a
    /// theme asset makes it appear in its dropdown with no UI work. Choosing an option stores
    /// that player's choice; the Start button (wired to a <see cref="SceneLoader"/>) loads the game.
    /// </summary>
    public sealed class ThemePopup : Popup
    {
        internal const int MaxVisibleOptions = 4;

        [SerializeField] private TMP_Dropdown _player1Dropdown;
        [SerializeField] private TMP_Dropdown _player2Dropdown;

        private readonly List<MarkTheme> _player1Themes = new List<MarkTheme>();
        private readonly List<MarkTheme> _player2Themes = new List<MarkTheme>();

        protected override void Awake()
        {
            base.Awake();

            Populate(_player1Dropdown, _player1Themes, Mark.X);
            Populate(_player2Dropdown, _player2Themes, Mark.O);
        }

        /// <summary>
        /// Re-selects both players' stored choices each time the popup opens, so a reopened
        /// popup always shows what is actually saved. This runs here rather than in OnEnable:
        /// reopening while the close animation is still playing leaves the object active, so
        /// OnEnable would not fire and a dropdown could keep advertising a stale choice.
        /// </summary>
        protected override void OnOpening()
        {
            Refresh(_player1Dropdown, _player1Themes, Mark.X);
            Refresh(_player2Dropdown, _player2Themes, Mark.O);
        }

        /// <summary>
        /// Fills one dropdown with the looks offered for <paramref name="mark"/>. The dropdown
        /// reads only that mark's catalogue, which is what lets the two end up with different
        /// numbers of options. A missing catalogue leaves the dropdown empty instead of
        /// throwing — a broken theme asset must not take the popup down with it.
        /// </summary>
        /// <param name="dropdown">Dropdown to fill; whatever it was authored with is replaced.</param>
        /// <param name="offered">Receives the themes actually added, in option order.</param>
        /// <param name="mark">The mark whose look this dropdown chooses.</param>
        private static void Populate(TMP_Dropdown dropdown, List<MarkTheme> offered, Mark mark)
        {
            if (dropdown == null)
            {
                return;
            }

            dropdown.ClearOptions();
            dropdown.onValueChanged.AddListener(optionIndex => Choose(offered, mark, optionIndex));

            ThemeLibrary library = ThemeService.Library;
            IReadOnlyList<MarkTheme> themes = library == null ? null : library.GetThemes(mark);
            if (themes == null)
            {
                return;
            }

            var options = new List<TMP_Dropdown.OptionData>(themes.Count);
            foreach (MarkTheme theme in themes)
            {
                if (theme == null)
                {
                    continue;
                }

                offered.Add(theme);
                options.Add(new TMP_Dropdown.OptionData(string.Empty, theme.Sprite, Color.white));
            }

            dropdown.AddOptions(options);
            SizeTemplate(dropdown, offered.Count);
        }

        /// <summary>
        /// Caps the open list at <see cref="MaxVisibleOptions"/> rows, or shrinks it to fit when
        /// the catalogue offers fewer than that. TMP never resizes the list to match how many
        /// options it actually got, so a template authored for one catalogue length looks wrong
        /// — too tall and empty, or too short and clipped — for any other. Row height is read
        /// back from the template rather than duplicated as a constant, so it can never drift
        /// out of sync with however the prefab happens to be authored.
        /// </summary>
        /// <param name="dropdown">Dropdown whose template height to set.</param>
        /// <param name="optionCount">How many options were actually added.</param>
        private static void SizeTemplate(TMP_Dropdown dropdown, int optionCount)
        {
            if (dropdown.template == null || dropdown.itemText == null)
            {
                return;
            }

            float itemHeight = ((RectTransform)dropdown.itemText.transform.parent).rect.height;
            int visibleRows = Mathf.Min(optionCount, MaxVisibleOptions);
            Vector2 size = dropdown.template.sizeDelta;
            dropdown.template.sizeDelta = new Vector2(size.x, itemHeight * visibleRows);
        }

        /// <summary>
        /// Stores the look behind <paramref name="optionIndex"/> as the choice for
        /// <paramref name="mark"/>. Only this dropdown's own mark is written, so the other
        /// player's pick is left untouched.
        /// </summary>
        private static void Choose(List<MarkTheme> offered, Mark mark, int optionIndex)
        {
            AudioManager.PlayButtonClick();
            ThemeService.Select(mark, offered[optionIndex]);
        }

        /// <summary>
        /// Points one dropdown at the theme currently stored for <paramref name="mark"/>,
        /// falling back to the first option when that theme is not one this dropdown offers, so
        /// it can never advertise a look the game will not draw. Set without notify: showing the
        /// saved choice must not save it straight back — or play a click nobody made.
        /// </summary>
        private static void Refresh(TMP_Dropdown dropdown, List<MarkTheme> offered, Mark mark)
        {
            if (dropdown == null)
            {
                return;
            }

            int optionIndex = offered.IndexOf(ThemeService.GetSelected(mark));
            dropdown.SetValueWithoutNotify(Mathf.Max(optionIndex, 0));
        }
    }
}
