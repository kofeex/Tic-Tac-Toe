using System;
using NUnit.Framework;
using TicTacToe.Core;

namespace TicTacToe.Tests
{
    /// <summary>EditMode tests for the pure C# <see cref="MatchStatistics"/> record.</summary>
    public sealed class MatchStatisticsTests
    {
        /// <summary>Accumulated seconds are doubles; compare them with a tolerance, not for bit equality.</summary>
        private const double Tolerance = 1e-6;

        // ----- Fresh record -----

        [Test]
        public void NewRecord_IsEmpty()
        {
            var statistics = new MatchStatistics();

            AssertCounts(statistics, gamesPlayed: 0, player1Wins: 0, player2Wins: 0, draws: 0);
            Assert.That(statistics.TotalDurationSeconds, Is.EqualTo(0d).Within(Tolerance));
            Assert.That(statistics.TotalDuration, Is.EqualTo(TimeSpan.Zero));
            Assert.That(statistics.AverageDuration, Is.EqualTo(TimeSpan.Zero));
        }

        [Test]
        public void AverageDuration_DurationWithoutGames_IsZeroInsteadOfDividingByZero()
        {
            // The fields are public so the saved file could be truncated or hand-edited into
            // this state; without the guard the division would overflow TimeSpan.FromSeconds.
            var statistics = new MatchStatistics { TotalDurationSeconds = 90d };

            Assert.That(statistics.AverageDuration, Is.EqualTo(TimeSpan.Zero));
        }

        // ----- Recording results -----

        [Test]
        public void RecordMatch_XWin_CreditsPlayerOneOnly()
        {
            var statistics = new MatchStatistics();

            statistics.RecordMatch(GameStatus.XWins, TimeSpan.FromSeconds(45));

            AssertCounts(statistics, gamesPlayed: 1, player1Wins: 1, player2Wins: 0, draws: 0);
        }

        [Test]
        public void RecordMatch_OWin_CreditsPlayerTwoOnly()
        {
            var statistics = new MatchStatistics();

            statistics.RecordMatch(GameStatus.OWins, TimeSpan.FromSeconds(45));

            AssertCounts(statistics, gamesPlayed: 1, player1Wins: 0, player2Wins: 1, draws: 0);
        }

        [Test]
        public void RecordMatch_Draw_CountsTheDrawOnly()
        {
            var statistics = new MatchStatistics();

            statistics.RecordMatch(GameStatus.Draw, TimeSpan.FromSeconds(45));

            AssertCounts(statistics, gamesPlayed: 1, player1Wins: 0, player2Wins: 0, draws: 1);
        }

        [TestCase(GameStatus.XWins)]
        [TestCase(GameStatus.OWins)]
        [TestCase(GameStatus.Draw)]
        public void RecordMatch_AnyResult_CountsTheGameAndItsDuration(GameStatus result)
        {
            var statistics = new MatchStatistics();

            statistics.RecordMatch(result, TimeSpan.FromSeconds(12));

            Assert.That(statistics.GamesPlayed, Is.EqualTo(1));
            Assert.That(statistics.TotalDurationSeconds, Is.EqualTo(12d).Within(Tolerance));
        }

        [Test]
        public void RecordMatch_MixedResults_AccumulatesEveryCounter()
        {
            var statistics = new MatchStatistics();

            statistics.RecordMatch(GameStatus.XWins, TimeSpan.FromSeconds(10));
            statistics.RecordMatch(GameStatus.Draw, TimeSpan.FromSeconds(20));
            statistics.RecordMatch(GameStatus.OWins, TimeSpan.FromSeconds(30));
            statistics.RecordMatch(GameStatus.XWins, TimeSpan.FromSeconds(40));

            AssertCounts(statistics, gamesPlayed: 4, player1Wins: 2, player2Wins: 1, draws: 1);
            Assert.That(statistics.TotalDurationSeconds, Is.EqualTo(100d).Within(Tolerance));
        }

        // ----- Durations -----

        [Test]
        public void AverageDuration_SingleMatch_IsThatMatchsLength()
        {
            var statistics = new MatchStatistics();

            statistics.RecordMatch(GameStatus.XWins, TimeSpan.FromSeconds(37));

            Assert.That(statistics.AverageDuration, Is.EqualTo(TimeSpan.FromSeconds(37)));
        }

