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
        public Dictionary<string, string> InvalidFiles { get; set; } = new Dictionary<string, string>();

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

        public static bool HasValidBundleSignature(string filePath)
        {
            try
            {
                using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    if (stream.Length < 8)
                        return false;

                    byte[] header = new byte[8];
                    if (stream.Read(header, 0, 8) != 8)
                        return false;

                    if (header[0] == 'U' && header[1] == 'n' && header[2] == 'i' && header[3] == 't' &&
                        header[4] == 'y' && header[5] == 'F' && header[6] == 'S')
                        return true;

                    if (header[0] == 'U' && header[1] == 'n' && header[2] == 'i' && header[3] == 't' &&
                        header[4] == 'y' && header[5] == 'R' && header[6] == 'a' && header[7] == 'w')
                        return true;

                    if (header[0] == 'U' && header[1] == 'n' && header[2] == 'i' && header[3] == 't' &&
                        header[4] == 'y' && header[5] == 'W' && header[6] == 'e' && header[7] == 'b')
                        return true;

                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        public bool IsFileInvalid(string filePath, string currentSignature)
        {
            if (string.IsNullOrEmpty(currentSignature))
                return true;

            if (!InvalidFiles.TryGetValue(filePath, out string cachedSignature))
                return false;

            return cachedSignature == currentSignature;
        }

        public void MarkFileInvalid(string filePath, string signature)
        {
            InvalidFiles[filePath] = signature;
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

                if (data.TryGetValue("InvalidFiles", out object invalidVal) && invalidVal is Dictionary<string, object> invalidDict)
                {
                    foreach (var kv in invalidDict)
                    {
                        if (kv.Value != null)
                            InvalidFiles[kv.Key] = kv.Value.ToString();
                    }
                }

                Mod.Log.LogInfo($"Loaded car cache from {CachePath} ({Files.Count} file(s), {InvalidFiles.Count} invalid)");
            }
            catch (Exception ex)
            {
                Mod.Log.LogWarning($"Failed to load car cache: {ex.Message}");
                Files.Clear();
                InvalidFiles.Clear();
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

                var invalidDict = new Dictionary<string, object>();
                foreach (var kv in InvalidFiles)
                    invalidDict[kv.Key] = kv.Value;

                var data = new Dictionary<string, object>
                {
                    ["Version"] = Version,
                    ["CombinedHash"] = CombinedHash,
                    ["Files"] = filesDict,
                    ["InvalidFiles"] = invalidDict
                };

                string cacheDir = Path.GetDirectoryName(CachePath);
                if (!Directory.Exists(cacheDir))
                    Directory.CreateDirectory(cacheDir);

                var writer = new JsonWriter(new DataWriterSettings { PrettyPrint = true });
                string json = writer.Write(data);
                File.WriteAllText(CachePath, json);

                Mod.Log.LogInfo($"Saved car cache to {CachePath} ({Files.Count} file(s), {InvalidFiles.Count} invalid)");
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
