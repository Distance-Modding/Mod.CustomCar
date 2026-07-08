using JsonFx.Json;
using JsonFx.Serialization;
using System;
using System.Collections.Generic;
using System.IO;

namespace Distance.CustomCar.Data.Car
{
    public class PrefabIndex
    {
        private static readonly string IndexPath;

        static PrefabIndex()
        {
            string modDir = Path.GetDirectoryName(typeof(PrefabIndex).Assembly.Location);
            IndexPath = Path.Combine(modDir, Path.Combine("Settings", "prefab_index.json"));
        }

        public Dictionary<string, PrefabIndexEntry> Files { get; set; } = new Dictionary<string, PrefabIndexEntry>();

        public string[] GetPrefabNames(string filePath)
        {
            if (Files.TryGetValue(filePath, out PrefabIndexEntry entry) && entry.PrefabNames != null)
                return entry.PrefabNames;
            return null;
        }

        public void SetPrefabNames(string filePath, string[] prefabNames)
        {
            Files[filePath] = new PrefabIndexEntry
            {
                LastWriteTime = GetLastWriteSafe(filePath),
                PrefabNames = prefabNames ?? new string[0]
            };
        }

        public bool IsUpToDate(string filePath)
        {
            if (!Files.TryGetValue(filePath, out PrefabIndexEntry entry))
                return false;

            return entry.LastWriteTime == GetLastWriteSafe(filePath);
        }

        public void RemoveStaleEntries(HashSet<string> validPaths)
        {
            List<string> toRemove = new List<string>();
            foreach (string path in Files.Keys)
            {
                if (!validPaths.Contains(path))
                    toRemove.Add(path);
            }
            foreach (string path in toRemove)
                Files.Remove(path);
        }

        private static long GetLastWriteSafe(string path)
        {
            try
            {
                return File.GetLastWriteTimeUtc(path).Ticks;
            }
            catch
            {
                return 0;
            }
        }

        public void Load()
        {
            try
            {
                if (!File.Exists(IndexPath))
                    return;

                string json = File.ReadAllText(IndexPath);
                var reader = new JsonReader();
                var data = reader.Read<Dictionary<string, object>>(json);
                if (data == null)
                    return;

                Files.Clear();
                foreach (var kv in data)
                {
                    if (kv.Value is Dictionary<string, object> entry)
                    {
                        var prefabIndexEntry = new PrefabIndexEntry();
                        if (entry.TryGetValue("LastWriteTime", out object lwt))
                            prefabIndexEntry.LastWriteTime = Convert.ToInt64(lwt);
                        if (entry.TryGetValue("PrefabNames", out object names) && names is List<object> nameList)
                        {
                            prefabIndexEntry.PrefabNames = nameList.ConvertAll(x => x?.ToString() ?? string.Empty).ToArray();
                        }
                        Files[kv.Key] = prefabIndexEntry;
                    }
                }
            }
            catch (Exception ex)
            {
                Mod.Log.LogWarning($"Failed to load prefab index: {ex.Message}");
                Files.Clear();
            }
        }

        public void Save()
        {
            try
            {
                var data = new Dictionary<string, object>();
                foreach (var kv in Files)
                {
                    var entry = new Dictionary<string, object>
                    {
                        ["LastWriteTime"] = kv.Value.LastWriteTime,
                        ["PrefabNames"] = new List<object>(kv.Value.PrefabNames ?? new string[0])
                    };
                    data[kv.Key] = entry;
                }

                string dir = Path.GetDirectoryName(IndexPath);
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                var writer = new JsonWriter(new DataWriterSettings { PrettyPrint = true });
                File.WriteAllText(IndexPath, writer.Write(data));
            }
            catch (Exception ex)
            {
                Mod.Log.LogWarning($"Failed to save prefab index: {ex.Message}");
            }
        }
    }

    public class PrefabIndexEntry
    {
        public long LastWriteTime { get; set; }
        public string[] PrefabNames { get; set; } = new string[0];
    }
}
