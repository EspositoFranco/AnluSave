using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Anlu.Save.Storage
{
    /// <summary>
    /// Persistencia sobre <see cref="PlayerPrefs"/> (registro de Windows / plist en macOS-iOS /
    /// SharedPreferences en Android / IndexedDB en WebGL). El payload binario se guarda en Base64.
    /// Ideal para settings y saves chicos; para saves grandes preferí <see cref="FileStorage"/>.
    /// </summary>
    public sealed class PlayerPrefsStorage : ISaveStorage
    {
        private readonly string _prefix;

        /// <summary>Crea el storage. <paramref name="prefix"/> namespacea las keys dentro de PlayerPrefs.</summary>
        public PlayerPrefsStorage(string prefix = "anlu_save_") => _prefix = prefix ?? string.Empty;

        private string KeyFor(string key) => _prefix + key;

        /// <inheritdoc />
        public Task<bool> ExistsAsync(string key) => Task.FromResult(PlayerPrefs.HasKey(KeyFor(key)));

        /// <inheritdoc />
        public Task<byte[]> LoadAsync(string key)
        {
            string full = KeyFor(key);
            if (!PlayerPrefs.HasKey(full)) return Task.FromResult<byte[]>(null);

            try { return Task.FromResult(Convert.FromBase64String(PlayerPrefs.GetString(full))); }
            catch (FormatException e)
            {
                Debug.LogError($"[Anlu.Save] Payload Base64 inválido en '{full}'. {e.Message}");
                return Task.FromResult<byte[]>(null);
            }
        }

        /// <inheritdoc />
        public Task SaveAsync(string key, byte[] data)
        {
            PlayerPrefs.SetString(KeyFor(key), Convert.ToBase64String(data));
            PlayerPrefs.Save();
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task DeleteAsync(string key)
        {
            PlayerPrefs.DeleteKey(KeyFor(key));
            PlayerPrefs.Save();
            return Task.CompletedTask;
        }
    }
}
