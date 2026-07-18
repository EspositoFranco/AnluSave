namespace Anlu.Save.Storage
{
    /// <summary>
    /// Provee las claves para <see cref="EncryptedStorage"/>. El consumidor la implementa: la
    /// librería nunca hardcodea claves (eso sería seguridad de cartón).
    /// </summary>
    public interface IEncryptionKeyProvider
    {
        /// <summary>Clave AES-256 (exactamente 32 bytes).</summary>
        byte[] GetEncryptionKey();

        /// <summary>Clave HMAC-SHA256 (recomendado 32 bytes), distinta de la de encriptación.</summary>
        byte[] GetMacKey();
    }
}
