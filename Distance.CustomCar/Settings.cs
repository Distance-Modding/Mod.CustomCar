using Newtonsoft.Json;
using Formatting = Newtonsoft.Json.Formatting;
using System;
using System.IO;
using System.Reflection;

//WAAAAHHH I GET AN ERROR WAAAH!!!

namespace Distance.CustomCar
{
    public class Settings : Section
    {
        private string FileName { get; }
        private string RootDirectory { get; }
        private string SettingsDirectory => Path.Combine(RootDirectory, "Settings");
        private string FilePath => Path.Combine(SettingsDirectory, FileName);

        public Settings(string fileName)
        {
            RootDirectory = Path.GetDirectoryName(Assembly.GetCallingAssembly().Location);

            FileName = $"{fileName}.json";

            Mod.Log.LogInfo($"Settings instance for '{FilePath}' initializing...");

            if (File.Exists(FilePath))
            {
                using (var sr = new StreamReader(FilePath))
                {
                    var json = sr.ReadToEnd();
                    Section sec = null;

                    try
                    {
                        sec = JsonConvert.DeserializeObject<Section>(json);
                    }
                    catch (JsonException je)
                    {
                        Mod.Log.LogInfo(je);
                    }
                    catch (Exception e)
                    {
                        Mod.Log.LogInfo(e);
                    }

                    Mod.Log.LogInfo("Just Deserialized the json");

                    if (sec != null)
                    {
                        foreach (var k in sec.Keys)
                            Add(k, sec[k]);
                    }
                }
            }

            Dirty = false;
        }

        public void Save(bool formatJson = true)
        {
            if (!Directory.Exists(SettingsDirectory))
                Directory.CreateDirectory(SettingsDirectory);

            try
            {
                using (var sw = new StreamWriter(FilePath, false))
                {
                    sw.WriteLine(JsonConvert.SerializeObject(this, Formatting.Indented));
                }

                Dirty = false;
            }
            catch (JsonException je)
            {
                Mod.Log.LogInfo(je);
            }
            catch (Exception e)
            {
                Mod.Log.LogInfo(e);
            }
        }

        public void SaveIfDirty(bool formatJson = true)
        {
            if (Dirty)
                Save(formatJson);
        }
    }
}
