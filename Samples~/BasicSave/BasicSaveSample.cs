using System.Collections.Generic;
using Anlu.Save.Policy;
using Anlu.Save.Serialization;
using Anlu.Save.Storage;
using UnityEngine;

namespace Anlu.Save.Samples
{
    /// <summary>
    /// Ejemplo mínimo de wiring del motor de guardado (DI manual). Cargá al iniciar, marcá sucio al
    /// cambiar, y dejá que la política de escritura + <see cref="SaveFlushBehaviour"/> persistan solos.
    /// </summary>
    public sealed class BasicSaveSample : MonoBehaviour
    {
        [Tooltip("Segundos de calma tras un cambio antes de escribir a disco.")]
        [SerializeField, Min(0f)] private float _debounceSeconds = 1f;

        private ISaveService<PlayerSaveData> _save;

        private async void Awake()
        {
            // Storage: archivo con backup. Envolvé en EncryptedStorage(inner, keys) para anti-tampering.
            ISaveStorage storage = new FileStorage("Saves");

            // Serializador por defecto (JsonUtility). Cero dependencias.
            ISerializer serializer = new JsonUtilitySerializer(prettyPrint: true);

            // Cadena de migraciones (vacía en v1: el esquema actual es 1).
            var migrations = new SaveMigrationRunner<PlayerSaveData>(
                currentVersion: 1,
                migrations: new List<ISaveMigration<PlayerSaveData>>());

            _save = new SaveService<PlayerSaveData>(storage, serializer, migrations, key: "player", _debounceSeconds);

            // Ciclo de vida: flush en pausa/foco/quit + tick del debounce cada frame.
            gameObject.AddComponent<SaveFlushBehaviour>().Bind(_save);

            await _save.LoadAsync();
            Debug.Log($"[Sample] Cargado: gold={_save.Data.gold}, level={_save.Data.level}");
        }

        /// <summary>Ejemplo de mutación: sumá oro y marcá sucio. La escritura la agenda la política.</summary>
        public void AddGold(int amount)
        {
            if (_save == null || !_save.IsLoaded) return;

            _save.Data.gold += amount;
            _save.MarkDirty();
        }

        /// <summary>Flush inmediato (p. ej. al tocar "Guardar" en un menú).</summary>
        public async void SaveNow()
        {
            if (_save != null) await _save.FlushAsync();
        }
    }
}
