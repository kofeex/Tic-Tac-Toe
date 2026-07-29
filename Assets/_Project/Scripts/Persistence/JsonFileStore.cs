using System;
using System.IO;
using UnityEngine;

namespace TicTacToe.Persistence
{
    /// <summary>
    /// Loads and saves plain C# data objects as JSON files in
    /// <see cref="Application.persistentDataPath"/>, so they survive app restarts.
    /// A missing or unreadable file falls back to a fresh instance.
    /// </summary>
    public static class JsonFileStore
    {
        /// <summary>Reads <paramref name="fileName"/>, or returns a new <typeparamref name="T"/> when absent or corrupt.</summary>
        public static T Load<T>(string fileName) where T : class, new()
        {
            string path = GetPath(fileName);
            try
            {
                if (File.Exists(path))
                {
                    var data = JsonUtility.FromJson<T>(File.ReadAllText(path));
                    if (data != null)
                    {
                        return data;
                    }
                }
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Could not read '{path}' ({exception.Message}); using defaults.");
            }

            return new T();
        }

        /// <summary>Writes <paramref name="data"/> to <paramref name="fileName"/> as pretty-printed JSON.</summary>
        public static void Save<T>(string fileName, T data) where T : class
        {
            string path = GetPath(fileName);
            try
            {
                File.WriteAllText(path, JsonUtility.ToJson(data, prettyPrint: true));
            }
            catch (Exception exception)
            {
                Debug.LogError($"Could not write '{path}' ({exception.Message}).");
            }
        }

        private static string GetPath(string fileName) => Path.Combine(Application.persistentDataPath, fileName);
    }
}
