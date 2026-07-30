using System.Collections.Generic;
using TicTacToe.Core;
using UnityEngine;

namespace TicTacToe.Themes
{
    /// <summary>
    /// The catalogues of looks offered in the theme selection popup, in display order. Each
    /// mark gets its own list on purpose: an X look and an O look are separate assets, so the
    /// two catalogues may be different lengths and one mark can offer a style the other has no
    /// counterpart for. The popup builds each of its rows from the matching list, so a new
    /// theme appears in the UI as soon as it is added here.
    /// </summary>
    [CreateAssetMenu(fileName = "ThemeLibrary", menuName = "Tic Tac Toe/Theme Library")]
    public sealed class ThemeLibrary : ScriptableObject
    {
        [SerializeField] private XMarkTheme[] _xThemes;
        [SerializeField] private OMarkTheme[] _oThemes;

        /// <summary>Every selectable X look, in the order they should be shown.</summary>
        public IReadOnlyList<MarkTheme> XThemes => _xThemes;

        /// <summary>Every selectable O look, in the order they should be shown.</summary>
        public IReadOnlyList<MarkTheme> OThemes => _oThemes;

        /// <summary>The looks offered for <paramref name="mark"/>, in the order they should be shown.</summary>
        public IReadOnlyList<MarkTheme> GetThemes(Mark mark) => mark == Mark.X ? XThemes : OThemes;

        /// <summary>
        /// The look used for <paramref name="mark"/> when nothing has been chosen yet; null when
        /// that catalogue is empty, since an empty catalogue is a broken asset rather than a
        /// reason to stop the game.
        /// </summary>
        public MarkTheme Default(Mark mark)
        {
            IReadOnlyList<MarkTheme> themes = GetThemes(mark);
            return themes is { Count: > 0 } ? themes[0] : null;
        }

        /// <summary>
        /// Looks a theme up by <see cref="MarkTheme.Id"/> within <paramref name="mark"/>'s own
        /// catalogue, falling back to <see cref="Default(Mark)"/> so a save file naming a removed
        /// theme still loads. The search stays scoped to one mark because ids are only unique per
        /// catalogue: the X and the O half of one visual style normally share an id, and an id
        /// that exists for one mark only must not leak into the other's row.
        /// </summary>
        public MarkTheme Find(Mark mark, string id)
        {
            IReadOnlyList<MarkTheme> themes = GetThemes(mark);
            if (themes != null && !string.IsNullOrEmpty(id))
            {
                foreach (MarkTheme theme in themes)
                {
                    if (theme != null && theme.Id == id)
                    {
                        return theme;
                    }
                }
            }

            return Default(mark);
        }
    }
}
