using System.Collections.Generic;
using UnityEngine;

namespace Anlu.Save
{
    /// <summary>
    /// Ejecuta la cadena de <see cref="ISaveMigration{T}"/> desde la versión del documento hasta
    /// <see cref="CurrentVersion"/>. Reemplaza el <c>switch</c> hardcodeado por objetos registrables.
    /// </summary>
    /// <typeparam name="T">Modelo de guardado versionado.</typeparam>
    public sealed class SaveMigrationRunner<T> where T : IVersionedSave
    {
        private readonly Dictionary<int, ISaveMigration<T>> _byFromVersion = new();

        /// <summary>Versión de esquema objetivo de este build.</summary>
        public int CurrentVersion { get; }

        /// <summary>Crea el runner. Cada migración debe tener un <c>FromVersion</c> único.</summary>
        public SaveMigrationRunner(int currentVersion, IEnumerable<ISaveMigration<T>> migrations = null)
        {
            CurrentVersion = currentVersion;
            if (migrations == null) return;

            foreach (var migration in migrations)
            {
                if (migration == null) continue;
                if (_byFromVersion.ContainsKey(migration.FromVersion))
                {
                    Debug.LogWarning($"[Anlu.Save] Migración duplicada para FromVersion={migration.FromVersion}; se ignora la segunda.");
                    continue;
                }
                _byFromVersion[migration.FromVersion] = migration;
            }
        }

        /// <summary>Devuelve <paramref name="data"/> migrado al esquema actual.</summary>
        public T Migrate(T data)
        {
            if (data == null) return data;

            if (data.SchemaVersion > CurrentVersion)
            {
                Debug.LogWarning($"[Anlu.Save] schemaVersion {data.SchemaVersion} es más nueva que la soportada ({CurrentVersion}); se clampea.");
                data.SchemaVersion = CurrentVersion;
                return data;
            }

            while (data.SchemaVersion < CurrentVersion)
            {
                int from = data.SchemaVersion;
                if (_byFromVersion.TryGetValue(from, out var migration))
                {
                    migration.Apply(data);
                    if (data.SchemaVersion <= from)
                    {
                        Debug.LogError($"[Anlu.Save] La migración desde v{from} no avanzó la versión; se corta la cadena para evitar un loop.");
                        data.SchemaVersion = CurrentVersion;
                    }
                }
                else
                {
                    // No hay paso explícito: el esquema es aditivo, se adopta la versión actual.
                    data.SchemaVersion = CurrentVersion;
                }
            }

            return data;
        }
    }
}
