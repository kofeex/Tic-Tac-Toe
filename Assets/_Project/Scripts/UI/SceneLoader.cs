using UnityEngine;

namespace TicTacToe.UI
{
    /// <summary>
    /// Loads a scene by name in response to a UI event. Lives on the prefab that triggers
    /// the transition, so the button wiring is stored in the prefab itself and never
    /// depends on a scene object reference (those cannot be saved into a prefab).
    /// </summary>
    public sealed class SceneLoader : MonoBehaviour
    {
        [SerializeField] private string _sceneName;

        /// <summary>Loads the configured scene through a fade. Hook this to a Button's onClick.</summary>
        public void Load() => SceneFader.Load(_sceneName);
    }
}
