using TicTacToe.Core;
using UnityEngine;

namespace TicTacToe.Themes
{
    /// <summary>
    /// The pair of looks a match is played with: one picked by the player who owns the X and
    /// one by the player who owns the O. Each theme covers a single mark, so the two players
    /// can play with completely different looks and neither catalogue constrains the other.
    /// Immutable, so a running match keeps the look it started with even if the menu changes
    /// the stored choice afterwards.
    /// </summary>
    public sealed class ThemeSelection
    {
        /// <summary>Pairs up the two players' themes; either may be null and is handled as "missing".</summary>
        /// <param name="xTheme">Theme picked for the X mark (Player 1).</param>
        /// <param name="oTheme">Theme picked for the O mark (Player 2).</param>
        public ThemeSelection(MarkTheme xTheme, MarkTheme oTheme)
        {
            XTheme = xTheme;
            OTheme = oTheme;
        }

        /// <summary>Theme picked for the X mark (Player 1); null when none could be resolved.</summary>
        public MarkTheme XTheme { get; }

        /// <summary>Theme picked for the O mark (Player 2); null when none could be resolved.</summary>
        public MarkTheme OTheme { get; }

        /// <summary>
        /// The artwork for <paramref name="mark"/>, taken from the theme of the player who owns
        /// that mark — a theme carries exactly the one sprite its own mark is drawn with.
        /// Returns null when the theme or its sprite is missing so the view can just hide the
        /// image: a broken theme asset must not take the match down with it.
        /// </summary>
        public Sprite GetSprite(Mark mark)
        {
            MarkTheme theme = GetTheme(mark);
            return theme == null ? null : theme.Sprite;
        }

        /// <summary>
        /// The strike colour of the <paramref name="winner"/>'s theme, so the winning line
        /// matches whoever won. Falls back to white when that theme is missing.
        /// </summary>
        public Color GetStrikeColor(Mark winner)
        {
            MarkTheme theme = GetTheme(winner);
            return theme == null ? Color.white : theme.StrikeColor;
        }

        /// <summary>The theme belonging to the player who owns <paramref name="mark"/>.</summary>
        private MarkTheme GetTheme(Mark mark) => mark == Mark.X ? XTheme : OTheme;
    }
}
