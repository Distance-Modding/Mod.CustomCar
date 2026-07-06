using Distance.CustomCar.Data.Car;
using NUnit.Framework;
using System;
using System.IO;

namespace Distance.CustomCar.Tests
{
    [TestFixture]
    public class CarCacheTests
    {
        private string tempDir_;
        private string tempFileA_;
        private string tempFileB_;

        [SetUp]
        public void Setup()
        {
            tempDir_ = Path.Combine(Path.GetTempPath(), "CarCacheTests_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempDir_);

            tempFileA_ = Path.Combine(tempDir_, "carA");
            File.WriteAllBytes(tempFileA_, new byte[] { 0, 1, 2, 3, 4, 5 });

            tempFileB_ = Path.Combine(tempDir_, "carB");
            File.WriteAllBytes(tempFileB_, new byte[] { 5, 4, 3, 2, 1, 0 });
        }

        [TearDown]
        public void Teardown()
        {
            if (Directory.Exists(tempDir_))
            {
                Directory.Delete(tempDir_, true);
            }
        }

        [Test]
        public void ComputeFileHash_SameFile_ProducesSameHash()
        {
            string hash1 = CarCache.GetFileSignature(tempFileA_);
            string hash2 = CarCache.GetFileSignature(tempFileA_);

            Assert.AreEqual(hash1, hash2);
            Assert.IsNotEmpty(hash1);
        }

        [Test]
        public void ComputeFileHash_DifferentFiles_ProducesDifferentHash()
        {
            string hashA = CarCache.GetFileSignature(tempFileA_);
            string hashB = CarCache.GetFileSignature(tempFileB_);

            Assert.AreNotEqual(hashA, hashB);
        }

        [Test]
        public void ComputeFileHash_NonexistentFile_ReturnsEmpty()
        {
            string hash = CarCache.GetFileSignature(Path.Combine(tempDir_, "nonexistent"));
            Assert.IsEmpty(hash);
        }

        [Test]
        public void ComputeFileHash_ChangedFile_ProducesDifferentHash()
        {
            string before = CarCache.GetFileSignature(tempFileA_);
            File.WriteAllBytes(tempFileA_, new byte[] { 9, 9, 9, 9 });
            string after = CarCache.GetFileSignature(tempFileA_);

            Assert.AreNotEqual(before, after);
        }

        [Test]
        public void ComputeCombinedHash_IdenticalInputs_ProducesSameHash()
        {
            var dict1 = new System.Collections.Generic.Dictionary<string, string>
            {
                ["fileA"] = "hashA",
                ["fileB"] = "hashB"
            };
            var dict2 = new System.Collections.Generic.Dictionary<string, string>
            {
                ["fileA"] = "hashA",
                ["fileB"] = "hashB"
            };

            string combined1 = CarCache.ComputeCombinedHash(dict1);
            string combined2 = CarCache.ComputeCombinedHash(dict2);

            Assert.AreEqual(combined1, combined2);
        }

        [Test]
        public void ComputeCombinedHash_DifferentInputs_ProducesDifferentHash()
        {
            var dict1 = new System.Collections.Generic.Dictionary<string, string>
            {
                ["fileA"] = "hashA"
            };
            var dict2 = new System.Collections.Generic.Dictionary<string, string>
            {
                ["fileA"] = "hashB"
            };

            string combined1 = CarCache.ComputeCombinedHash(dict1);
            string combined2 = CarCache.ComputeCombinedHash(dict2);

            Assert.AreNotEqual(combined1, combined2);
        }

        [Test]
        public void ComputeCombinedHash_KeyOrdering_IsDeterministic()
        {
            var dict1 = new System.Collections.Generic.Dictionary<string, string>
            {
                ["z_file"] = "hashZ",
                ["a_file"] = "hashA"
            };
            var dict2 = new System.Collections.Generic.Dictionary<string, string>
            {
                ["a_file"] = "hashA",
                ["z_file"] = "hashZ"
            };

            string combined1 = CarCache.ComputeCombinedHash(dict1);
            string combined2 = CarCache.ComputeCombinedHash(dict2);

            Assert.AreEqual(combined1, combined2);
        }

        [Test]
        public void HasFileChanged_NewFile_ReturnsTrue()
        {
            var cache = new CarCache();
            bool changed = cache.HasFileChanged(tempFileA_, out string hash);

            Assert.IsTrue(changed);
            Assert.IsNotEmpty(hash);
        }

        [Test]
        public void HasFileChanged_CachedFile_ReturnsFalse()
        {
            var cache = new CarCache();
            string hash = CarCache.GetFileSignature(tempFileA_);
            cache.UpdateFile(tempFileA_, hash, new[] { "prefab1" });

            bool changed = cache.HasFileChanged(tempFileA_, out _);
            Assert.IsFalse(changed);
        }

        [Test]
        public void HasFileChanged_ModifiedFile_ReturnsTrue()
        {
            var cache = new CarCache();
            string hash = CarCache.GetFileSignature(tempFileA_);
            cache.UpdateFile(tempFileA_, hash, new[] { "prefab1" });

            File.WriteAllBytes(tempFileA_, new byte[] { 9, 9, 9 });

            bool changed = cache.HasFileChanged(tempFileA_, out _);
            Assert.IsTrue(changed);
        }

        [Test]
        public void UpdateFile_StoresAndRetrievesPrefabNames()
        {
            var cache = new CarCache();
            string hash = CarCache.GetFileSignature(tempFileA_);
            string[] prefabs = new[] { "Assets/Car.prefab", "Assets/Extra.prefab" };

            cache.UpdateFile(tempFileA_, hash, prefabs);
            string[] retrieved = cache.GetPrefabNames(tempFileA_);

            Assert.AreEqual(prefabs.Length, retrieved.Length);
            Assert.Contains("Assets/Car.prefab", retrieved);
            Assert.Contains("Assets/Extra.prefab", retrieved);
        }

        [Test]
        public void RemoveFile_RemovesEntry()
        {
            var cache = new CarCache();
            string hash = CarCache.GetFileSignature(tempFileA_);
            cache.UpdateFile(tempFileA_, hash, new[] { "prefab1" });
            Assert.IsNotEmpty(cache.GetPrefabNames(tempFileA_));

            cache.RemoveFile(tempFileA_);
            Assert.IsEmpty(cache.GetPrefabNames(tempFileA_));
        }

        [Test]
        public void GetPrefabNames_UnknownFile_ReturnsEmpty()
        {
            var cache = new CarCache();
            string[] names = cache.GetPrefabNames("nonexistent_path");

            Assert.IsEmpty(names);
        }

        [Test]
        public void SaveAndLoad_Roundtrip_PreservesAllEntries()
        {
            var cache = new CarCache();
            string hash = CarCache.GetFileSignature(tempFileA_);
            cache.UpdateFile(tempFileA_, hash, new[] { "Assets/Car.prefab" });
            cache.CombinedHash = "test_combined_hash";

            cache.Save();

            var loaded = new CarCache();
            loaded.Load();

            Assert.AreEqual(cache.CombinedHash, loaded.CombinedHash);
            Assert.IsTrue(loaded.Files.ContainsKey(tempFileA_));
            Assert.AreEqual(hash, loaded.Files[tempFileA_].Hash);
        }

        [Test]
        public void Load_FromNonExistentCache_DoesNotThrow()
        {
            var cache = new CarCache();
            Assert.DoesNotThrow(() => cache.Load());
            Assert.IsEmpty(cache.Files);
        }

        [Test]
        public void Save_WithNoFiles_DoesNotThrow()
        {
            var cache = new CarCache();
            Assert.DoesNotThrow(() => cache.Save());
        }
    }
}
