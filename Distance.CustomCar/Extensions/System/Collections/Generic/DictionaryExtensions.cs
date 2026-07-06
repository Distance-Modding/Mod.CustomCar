#pragma warning disable RCS1110
using System;
using System.Collections.Generic;

public static class DictionaryExtensions
{
    public static bool ContainsKey<T>(this Dictionary<string, object> obj, string key)
    {
        try
        {
            obj.GetItem<T>(key);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static T GetItem<T>(this Dictionary<string, object> obj, string key)
    {
        if (!obj.ContainsKey(key))
        {
            throw new KeyNotFoundException($"The key requested doesn't exist in store: '{key}'.");
        }

        try
        {
            return (T)System.Convert.ChangeType(obj[key], typeof(T));
        }
        catch (System.Exception e)
        {
            throw new System.Exception($"Failed type conversion exception has been thrown.", e);
        }
    }

    public static T GetOrCreate<T>(this Dictionary<string, object> obj, string key) where T : new()
    {
        if (!obj.ContainsKey(key))
        {
            obj[key] = new T();
        }

        return obj.GetItem<T>(key);
    }

    public static T GetOrCreate<T>(this Dictionary<string, object> obj, string key, T defaultValue)
    {
        if (!obj.ContainsKey<T>(key))
        {
            obj[key] = defaultValue;
        }

        return obj.GetItem<T>(key);
    }

    public static T GetOrCreate<T>(this Dictionary<string, object> obj, string key, Func<T> factory) where T : class
    {
        if (!obj.ContainsKey(key))
        {
            T value = factory();
            obj[key] = value;
            return value;
        }
        return (T)obj[key];
    }
}
