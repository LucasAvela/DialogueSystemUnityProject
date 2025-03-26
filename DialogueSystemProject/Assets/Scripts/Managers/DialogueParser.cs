using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

#region Dialogue
[System.Serializable]
public class DialogueEntry
{
    public Dictionary<string, string> Actor;
    public Dictionary<string, string> Text;
    public string Next_Key;
    public string Question;
    public ScriptsData Scripts;
}

[System.Serializable]
public class ScriptsData
{
    public List<string> Insert;
    public List<string> Start;
    public List<string> Middle;
    public List<string> End;
}
#endregion

#region SimpleDialogue
[System.Serializable]
public class DialogueEntrySimple
{
    public Dictionary<string, string> Text;
}
#endregion

#region DialogueUI
[System.Serializable]
public class DialogueEntryUI
{
    public Dictionary<string, string> Text;
}
#endregion

#region  DialogueAnswers
[System.Serializable]
public class QuestionEntry
{
    public string UIKey;
    public string NextKey;
}
#endregion

public class DialogueParser : MonoBehaviour
{
    [Header("Dialogue")]
    [SerializeField] private TextAsset jsonFileDialogue;
    public Dictionary<string, DialogueEntry> dialogueDictionary;

    [Header("Simple Dialogue")]
    [SerializeField] private TextAsset jsonFileSimpleDialogue;
    public Dictionary<string, DialogueEntrySimple> simpleDialogueDictionary;

    [Header("UI Dialogue")]
    [SerializeField] private TextAsset jsonFileUIDialogue;
    public Dictionary<string, DialogueEntryUI> uiDialogueDictionary;

    [Header("Question Dialogue")]
    [SerializeField] private TextAsset jsonFileQuestionDialogue;
    public Dictionary<string, List<QuestionEntry>> questionDialogueDictionary;

    void Awake()
    {
        DialogueAwake();
        SimpleDialogueAwake();
        UIDialogueAwake();
        QuestionDialogueAwake();
    }

    #region DialogueFunctions
    void DialogueAwake()
    {
        if (jsonFileDialogue != null)
        {
            dialogueDictionary = ParseJsonToDictionary(jsonFileDialogue.text);
            if (dialogueDictionary == null)
            {
                Debug.LogError("Failed to parse DIALOGUE JSON to dictionary.");
            }
        }
        else
        {
            Debug.LogError("DIALOGUE JSON file is not assigned.");
        }
    }

    public DialogueEntry GetDialogueByKey(string key)
    {
        if (dialogueDictionary != null & dialogueDictionary.ContainsKey(key))
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
    #endregion

    #region SimpleDialogueFunctions
    void SimpleDialogueAwake()
    {
        if (jsonFileSimpleDialogue != null)
        {
            simpleDialogueDictionary = ParseJsonToSimpleDictionary(jsonFileSimpleDialogue.text);
            if (simpleDialogueDictionary == null)
            {
                Debug.LogError("Failed to parse JSON to dictionary.");
            }
        }
        else
        {
            Debug.LogError("JSON file is not assigned.");
        }
    }

    public DialogueEntrySimple GetSimpleDialogueByKey(string key)
    {
        if (simpleDialogueDictionary != null && simpleDialogueDictionary.ContainsKey(key))
        {
            return simpleDialogueDictionary[key];
        }
        else
        {
            Debug.LogError($"Dialogue with key '{key}' not found.");
            return null;
        }
    }

    Dictionary<string, DialogueEntrySimple> ParseJsonToSimpleDictionary(string json)
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
    #endregion

    #region UIDialogueFunctions
    void UIDialogueAwake()
    {
        if (jsonFileUIDialogue != null)
        {
            uiDialogueDictionary = ParseJsonToUIDictionary(jsonFileUIDialogue.text);
            if (uiDialogueDictionary == null)
            {
                Debug.LogError("Failed to parse JSON to dictionary.");
            }
        }
        else
        {
            Debug.LogError("JSON file is not assigned.");
        }
    }

    public DialogueEntryUI GetUIDialogueByKey(string key)
    {
        if (uiDialogueDictionary != null && uiDialogueDictionary.ContainsKey(key))
        {
            return uiDialogueDictionary[key];
        }
        else
        {
            Debug.LogError($"Dialogue with key '{key}' not found.");
            return null;
        }
    }

    Dictionary<string, DialogueEntryUI> ParseJsonToUIDictionary(string json)
    {
        if (string.IsNullOrEmpty(json))
        {
            Debug.LogError("JSON string is null or empty.");
            return null;
        }

        try
        {
            return JsonConvert.DeserializeObject<Dictionary<string, DialogueEntryUI>>(json);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Exception while parsing JSON: {ex.Message}");
            return null;
        }
    }
    #endregion

    #region QuestionDialogueFunctions
    void QuestionDialogueAwake()
    {
        if (jsonFileQuestionDialogue != null)
        {
            questionDialogueDictionary = ParseJsonToQuestionDictionary(jsonFileQuestionDialogue.text);
            if (questionDialogueDictionary == null)
            {
                Debug.LogError("Failed to parse JSON to dictionary.");
            }
        }
        else
        {
            Debug.LogError("JSON file is not assigned.");
        }
    }

    public List<QuestionEntry> GetQuestionDialogueByKey(string key)
    {
        if (questionDialogueDictionary != null && questionDialogueDictionary.ContainsKey(key))
        {
            return questionDialogueDictionary[key];
        }
        else
        {
            Debug.LogError($"Dialogue with key '{key}' not found.");
            return null;
        }
    }

    Dictionary<string, List<QuestionEntry>> ParseJsonToQuestionDictionary(string json)
    {
        if (string.IsNullOrEmpty(json))
        {
            Debug.LogError("JSON string is null or empty.");
            return null;
        }

        try
        {
            return JsonConvert.DeserializeObject<Dictionary<string, List<QuestionEntry>>>(json);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Exception while parsing JSON: {ex.Message}");
            return null;
        }
    }
    #endregion
}
