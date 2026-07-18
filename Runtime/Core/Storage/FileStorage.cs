using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace Anlu.Save.Storage
{
    /// <summary>
    /// Persistencia en archivo dentro de <see cref="Application.persistentDataPath"/>. Escritura
    /// atómica (tmp → rename) y backup rotativo (.bak): un corte a mitad de escritura nunca corrompe
    /// el save vigente. Base para PC, móvil y WebGL (este último vía <see cref="WebGLStorage"/>).
    /// </summary>
    public sealed class FileStorage : ISaveStorage
    {
        private readonly string _rootDir;
        private readonly bool _keepBackup;

        /// <summary>Crea el storage. <paramref name="subFolder"/> se cuelga de persistentDataPath.</summary>
        public FileStorage(string subFolder = "Saves", bool keepBackup = true)
        {
            _rootDir = string.IsNullOrEmpty(subFolder)
                ? Application.persistentDataPath
                : Path.Combine(Application.persistentDataPath, subFolder);
            _keepBackup = keepBackup;
            Directory.CreateDirectory(_rootDir);
        }

        private string PathFor(string key) => Path.Combine(_rootDir, key + SaveKeys.SaveExtension);
        private string TempFor(string key) => Path.Combine(_rootDir, key + SaveKeys.TempExtension);
        private string BackupFor(string key) => Path.Combine(_rootDir, key + SaveKeys.BackupExtension);

        /// <inheritdoc />
        public Task<bool> ExistsAsync(string key)
            => Task.FromResult(File.Exists(PathFor(key)) || File.Exists(BackupFor(key)));

        /// <inheritdoc />
        public Task<byte[]> LoadAsync(string key)
        {
            string main = PathFor(key);
            if (File.Exists(main))
            {
                try { return Task.FromResult(File.ReadAllBytes(main)); }
                catch (Exception e) { Debug.LogError($"[Anlu.Save] Error leyendo '{main}', se intenta el backup. {e.Message}"); }
            }

            string backup = BackupFor(key);
            if (File.Exists(backup))
            {
                Debug.LogWarning($"[Anlu.Save] Cayendo al backup para el slot '{key}'.");
                return Task.FromResult(File.ReadAllBytes(backup));
            }

            return Task.FromResult<byte[]>(null);
        }

        /// <inheritdoc />
        public Task SaveAsync(string key, byte[] data)
        {
            string main = PathFor(key);
            string temp = TempFor(key);

            // 1) Escribir a temporal. Si acá se corta la luz, el save vigente sigue intacto.
            File.WriteAllBytes(temp, data);

            // 2) Rotar el vigente a backup (o borrarlo) para dejar libre el destino.
            if (File.Exists(main))
            {
                if (_keepBackup)
                {
                    string backup = BackupFor(key);
                    if (File.Exists(backup)) File.Delete(backup);
                    File.Move(main, backup);
                }
                else
                {
                    File.Delete(main);
                }
            }

            // 3) Promover el temporal a vigente (rename atómico en el mismo volumen).
            File.Move(temp, main);
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task DeleteAsync(string key)
        {
            SafeDelete(PathFor(key));
            SafeDelete(BackupFor(key));
            SafeDelete(TempFor(key));
            return Task.CompletedTask;
        }

        private static void SafeDelete(string path)
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }
}
