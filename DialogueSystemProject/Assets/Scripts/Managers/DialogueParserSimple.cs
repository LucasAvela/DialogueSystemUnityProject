using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

[System.Serializable]
public class DialogueEntrySimple
{
    public Dictionary<string, string> Text;
}

public class DialogueParserSimple : MonoBehaviour
{
    public TextAsset jsonFile;
    public Dictionary<string, DialogueEntrySimple> dialogueDictionary;

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

    public DialogueEntrySimple GetDialogueByKey(string key)
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

    Dictionary<string, DialogueEntrySimple> ParseJsonToDictionary(string json)
    {
        if (string.IsNullOrEmpty(json))
        {
            Debug.LogError("JSON string is null or empty.");
            return null;
        }

        try
        {
            return JsonConvert.DeserializeObject<Dictionary<string, DialogueEntrySimple>>(json);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Exception while parsing JSON: {ex.Message}");
            return null;
        }
    }
}
