using System;
using System.Text;
using System.Threading.Tasks;
using Anlu.Save.Policy;
using Anlu.Save.Serialization;
using Anlu.Save.Storage;
using UnityEngine;

namespace Anlu.Save
{
    /// <summary>
    /// Motor de guardado genérico: orquesta serializador + storage + migraciones + política de
    /// escritura. Es un POCO (no MonoBehaviour) para ser testeable; el ciclo de vida de escena lo
    /// aporta <see cref="SaveFlushBehaviour"/>. Construcción por DI manual (estilo Anlu).
    /// </summary>
    /// <typeparam name="T">Documento de guardado del juego. Serializable, versionado y con ctor vacío.</typeparam>
    public sealed class SaveService<T> : ISaveService<T> where T : class, IVersionedSave, new()
    {
        private readonly ISaveStorage _storage;
        private readonly ISerializer _serializer;
        private readonly SaveMigrationRunner<T> _migrations;
        private readonly WritePolicy _policy;
        private readonly string _key;

        private bool _isFlushing;

        /// <inheritdoc />
        public T Data { get; private set; }

        /// <inheritdoc />
        public bool IsLoaded { get; private set; }

        /// <inheritdoc />
        public event Action<T> Loaded;

        /// <inheritdoc />
        public event Action<T> Saved;

        /// <summary>Crea el servicio. Todo se inyecta: storage, serializador, migraciones y debounce.</summary>
        public SaveService(
            ISaveStorage storage,
            ISerializer serializer,
            SaveMigrationRunner<T> migrations,
            string key = null,
            float debounceSeconds = 1f)
        {
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            _migrations = migrations ?? throw new ArgumentNullException(nameof(migrations));
            _key = string.IsNullOrEmpty(key) ? SaveKeys.DefaultSlot : key;
            _policy = new WritePolicy(debounceSeconds);
        }

        /// <inheritdoc />
        public async Task<T> LoadAsync()
        {
            T loaded = null;

            if (await _storage.ExistsAsync(_key))
            {
                try
                {
                    byte[] bytes = await _storage.LoadAsync(_key);
                    if (bytes != null && bytes.Length > 0)
                        loaded = _serializer.Deserialize<T>(Encoding.UTF8.GetString(bytes));
                }
                catch (Exception e)
                {
                    Debug.LogError($"[Anlu.Save] Save corrupto en '{_key}', se usa default. {e.Message}");
                    loaded = null;
                }
            }

            loaded ??= new T();
            Data = _migrations.Migrate(loaded);
            IsLoaded = true;
            _policy.Clear();
            Loaded?.Invoke(Data);
            return Data;
        }

        /// <inheritdoc />
        public void MarkDirty() => _policy.MarkDirty();

        /// <inheritdoc />
        public void Tick(float deltaTime)
        {
            if (_isFlushing) return;
            if (_policy.Tick(deltaTime)) FlushFireAndForget();
        }

        /// <inheritdoc />
        public async Task FlushAsync()
        {
            if (Data == null || _isFlushing) return;

            _isFlushing = true;
            try
            {
                byte[] bytes = Encoding.UTF8.GetBytes(_serializer.Serialize(Data));
                await _storage.SaveAsync(_key, bytes);
                _policy.Clear();
                Saved?.Invoke(Data);
            }
            finally
            {
                _isFlushing = false;
            }
        }

        /// <inheritdoc />
        public async Task DeleteAsync()
        {
            await _storage.DeleteAsync(_key);
            Data = new T();
            _policy.Clear();
        }

        private async void FlushFireAndForget()
        {
            try { await FlushAsync(); }
            catch (Exception e) { Debug.LogError($"[Anlu.Save] Flush diferido falló: {e}"); }
        }
    }
}
