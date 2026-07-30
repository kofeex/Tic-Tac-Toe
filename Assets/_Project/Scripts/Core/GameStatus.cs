namespace TicTacToe.Core
{
    /// <summary>Lifecycle state of a match as tracked by the <see cref="Board"/>.</summary>
    public enum GameStatus
    {
        InProgress,
        XWins,
        OWins,
        Draw
    }
}
