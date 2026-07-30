using UnityEngine;

namespace TicTacToe.Themes
{
    /// <summary>
    /// One selectable look for a single mark. A theme used to carry the artwork for both marks
    /// at once, which forced the X and the O catalogues to stay the same length; holding one
    /// sprite per asset frees them to differ, so a look that only works as an X no longer has
    /// to ship with an O to go with it. Adding a theme means creating another asset and listing
    /// it in the <see cref="ThemeLibrary"/> — no code changes anywhere.
    /// All of a theme's data lives here; the concrete <see cref="XMarkTheme"/> and
    /// <see cref="OMarkTheme"/> subclasses exist only to make the mark part of the asset's type.
    /// </summary>
    public abstract class MarkTheme : ScriptableObject
    {
        [SerializeField] private string _id;
        [SerializeField] private string _displayName;
        [SerializeField] private Sprite _sprite;
        [SerializeField] private Color _strikeColor = new Color(1f, 0.84f, 0.35f, 0.95f);

        /// <summary>
        /// Stable key written to the save file; renaming the asset must not break it. Only has
        /// to be unique within one mark's catalogue, so the X and the O half of the same visual
        /// style are free to share an id.
        /// </summary>
        public string Id => _id;

        /// <summary>Name shown on the theme card.</summary>
        public string DisplayName => _displayName;

        /// <summary>The artwork this theme draws its own mark with.</summary>
        public Sprite Sprite => _sprite;

        /// <summary>Colour of the winning strike line when this theme's owner wins.</summary>
        public Color StrikeColor => _strikeColor;
    }
}
