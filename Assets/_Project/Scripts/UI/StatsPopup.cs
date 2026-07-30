using TicTacToe.Core;
using TicTacToe.Persistence;
using TMPro;
using UnityEngine;

namespace TicTacToe.UI
{
    /// <summary>
    /// The Stats popup: shows the persisted lifetime record. It has no buttons of its own —
    /// the dimmer closes it — so all it does is fill in the value labels when it opens.
    /// </summary>
    public sealed class StatsPopup : Popup
    {
        /// <summary>Shown instead of a duration until at least one match has been recorded.</summary>
        private const string NoDurationText = "--:--";

        [SerializeField] private TMP_Text _gamesPlayedValue;
        [SerializeField] private TMP_Text _player1WinsValue;
        [SerializeField] private TMP_Text _player2WinsValue;
        [SerializeField] private TMP_Text _drawsValue;
        [SerializeField] private TMP_Text _averageDurationValue;

        protected override void OnOpening()
        {
            MatchStatistics statistics = StatisticsService.Current;

            _gamesPlayedValue.text = statistics.GamesPlayed.ToString();
            _player1WinsValue.text = statistics.Player1Wins.ToString();
            _player2WinsValue.text = statistics.Player2Wins.ToString();
            _drawsValue.text = statistics.Draws.ToString();
            _averageDurationValue.text = statistics.GamesPlayed == 0
                ? NoDurationText
                : DurationText.Format(statistics.AverageDuration);
        }
    }
}
