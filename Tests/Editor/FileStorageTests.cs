using System.Text;
using System.Threading.Tasks;
using Anlu.Save.Storage;
using NUnit.Framework;

namespace Anlu.Save.Tests
{
    public class FileStorageTests
    {
        private const string Key = "unit_test_slot";
        private FileStorage _storage;

        [SetUp]
        public void SetUp() => _storage = new FileStorage("AnluSaveTests");

        [TearDown]
        public async Task TearDown() => await _storage.DeleteAsync(Key);

        [Test]
        public async Task SaveThenLoad_RoundTrips()
        {
            byte[] payload = Encoding.UTF8.GetBytes("hola mundo");
            await _storage.SaveAsync(Key, payload);

            byte[] loaded = await _storage.LoadAsync(Key);

            Assert.AreEqual(payload, loaded);
        }

        [Test]
        public async Task Exists_TrueAfterSave_FalseAfterDelete()
        {
            await _storage.SaveAsync(Key, new byte[] { 1, 2, 3 });
            Assert.IsTrue(await _storage.ExistsAsync(Key));

            await _storage.DeleteAsync(Key);
            Assert.IsFalse(await _storage.ExistsAsync(Key));
        }

        [Test]
        public async Task SecondSave_PromotesNewData_AndKeepsMainReadable()
        {
            await _storage.SaveAsync(Key, Encoding.UTF8.GetBytes("v1"));
            await _storage.SaveAsync(Key, Encoding.UTF8.GetBytes("v2"));

            byte[] loaded = await _storage.LoadAsync(Key);

            Assert.AreEqual("v2", Encoding.UTF8.GetString(loaded), "Load prioriza el principal, no el backup");
        }
    }
}
