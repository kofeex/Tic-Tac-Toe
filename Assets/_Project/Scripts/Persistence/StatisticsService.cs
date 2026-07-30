using System;
using TicTacToe.Core;

namespace TicTacToe.Persistence
{
    /// <summary>
    /// The game's match record: static access to a <see cref="MatchStatistics"/> stored as
    /// JSON, so results survive app restarts. The file is read once on first use and written
    /// after every change — matches finish rarely, so saving eagerly costs nothing and means
    /// a crash (or a force-quit from the menu) can never lose a result.
    /// </summary>
    public static class StatisticsService
    {
        private const string StatisticsFile = "stats.json";

        private static MatchStatistics _current;

        /// <summary>
        /// The recorded statistics, loaded from disk on first access and cached afterwards.
        /// The cache lives as long as the loaded assemblies do, so in the editor stats carry
        /// from one play-mode session to the next through the file rather than through memory —
        /// which is exactly how a built player behaves.
        /// </summary>
        public static MatchStatistics Current => _current ??= JsonFileStore.Load<MatchStatistics>(StatisticsFile);

        /// <summary>Adds a finished match to the record and saves it immediately.</summary>
        /// <param name="result">How the match ended; a match still in progress is not a result.</param>
        /// <param name="duration">How long the match ran.</param>
        public static void RecordMatch(GameStatus result, TimeSpan duration)
        {
            Current.RecordMatch(result, duration);
            Save();
        }

        /// <summary>Wipes the record back to empty and saves it.</summary>
        public static void Clear()
        {
            Current.Reset();
            Save();
        }

        private static void Save() => JsonFileStore.Save(StatisticsFile, Current);
    }
}
