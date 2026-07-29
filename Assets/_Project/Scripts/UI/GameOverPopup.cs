using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TicTacToe.UI
{
    /// <summary>
    /// The Game Over popup: shows the match result and duration and raises
    /// <see cref="RetryClicked"/> for whoever owns the match. The Exit button is wired
    /// to a <see cref="SceneLoader"/> inside the prefab, so it needs no scene references.
    /// </summary>
    public sealed class GameOverPopup : Popup
    {
        [SerializeField] private TMP_Text _resultLabel;
        [SerializeField] private TMP_Text _durationLabel;
        [SerializeField] private Button _retryButton;

        /// <summary>Raised when the Retry button is pressed.</summary>
        public event Action RetryClicked;

        protected override void Awake()
        {
            base.Awake();
            _retryButton.onClick.AddListener(() => RetryClicked?.Invoke());
        }

        /// <summary>Fills in the result and match duration, then opens the popup.</summary>
        public void Show(string resultText, TimeSpan matchDuration)
        {
            _resultLabel.text = resultText;
            _durationLabel.text = $"Time  {DurationText.Format(matchDuration)}";
            Open();
        }
    }
}
