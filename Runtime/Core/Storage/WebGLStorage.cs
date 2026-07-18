using System.Threading.Tasks;
#if UNITY_WEBGL && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif

namespace Anlu.Save.Storage
{
    /// <summary>
    /// Storage para WebGL. Escribe vía <see cref="FileStorage"/> sobre el FS virtual (idbfs) y fuerza
    /// el <c>FS.syncfs</c> para persistir a IndexedDB. Sin este sync, el progreso se pierde al cerrar
    /// la pestaña. Fuera de WebGL delega en FileStorage sin sincronizar (para probar en editor).
    /// </summary>
    public sealed class WebGLStorage : ISaveStorage
    {
        private readonly FileStorage _inner;

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void AnluSaveSyncFs();
#endif

        /// <summary>Crea el storage web apoyado en un <see cref="FileStorage"/> con backup.</summary>
        public WebGLStorage(string subFolder = "Saves") => _inner = new FileStorage(subFolder, keepBackup: true);

        /// <inheritdoc />
        public Task<bool> ExistsAsync(string key) => _inner.ExistsAsync(key);

        /// <inheritdoc />
        public Task<byte[]> LoadAsync(string key) => _inner.LoadAsync(key);

        /// <inheritdoc />
        public async Task SaveAsync(string key, byte[] data)
        {
            await _inner.SaveAsync(key, data);
            Sync();
        }

        /// <inheritdoc />
        public async Task DeleteAsync(string key)
        {
            await _inner.DeleteAsync(key);
            Sync();
        }

        private static void Sync()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            AnluSaveSyncFs();
#endif
        }
    }
}
