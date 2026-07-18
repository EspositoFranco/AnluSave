using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Anlu.Save.Integrity;
using UnityEngine;

namespace Anlu.Save.Storage
{
    /// <summary>
    /// Decorator que encripta (AES-256-CBC) y autentica (encrypt-then-MAC, HMAC-SHA256) el payload
    /// de cualquier <see cref="ISaveStorage"/> interno. Detecta manoseo y corrupción. Formato en
    /// disco: <c>[IV 16][cipher…][hmac 32]</c>.
    /// </summary>
    /// <remarks>No es DRM: frena al que edita el JSON con el bloc de notas, no a un atacante decidido
    /// (la clave viaja en el cliente). Para eso hace falta un backend server-authoritative.</remarks>
    public sealed class EncryptedStorage : ISaveStorage
    {
        private const int IvSize = 16;

        private readonly ISaveStorage _inner;
        private readonly IEncryptionKeyProvider _keys;

        /// <summary>Envuelve <paramref name="inner"/> agregando encriptación y autenticación.</summary>
        public EncryptedStorage(ISaveStorage inner, IEncryptionKeyProvider keys)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
            _keys = keys ?? throw new ArgumentNullException(nameof(keys));
        }

        /// <inheritdoc />
        public Task<bool> ExistsAsync(string key) => _inner.ExistsAsync(key);

        /// <inheritdoc />
        public async Task<byte[]> LoadAsync(string key)
        {
            byte[] blob = await _inner.LoadAsync(key);
            if (blob == null) return null;
            if (blob.Length < IvSize + ChecksumUtil.HmacSize)
            {
                Debug.LogError("[Anlu.Save] Payload encriptado demasiado corto; se descarta.");
                return null;
            }

            int cipherLen = blob.Length - IvSize - ChecksumUtil.HmacSize;
            var iv = new byte[IvSize];
            var cipher = new byte[cipherLen];
            var tag = new byte[ChecksumUtil.HmacSize];
            Buffer.BlockCopy(blob, 0, iv, 0, IvSize);
            Buffer.BlockCopy(blob, IvSize, cipher, 0, cipherLen);
            Buffer.BlockCopy(blob, IvSize + cipherLen, tag, 0, ChecksumUtil.HmacSize);

            // Verificar HMAC sobre IV+cipher ANTES de desencriptar (encrypt-then-MAC).
            var authenticated = new byte[IvSize + cipherLen];
            Buffer.BlockCopy(iv, 0, authenticated, 0, IvSize);
            Buffer.BlockCopy(cipher, 0, authenticated, IvSize, cipherLen);
            byte[] expected = ChecksumUtil.ComputeHmac(_keys.GetMacKey(), authenticated);
            if (!ChecksumUtil.ConstantTimeEquals(expected, tag))
            {
                Debug.LogError("[Anlu.Save] HMAC inválido: el save fue manoseado o está corrupto. Se descarta.");
                return null;
            }

            using var aes = CreateAes();
            aes.IV = iv;
            using var decryptor = aes.CreateDecryptor();
            return decryptor.TransformFinalBlock(cipher, 0, cipher.Length);
        }

        /// <inheritdoc />
        public Task SaveAsync(string key, byte[] data)
        {
            using var aes = CreateAes();
            aes.GenerateIV();
            byte[] iv = aes.IV;

            byte[] cipher;
            using (var encryptor = aes.CreateEncryptor())
                cipher = encryptor.TransformFinalBlock(data, 0, data.Length);

            var authenticated = new byte[IvSize + cipher.Length];
            Buffer.BlockCopy(iv, 0, authenticated, 0, IvSize);
            Buffer.BlockCopy(cipher, 0, authenticated, IvSize, cipher.Length);
            byte[] tag = ChecksumUtil.ComputeHmac(_keys.GetMacKey(), authenticated);

            var blob = new byte[authenticated.Length + tag.Length];
            Buffer.BlockCopy(authenticated, 0, blob, 0, authenticated.Length);
            Buffer.BlockCopy(tag, 0, blob, authenticated.Length, tag.Length);

            return _inner.SaveAsync(key, blob);
        }

        /// <inheritdoc />
        public Task DeleteAsync(string key) => _inner.DeleteAsync(key);

        private Aes CreateAes()
        {
            var aes = Aes.Create();
            aes.KeySize = 256;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.Key = _keys.GetEncryptionKey();
            return aes;
        }
    }
}
