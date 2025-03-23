using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

[System.Serializable]
public class DialogueEntry
{
    public Dictionary<string, string> Actor;
    public Dictionary<string, string> Text;
    public ScriptsData Scripts;
    public string Next_Key;
}

[System.Serializable]
public class ScriptsData
{
    public List<string> Start;
    public List<string> Middle;
    public List<string> End;
}

public class DialogueParser : MonoBehaviour
{
    public TextAsset jsonFile;
    public Dictionary<string, DialogueEntry> dialogueDictionary;

    void Awake()
    {
        if (jsonFile != null)
        {
            dialogueDictionary = ParseJsonToDictionary(jsonFile.text);
            if (dialogueDictionary == null)
            {
                Debug.LogError("Failed to parse JSON to dictionary.");
            }
        }
        else
        {
            Debug.LogError("JSON file is not assigned.");
        }
    }

    public DialogueEntry GetDialogueByKey(string key)
    {
        if (dialogueDictionary != null && dialogueDictionary.ContainsKey(key))
        {
            return dialogueDictionary[key];
        }
        else
        {
            Debug.LogError($"Dialogue with key '{key}' not found.");
            return null;
        }
    }

    Dictionary<string, DialogueEntry> ParseJsonToDictionary(string json)
    {
        if (string.IsNullOrEmpty(json))
        {
            Debug.LogError("JSON string is null or empty.");
            return null;
        }

        try
        {
            return JsonConvert.DeserializeObject<Dictionary<string, DialogueEntry>>(json);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Exception while parsing JSON: {ex.Message}");
            return null;
        }
    }
}
