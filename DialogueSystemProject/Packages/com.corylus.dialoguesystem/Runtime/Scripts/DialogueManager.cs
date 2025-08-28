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
    [SerializeField] private DialogueRuntimeHandler _dialogueRuntimeHandler;
    [SerializeField] private DialogueScriptManager _dialogueScriptManager;
    [SerializeField] private string _language;

    [Header("Settings")]
    [SerializeField] public char midScriptChar;
    [SerializeField] public string NPCActorKey;

    public event System.Action onLanguageUpdated;

    public DialogueData GetDialogueData(string key)
    {
        DialogueEntry dialogue = _dialogueRuntimeHandler.GetDialogueByKey(key);

        string nextKey = dialogue.NextKey;
        string question = dialogue.Question;
        string actor = GetActor(dialogue.Actor);
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

        return new DialogueData(key, nextKey, question, actor, text, startScriptsList, middleScriptsList, endScriptsList);
    }

    public string GetSimpleDialogue(string key)
    {
        SimpleDialogueEntry simpleDialogue = _dialogueRuntimeHandler.GetSimpleDialogueByKey(key);
        string text = simpleDialogue.Text[_language];

        if (simpleDialogue.Scripts.Insert != null)
        {
            foreach (string insert in simpleDialogue.Scripts.Insert)
            {
                text = _dialogueScriptManager.InsertText(insert, text);
            }
        }

        return text;
    }

    public string GetSimpleText(string key)
    {
        SimpleTextEntry simpleText = _dialogueRuntimeHandler.GetSimpleTextByKey(key);
        string text = simpleText.Text[_language];

        if (simpleText.Scripts.Insert != null)
        {
            foreach (string insert in simpleText.Scripts.Insert)
            {
                text = _dialogueScriptManager.InsertText(insert, text);
            }
        }

        return text;
    }

    public string GetActor(string key)
    {
        if (key == NPCActorKey) return key;

        CharactersEntry character = _dialogueRuntimeHandler.GetCharacterByKey(key);
        string actorName = character.Actor[_language];

        if (character.Scripts.Insert != null)
        {
            foreach (string insert in character.Scripts.Insert)
            {
                actorName = _dialogueScriptManager.InsertText(insert, actorName);
            }
        }

        return actorName;
    }

    public List<QuestionsEntry> GetQuestions(string key)
    {
        return _dialogueRuntimeHandler.GetQuestionByKey(key);
    }

    public void ChangeLanguage(string newLanguage)
    {
        _language = newLanguage;
        onLanguageUpdated?.Invoke();
    }

    public void ExecuteMethod(DialogueController dialogueController, string method)
    {
        _dialogueScriptManager.CallMethod(dialogueController, method);
    }

    public IEnumerator ExecuteCoroutine(string coroutine)
    {
        yield return StartCoroutine(_dialogueScriptManager.CallCoroutine(coroutine));
    }

    [ContextMenu("Reload Language")]
    public void ReloadLanguage()
    {
        onLanguageUpdated?.Invoke();
    }
}

public class DialogueData
{
    public string Key;
    public string NextKey;
    public string Question;
    public string Actor;
    public string Text;
    public List<string> StartScriptsList;
    public List<string> MiddleScriptsList;
    public List<string> EndScriptsList;

    public DialogueData(string key, string nextKey, string question, string actor, string text, List<string> startScriptsList, List<string> middleScriptsList, List<string> endScriptsList)
    {
        Key = key;
        NextKey = nextKey;
        Question = question;
        Actor = actor;
        Text = text;
        StartScriptsList = startScriptsList;
        MiddleScriptsList = middleScriptsList;
        EndScriptsList = endScriptsList;
    }
}