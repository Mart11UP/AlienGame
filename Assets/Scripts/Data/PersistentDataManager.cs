using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Alien.Data
{
    // Based on a script from a previous project
    public static class PersistentDataManager
    {
        private const string DataFileName = "PersistentData.json";

        private static PersistentDataCollection persistentData = new();
        private static bool isDataLoaded;
        public static event Action OnDataChanged;
        private static string DataFilePath => Path.Combine(Application.persistentDataPath, DataFileName);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetRuntimeData()
        {
            persistentData = new PersistentDataCollection();
            isDataLoaded = false;
            OnDataChanged = null;
        }

        public static bool TrySaveValue<T>(string key, T value) => SaveEntry(key, value, false);

        public static bool TrySaveGroup<T>(string groupId, T value) => SaveEntry(groupId, value, true);

        public static T GetValue<T>(string key, T defaultValue = default)
        {
            if (TryGetValue(key, out T value)) return value;

            return defaultValue;
        }

        public static T GetGroup<T>(string groupId, T defaultValue = default)
        {
            if (TryGetGroup(groupId, out T value)) return value;

            return defaultValue;
        }

        public static bool TryGetValue<T>(string key, out T value) => TryGetEntry(key, false, out value);

        public static bool TryGetGroup<T>(string groupId, out T value) => TryGetEntry(groupId, true, out value);

        public static bool ContainsValue(string key)
        {
            EnsureDataLoaded();

            if (string.IsNullOrEmpty(key)) return false;

            return GetEntry(key, false) != null;
        }

        public static bool ContainsGroup(string groupId)
        {
            EnsureDataLoaded();

            if (string.IsNullOrEmpty(groupId)) return false;

            return GetEntry(groupId, true) != null;
        }

        public static bool TryDeleteValue(string key) => DeleteEntry(key, false);

        public static bool TryDeleteGroup(string groupId) => DeleteEntry(groupId, true);

        public static bool TryRemoveAllData()
        {
            EnsureDataLoaded();

            try
            {
                if (File.Exists(DataFilePath)) File.Delete(DataFilePath);

                persistentData.Entries.Clear();

                OnDataChanged?.Invoke();
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogError($"Failed to remove all persistent data. {exception.Message}");
                return false;
            }
        }

        private static bool SaveEntry<T>(string key, T value, bool isGroup)
        {
            EnsureDataLoaded();

            if (!ValidateSave(key, value)) return false;

            string jsonValue;

            try
            {
                PersistentValue<T> persistentValue = new(value);
                jsonValue = JsonUtility.ToJson(persistentValue);
            }
            catch (Exception exception)
            {
                Debug.LogError($"Failed to serialize persistent data '{key}'. {exception.Message}");
                return false;
            }

            PersistentDataEntry entry = GetEntry(key, isGroup);

            if (entry == null)
            {
                entry = new PersistentDataEntry(key, typeof(T).FullName, jsonValue, isGroup);
                persistentData.Entries.Add(entry);
            }
            else
            {
                entry.TypeName = typeof(T).FullName;
                entry.JsonValue = jsonValue;
            }

            if (!SaveData()) return false;

            OnDataChanged?.Invoke();
            return true;
        }

        private static bool TryGetEntry<T>(string key, bool isGroup, out T value)
        {
            EnsureDataLoaded();

            value = default;

            if (string.IsNullOrEmpty(key)) return false;

            PersistentDataEntry entry = GetEntry(key, isGroup);

            if (entry == null) return false;

            if (entry.TypeName != typeof(T).FullName)
            {
                Debug.LogWarning($"Persistent data '{key}' was saved as '{entry.TypeName}' " + $"but was requested as '{typeof(T).FullName}'.");
                return false;
            }

            try
            {
                PersistentValue<T> persistentValue = JsonUtility.FromJson<PersistentValue<T>>(entry.JsonValue);

                if (persistentValue == null) return false;

                value = persistentValue.Value;
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogError($"Failed to deserialize persistent data '{key}'. {exception.Message}");
                return false;
            }
        }

        private static bool DeleteEntry(string key, bool isGroup)
        {
            EnsureDataLoaded();

            if (string.IsNullOrEmpty(key)) return false;

            PersistentDataEntry entry = GetEntry(key, isGroup);

            if (entry == null) return false;

            persistentData.Entries.Remove(entry);

            if (!SaveData())
            {
                persistentData.Entries.Add(entry);
                return false;
            }

            OnDataChanged?.Invoke();
            return true;
        }

        private static PersistentDataEntry GetEntry(string key, bool isGroup)
        {
            foreach (PersistentDataEntry entry in persistentData.Entries)
                if (entry.Key == key && entry.IsGroup == isGroup) return entry;

            return null;
        }

        private static bool SaveData()
        {
            try
            {
                string jsonData = JsonUtility.ToJson(persistentData, true);

                Directory.CreateDirectory(Application.persistentDataPath);
                File.WriteAllText(DataFilePath, jsonData);

                return true;
            }
            catch (Exception exception)
            {
                Debug.LogError($"Failed to save persistent data. {exception.Message}");
                return false;
            }
        }

        private static void EnsureDataLoaded()
        {
            if (isDataLoaded) return;

            isDataLoaded = true;
            LoadData();
        }

        private static void LoadData()
        {
            if (!File.Exists(DataFilePath)) return;

            try
            {
                string jsonData = File.ReadAllText(DataFilePath);
                PersistentDataCollection loadedData = JsonUtility.FromJson<PersistentDataCollection>(jsonData);

                if (loadedData == null)
                {
                    Debug.LogWarning("The persistent data file could not be loaded.");
                    return;
                }

                persistentData = loadedData;

                persistentData.Entries ??= new List<PersistentDataEntry>();
            }
            catch (Exception exception)
            {
                Debug.LogError($"Failed to load persistent data. {exception.Message}");
            }
        }

        private static bool ValidateSave<T>(string key, T value)
        {
            if (string.IsNullOrEmpty(key))
            {
                Debug.LogWarning("The persistent data key cannot be null or empty.");
                return false;
            }

            if (value is null)
            {
                Debug.LogWarning($"The persistent data value for '{key}' cannot be null.");
                return false;
            }

            return true;
        }

        [Serializable]
        private class PersistentDataCollection
        {
            public List<PersistentDataEntry> Entries = new();
        }

        [Serializable]
        private class PersistentDataEntry
        {
            public string Key;
            public string TypeName;
            public string JsonValue;
            public bool IsGroup;

            public PersistentDataEntry(string key, string typeName, string jsonValue, bool isGroup)
            {
                Key = key;
                TypeName = typeName;
                JsonValue = jsonValue;
                IsGroup = isGroup;
            }
        }

        [Serializable]
        private class PersistentValue<T>
        {
            public T Value;

            public PersistentValue(T value)
            {
                Value = value;
            }
        }
    }
}