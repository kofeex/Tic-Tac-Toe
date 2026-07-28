using System;
using System.Collections.Generic;

namespace TicTacToe.Core
{
    /// <summary>
    /// Pure C# rules state machine for square tic-tac-toe boards of any size
    /// (3×3 by default). Tracks turns, validates moves and detects wins and draws.
    /// It has no Unity dependency, so it can be unit-tested in isolation.
    /// </summary>
    public sealed class Board
    {
        /// <summary>The four line directions a win can run along; each is scanned both ways.</summary>
        private static readonly (int Row, int Col)[] Directions =
        {
            (0, 1),  // Horizontal.
            (1, 0),  // Vertical.
            (1, 1),  // Main diagonal.
            (1, -1)  // Anti-diagonal.
        };

        private readonly Mark[] _cells;
        private readonly List<(int Row, int Col)> _winningCells;

        /// <summary>Creates an empty <paramref name="size"/>×<paramref name="size"/> board with X to move.</summary>
        /// <param name="size">Number of rows and columns; at least 3.</param>
        /// <param name="winLength">
        /// Marks in a row needed to win, between 3 and <paramref name="size"/>; 0 means "use <paramref name="size"/>".
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">A parameter is outside the ranges above.</exception>
        public Board(int size = 3, int winLength = 0)
        {
            if (size < 3)
            {
                throw new ArgumentOutOfRangeException(nameof(size), size, "Board size must be at least 3.");
            }

            if (winLength == 0)
            {
                winLength = size;
            }

            if (winLength < 3 || winLength > size)
            {
                throw new ArgumentOutOfRangeException(nameof(winLength), winLength, "Win length must be between 3 and the board size.");
            }

            Size = size;
            WinLength = winLength;
            _cells = new Mark[size * size];
            _winningCells = new List<(int Row, int Col)>(size);

            Reset();
        }

        /// <summary>Number of rows and columns.</summary>
        public int Size { get; }

        /// <summary>How many marks in an unbroken line are needed to win.</summary>
        public int WinLength { get; }

        /// <summary>The player whose turn it is; X always moves first.</summary>
        public Mark CurrentPlayer { get; private set; }

        /// <summary>The current phase of the match.</summary>
        public GameStatus Status { get; private set; }

        /// <summary>Total marks placed since the last reset.</summary>
        public int MoveCount { get; private set; }

        /// <summary>
        /// The cells of the winning line in order along the line, or empty while nobody has won.
        /// The line can exceed <see cref="WinLength"/> when the final move bridges two runs.
        /// </summary>
        public IReadOnlyList<(int Row, int Col)> WinningCells => _winningCells;

        /// <summary>Returns the mark occupying the given cell.</summary>
        /// <exception cref="ArgumentOutOfRangeException">The coordinates are outside the board.</exception>
        public Mark GetCell(int row, int col)
        {
            if (row < 0 || row >= Size)
            {
                throw new ArgumentOutOfRangeException(nameof(row), row, $"Row must be between 0 and {Size - 1}.");
            }

            if (col < 0 || col >= Size)
            {
                throw new ArgumentOutOfRangeException(nameof(col), col, $"Column must be between 0 and {Size - 1}.");
            }

            return _cells[row * Size + col];
        }

        /// <summary>
        /// Attempts to place the current player's mark. Returns false without changing any state
        /// when the game is already over, the coordinates are outside the board or the cell is
        /// occupied. On success the move is applied, the status re-evaluated, and — only while
        /// the game continues — the turn passes to the other player.
        /// </summary>
        public bool TryPlaceMark(int row, int col)
        {
            if (Status != GameStatus.InProgress || !IsInside(row, col) || _cells[row * Size + col] != Mark.None)
            {
                return false;
            }

            _cells[row * Size + col] = CurrentPlayer;
            MoveCount++;

            if (TryFindWinningLine(row, col))
            {
                Status = CurrentPlayer == Mark.X ? GameStatus.XWins : GameStatus.OWins;
            }
            else if (MoveCount == _cells.Length)
            {
                Status = GameStatus.Draw;
            }
            else
            {
                CurrentPlayer = CurrentPlayer == Mark.X ? Mark.O : Mark.X;
            }

            return true;
        }

        /// <summary>Clears the board back to an empty in-progress game with X to move.</summary>
        public void Reset()
        {
            Array.Clear(_cells, 0, _cells.Length);
            _winningCells.Clear();
            CurrentPlayer = Mark.X;
            Status = GameStatus.InProgress;
            MoveCount = 0;
        }

        /// <summary>
        /// Checks only the four lines through the cell just played — the rest of the board
        /// cannot have changed. Fills <see cref="_winningCells"/> when a winning run is found.
        /// </summary>
        private bool TryFindWinningLine(int row, int col)
        {
            Mark mark = _cells[row * Size + col];

            for (int i = 0; i < Directions.Length; i++)
            {
                (int rowStep, int colStep) = Directions[i];

                // Walk backwards to the start of the contiguous run through the played cell...
                int startRow = row;
                int startCol = col;
                while (HasMarkAt(startRow - rowStep, startCol - colStep, mark))
                {
                    startRow -= rowStep;
                    startCol -= colStep;
                }

                // ...then measure the full run forwards from there.
                int runLength = 0;
                while (HasMarkAt(startRow + runLength * rowStep, startCol + runLength * colStep, mark))
                {
                    runLength++;
                }

                if (runLength >= WinLength)
                {
                    for (int step = 0; step < runLength; step++)
                    {
                        _winningCells.Add((startRow + step * rowStep, startCol + step * colStep));
                    }

                    return true;
                }
            }

            return false;
        }

        private bool HasMarkAt(int row, int col, Mark mark) =>
            IsInside(row, col) && _cells[row * Size + col] == mark;

        private bool IsInside(int row, int col) =>
            row >= 0 && row < Size && col >= 0 && col < Size;
    }
}
