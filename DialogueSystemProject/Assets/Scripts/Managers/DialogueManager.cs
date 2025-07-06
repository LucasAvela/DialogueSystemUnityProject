using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    #region Singleton
    private static DialogueManager _instance;
    public static DialogueManager Instance { get { return _instance; } }

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }
    #endregion

    [Header("Dependencies")]
    [SerializeField] private DialogueParser _dialogueParser;
    [SerializeField] private DialogueScriptManager _dialogueScriptManager;
    [SerializeField] private string _language = "en_us";

    public event System.Action onDialogueUpdated;

    public DialogueData GetDialogueData(string key)
    {
        DialogueEntry dialogue = _dialogueParser.GetDialogueByKey(key);

        string nextKey = dialogue.Next_Key;
        string actor = dialogue.Actor[_language];
        string text = dialogue.Text[_language];
        List<string> startScriptsList = new List<string>(dialogue.Scripts.Start);
        List<string> middleScriptsList = new List<string>(dialogue.Scripts.Middle);
        List<string> endScriptsList = new List<string>(dialogue.Scripts.End);

        if (dialogue.Scripts.Insert != null)
        {
            foreach (string insert in dialogue.Scripts.Insert)
            {
                text = _dialogueScriptManager.InsertText(insert, text);
            }
        }

        if (actor.Contains('{'))
        {
            string insert = actor.Replace("{", "").Replace("}", "");
            actor = _dialogueScriptManager.InsertText(insert, actor);
            print(actor);
        }

        return new DialogueData(key, nextKey, actor, text, startScriptsList, middleScriptsList, endScriptsList);
    }

    public string GetSimpleDialogue(string key)
    {
        DialogueEntrySimple dialogue = _dialogueParser.GetSimpleDialogueByKey(key);
        string text = dialogue.Text[_language];

        if (dialogue.Scripts.Insert != null)
        {
            foreach (string insert in dialogue.Scripts.Insert)
            {
                text = _dialogueScriptManager.InsertText(insert, text);
            }
        }

        return text;
    }

    public string GetSimpleText(string key)
    {
        DialogueEntryUI dialogue = _dialogueParser.GetUIDialogueByKey(key);
        string text = dialogue.Text[_language];

        if (dialogue.Scripts.Insert != null)
        {
            foreach (string insert in dialogue.Scripts.Insert)
            {
                text = _dialogueScriptManager.InsertText(insert, text);
            }
        }

        return text;
    }

    public void ChangeLanguage(string newLanguage)
    {
        _language = newLanguage;
        onDialogueUpdated?.Invoke();
    }

    public void ExecuteMethod(string method)
    {
        _dialogueScriptManager.CallMethod(method);
    }

    public IEnumerator ExecuteCoroutine(string coroutine)
    {
        yield return StartCoroutine(_dialogueScriptManager.CallCoroutine(coroutine));
    }
}

public class DialogueData
{
    public string Key;
    public string NextKey;
    public string Actor;
    public string Text;
    public List<string> StartScriptsList;
    public List<string> MiddleScriptsList;
    public List<string> EndScriptsList;

    public DialogueData(string key, string nextKey, string actor, string text, List<string> startScriptsList, List<string> middleScriptsList, List<string> endScriptsList)
    {
        Key = key;
        NextKey = nextKey;
        Actor = actor;
        Text = text;
        StartScriptsList = startScriptsList;
        MiddleScriptsList = middleScriptsList;
        EndScriptsList = endScriptsList;
    }
}