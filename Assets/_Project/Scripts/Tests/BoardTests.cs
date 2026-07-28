using System;
using NUnit.Framework;
using TicTacToe.Core;

namespace TicTacToe.Tests
{
    /// <summary>EditMode tests for the pure C# <see cref="Board"/> rules state machine.</summary>
    public sealed class BoardTests
    {
        // ----- Construction -----

        [Test]
        public void Constructor_Defaults_StartsCleanWithXToMove()
        {
            var board = new Board();

            Assert.That(board.Size, Is.EqualTo(3));
            Assert.That(board.WinLength, Is.EqualTo(3));
            Assert.That(board.CurrentPlayer, Is.EqualTo(Mark.X));
            Assert.That(board.Status, Is.EqualTo(GameStatus.InProgress));
            Assert.That(board.MoveCount, Is.EqualTo(0));
            Assert.That(board.WinningCells, Is.Empty);
            AssertAllCellsEmpty(board);
        }

        [Test]
        public void Constructor_WinLengthOmitted_DefaultsToBoardSize()
        {
            var board = new Board(5);

            Assert.That(board.Size, Is.EqualTo(5));
            Assert.That(board.WinLength, Is.EqualTo(5));
        }

        [Test]
        public void Constructor_SizeBelowThree_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new Board(2));
        }

        [Test]
        public void Constructor_WinLengthBelowThree_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new Board(3, 2));
        }

        [Test]
        public void Constructor_WinLengthAboveSize_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new Board(3, 4));
        }

        // ----- Cell access -----

        [TestCase(-1, 0)]
        [TestCase(0, -1)]
        [TestCase(3, 0)]
        [TestCase(0, 3)]
        public void GetCell_OutOfBounds_Throws(int row, int col)
        {
            var board = new Board();

            Assert.Throws<ArgumentOutOfRangeException>(() => board.GetCell(row, col));
        }

        // ----- Basic move rules -----

        [TestCase(-1, 0)]
        [TestCase(0, -1)]
        [TestCase(3, 0)]
        [TestCase(0, 3)]
        public void TryPlaceMark_OutOfBounds_ReturnsFalseWithoutStateChange(int row, int col)
        {
            var board = new Board();

            Assert.That(board.TryPlaceMark(row, col), Is.False);
            Assert.That(board.CurrentPlayer, Is.EqualTo(Mark.X));
            Assert.That(board.MoveCount, Is.EqualTo(0));
            Assert.That(board.Status, Is.EqualTo(GameStatus.InProgress));
        }

        [Test]
        public void TryPlaceMark_OccupiedCell_ReturnsFalseAndKeepsTurn()
        {
            var board = new Board();
            Play(board, (1, 1));

            Assert.That(board.TryPlaceMark(1, 1), Is.False);
            Assert.That(board.GetCell(1, 1), Is.EqualTo(Mark.X), "The original mark must not be overwritten.");
            Assert.That(board.CurrentPlayer, Is.EqualTo(Mark.O), "O must not lose the turn for tapping an occupied cell.");
            Assert.That(board.MoveCount, Is.EqualTo(1));
        }

        [Test]
        public void TryPlaceMark_ValidMoves_AlternatePlayersAndCountMoves()
        {
            var board = new Board();

            Play(board, (0, 0));
            Assert.That(board.GetCell(0, 0), Is.EqualTo(Mark.X));
            Assert.That(board.CurrentPlayer, Is.EqualTo(Mark.O));
            Assert.That(board.MoveCount, Is.EqualTo(1));

            Play(board, (1, 1));
            Assert.That(board.GetCell(1, 1), Is.EqualTo(Mark.O));
            Assert.That(board.CurrentPlayer, Is.EqualTo(Mark.X));
            Assert.That(board.MoveCount, Is.EqualTo(2));
        }

        // ----- Wins on 3×3 -----

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        public void TryPlaceMark_XCompletesRow_XWins(int row)
        {
            var board = new Board();
            int otherRow = (row + 1) % 3;

            Play(board, (row, 0), (otherRow, 0), (row, 1), (otherRow, 1), (row, 2));

            Assert.That(board.Status, Is.EqualTo(GameStatus.XWins));
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        public void TryPlaceMark_XCompletesColumn_XWins(int col)
        {
            var board = new Board();
            int otherCol = (col + 1) % 3;

            Play(board, (0, col), (0, otherCol), (1, col), (1, otherCol), (2, col));

            Assert.That(board.Status, Is.EqualTo(GameStatus.XWins));
        }

        [Test]
        public void TryPlaceMark_XCompletesMainDiagonal_XWins()
        {
            var board = new Board();

            Play(board, (0, 0), (0, 1), (1, 1), (0, 2), (2, 2));

            Assert.That(board.Status, Is.EqualTo(GameStatus.XWins));
        }

        [Test]
        public void TryPlaceMark_XCompletesAntiDiagonal_XWins()
        {
            var board = new Board();

            Play(board, (0, 2), (0, 0), (1, 1), (0, 1), (2, 0));

            Assert.That(board.Status, Is.EqualTo(GameStatus.XWins));
        }

        [Test]
        public void TryPlaceMark_OCompletesRow_OWins()
        {
            var board = new Board();

            Play(board, (0, 0), (2, 0), (0, 1), (2, 1), (1, 2), (2, 2));

            Assert.That(board.Status, Is.EqualTo(GameStatus.OWins));
        }

        // ----- Winning line contents -----

        [Test]
        public void WinningCells_AfterRowWin_HoldsTheRowInOrder()
        {
            var board = new Board();

            Play(board, (1, 0), (0, 0), (1, 1), (0, 1), (1, 2));

            Assert.That(board.WinningCells, Is.EqualTo(new[] { (1, 0), (1, 1), (1, 2) }));
        }

        [Test]
        public void WinningCells_AfterAntiDiagonalWin_HoldsTheDiagonalInOrder()
        {
            var board = new Board();

            Play(board, (0, 2), (0, 0), (1, 1), (0, 1), (2, 0));

            Assert.That(board.WinningCells, Is.EqualTo(new[] { (0, 2), (1, 1), (2, 0) }));
        }

        // ----- Endings -----

        [Test]
        public void TryPlaceMark_AfterGameOver_ReturnsFalseWithoutStateChange()
        {
            var board = new Board();
            Play(board, (0, 0), (1, 0), (0, 1), (1, 1), (0, 2)); // X wins the top row.

            Assert.That(board.TryPlaceMark(2, 2), Is.False);
            Assert.That(board.GetCell(2, 2), Is.EqualTo(Mark.None));
            Assert.That(board.Status, Is.EqualTo(GameStatus.XWins));
            Assert.That(board.MoveCount, Is.EqualTo(5));
        }

        [Test]
        public void TryPlaceMark_FullBoardWithoutWinner_EndsInDraw()
        {
            var board = new Board();

            // Final position:  X O X / X O O / O X X — no line for either player.
            Play(board,
                (0, 0), (0, 1), (0, 2),
                (1, 1), (1, 0), (1, 2),
                (2, 1), (2, 0), (2, 2));

            Assert.That(board.Status, Is.EqualTo(GameStatus.Draw));
            Assert.That(board.MoveCount, Is.EqualTo(9));
            Assert.That(board.WinningCells, Is.Empty);
        }

        // ----- Larger boards -----

        [Test]
        public void TryPlaceMark_FourByFour_ThreeInARowDoesNotWin()
        {
            var board = new Board(4);

            Play(board, (0, 0), (1, 0), (0, 1), (1, 1), (0, 2));

            Assert.That(board.Status, Is.EqualTo(GameStatus.InProgress));
            Assert.That(board.WinningCells, Is.Empty);
        }

        [Test]
        public void TryPlaceMark_FourByFour_FourInARowWins()
        {
            var board = new Board(4);

            Play(board, (0, 0), (1, 0), (0, 1), (1, 1), (0, 2), (1, 2), (0, 3));

            Assert.That(board.Status, Is.EqualTo(GameStatus.XWins));
            Assert.That(board.WinningCells, Is.EqualTo(new[] { (0, 0), (0, 1), (0, 2), (0, 3) }));
        }

        [Test]
        public void TryPlaceMark_FiveByFiveWithWinLengthFour_MidBoardRunWins()
        {
            var board = new Board(5, 4);

            // X builds (2,1)…(2,4); the winning move (2,3) lands inside the run,
            // and the run never touches the first column of its row.
            Play(board, (2, 1), (0, 0), (2, 2), (0, 1), (2, 4), (0, 2), (2, 3));

            Assert.That(board.Status, Is.EqualTo(GameStatus.XWins));
            Assert.That(board.WinningCells, Is.EqualTo(new[] { (2, 1), (2, 2), (2, 3), (2, 4) }));
        }

        // ----- Reset -----

        [Test]
        public void Reset_AfterFinishedGame_RestoresCleanState()
        {
            var board = new Board();
            Play(board, (0, 0), (1, 0), (0, 1), (1, 1), (0, 2)); // X wins the top row.

            board.Reset();

            Assert.That(board.Status, Is.EqualTo(GameStatus.InProgress));
            Assert.That(board.CurrentPlayer, Is.EqualTo(Mark.X));
            Assert.That(board.MoveCount, Is.EqualTo(0));
            Assert.That(board.WinningCells, Is.Empty);
            AssertAllCellsEmpty(board);
        }

        // ----- Helpers -----

        /// <summary>Plays the given cells in order, asserting every move is accepted.</summary>
        private static void Play(Board board, params (int Row, int Col)[] moves)
        {
            foreach ((int row, int col) in moves)
            {
                Assert.That(board.TryPlaceMark(row, col), Is.True, $"Move at ({row}, {col}) was rejected.");
            }
        }

        private static void AssertAllCellsEmpty(Board board)
        {
            for (int row = 0; row < board.Size; row++)
            {
                for (int col = 0; col < board.Size; col++)
                {
                    Assert.That(board.GetCell(row, col), Is.EqualTo(Mark.None), $"Cell ({row}, {col}) should be empty.");
                }
            }
        }
    }
}
