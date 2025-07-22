using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DialogueController : MonoBehaviour
{
    [SerializeField] string _dialogueKey;
    [SerializeField] string _simpleDialogueKey;
    [SerializeField] string _simpleTextKey;

    [Header("Dialogue Content")] // Actual dialogue content and metadata
    private string _actualDialogueKey = null;
    private string _nextDialogueKey = null;
    private string _actualDialogueActor = null;
    private string _actualDialogueText = null;
    private List<string> _actualStartScriptsList = new List<string>();
    private List<string> _actualMiddleScriptsList = new List<string>();
    private List<string> _actualEndScriptsList = new List<string>();
    private List<string> _actualTagsList = new List<string>();

    [Header("Internals")] // Internal state and references for managing dialogue
    private DialogueManager _dialogueManager = null;
    private Coroutine _writingDialogueCoroutine = null;
    private Coroutine _instantDialogueCoroutine = null;

    private void Start()
    {
        _dialogueManager = DialogueManager.Instance;

        if (_dialogueManager == null)
        {
            Debug.LogError("DialogueManager instance not found. Please ensure it is initialized before using DialogueWriterController.");
            return;
        }
    }

    [ContextMenu("Debug Dialogue")]
    public void DebugDialogue()
    {
        DialogueData dialogueData = _dialogueManager.GetDialogueData(_dialogueKey);

        _actualDialogueKey = dialogueData.Key;
        _nextDialogueKey = dialogueData.NextKey;
        _actualDialogueActor = dialogueData.Actor;
        _actualDialogueText = dialogueData.Text;
        _actualStartScriptsList = dialogueData.StartScriptsList;
        _actualMiddleScriptsList = dialogueData.MiddleScriptsList;
        _actualEndScriptsList = dialogueData.EndScriptsList;

        Debug.Log("<b>Dialogue Data</b>" +
            $"\nKey: {_actualDialogueKey}\nNext Key: {_nextDialogueKey}\nActor: {_actualDialogueActor}\nText: {_actualDialogueText}");
    }

    [ContextMenu("Debug Simple Dialogue")]
    public void DebugSimpleDialogue()
    {
        string simpleDialogue = _dialogueManager.GetSimpleDialogue(_simpleDialogueKey);
        Debug.Log("<b>Simple Dialogue Data</b>" +
            $"\nText: {simpleDialogue}");
    }

    [ContextMenu("Debug Simple Text")]
    public void DebugSimpleText()
    {
        string simpleText = _dialogueManager.GetSimpleText(_simpleTextKey);
        Debug.Log("<b>Simple Text Data</b>" +
            $"\nText: {simpleText}");
    }
}