        [Test]
        public void AverageDuration_MultipleMatches_IsTheMean()
        {
            var statistics = new MatchStatistics();

            statistics.RecordMatch(GameStatus.XWins, TimeSpan.FromSeconds(10));
            statistics.RecordMatch(GameStatus.OWins, TimeSpan.FromSeconds(20));
            statistics.RecordMatch(GameStatus.Draw, TimeSpan.FromSeconds(30));

            Assert.That(statistics.AverageDuration, Is.EqualTo(TimeSpan.FromSeconds(20)), "60s over 3 matches.");
        }

        [Test]
        public void AverageDuration_UnevenMatches_KeepsSubSecondPrecision()
        {
            var statistics = new MatchStatistics();

            statistics.RecordMatch(GameStatus.XWins, TimeSpan.FromSeconds(5));
            statistics.RecordMatch(GameStatus.OWins, TimeSpan.FromSeconds(10));

            Assert.That(statistics.AverageDuration, Is.EqualTo(TimeSpan.FromSeconds(7.5)), "15s over 2 matches.");
        }

        [Test]
        public void TotalDuration_AfterRecording_SumsEveryMatch()
        {
            var statistics = new MatchStatistics();

            statistics.RecordMatch(GameStatus.XWins, TimeSpan.FromMinutes(1));
            statistics.RecordMatch(GameStatus.Draw, TimeSpan.FromSeconds(30));

            Assert.That(statistics.TotalDuration, Is.EqualTo(TimeSpan.FromSeconds(90)));
        }

        [Test]
        public void RecordMatch_ZeroDuration_IsAccepted()
        {
            // A match decided within a single frame is unusual but not invalid.
            var statistics = new MatchStatistics();

            statistics.RecordMatch(GameStatus.Draw, TimeSpan.Zero);

            AssertCounts(statistics, gamesPlayed: 1, player1Wins: 0, player2Wins: 0, draws: 1);
            Assert.That(statistics.AverageDuration, Is.EqualTo(TimeSpan.Zero));
        }

        // ----- Invalid input -----

        [Test]
        public void RecordMatch_InProgress_Throws()
        {
            var statistics = new MatchStatistics();

            Assert.Throws<ArgumentException>(() => statistics.RecordMatch(GameStatus.InProgress, TimeSpan.FromSeconds(10)));
        }

        [Test]
        public void RecordMatch_InProgress_LeavesTheRecordUnchanged()
        {
            var statistics = new MatchStatistics();
            statistics.RecordMatch(GameStatus.XWins, TimeSpan.FromSeconds(10));

            Assert.Throws<ArgumentException>(() => statistics.RecordMatch(GameStatus.InProgress, TimeSpan.FromSeconds(99)));

            AssertCounts(statistics, gamesPlayed: 1, player1Wins: 1, player2Wins: 0, draws: 0);
            Assert.That(statistics.TotalDurationSeconds, Is.EqualTo(10d).Within(Tolerance));
        }

        [Test]
        public void RecordMatch_UndefinedStatus_Throws()
        {
            var statistics = new MatchStatistics();

            Assert.Throws<ArgumentException>(() => statistics.RecordMatch((GameStatus)99, TimeSpan.FromSeconds(10)));
        }

        [TestCase(-0.001)]
        [TestCase(-1.0)]
        [TestCase(-3600.0)]
        public void RecordMatch_NegativeDuration_ThrowsAndLeavesTheRecordUnchanged(double negativeSeconds)
        {
            var statistics = new MatchStatistics();
            statistics.RecordMatch(GameStatus.Draw, TimeSpan.FromSeconds(20));

            Assert.Throws<ArgumentOutOfRangeException>(
                () => statistics.RecordMatch(GameStatus.XWins, TimeSpan.FromSeconds(negativeSeconds)));

            AssertCounts(statistics, gamesPlayed: 1, player1Wins: 0, player2Wins: 0, draws: 1);
            Assert.That(statistics.TotalDurationSeconds, Is.EqualTo(20d).Within(Tolerance));
        }

        // ----- Reset -----

