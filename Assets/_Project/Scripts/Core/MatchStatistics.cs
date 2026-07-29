using System;

namespace TicTacToe.Core
{
    /// <summary>
    /// The lifetime record of finished matches: how many were played, who won them and
    /// how long they took. Player 1 is always X and player 2 is always O.
    /// It has no Unity dependency, so it can be unit-tested in isolation, and it is
    /// <see cref="SerializableAttribute"/> so JSON persistence can round-trip it.
    /// </summary>
    [Serializable]
    public sealed class MatchStatistics
    {
        // Public fields rather than properties: Unity's serializer (and therefore
        // JsonUtility) only writes fields, so these are the JSON document's shape.

        /// <summary>Total finished matches recorded.</summary>
        public int GamesPlayed;

        /// <summary>Matches won by player 1 (X).</summary>
        public int Player1Wins;

        /// <summary>Matches won by player 2 (O).</summary>
        public int Player2Wins;

        /// <summary>Matches that ended in a draw.</summary>
        public int Draws;

        /// <summary>
        /// Accumulated length of every recorded match, in seconds. Stored as a number
        /// because <see cref="TimeSpan"/> is not a serializable field type.
        /// </summary>
        public double TotalDurationSeconds;

        /// <summary>Accumulated length of every recorded match.</summary>
        public TimeSpan TotalDuration => TimeSpan.FromSeconds(TotalDurationSeconds);

        /// <summary>
        /// Mean match length, or <see cref="TimeSpan.Zero"/> when nothing has been
        /// recorded yet — computed on demand, so it is never stored or divided by zero.
        /// </summary>
        public TimeSpan AverageDuration =>
            GamesPlayed == 0 ? TimeSpan.Zero : TimeSpan.FromSeconds(TotalDurationSeconds / GamesPlayed);

        /// <summary>
        /// Adds one finished match to the record: counts the game, credits the winner
        /// (or the draw) and accumulates the match length.
        /// </summary>
        /// <param name="result">How the match ended; a match still in progress is not a result.</param>
        /// <param name="duration">How long the match ran; zero is allowed, negative is not.</param>
        /// <exception cref="ArgumentException">
        /// <paramref name="result"/> is <see cref="GameStatus.InProgress"/> or an undefined status —
        /// recording an unfinished match is a programming error, so it fails loudly instead of
        /// silently inflating the totals.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="duration"/> is negative.</exception>
        public void RecordMatch(GameStatus result, TimeSpan duration)
        {
            if (duration < TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(duration), duration, "Match duration cannot be negative.");
            }

            // Both guards run before any field changes, so a rejected call leaves the record untouched.
            switch (result)
            {
                case GameStatus.XWins:
                    Player1Wins++;
                    break;
                case GameStatus.OWins:
                    Player2Wins++;
                    break;
                case GameStatus.Draw:
                    Draws++;
                    break;
                default:
                    throw new ArgumentException($"Cannot record '{result}': only a finished match is a result.", nameof(result));
            }

            GamesPlayed++;
            TotalDurationSeconds += duration.TotalSeconds;
        }

        /// <summary>Clears the record back to a fresh, empty one.</summary>
        public void Reset()
        {
            GamesPlayed = 0;
            Player1Wins = 0;
            Player2Wins = 0;
            Draws = 0;
            TotalDurationSeconds = 0d;
        }
    }
}
