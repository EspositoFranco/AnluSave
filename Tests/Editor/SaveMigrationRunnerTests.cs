using NUnit.Framework;

namespace Anlu.Save.Tests
{
    public class SaveMigrationRunnerTests
    {
        private class TestSave : IVersionedSave
        {
            public int SchemaVersion { get; set; }
            public int Value;
        }

        private class DoubleValueV1ToV2 : ISaveMigration<TestSave>
        {
            public int FromVersion => 1;
            public void Apply(TestSave data)
            {
                data.Value *= 2;
                data.SchemaVersion = 2;
            }
        }

        [Test]
        public void Migrate_AppliesChain_UpToCurrentVersion()
        {
            var runner = new SaveMigrationRunner<TestSave>(2, new[] { new DoubleValueV1ToV2() });
            var data = new TestSave { SchemaVersion = 1, Value = 5 };

            var result = runner.Migrate(data);

            Assert.AreEqual(2, result.SchemaVersion);
            Assert.AreEqual(10, result.Value);
        }

        [Test]
        public void Migrate_NoStepForVersion_ClampsToCurrent()
        {
            var runner = new SaveMigrationRunner<TestSave>(3);
            var data = new TestSave { SchemaVersion = 0, Value = 1 };

            var result = runner.Migrate(data);

            Assert.AreEqual(3, result.SchemaVersion);
            Assert.AreEqual(1, result.Value, "sin migración explícita el esquema es aditivo; no se toca el valor");
        }

        [Test]
        public void Migrate_NewerThanCurrent_ClampsDown()
        {
            var runner = new SaveMigrationRunner<TestSave>(1);
            var data = new TestSave { SchemaVersion = 99 };

            var result = runner.Migrate(data);

            Assert.AreEqual(1, result.SchemaVersion);
        }
    }
}
