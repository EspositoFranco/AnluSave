using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Anlu.Save.Storage;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Anlu.Save.Tests
{
    public class EncryptedStorageTests
    {
        private class FakeKeys : IEncryptionKeyProvider
        {
            private readonly byte[] _enc = new byte[32];
            private readonly byte[] _mac = new byte[32];

            public FakeKeys()
            {
                for (int i = 0; i < 32; i++)
                {
                    _enc[i] = (byte)i;
                    _mac[i] = (byte)(255 - i);
                }
            }

            public byte[] GetEncryptionKey() => _enc;
            public byte[] GetMacKey() => _mac;
        }

        private class MemoryStorage : ISaveStorage
        {
            public byte[] Blob;
            public Task<bool> ExistsAsync(string key) => Task.FromResult(Blob != null);
            public Task<byte[]> LoadAsync(string key) => Task.FromResult(Blob);
            public Task SaveAsync(string key, byte[] data) { Blob = data; return Task.CompletedTask; }
            public Task DeleteAsync(string key) { Blob = null; return Task.CompletedTask; }
        }

        [Test]
        public async Task Encrypt_RoundTrips()
        {
            var inner = new MemoryStorage();
            var storage = new EncryptedStorage(inner, new FakeKeys());
            byte[] payload = Encoding.UTF8.GetBytes("{\"gold\":100}");

            await storage.SaveAsync("k", payload);
            byte[] loaded = await storage.LoadAsync("k");

            Assert.AreEqual(payload, loaded);
        }

        [Test]
        public async Task StoredBlob_IsNotPlaintext()
        {
            var inner = new MemoryStorage();
            var storage = new EncryptedStorage(inner, new FakeKeys());

            await storage.SaveAsync("k", Encoding.UTF8.GetBytes("SECRET_VALUE"));

            Assert.IsFalse(Encoding.UTF8.GetString(inner.Blob).Contains("SECRET_VALUE"));
        }

        [Test]
        public async Task TamperedBlob_IsRejected()
        {
            var inner = new MemoryStorage();
            var storage = new EncryptedStorage(inner, new FakeKeys());
            await storage.SaveAsync("k", Encoding.UTF8.GetBytes("data"));

            inner.Blob[inner.Blob.Length - 1] ^= 0xFF; // corromper el tag HMAC

            LogAssert.Expect(LogType.Error, new Regex("HMAC inválido"));
            byte[] loaded = await storage.LoadAsync("k");

            Assert.IsNull(loaded);
        }
    }
}
