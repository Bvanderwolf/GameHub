using System.Collections.Generic;
using UnityEngine;

public class ResourceManager
{
    private class ResourceDictionary
    {
        private Dictionary<string, object> _dict = new Dictionary<string, object>();

        public void Add<T> (string key, object value) where T : class
        {
            if (!_dict.ContainsKey(key))
            {
                _dict.Add(key, value);
                if (value == null) { Debug.LogWarning($"added null value in dictionary with key value {key}"); }
            }
            else
            {
                Debug.LogWarning($"key: {key} already in dictionary with value: {_dict[key]}");
            }
        }

        public bool HasKey (string key)
        {
            return _dict.ContainsKey(key);
        }

        public T GetValue<T> (string key) where T : class
        {
            if (_dict.ContainsKey(key))
            {
                return _dict[key] as T;
            }
            else
            {
                Debug.LogWarning($"key: {key} is not in dictionary");
                return null;
            }
        }
    }

    private static ResourceDictionary resourceDict = new ResourceDictionary();

    public static void AddResource<T> (string name, string path, HubGames hubgame) where T : class
    {
        if (!resourceDict.HasKey(name))
        {
            resourceDict.Add<T>(name, Resources.Load(path, typeof(T)));
        }
    }

    public static T GetResource<T> (string name) where T : class
    {
        return resourceDict.GetValue<T>(name);
    }
}