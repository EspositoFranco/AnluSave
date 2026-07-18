namespace Anlu.Save
{
    /// <summary>Constantes compartidas de la librería (sin magic strings dispersos por el código).</summary>
    public static class SaveKeys
    {
        /// <summary>Slot por defecto cuando el juego no especifica uno.</summary>
        public const string DefaultSlot = "save_main";

        /// <summary>Extensión del archivo de guardado principal (FileStorage).</summary>
        public const string SaveExtension = ".dat";

        /// <summary>Extensión del backup rotativo (FileStorage).</summary>
        public const string BackupExtension = ".bak";

        /// <summary>Extensión del temporal de escritura atómica (FileStorage).</summary>
        public const string TempExtension = ".tmp";
    }
}
