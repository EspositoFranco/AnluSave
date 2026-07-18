using System.Threading.Tasks;

namespace Anlu.Save.Storage
{
    /// <summary>
    /// Backend de persistencia intercambiable. Async-first: IndexedDB (WebGL) y los SDK de consola
    /// son asíncronos; las implementaciones síncronas simplemente completan de inmediato. Trabaja
    /// con <c>byte[]</c> para que los decorators (encriptación) apliquen de forma uniforme.
    /// </summary>
    /// <remarks>WebGL es single-thread: las implementaciones NO deben usar <c>Task.Run</c>.</remarks>
    public interface ISaveStorage
    {
        /// <summary>True si existe un guardado bajo <paramref name="key"/>.</summary>
        Task<bool> ExistsAsync(string key);

        /// <summary>Lee el payload de <paramref name="key"/>, o null si no existe.</summary>
        Task<byte[]> LoadAsync(string key);

        /// <summary>Escribe <paramref name="data"/> bajo <paramref name="key"/>.</summary>
        Task SaveAsync(string key, byte[] data);

        /// <summary>Elimina el guardado de <paramref name="key"/> (no falla si no existe).</summary>
        Task DeleteAsync(string key);
    }
}
