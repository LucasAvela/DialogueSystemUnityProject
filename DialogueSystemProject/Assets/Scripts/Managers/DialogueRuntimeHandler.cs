using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

#region DialogueData
public class DialogueEntry
{
    public string Actor;
    public Dictionary<string, string> Text;
    public string NextKey;
    public string Question;
    public DialogueScripts Scripts;
}
public class DialogueScripts
{
    public List<string> Insert;
    public List<string> Start;
    public List<string> Middle;
    public List<string> End;
}
#endregion

#region SimpleDialogueData
public class SimpleDialogueEntry
{
    public Dictionary<string, string> Text;
    public SimpleDialogueScripts Scripts;
}
public class SimpleDialogueScripts
{
    public List<string> Insert;
}
#endregion

#region SimpleTextData
public class SimpleTextEntry
{
    public Dictionary<string, string> Text;
    public SimpleTextScripts Scripts;
}
public class SimpleTextScripts
{
    public List<string> Insert;
}
#endregion

#region CharactersData
public class CharactersEntry
{
    public string Original;
    public Dictionary<string, string> Actor;
    public CharactersScripts Scripts;
}
public class CharactersScripts
{
    public List<string> Insert;
}
#endregion

#region QuestionsData
public class QuestionsEntry
{
    public string TextKey;
    public string NextKey;
}
#endregion

public class DialogueRuntimeHandler : MonoBehaviour
{
    [Header("JSON Data Files")]
    [SerializeField] private TextAsset _jsonFileDialogue;
    [SerializeField] private TextAsset _jsonFileSimpleDialogue;
    [SerializeField] private TextAsset _jsonFileSimpleText;
    [SerializeField] private TextAsset _jsonFileCharacters;
    [SerializeField] private TextAsset _jsonFileQuestions;

    public Dictionary<string, DialogueEntry> dialogueDictionary;
    public Dictionary<string, SimpleDialogueEntry> simpleDialogueDictionary;
    public Dictionary<string, SimpleTextEntry> simpleTextDictionary;
    public Dictionary<string, CharactersEntry> charactersDictionary;
    public Dictionary<string, List<QuestionsEntry>> questionsDictionary;

    private void Awake()
    {
        if (_jsonFileDialogue != null)
        {
            dialogueDictionary = JsonToDictionary<DialogueEntry>(_jsonFileDialogue.text);
            if (dialogueDictionary == null)
            {
                Debug.LogError("❌ Failed to parse *Dialogue* JSON to dictionary.");
            }
        }
        else
        {
            Debug.LogError("⚠️ *Dialogue* JSON file is not assigned.");
        }

        if (_jsonFileSimpleDialogue != null)
        {
            simpleDialogueDictionary = JsonToDictionary<SimpleDialogueEntry>(_jsonFileSimpleDialogue.text);
            if (simpleDialogueDictionary == null)
            {
                Debug.LogError("❌ Failed to parse *Simple Dialogue* JSON to dictionary.");
            }
        }
        else
        {
            Debug.LogError("⚠️ *Simple Dialogue* JSON file is not assigned.");
        }

        if (_jsonFileSimpleText != null)
        {
            simpleTextDictionary = JsonToDictionary<SimpleTextEntry>(_jsonFileSimpleText.text);
            if (simpleTextDictionary == null)
            {
                Debug.LogError("❌ Failed to parse *Simple Text* JSON to dictionary.");
            }
        }
        else
        {
            Debug.LogError("⚠️ *Simple Text* JSON file is not assigned.");
        }

        if (_jsonFileCharacters != null)
        {
            charactersDictionary = JsonToDictionary<CharactersEntry>(_jsonFileCharacters.text);
            if (charactersDictionary == null)
            {
                Debug.LogError("❌ Failed to parse *Characters* JSON to dictionary.");
            }
        }
        else
        {
            Debug.LogError("⚠️ *Characters* JSON file is not assigned.");
        }

        if (_jsonFileQuestions != null)
        {
            questionsDictionary = JsonToDictionaryList<QuestionsEntry>(_jsonFileQuestions.text);
            if (questionsDictionary == null)
            {
                Debug.LogError("❌ Failed to parse *Questions* JSON to dictionary.");
            }
        }
        else
        {
            Debug.LogError("⚠️ *Questions* JSON file is not assigned.");
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

    public SimpleDialogueEntry GetSimpleDialogueByKey(string key)
    {
        if (simpleDialogueDictionary != null && simpleDialogueDictionary.ContainsKey(key))
        {
            return simpleDialogueDictionary[key];
        }
        else
        {
            Debug.LogError($"Simple Dialogue with key '{key}' not found.");
            return null;
        }
    }

    public SimpleTextEntry GetSimpleTextByKey(string key)
    {
        if (simpleTextDictionary != null && simpleTextDictionary.ContainsKey(key))
        {
            return simpleTextDictionary[key];
        }
        else
        {
            Debug.LogError($"Simple Text with key '{key}' not found.");
            return null;
        }
    }

    public CharactersEntry GetCharacterByKey(string key)
    {
        if (charactersDictionary != null && charactersDictionary.ContainsKey(key))
        {
            return charactersDictionary[key];
        }
        else
        {
            Debug.LogError($"Character with key '{key}' not found.");
            return null;
        }
    }

    public List<QuestionsEntry> GetQuestionByKey(string key)
    {
        if (questionsDictionary != null && questionsDictionary.ContainsKey(key))
        {
            return questionsDictionary[key];
        }
        else
        {
            Debug.LogError($"Question with key '{key}' not found.");
            return null;
        }
    }

    #region Utility
    Dictionary<string, T> JsonToDictionary<T>(string json)
    {
        if (string.IsNullOrEmpty(json))
        {
            Debug.LogError("JSON string is null or empty.");
            return null;
        }

        try
        {
            return JsonConvert.DeserializeObject<Dictionary<string, T>>(json);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Exception while parsing JSON: {ex.Message}");
            return null;
        }
    }

    Dictionary<string, List<T>> JsonToDictionaryList<T>(string json)
    {
        if (string.IsNullOrEmpty(json))
        {
            Debug.LogError("JSON string is null or empty.");
            return null;
        }

        try
        {
            return JsonConvert.DeserializeObject<Dictionary<string, List<T>>>(json);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Exception while parsing JSON: {ex.Message}");
            return null;
        }
    }
    #endregion
}
