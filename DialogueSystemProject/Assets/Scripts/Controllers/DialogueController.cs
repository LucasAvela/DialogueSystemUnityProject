using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DialogueController : MonoBehaviour
{
    [Header("Dialogue Settings")] // Settings for the dialogue UI
    [SerializeField] private GameObject _dialoguePanel;
    [SerializeField] private TextMeshProUGUI _dialogueText;
    [SerializeField] private TextMeshProUGUI _dialogueActorText;
    [SerializeField] private bool _isDialogueInstant = false;
    [SerializeField] private float _writingTime = 0.05f;
    [SerializeField] private string _alphaTag = "<alpha=#00>";

    [Header("Animations Settings")] // Animations
    [SerializeField] private Animator _dialogueAnimator;
    [SerializeField] private AnimationClip _openPanelAnimation;
    [SerializeField] private AnimationClip _closePanelAnimation;
    [SerializeField] private AnimationClip _showTextAnimation;

    [Header("Dialogue Runtime Flags")] // Flags to control dialogue state and behavior
    [SerializeField] private bool _onDialogue = false;
    [SerializeField] private bool _onWritingDialogue = false;
    [SerializeField] private bool _onMiddleScriptRunning = false;
    [SerializeField] private bool _onDialoguePanelAnimation = false;
    [SerializeField] private bool _onDialogueTextAnimation = false;
    [SerializeField] private bool _skipWritingDialogue = false;
    [SerializeField] private bool _stopDialogue = false;

    [Header("Dialogue Content")] // Actual dialogue content and metadata
    [TextArea(1, 2)][SerializeField] private string _actualDialogueKey = null;
    [TextArea(1, 2)][SerializeField] private string _nextDialogueKey = null;
    [TextArea(1, 2)][SerializeField] private string _actualDialogueActor = null;
    [TextArea(3, 9)][SerializeField] private string _actualDialogueText = null;
    [SerializeField] private List<string> _actualStartScriptsList = new List<string>();
    [SerializeField] private List<string> _actualMiddleScriptsList = new List<string>();
    [SerializeField] private List<string> _actualEndScriptsList = new List<string>();
    [SerializeField] private List<string> _actualSpritesList = new List<string>();
    [SerializeField] private List<int> _actualMiddleScriptsListIndex = new List<int>();

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

        ClearDialogue();
    }

    public void StartDialogue(string key)
    {
        if (!_onDialogue && !_onDialoguePanelAnimation)
        {
            _onDialogue = true;
            _actualDialogueKey = key;
            StartCoroutine(OpenDialoguePanel());
        }
    }

    public void ConsumeInput()
    {
        if (!_onDialogue || _onDialoguePanelAnimation || _onDialogueTextAnimation || _onMiddleScriptRunning) return;

        if (_onWritingDialogue)
        {
            _skipWritingDialogue = true;
            return;
        }

        if (_actualEndScriptsList != null)
        {
            foreach (string script in _actualEndScriptsList)
            {
                if (script[0] == '&')
                {
                    StartCoroutine(_dialogueManager.ExecuteCoroutine(script.Substring(1)));
                }
                else
                {
                    _dialogueManager.ExecuteMethod(script);
                }
            }
        }

        if (_nextDialogueKey != null && _nextDialogueKey != "")
        {
            _actualDialogueKey = _nextDialogueKey;
            UpdateDialogue();
        }
        else
        {
            StartCoroutine(CloseDialogue());
        }
    }

    private void UpdateDialogue()
    {
        ClearDialogue();
        DialogueData dialogueData = _dialogueManager.GetDialogueData(_actualDialogueKey);

        _actualDialogueKey = dialogueData.Key;
        _nextDialogueKey = dialogueData.NextKey;
        _actualDialogueActor = dialogueData.Actor;
        _actualDialogueText = dialogueData.Text;
        _actualStartScriptsList = dialogueData.StartScriptsList;
        _actualMiddleScriptsList = dialogueData.MiddleScriptsList;
        _actualEndScriptsList = dialogueData.EndScriptsList;

        DisplayDialogue();
    }

    private void ClearDialogue()
    {
        _dialogueText.text = "";
        _dialogueActorText.text = "";
        _nextDialogueKey = null;
        _actualDialogueActor = null;
        _actualDialogueText = null;
        _actualStartScriptsList.Clear();
        _actualMiddleScriptsList.Clear();
        _actualEndScriptsList.Clear();
        _actualMiddleScriptsListIndex.Clear();
    }

    private void DisplayDialogue()
    {
        if (_isDialogueInstant)
        {
            //_instantDialogueCoroutine = StartCoroutine(DisplayInstantDialogue());
        }
        else
        {
            _writingDialogueCoroutine = StartCoroutine(WriteDialogue());
        }

        _dialogueActorText.text = _actualDialogueActor;
    }

    private IEnumerator WriteDialogue()
    {
        string text = _actualDialogueText;
        _onWritingDialogue = true;

        if (_actualStartScriptsList != null)
        {
            foreach (string script in _actualStartScriptsList)
            {
                _dialogueManager.ExecuteMethod(script);
            }
        }

        for (int i = 0; i < text.Length; i++)
        {
            if (text[i] == '§')
            {
                text = text.Remove(i, 1);
                _actualMiddleScriptsListIndex.Add(i);
            }
        }

        for (int i = 0; i < text.Length; i++)
        {
            if (text[i] == '<')
            {
                int endTag = text.IndexOf('>', i);
                if (endTag != -1)
                {
                    string fullTag = text.Substring(i, endTag - i + 1);
                    if (fullTag.Length == 9 && fullTag.StartsWith("<#") && fullTag[8] == '>')
                    {
                        string hex = fullTag.Substring(2, 6);
                        string newTag = $"<#{hex}00>";
                        text = text.Remove(i, fullTag.Length).Insert(i, newTag);
                        endTag = i + newTag.Length - 1;
                    }
                    else if (fullTag == "</color>")
                    {
                        string newTag = "<alpha=#00>";
                        text = text.Remove(i, fullTag.Length).Insert(i, newTag);
                        endTag = i + newTag.Length - 1;
                    }
                    else if (fullTag.StartsWith("<sprite"))
                    {
                        _actualSpritesList.Add(fullTag);
                        text = text.Remove(i, fullTag.Length).Insert(i, "◌");
                    }

                    i = endTag;
                }
            }
        }

        int writeCursor = 0;
        int scriptIndex = 0;
        int spriteIndex = 0;

        while (writeCursor <= text.Length)
        {
            if (_actualMiddleScriptsListIndex.Contains(writeCursor))
            {
                _onMiddleScriptRunning = true;

                if (_actualMiddleScriptsList[scriptIndex][0] == '&')
                {
                    yield return StartCoroutine(_dialogueManager.ExecuteCoroutine(_actualMiddleScriptsList[scriptIndex].Substring(1)));
                }
                else
                {
                    _dialogueManager.ExecuteMethod(_actualMiddleScriptsList[scriptIndex]);
                }

                _onMiddleScriptRunning = false;
                scriptIndex++;
            }

            if (writeCursor < text.Length && text[writeCursor] == '<')
            {
                int endTag = text.IndexOf('>', writeCursor);
                if (endTag != -1)
                {
                    string fullTag = text.Substring(writeCursor, endTag - writeCursor + 1);
                    if (fullTag.Length == 11 && fullTag.StartsWith("<#") && fullTag.EndsWith("00>"))
                    {
                        string correctedTag = fullTag.Substring(0, fullTag.Length - 3) + ">";
                        text = text.Remove(writeCursor, fullTag.Length).Insert(writeCursor, correctedTag);
                        endTag = writeCursor + correctedTag.Length - 1;
                    }
                    else if (fullTag == "<alpha=#00>")
                    {
                        string correctedTag = "</color>";
                        text = text.Remove(writeCursor, fullTag.Length).Insert(writeCursor, "</color>");
                        endTag = writeCursor + correctedTag.Length - 1;
                    }

                    writeCursor = endTag + 1;
                    continue;
                }
            }

            if (writeCursor < text.Length && text[writeCursor] == '◌')
            {
                text = text.Remove(writeCursor, 1).Insert(writeCursor, _actualSpritesList[spriteIndex]);
                writeCursor += _actualSpritesList[spriteIndex].Length;
                spriteIndex++;
            }

            if (_skipWritingDialogue && !_onMiddleScriptRunning)
            {
                bool toScript = false;
                int end = text.Length;
                if (_actualMiddleScriptsList != null && _actualMiddleScriptsListIndex.Count > scriptIndex && _actualMiddleScriptsListIndex[scriptIndex] > writeCursor)
                {
                    toScript = true;
                    end = _actualMiddleScriptsListIndex[scriptIndex];
                }

                for (int i = writeCursor; i < end && i < text.Length; i++)
                {
                    if (text[i] == '<')
                    {
                        int endTag = text.IndexOf('>', i);
                        if (endTag != -1)
                        {
                            string fullTag = text.Substring(i, endTag - i + 1);
                            if (fullTag.Length == 11 && fullTag.StartsWith("<#") && fullTag.EndsWith("00>"))
                            {
                                string correctedTag = fullTag.Substring(0, fullTag.Length - 3) + ">";
                                text = text.Remove(i, fullTag.Length).Insert(i, correctedTag);
                                endTag = i + correctedTag.Length - 1;
                            }
                            else if (fullTag == "<alpha=#00>")
                            {
                                string correctedTag = "</color>";
                                text = text.Remove(i, fullTag.Length).Insert(i, correctedTag);
                                endTag = i + correctedTag.Length - 1;
                            }
                            i = endTag;
                        }
                    }

                    if (!toScript)
                    {
                        end = text.Length;
                    }
                }

                if (end != text.Length)
                {
                    writeCursor = end;
                    _dialogueText.text = text.Insert(writeCursor, _alphaTag);
                    _skipWritingDialogue = false;
                    continue;
                }
                else
                {
                    _skipWritingDialogue = false;
                    break;
                }
            }

            _dialogueText.text = text.Insert(writeCursor, _alphaTag);
            writeCursor++;
            yield return new WaitForSecondsRealtime(_writingTime);
        }

        _dialogueText.text = text;
        _onWritingDialogue = false;
    }

    #region Animations
    private IEnumerator OpenDialoguePanel()
    {
        _onDialoguePanelAnimation = true;
        _dialoguePanel.SetActive(true);

        if (_dialogueAnimator != null && _openPanelAnimation != null)
        {
            _dialogueAnimator.Play(_openPanelAnimation.name);

            yield return null;

            while (_dialogueAnimator.GetCurrentAnimatorStateInfo(0).shortNameHash == Animator.StringToHash(_openPanelAnimation.name) && _dialogueAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
            {
                yield return null;
            }
        }

        UpdateDialogue();
        _onDialoguePanelAnimation = false;
    }

    private IEnumerator CloseDialogue()
    {
        _onDialoguePanelAnimation = true;

        if (_dialogueAnimator != null && _closePanelAnimation != null)
        {
            _dialogueAnimator.Play(_closePanelAnimation.name);

            yield return null;

            while (_dialogueAnimator.GetCurrentAnimatorStateInfo(0).shortNameHash == Animator.StringToHash(_closePanelAnimation.name) && _dialogueAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
            {
                yield return null;
            }
        }

        ClearDialogue();
        _dialoguePanel.SetActive(false);
        _onDialoguePanelAnimation = false;
        _onDialogue = false;
    }
    #endregion
}