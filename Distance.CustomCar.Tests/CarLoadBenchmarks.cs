using Distance.CustomCar.Data.Car;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Distance.CustomCar.Tests
{
    [TestFixture]
    public class CarLoadBenchmarks
    {
        private string benchDir_;
        private const int FileCount = 100;

        [SetUp]
        public void Setup()
        {
            benchDir_ = Path.Combine(Path.GetTempPath(), "CarBench_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(benchDir_);

            byte[] content = new byte[1024];
            new Random(42).NextBytes(content);

            for (int i = 0; i < FileCount; i++)
            {
                File.WriteAllBytes(Path.Combine(benchDir_, $"car{i}"), content);
            }
        }

        [TearDown]
        public void Teardown()
        {
            if (Directory.Exists(benchDir_))
            {
                Directory.Delete(benchDir_, true);
            }
        }

        [Test]
        [Category("Benchmark")]
        public void Benchmark_HashComputation_100Files()
        {
            var sw = Stopwatch.StartNew();

            for (int i = 0; i < FileCount; i++)
            {
                string path = Path.Combine(benchDir_, $"car{i}");
                string hash = CarCache.GetFileSignature(path);
                Assert.IsNotEmpty(hash);
            }

            sw.Stop();
            double msPerFile = sw.Elapsed.TotalMilliseconds / FileCount;
            Console.WriteLine($"Hashed {FileCount} files in {sw.Elapsed.TotalMilliseconds:F1}ms ({msPerFile:F3}ms/file)");
        }

        [Test]
        [Category("Benchmark")]
        public void Benchmark_CacheChangeDetection_AllCached()
        {
            var cache = new CarCache();
            var hashes = new Dictionary<string, string>();

            for (int i = 0; i < FileCount; i++)
            {
                string path = Path.Combine(benchDir_, $"car{i}");
                string hash = CarCache.GetFileSignature(path);
                hashes[path] = hash;
                cache.UpdateFile(path, hash, new[] { $"Assets/Car{i}.prefab" });
            }

            var sw = Stopwatch.StartNew();
            int changedCount = 0;

            foreach (var kv in hashes)
            {
                if (cache.HasFileChanged(kv.Key, out _))
                    changedCount++;
            }

            sw.Stop();
            Assert.AreEqual(0, changedCount);
            Console.WriteLine($"Checked {FileCount} cached files in {sw.Elapsed.TotalMilliseconds:F1}ms ({sw.Elapsed.TotalMilliseconds / FileCount:F3}ms/file)");
        }

        [Test]
        [Category("Benchmark")]
        public void Benchmark_CacheChangeDetection_AllNew()
        {
            var cache = new CarCache();

            var sw = Stopwatch.StartNew();
            int changedCount = 0;

            for (int i = 0; i < FileCount; i++)
            {
                string path = Path.Combine(benchDir_, $"car{i}");
                if (cache.HasFileChanged(path, out string hash))
                {
                    changedCount++;
                    cache.UpdateFile(path, hash, new[] { $"Assets/Car{i}.prefab" });
                }
            }

            sw.Stop();
            Assert.AreEqual(FileCount, changedCount);
            Console.WriteLine($"Detected & cached {FileCount} new files in {sw.Elapsed.TotalMilliseconds:F1}ms ({sw.Elapsed.TotalMilliseconds / FileCount:F3}ms/file)");
        }

        [Test]
        [Category("Benchmark")]
        public void Benchmark_CombinedHash_100Files()
        {
            var hashes = new Dictionary<string, string>();
            for (int i = 0; i < FileCount; i++)
            {
                string path = Path.Combine(benchDir_, $"car{i}");
                hashes[path] = CarCache.GetFileSignature(path);
            }

            var sw = Stopwatch.StartNew();

            string combined = CarCache.ComputeCombinedHash(hashes);

            sw.Stop();
            Assert.IsNotEmpty(combined);
            Console.WriteLine($"Combined hash of {FileCount} entries in {sw.Elapsed.TotalMilliseconds:F3}ms");
        }

        [Test]
        [Category("Benchmark")]
        public void Benchmark_CacheSaveAndLoad_100Entries()
        {
            var cache = new CarCache();
            for (int i = 0; i < FileCount; i++)
            {
                cache.UpdateFile(
                    Path.Combine(benchDir_, $"car{i}"),
                    $"hash{i:D64}",
                    new[] { $"Assets/Car{i}.prefab" }
                );
            }

            var saveSw = Stopwatch.StartNew();
            cache.Save();
            saveSw.Stop();

            var loadSw = Stopwatch.StartNew();
            var loaded = new CarCache();
            loaded.Load();
            loadSw.Stop();

            Assert.AreEqual(FileCount, loaded.Files.Count);
            Console.WriteLine($"Saved {FileCount} entries in {saveSw.Elapsed.TotalMilliseconds:F3}ms, loaded in {loadSw.Elapsed.TotalMilliseconds:F3}ms");
        }
    }
}
