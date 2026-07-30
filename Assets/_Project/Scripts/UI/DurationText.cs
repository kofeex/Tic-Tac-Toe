using System;

namespace TicTacToe.UI
{
    /// <summary>Formats match durations the same way everywhere (HUD, popups, stats).</summary>
    public static class DurationText
    {
        /// <summary>Formats a duration as "m:ss" — minutes unpadded, seconds two digits.</summary>
        public static string Format(TimeSpan duration) => $"{(int)duration.TotalMinutes}:{duration.Seconds:00}";
    }
}
