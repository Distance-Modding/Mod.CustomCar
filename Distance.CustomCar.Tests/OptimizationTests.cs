using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace Distance.CustomCar.Tests
{
    [TestFixture]
    public class SectionFactoryTests
    {
        [Test]
        public void GetOrCreate_Factory_KeyDoesNotExist_CallsFactory()
        {
            var section = new Section();
            bool factoryCalled = false;

            var result = section.GetOrCreate("newKey", () =>
            {
                factoryCalled = true;
                return new Dictionary<string, object>();
            });

            Assert.IsTrue(factoryCalled);
            Assert.IsNotNull(result);
            Assert.IsTrue(section.ContainsKey("newKey"));
        }

        [Test]
        public void GetOrCreate_Factory_KeyExists_DoesNotCallFactory()
        {
            var section = new Section();
            var existing = new Dictionary<string, object> { ["nested"] = "value" };
            section["existingKey"] = existing;
            bool factoryCalled = false;

            var result = section.GetOrCreate("existingKey", () =>
            {
                factoryCalled = true;
                return new Dictionary<string, object>();
            });

            Assert.IsFalse(factoryCalled, "Factory should not be called when key already exists");
            Assert.AreSame(existing, result);
        }

        [Test]
        public void GetOrCreate_Factory_StoresAndReturnsCreatedValue()
        {
            var section = new Section();
            var result = section.GetOrCreate("myKey", () => new Dictionary<string, object>());

            Assert.IsNotNull(result);
            Assert.AreSame(result, section["myKey"]);
        }

        [Test]
        public void GetOrCreate_Factory_NullFactory_Throws()
        {
            var section = new Section();
            Assert.Throws<NullReferenceException>(() =>
                section.GetOrCreate<Dictionary<string, object>>("key", (Func<Dictionary<string, object>>)null));
        }
    }

    [TestFixture]
    public class DictionaryExtensionsFactoryTests
    {
        [Test]
        public void GetOrCreate_Extension_Factory_KeyDoesNotExist_CallsFactory()
        {
            var dict = new Dictionary<string, object>();
            bool factoryCalled = false;

            var result = dict.GetOrCreate("newKey", () =>
            {
                factoryCalled = true;
                return new Dictionary<string, object>();
            });

            Assert.IsTrue(factoryCalled);
            Assert.IsNotNull(result);
            Assert.IsTrue(dict.ContainsKey("newKey"));
        }

        [Test]
        public void GetOrCreate_Extension_Factory_KeyExists_DoesNotCallFactory()
        {
            var dict = new Dictionary<string, object>();
            var existing = new Dictionary<string, object> { ["nested"] = "value" };
            dict["existingKey"] = existing;
            bool factoryCalled = false;

            var result = dict.GetOrCreate("existingKey", () =>
            {
                factoryCalled = true;
                return new Dictionary<string, object>();
            });

            Assert.IsFalse(factoryCalled, "Factory should not be called when key already exists");
            Assert.AreSame(existing, result);
        }

        [Test]
        public void GetOrCreate_Extension_Factory_ReturnsExistingValue()
        {
            var dict = new Dictionary<string, object>();
            var existing = new Dictionary<string, object>();
            dict["key"] = existing;

            var result = dict.GetOrCreate("key", () => new Dictionary<string, object>());

            Assert.AreSame(existing, result);
        }
    }
}
