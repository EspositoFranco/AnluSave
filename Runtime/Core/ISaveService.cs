using System;
using System.Threading.Tasks;

namespace Anlu.Save
{
    /// <summary>
    /// Ciclo de vida de persistencia agnóstico del tipo de dato. Lo consumen los componentes de
    /// escena (p. ej. <c>SaveFlushBehaviour</c>) que no necesitan conocer el modelo concreto.
    /// </summary>
    public interface ISaveService
    {
        /// <summary>True una vez que <c>LoadAsync</c> completó al menos una vez.</summary>
        bool IsLoaded { get; }

        /// <summary>Agenda una escritura con debounce (ver <c>WritePolicy</c>).</summary>
        void MarkDirty();

        /// <summary>Avanza el debounce y dispara un flush diferido cuando corresponde. Llamar cada frame.</summary>
        void Tick(float deltaTime);

        /// <summary>Escribe el estado actual al storage de inmediato (quit, pérdida de foco, fin de run).</summary>
        Task FlushAsync();
    }

    /// <summary>
    /// Servicio de guardado genérico sobre el modelo <typeparamref name="T"/> del juego. La
    /// librería no conoce la forma de <typeparamref name="T"/>: vos se lo das. Así sirve para un
    /// tower-defense, un match-3, lo que sea.
    /// </summary>
    /// <typeparam name="T">Documento de guardado del juego. Serializable y versionado.</typeparam>
    public interface ISaveService<T> : ISaveService where T : class, IVersionedSave, new()
    {
        /// <summary>Datos vivos. Nunca null tras <see cref="LoadAsync"/> (se crea un default si no había).</summary>
        T Data { get; }

        /// <summary>Se dispara tras cargar y migrar los datos.</summary>
        event Action<T> Loaded;

        /// <summary>Se dispara tras cada escritura exitosa.</summary>
        event Action<T> Saved;

        /// <summary>Carga y migra el save. Crea un default si no existía o estaba corrupto.</summary>
        Task<T> LoadAsync();

        /// <summary>Borra el save persistido. <see cref="Data"/> se resetea a un default nuevo.</summary>
        Task DeleteAsync();
    }
}
