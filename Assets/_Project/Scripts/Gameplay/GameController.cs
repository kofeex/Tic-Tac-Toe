using System;
using System.Collections;
using TicTacToe.Audio;
using TicTacToe.Core;
using TicTacToe.Persistence;
using TicTacToe.Themes;
using TicTacToe.UI;
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
        private const float StrikeToPopupPause = 0.55f;
        private const float DrawToPopupPause = 0.45f;

        [SerializeField] private BoardView _boardView;
        [SerializeField] private TMP_Text _turnLabel;
        [SerializeField] private GameOverPopup _gameOverPopup;
        [SerializeField, Min(3)] private int _boardSize = 3;

        private Board _board;
        private float _matchStartTime;
        private TimeSpan? _finalDuration;

        /// <summary>Raised whenever the board changes: a mark was placed or a new match started.</summary>
        public event Action StateChanged;

        /// <summary>How long the current match has run; frozen at the final value once it ends.</summary>
        public TimeSpan MatchDuration =>
            _board == null ? TimeSpan.Zero : _finalDuration ?? TimeSpan.FromSeconds(Time.time - _matchStartTime);

        /// <summary>Moves the given player has made this match.</summary>
        public int GetMoveCount(Mark player) => _board?.GetMoveCount(player) ?? 0;

        private void Start()
        {
            _board = new Board(_boardSize);
            _gameOverPopup.RetryClicked += Restart;
            StartMatch();
        }

        private void OnDestroy()
        {
            if (_gameOverPopup != null)
            {
                _gameOverPopup.RetryClicked -= Restart;
            }
        }

        /// <summary>Starts a fresh match on the same board; wired to the Game Over popup's Retry.</summary>
        public void Restart()
        {
            _gameOverPopup.Close();
            _board.Reset();
            StartMatch();
        }

        private void StartMatch()
        {
            _boardView.Clear();
            _boardView.Build(_board.Size, ThemeService.CurrentSelection, OnCellClicked);
            _matchStartTime = Time.time;
            _finalDuration = null;
            RefreshTurnLabel();
            StateChanged?.Invoke();
        }

        private void OnCellClicked(int row, int col)
        {
            if (!_board.TryPlaceMark(row, col))
            {
                return;
            }

            _boardView.SetMark(row, col, _board.GetCell(row, col));
            AudioManager.PlayMarkPlacement();
            StateChanged?.Invoke();

            if (_board.Status == GameStatus.InProgress)
            {
                RefreshTurnLabel();
            }
            else
            {
                StartCoroutine(FinishMatch());
            }
        }

        /// <summary>Freezes the board, plays the strike on a win, then opens the Game Over popup.</summary>
        private IEnumerator FinishMatch()
        {
            _finalDuration = TimeSpan.FromSeconds(Time.time - _matchStartTime);
            TimeSpan matchDuration = _finalDuration.Value;
            string resultText = GetResultText(_board.Status);

            // Recorded the moment the match ends, so leaving during the strike still counts it.
            StatisticsService.RecordMatch(_board.Status, matchDuration);

            _boardView.SetBoardInteractable(false);
            _turnLabel.text = resultText;

            if (_board.Status == GameStatus.Draw)
            {
                yield return new WaitForSeconds(DrawToPopupPause);
            }
            else
            {
                AudioManager.PlayStrike();
                _boardView.ShowStrike(_board.WinningCells, GetWinner(_board.Status));
                yield return new WaitForSeconds(BoardView.StrikeDurationSeconds + StrikeToPopupPause);
            }

            _gameOverPopup.Show(resultText, matchDuration);
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

        /// <summary>
        /// Maps a status to the mark that won it, so the strike can be drawn in the winner's
        /// own theme colour. <see cref="Mark.None"/> for a draw or an unfinished match.
        /// </summary>
        private static Mark GetWinner(GameStatus status) => status switch
        {
            GameStatus.XWins => Mark.X,
            GameStatus.OWins => Mark.O,
            _ => Mark.None
        };
    }
}
