namespace Anlu.Save
{
    /// <summary>
    /// Contrato mínimo del documento de guardado: expone su versión de esquema para que la cadena
    /// de migraciones sepa desde dónde migrar. Implementalo en TU modelo de save.
    /// </summary>
    public interface IVersionedSave
    {
        /// <summary>Versión de esquema con la que se serializó este documento.</summary>
        int SchemaVersion { get; set; }
    }
}
