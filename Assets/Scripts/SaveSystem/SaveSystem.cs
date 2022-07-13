using System.IO;
using UnityEngine;
using System.Collections.Generic;
using System;

public static class SaveSystem
{
    public static string SaveFilePath => Path.Combine(Application.dataPath, "save.txt");
    public static bool DoesSaveExists => File.Exists(SaveFilePath);

    private static Dictionary<string, object> keyValuePairs = new Dictionary<string, object>();

    public static void AddEntry(string key, object value)
    {
        keyValuePairs[key] = value;
        SaveToFile();
    } 

    public static void RemoveEntry(string key)
    {
        if (!EntryExsists(key))
            return;
        keyValuePairs.Remove(key);
    }

    public static T LoadEntry<T>(string key)
    {
        if (!EntryExsists(key))
            return default;

        object value = keyValuePairs[key];

        try
        {
            T result = (T)Convert.ChangeType(value, typeof(T));
            return result;
        }
        catch
        {
            return default;
        }
    }

    public static void SaveToFile()
    {
        List<string> toSave = new List<string>();
        foreach (var keyValue in keyValuePairs)
        {
            string entry = $"{keyValue.Key}:{keyValue.Value}";
            toSave.Add(entry);
        }
        File.WriteAllLines(SaveFilePath, toSave);
    }

    public static void LoadFromFile()
    {
        if (!DoesSaveExists)
            return;

        ClearEntries();

        string[] lines = File.ReadAllLines(SaveFilePath);

        foreach (string line in lines)
        {
            string[] splitted = line.Split(':');
            keyValuePairs.Add(splitted[0], splitted[1]);
        }
    }

    public static void ClearEntries() => keyValuePairs.Clear();

    public static bool EntryExsists(string key) => keyValuePairs.ContainsKey(key);
}
