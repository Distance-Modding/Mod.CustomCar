using System;
using System.Collections.Generic;

namespace Distance.CustomCar
{
    public class Section : Dictionary<string, object>
    {

        public event EventHandler<SettingsChangedEventArgs> ValueChanged;

        public new object this[string key]
        {
            get
            {
                if (!ContainsKey(key))
                    return null;

                return base[key];
            }

            set
            {
                if (!ContainsKey(key))
                {
                    Add(key, value);
                    ValueChanged?.Invoke(this, new SettingsChangedEventArgs(key, null, base[key]));
                }
                else
                {
                    var oldValue = base[key];

                    base[key] = value;
                    ValueChanged?.Invoke(this, new SettingsChangedEventArgs(key, oldValue, base[key]));
                }
            }
        }

        public T GetItem<T>(string key)
        {
            if (!ContainsKey(key))
            {
                Mod.Log.LogError($"The key requested doesn't exist in store: '{key}'.");
                throw new KeyNotFoundException($"The key requested doesn't exist in store: '{key}'.");
            }


            try
            {
                return (T)Convert.ChangeType(this[key], typeof(T));
            }
            catch (Exception e)
            {
                Mod.Log.LogWarning($"Failed type conversion exception has been thrown. String: {key} \n{e}");
                throw new SettingsException($"Failed type conversion exception has been thrown.", key, false, e);
            }
        }

        public T GetOrCreate<T>(string key) where T : new()
        {
            if (!ContainsKey(key))
            {
                this[key] = new T();
                ValueChanged?.Invoke(this, new SettingsChangedEventArgs(key, null, this[key]));
            }

            return GetItem<T>(key);
        }

        public T GetOrCreate<T>(string key, T defaultValue)
        {
            if (!ContainsKey<T>(key))
            {
                this[key] = defaultValue;
                ValueChanged?.Invoke(this, new SettingsChangedEventArgs(key, null, this[key]));
            }

            //Mod.Log.LogInfo(T.GetType());

            return GetItem<T>(key);
        }

        public bool ContainsKey<T>(string key)
        {
            try
            {
                GetItem<T>(key);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
