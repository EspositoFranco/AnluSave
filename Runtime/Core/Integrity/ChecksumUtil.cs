using System.Security.Cryptography;

namespace Anlu.Save.Integrity
{
    /// <summary>Utilidades de integridad basadas en HMAC-SHA256 (detección de manoseo y corrupción).</summary>
    public static class ChecksumUtil
    {
        /// <summary>Tamaño del tag HMAC-SHA256 en bytes.</summary>
        public const int HmacSize = 32;

        /// <summary>Calcula el HMAC-SHA256 de <paramref name="data"/> con <paramref name="key"/>.</summary>
        public static byte[] ComputeHmac(byte[] key, byte[] data)
        {
            using var hmac = new HMACSHA256(key);
            return hmac.ComputeHash(data);
        }

        /// <summary>Compara dos tags en tiempo constante (evita timing attacks).</summary>
        public static bool ConstantTimeEquals(byte[] a, byte[] b)
        {
            if (a == null || b == null || a.Length != b.Length) return false;

            int diff = 0;
            for (int i = 0; i < a.Length; i++) diff |= a[i] ^ b[i];
            return diff == 0;
        }
    }
}
