using UnityEngine;

namespace TicTacToe.Themes
{
    /// <summary>
    /// A look for the O mark. The empty body is the whole point: all the data is inherited from
    /// <see cref="MarkTheme"/> and this type exists purely so that "is an O look" is expressed
    /// in the type system. Because <see cref="ThemeLibrary"/> declares its O catalogue as
    /// <c>OMarkTheme[]</c>, the Inspector's object picker will not even offer an X asset for
    /// that array and a drag of one is rejected outright — the two catalogues cannot be crossed
    /// by a mis-drag. That is the engine enforcing the rule, rather than a naming convention
    /// nobody is obliged to follow.
    /// </summary>
    [CreateAssetMenu(fileName = "OTheme", menuName = "Tic Tac Toe/O Theme")]
    public sealed class OMarkTheme : MarkTheme
    {
    }
}
