using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Distance.CustomCar
{
    public class Assets
    {
        private string _filePath = null;

        private string RootDirectory { get; }
        private string FileName { get; set; }
        private string FilePath => _filePath ?? Path.Combine(Path.Combine(RootDirectory, "Assets"), FileName);

        public object Bundle { get; private set; }

        private Assets() { }

        /// <summary>
        /// Attempts to construct a Unity AssetBundle via a Centrifuge Type Bridge. 
        /// You will have to cast the Bundle property to Unity's AssetBundle type for usage.
        /// </summary>
        /// <param name="fileName">Filename/path relative to mod's private asset directory.</param>
        public Assets(string fileName)
        {
            RootDirectory = Path.GetDirectoryName(Assembly.GetCallingAssembly().Location);
            FileName = fileName;

            if (!File.Exists(FilePath))
            {
                Mod.Log.LogInfo($"Couldn't find requested asset bundle at {FilePath}");
                return;
            }

            Bundle = Load();
        }

        /// <summary>
        /// Attempts to construct a Unity AssetBundle via a Centrifuge Type Bridge.
        /// You will have to cast the Bundle property to Unity's AssetBundle type for usage.
        /// </summary>
        /// <param name="filePath">An absolute path to the AssetBundle</param>
        public static Assets FromUnsafePath(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Mod.Log.LogInfo($"Could not find requested asset bundle at {filePath}");
                return null;
            }

            var ret = new Assets
            {
                _filePath = filePath,
                FileName = Path.GetFileName(filePath)
            };
            ret.Bundle = ret.Load();

            if (ret.Bundle == null)
                return null;

            return ret;
        }

        private object Load()
        {
            try
            {
                var assetBundle = AssetBundleBridge.LoadFrom(FilePath);
                Mod.Log.LogInfo($"Loaded asset bundle {FilePath}");

                return assetBundle;
            }
            catch (Exception ex)
            {
                Mod.Log.LogInfo(ex);
                return null;
            }
        }
    }

    internal static class AssetBundleBridge
    {
        public static Type AssetBundleType => Kernel.FindTypeByFullName(
            "UnityEngine.AssetBundle",
            "UnityEngine"
        );

        private static MethodInfo LoadFromFile => AssetBundleType.GetMethod(
            "LoadFromFile",
            new[] { typeof(string) }
        );

        public static object LoadFrom(string path)
        {
            return LoadFromFile.Invoke(null, new[] { path });
        }
    }

    internal static class Kernel
    {
        internal static Type FindTypeByFullName(string fullName, string assemblyFilter)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                                                    .Where(a => a.GetName().Name.Contains(assemblyFilter));

            foreach (var asm in assemblies)
            {
                var type = asm.GetTypes().FirstOrDefault(t => t.FullName == fullName);

                if (type == null)
                    continue;

                return type;
            }

            Mod.Log.LogInfo($"Type {fullName} wasn't found in the main AppDomain at this moment.");
            throw new Exception($"Type {fullName} wasn't found in the main AppDomain at this moment.");
        }
    }
}
