using System;
using TicTacToe.Core;
using TicTacToe.UI;
using TMPro;
using UnityEngine;

namespace TicTacToe.Gameplay
{
    /// <summary>
    /// In-game HUD: the live match timer and both players' move counters.
    /// Move counts refresh on <see cref="GameController.StateChanged"/>; the timer text
    /// is polled in Update but only rewritten when the displayed second changes.
    /// </summary>
    public sealed class MatchHud : MonoBehaviour
    {
        [SerializeField] private GameController _gameController;
        [SerializeField] private TMP_Text _timerValue;
        [SerializeField] private TMP_Text _player1Moves;
        [SerializeField] private TMP_Text _player2Moves;

        private int _lastShownSecond = -1;

        private void Start()
        {
            _gameController.StateChanged += RefreshMoveCounts;
            RefreshMoveCounts();
        }

        private void OnDestroy()
        {
            if (_gameController != null)
            {
                _gameController.StateChanged -= RefreshMoveCounts;
            }
        }

        private void Update()
        {
            TimeSpan duration = _gameController.MatchDuration;
            int second = (int)duration.TotalSeconds;
            if (second == _lastShownSecond)
            {
                return;
            }

            _lastShownSecond = second;
            _timerValue.text = DurationText.Format(duration);
        }

        private void RefreshMoveCounts()
        {
            _player1Moves.text = _gameController.GetMoveCount(Mark.X).ToString();
            _player2Moves.text = _gameController.GetMoveCount(Mark.O).ToString();
        }
    }
}
