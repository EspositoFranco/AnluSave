using Anlu.Save.Policy;
using NUnit.Framework;

namespace Anlu.Save.Tests
{
    public class WritePolicyTests
    {
        [Test]
        public void Tick_WhenNotDirty_NeverFires()
        {
            var policy = new WritePolicy(0.5f);
            Assert.IsFalse(policy.Tick(1f));
        }

        [Test]
        public void Tick_FiresAfterDebounceElapses()
        {
            var policy = new WritePolicy(0.5f);
            policy.MarkDirty();

            Assert.IsFalse(policy.Tick(0.2f));
            Assert.IsFalse(policy.Tick(0.2f));
            Assert.IsTrue(policy.Tick(0.2f), "0.6s acumulados >= 0.5s de debounce");
        }

        [Test]
        public void Clear_ResetsDirtyState()
        {
            var policy = new WritePolicy(0.1f);
            policy.MarkDirty();
            policy.Clear();

            Assert.IsFalse(policy.IsDirty);
            Assert.IsFalse(policy.Tick(1f));
        }

        [Test]
        public void NegativeDebounce_IsClampedToZero()
        {
            var policy = new WritePolicy(-5f);
            Assert.AreEqual(0f, policy.DebounceSeconds);
        }
    }
}
