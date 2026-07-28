using TicTacToe.Core;
using TMPro;
using UnityEngine;

namespace TicTacToe.Gameplay
{
    /// <summary>
    /// Drives a match: owns the rules <see cref="Board"/>, feeds cell clicks into it
    /// and keeps the <see cref="BoardView"/> and turn label in sync. Player 1 is always X.
    /// </summary>
    public sealed class GameController : MonoBehaviour
    {
        [SerializeField] private BoardView _boardView;
        [SerializeField] private TMP_Text _turnLabel;
        [SerializeField, Min(3)] private int _boardSize = 3;

        private Board _board;

        private void Start()
        {
            _board = new Board(_boardSize);
            _boardView.Build(_board.Size, OnCellClicked);
            RefreshTurnLabel();
        }

        /// <summary>Starts a fresh match on the same board (wired to the Game Over popup's Retry in Phase 4).</summary>
        public void Restart()
        {
            _board.Reset();
            _boardView.Clear();
            _boardView.Build(_board.Size, OnCellClicked);
            RefreshTurnLabel();
        }

        private void OnCellClicked(int row, int col)
        {
            if (!_board.TryPlaceMark(row, col))
            {
                return;
            }

            _boardView.SetMark(row, col, _board.GetCell(row, col));

            if (_board.Status == GameStatus.InProgress)
            {
                RefreshTurnLabel();
            }
            else
            {
                _boardView.SetBoardInteractable(false);
                _turnLabel.text = GetResultText(_board.Status);
            }
        }

        private void RefreshTurnLabel() => _turnLabel.text = GetTurnText(_board.CurrentPlayer);

        /// <summary>Maps the player on turn to their display name; Player 1 is always X.</summary>
        private static string GetTurnText(Mark currentPlayer) =>
            currentPlayer == Mark.X ? "Player 1 (X)" : "Player 2 (O)";

        /// <summary>Maps a finished match's status to the result text.</summary>
        private static string GetResultText(GameStatus status) => status switch
        {
            GameStatus.XWins => "Player 1 Wins!",
            GameStatus.OWins => "Player 2 Wins!",
            _ => "Draw!"
        };
    }
}
