using System;
using System.Collections.Generic;

namespace Anlu.Save.Samples
{
    /// <summary>
    /// Modelo de guardado de ejemplo. Implementa <see cref="IVersionedSave"/> y es
    /// <c>[Serializable]</c> para que JsonUtility lo entienda. El campo <c>schemaVersion</c> es el
    /// que se persiste; la propiedad de la interfaz mapea a él.
    /// </summary>
    [Serializable]
    public class PlayerSaveData : IVersionedSave
    {
        public int schemaVersion = 1;
        public int gold;
        public int level = 1;
        public List<string> unlockedItems = new();

        /// <inheritdoc />
        public int SchemaVersion { get => schemaVersion; set => schemaVersion = value; }
    }
}
