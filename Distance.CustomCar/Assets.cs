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
                Mod.Log.LogError($"Couldn't find requested asset bundle at {FilePath}");
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
                Mod.Log.LogError($"Could not find requested asset bundle at {filePath}");
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
                return AssetBundleBridge.LoadFrom(FilePath);
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
        private static Type _assetBundleType;
        private static MethodInfo _loadFromFile;

        static AssetBundleBridge()
        {
            _assetBundleType = Kernel.FindTypeByFullName("UnityEngine.AssetBundle", "UnityEngine");
            _loadFromFile = _assetBundleType.GetMethod("LoadFromFile", new[] { typeof(string) });
        }

        public static Type AssetBundleType => _assetBundleType;

        private static MethodInfo LoadFromFile => _loadFromFile;

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

            Mod.Log.LogError($"Type {fullName} wasn't found in the main AppDomain at this moment.");
            throw new Exception($"Type {fullName} wasn't found in the main AppDomain at this moment.");
        }
    }
}
