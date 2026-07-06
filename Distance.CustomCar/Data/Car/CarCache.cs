using JsonFx.Json;
using JsonFx.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Distance.CustomCar.Data.Car
{
    public class CarCache
    {
        private static readonly string CachePath;

        static CarCache()
        {
            string modDir = Path.GetDirectoryName(typeof(CarCache).Assembly.Location);
            string cacheDir = Path.Combine(modDir, "Settings");
            CachePath = Path.Combine(cacheDir, "car_cache.json");
        }

        public int Version { get; set; } = 1;
        public string CombinedHash { get; set; } = string.Empty;
        public Dictionary<string, CarCacheEntry> Files { get; set; } = new Dictionary<string, CarCacheEntry>();

        public static string GetFileSignature(string filePath)
        {
            if (!File.Exists(filePath))
                return string.Empty;

            var info = new FileInfo(filePath);
            return $"{info.LastWriteTimeUtc.Ticks}|{info.Length}";
        }

        public static string ComputeCombinedHash(Dictionary<string, string> fileSignatures)
        {
            var ordered = fileSignatures.OrderBy(kv => kv.Key, StringComparer.OrdinalIgnoreCase);
            var combined = new StringBuilder();
            foreach (var kv in ordered)
            {
                combined.Append(kv.Key);
                combined.Append('=');
                combined.Append(kv.Value);
                combined.Append('|');
            }
            return combined.ToString();
        }

        public void Load()
        {
            try
            {
                if (!File.Exists(CachePath))
                {
                    Mod.Log.LogInfo($"No car cache file at {CachePath}");
                    return;
                }

                string json = File.ReadAllText(CachePath);
                var reader = new JsonReader();
                var data = reader.Read<Dictionary<string, object>>(json);

                if (data == null || data.Count == 0)
                    return;

                if (data.TryGetValue("Version", out object versionVal) && versionVal != null)
                    Version = Convert.ToInt32(versionVal);

                if (data.TryGetValue("CombinedHash", out object combinedHashVal) && combinedHashVal != null)
                    CombinedHash = combinedHashVal.ToString();

                if (data.TryGetValue("Files", out object filesVal) && filesVal is Dictionary<string, object> filesDict)
                {
                    foreach (var kv in filesDict)
                    {
                        if (kv.Value is Dictionary<string, object> entryDict)
                        {
                            var entry = new CarCacheEntry();
                            if (entryDict.TryGetValue("Hash", out object hashVal) && hashVal != null)
                                entry.Hash = hashVal.ToString();
                            if (entryDict.TryGetValue("PrefabNames", out object prefabsVal) && prefabsVal is List<object> prefabList)
                            {
                                entry.PrefabNames = prefabList.ConvertAll(x => x?.ToString() ?? string.Empty).ToArray();
                            }
                            if (entryDict.TryGetValue("LastModified", out object modVal) && modVal != null)
                            {
                                DateTime parsed;
                                DateTime.TryParse(modVal.ToString(), out parsed);
                                entry.LastModified = parsed;
                            }
                            Files[kv.Key] = entry;
                        }
                    }
                }

                Mod.Log.LogInfo($"Loaded car cache from {CachePath} ({Files.Count} file(s))");
            }
            catch (Exception ex)
            {
                Mod.Log.LogWarning($"Failed to load car cache: {ex.Message}");
                Files.Clear();
            }
        }

        public void Save()
        {
            try
            {
                var filesDict = new Dictionary<string, object>();
                foreach (var kv in Files)
                {
                    var entryDict = new Dictionary<string, object>
                    {
                        ["Hash"] = kv.Value.Hash,
                        ["PrefabNames"] = new List<object>(kv.Value.PrefabNames ?? new string[0]),
                        ["LastModified"] = kv.Value.LastModified.ToString("O")
                    };
                    filesDict[kv.Key] = entryDict;
                }

                var data = new Dictionary<string, object>
                {
                    ["Version"] = Version,
                    ["CombinedHash"] = CombinedHash,
                    ["Files"] = filesDict
                };

                string cacheDir = Path.GetDirectoryName(CachePath);
                if (!Directory.Exists(cacheDir))
                    Directory.CreateDirectory(cacheDir);

                var writer = new JsonWriter(new DataWriterSettings { PrettyPrint = true });
                string json = writer.Write(data);
                File.WriteAllText(CachePath, json);

                Mod.Log.LogInfo($"Saved car cache to {CachePath} ({Files.Count} file(s))");
            }
            catch (Exception ex)
            {
                Mod.Log.LogWarning($"Failed to save car cache: {ex.Message}");
            }
        }

        public bool HasFileChanged(string filePath, out string currentSignature)
        {
            currentSignature = GetFileSignature(filePath);

            if (string.IsNullOrEmpty(currentSignature))
                return true;

            if (!Files.TryGetValue(filePath, out CarCacheEntry entry))
                return true;

            return entry.Hash != currentSignature;
        }

        public void UpdateFile(string filePath, string signature, string[] prefabNames)
        {
            Files[filePath] = new CarCacheEntry
            {
                Hash = signature,
                PrefabNames = prefabNames ?? new string[0],
                LastModified = DateTime.UtcNow
            };
        }

        public void RemoveFile(string filePath)
        {
            Files.Remove(filePath);
        }

        public string[] GetPrefabNames(string filePath)
        {
            if (Files.TryGetValue(filePath, out CarCacheEntry entry) && entry.PrefabNames != null)
                return entry.PrefabNames;
            return new string[0];
        }
    }

    public class CarCacheEntry
    {
        public string Hash { get; set; } = string.Empty;
        public string[] PrefabNames { get; set; } = new string[0];
        public DateTime LastModified { get; set; }
    }
}
