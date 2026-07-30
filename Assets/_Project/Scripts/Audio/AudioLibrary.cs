using UnityEngine;
using UnityEngine.Audio;

namespace TicTacToe.Audio
{
    /// <summary>
    /// Every clip and mixer route the game uses, as a data asset: swapping a sound or
    /// the whole soundscape means editing this asset (or creating another), zero code changes.
    /// </summary>
    [CreateAssetMenu(fileName = "AudioLibrary", menuName = "Tic Tac Toe/Audio Library")]
    public sealed class AudioLibrary : ScriptableObject
    {
        [Header("Mixer")]
        [SerializeField] private AudioMixer _mixer;
        [SerializeField] private AudioMixerGroup _musicGroup;
        [SerializeField] private AudioMixerGroup _sfxGroup;

        [Header("Clips")]
        [SerializeField] private AudioClip _backgroundMusic;
        [SerializeField] private AudioClip _buttonClick;
        [SerializeField] private AudioClip _markPlacement;
        [SerializeField] private AudioClip _popup;
        [SerializeField] private AudioClip _strike;

        public AudioMixer Mixer => _mixer;

        public AudioMixerGroup MusicGroup => _musicGroup;

        public AudioMixerGroup SfxGroup => _sfxGroup;

        public AudioClip BackgroundMusic => _backgroundMusic;

        public AudioClip ButtonClick => _buttonClick;

        public AudioClip MarkPlacement => _markPlacement;

        public AudioClip Popup => _popup;

        public AudioClip Strike => _strike;
    }
}
