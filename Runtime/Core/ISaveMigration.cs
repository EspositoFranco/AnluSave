namespace Anlu.Save
{
    /// <summary>
    /// Un paso de migración de esquema. Cada paso es una clase independiente: agregar una migración
    /// = agregar una clase, sin tocar el motor (Open/Closed). Reemplaza el <c>switch</c> hardcodeado.
    /// </summary>
    /// <typeparam name="T">Modelo de guardado a migrar.</typeparam>
    public interface ISaveMigration<in T> where T : IVersionedSave
    {
        /// <summary>Versión de esquema desde la que aplica este paso.</summary>
        int FromVersion { get; }

        /// <summary>
        /// Transforma <paramref name="data"/> al esquema siguiente y actualiza su
        /// <see cref="IVersionedSave.SchemaVersion"/> al valor destino.
        /// </summary>
        void Apply(T data);
    }
}
