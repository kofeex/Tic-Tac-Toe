using System;

namespace TicTacToe.Audio
{
    /// <summary>The user's audio switches, persisted between runs via JSON.</summary>
    [Serializable]
    public sealed class AudioPreferences
    {
        public bool MusicEnabled = true;
        public bool SfxEnabled = true;
    }
}
