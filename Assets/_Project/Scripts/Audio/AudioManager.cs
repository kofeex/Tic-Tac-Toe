using TicTacToe.Persistence;
using UnityEngine;

namespace TicTacToe.Audio
{
    /// <summary>
    /// The game's one audio hub: plays the looping background music and all SFX through
    /// the mixer, and owns the persisted user preferences. Bootstrapped from a Resources
    /// prefab before the first scene loads, so audio works when playing from any scene,
    /// and survives scene changes.
    /// </summary>
    public sealed class AudioManager : MonoBehaviour
    {
        private const string PrefabResourcePath = "AudioManager";
        private const string PreferencesFile = "settings.json";
        private const string MusicVolumeParameter = "MusicVolume";
        private const string SfxVolumeParameter = "SfxVolume";
        private const float MutedDecibels = -80f;

        private static AudioManager _instance;

        [SerializeField] private AudioLibrary _library;
        [SerializeField] private AudioSource _musicSource;
        [SerializeField] private AudioSource _sfxSource;

        private AudioPreferences _preferences;

        /// <summary>Background music switch; setting it applies the mixer change and persists.</summary>
        public static bool MusicEnabled
        {
            get => _instance == null || _instance._preferences.MusicEnabled;
            set => SetPreference(preferences => preferences.MusicEnabled = value);
        }

        /// <summary>Sound-effects switch; setting it applies the mixer change and persists.</summary>
        public static bool SfxEnabled
        {
            get => _instance == null || _instance._preferences.SfxEnabled;
            set => SetPreference(preferences => preferences.SfxEnabled = value);
        }

        public static void PlayButtonClick() => PlaySfx(instance => instance._library.ButtonClick);

        public static void PlayMarkPlacement() => PlaySfx(instance => instance._library.MarkPlacement);

        public static void PlayPopup() => PlaySfx(instance => instance._library.Popup);

        public static void PlayStrike() => PlaySfx(instance => instance._library.Strike);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            if (_instance != null)
            {
                return;
            }

            var prefab = Resources.Load<GameObject>(PrefabResourcePath);
            if (prefab == null)
            {
                Debug.LogError($"AudioManager prefab missing from Resources/{PrefabResourcePath}.");
                return;
            }

            DontDestroyOnLoad(Instantiate(prefab));
        }

        private static void SetPreference(System.Action<AudioPreferences> change)
        {
            if (_instance == null)
            {
                return;
            }

            change(_instance._preferences);
            _instance.ApplyPreferences();
            JsonFileStore.Save(PreferencesFile, _instance._preferences);
        }

        private static void PlaySfx(System.Func<AudioManager, AudioClip> selectClip)
        {
            if (_instance == null)
            {
                return;
            }

            AudioClip clip = selectClip(_instance);
            if (clip != null)
            {
                _instance._sfxSource.PlayOneShot(clip);
            }
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            _preferences = JsonFileStore.Load<AudioPreferences>(PreferencesFile);
        }

        private void Start()
        {
            // Mixer values set in Awake get stomped by the mixer's own initialisation;
            // Start is the documented safe point to apply them.
            ApplyPreferences();

            _musicSource.clip = _library.BackgroundMusic;
            _musicSource.loop = true;
            _musicSource.Play();
        }

        /// <summary>Routes the on/off switches into the mixer's exposed volume parameters.</summary>
        private void ApplyPreferences()
        {
            _library.Mixer.SetFloat(MusicVolumeParameter, _preferences.MusicEnabled ? 0f : MutedDecibels);
            _library.Mixer.SetFloat(SfxVolumeParameter, _preferences.SfxEnabled ? 0f : MutedDecibels);
        }
    }
}
