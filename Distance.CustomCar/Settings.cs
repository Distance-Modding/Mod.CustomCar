using JsonFx.Json;
using JsonFx.Serialization;
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
                bool saveLater = false;

                using (var sr = new StreamReader(FilePath))
                {
                    string json = sr.ReadToEnd();
                    JsonReader reader = new JsonReader();
                    Section sec = null;

                    try
                    {
                        sec = reader.Read<Section>(json);
                    }
                    catch (Exception e)
                    {
                        Mod.Log.LogWarning(e);
                        saveLater = true;
                    }

                    if (sec != null)
                    {
                        foreach (string k in sec.Keys)
                        {
                            Add(k, sec[k]);
                        }
                    }
                }

                if (saveLater)
                {
                    Save();
                }
            }
        }

        public void Save(bool formatJson = true)
        {
            if (!Directory.Exists(SettingsDirectory))
                Directory.CreateDirectory(SettingsDirectory);

            DataWriterSettings st = new DataWriterSettings { PrettyPrint = formatJson };
            JsonWriter writer = new JsonWriter(st);

            try
            {
                using (var sw = new StreamWriter(FilePath, false))
                {
                    sw.WriteLine(writer.Write(this));
                }
            }
            catch (Exception e)
            {
                Mod.Log.LogWarning(e);
            }
        }
    }
}
