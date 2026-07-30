using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TicTacToe.UI
{
    /// <summary>
    /// The game's one scene-transition hub: fades a full-screen overlay to black, loads the
    /// next scene, then fades back in. Bootstrapped from a Resources prefab before the first
    /// scene loads (same pattern as AudioManager) so every SceneLoader in the game, regardless
    /// of which scene it lives in, can call through a single static entry point.
    /// </summary>
    public sealed class SceneFader : MonoBehaviour
    {
        private const string PrefabResourcePath = "SceneFader";
        private const float FadeDuration = 0.35f;

        private static SceneFader _instance;

        [SerializeField] private CanvasGroup _overlay;

        /// <summary>
        /// Fades to black, loads <paramref name="sceneName"/>, then fades back in. If the
        /// singleton never bootstrapped (missing prefab), falls back to a bare scene load so a
        /// missing overlay degrades to "no fade" rather than "scenes stop loading."
        /// </summary>
        public static void Load(string sceneName)
        {
            if (_instance == null)
            {
                SceneManager.LoadScene(sceneName);
                return;
            }

            _instance.StartCoroutine(_instance.LoadRoutine(sceneName));
        }

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
                Debug.LogError($"SceneFader prefab missing from Resources/{PrefabResourcePath}.");
                return;
            }

            DontDestroyOnLoad(Instantiate(prefab));
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;

            // The game opens on a clean, non-faded scene; only Load() should ever darken it.
            _overlay.alpha = 0f;
            _overlay.blocksRaycasts = false;
        }

        private IEnumerator LoadRoutine(string sceneName)
        {
            // Raycasts stay blocked for the whole fade-out -> load -> fade-in sequence: this is
            // the transition's other job besides looking good, it stops a double-tap on the
            // button that started the transition (e.g. Start) from firing it twice.
            yield return Fade(0f, 1f);
            SceneManager.LoadScene(sceneName);
            yield return Fade(1f, 0f);
            _overlay.blocksRaycasts = false;
        }

        private IEnumerator Fade(float from, float to)
        {
            _overlay.blocksRaycasts = true;

            // Unscaled time, accumulated exactly like Popup's coroutines: a fade must not stall
            // if anything ever pauses timescale, and floating-point accumulation in the loop
            // condition never lands exactly on 1f, so the final value is snapped after the loop.
            for (float t = 0f; t < 1f; t += Time.unscaledDeltaTime / FadeDuration)
            {
                _overlay.alpha = Mathf.Lerp(from, to, t);
                yield return null;
            }

            _overlay.alpha = to;
        }
    }
}