        [Test]
        public void Reset_AfterRecording_RestoresAFreshRecord()
        {
            var statistics = new MatchStatistics();
            statistics.RecordMatch(GameStatus.XWins, TimeSpan.FromSeconds(10));
            statistics.RecordMatch(GameStatus.OWins, TimeSpan.FromSeconds(20));
            statistics.RecordMatch(GameStatus.Draw, TimeSpan.FromSeconds(30));

            statistics.Reset();

            AssertCounts(statistics, gamesPlayed: 0, player1Wins: 0, player2Wins: 0, draws: 0);
            Assert.That(statistics.TotalDurationSeconds, Is.EqualTo(0d).Within(Tolerance));
            Assert.That(statistics.AverageDuration, Is.EqualTo(TimeSpan.Zero));
        }

        [Test]
        public void Reset_ThenRecording_CountsFromZeroAgain()
        {
            var statistics = new MatchStatistics();
            statistics.RecordMatch(GameStatus.XWins, TimeSpan.FromSeconds(60));
            statistics.Reset();

            statistics.RecordMatch(GameStatus.Draw, TimeSpan.FromSeconds(10));

            AssertCounts(statistics, gamesPlayed: 1, player1Wins: 0, player2Wins: 0, draws: 1);
            Assert.That(statistics.AverageDuration, Is.EqualTo(TimeSpan.FromSeconds(10)));
        }

        // ----- Persisted shape -----

        [Test]
        public void Fields_AfterRecording_HoldTheValuesThatGetSaved()
        {
            // Only these five public fields are written to JSON, so they alone must carry the
            // whole record; the durations stay derived rather than becoming stale saved copies.
            var statistics = new MatchStatistics();

            statistics.RecordMatch(GameStatus.XWins, TimeSpan.FromSeconds(30));
            statistics.RecordMatch(GameStatus.Draw, TimeSpan.FromSeconds(10));

            Assert.That(statistics.GamesPlayed, Is.EqualTo(2));
            Assert.That(statistics.Player1Wins, Is.EqualTo(1));
            Assert.That(statistics.Player2Wins, Is.EqualTo(0));
            Assert.That(statistics.Draws, Is.EqualTo(1));
            Assert.That(statistics.TotalDurationSeconds, Is.EqualTo(40d).Within(Tolerance));
        }

        [Test]
        public void AverageDuration_OnARestoredRecord_UsesTheStoredFields()
        {
            var statistics = CreateRestoredRecord();

            Assert.That(statistics.AverageDuration, Is.EqualTo(TimeSpan.FromSeconds(30)), "120s over 4 matches.");
        }

        [Test]
        public void RecordMatch_OnARestoredRecord_ContinuesFromTheStoredTotals()
        {
            var statistics = CreateRestoredRecord();

            statistics.RecordMatch(GameStatus.OWins, TimeSpan.FromSeconds(30));

            AssertCounts(statistics, gamesPlayed: 5, player1Wins: 2, player2Wins: 2, draws: 1);
            Assert.That(statistics.AverageDuration, Is.EqualTo(TimeSpan.FromSeconds(30)), "150s over 5 matches.");
        }

        // ----- Helpers -----

        /// <summary>A record in the state it would come back from disk in: fields set, nothing else.</summary>
        private static MatchStatistics CreateRestoredRecord() => new MatchStatistics
        {
            GamesPlayed = 4,
            Player1Wins = 2,
            Player2Wins = 1,
            Draws = 1,
            TotalDurationSeconds = 120d
        };

        /// <summary>Asserts all four counters at once, and that the expectations themselves add up.</summary>
        private static void AssertCounts(MatchStatistics statistics, int gamesPlayed, int player1Wins, int player2Wins, int draws)
        {
            Assert.That(player1Wins + player2Wins + draws, Is.EqualTo(gamesPlayed), "The expected counts are inconsistent.");
            Assert.That(statistics.GamesPlayed, Is.EqualTo(gamesPlayed), "Games played");
            Assert.That(statistics.Player1Wins, Is.EqualTo(player1Wins), "Player 1 wins");
            Assert.That(statistics.Player2Wins, Is.EqualTo(player2Wins), "Player 2 wins");
            Assert.That(statistics.Draws, Is.EqualTo(draws), "Draws");
        }
    }
}
