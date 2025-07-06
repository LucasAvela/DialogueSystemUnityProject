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

    [Header("Animations Settings")] 
    [SerializeField] private Animator _dialogueAnimator;
    [SerializeField] private AnimationClip _enableDialoguePanel;
    [SerializeField] private AnimationClip _disableDialoguePanel;
    [SerializeField] private AnimationClip _enableText;

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
    [SerializeField] private List<string> _actualTagsList = new List<string>();

    [Header("Internals")] // Internal state and references for managing dialogue
    private DialogueManager _dialogueManager = null;
    private Coroutine _writingDialogueCoroutine = null;
    private Coroutine _instantDialogueCoroutine = null;

    public event System.Action onDialogueStop;
    public event System.Action onDialogueWriteFinish;

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
            StartCoroutine(OpenDialogue(key));
        }
    }

    private void UpdateDialogue(string key)
    {
        ClearDialogue();
        DialogueData dialogueData = _dialogueManager.GetDialogueData(key);

        _actualDialogueKey = dialogueData.Key;
        _nextDialogueKey = dialogueData.NextKey;
        _actualDialogueActor = dialogueData.Actor;
        _actualDialogueText = dialogueData.Text;
        _actualStartScriptsList = dialogueData.StartScriptsList;
        _actualMiddleScriptsList = dialogueData.MiddleScriptsList;
        _actualEndScriptsList = dialogueData.EndScriptsList;

        DisplayDialogue();
    }

    public void ConsumeInput()
    {
        if (!_onDialogue || _onDialoguePanelAnimation || _onDialogueTextAnimation || _onMiddleScriptRunning) return;

        if (_onWritingDialogue) { _skipWritingDialogue = true; return; }

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
            UpdateDialogue(_nextDialogueKey);
        }
        else
        {
            StartCoroutine(CloseDialogue());
        }
    }

    public void StopDialogue()
    {
        if (!_onDialogue || _onDialoguePanelAnimation) return;

        _stopDialogue = true;

        if (!_onMiddleScriptRunning)
        {
            if (_writingDialogueCoroutine != null) StopCoroutine(_writingDialogueCoroutine);

            _onDialoguePanelAnimation = false;
            _onWritingDialogue = false;
            ClearDialogue();
            StartCoroutine(CloseDialogue());
            _onDialogue = false;
            _stopDialogue = false;
            onDialogueStop?.Invoke();
        }
    }

    private void ClearDialogue()
    {
        _dialogueText.text = "";
        _dialogueActorText.text = "";
        _actualDialogueKey = null;
        _nextDialogueKey = null;
        _actualDialogueActor = null;
        _actualDialogueText = null;
        _actualStartScriptsList.Clear();
        _actualMiddleScriptsList.Clear();
        _actualEndScriptsList.Clear();
        _actualTagsList.Clear();
    }

    private void DisplayDialogue()
    {
        if (_isDialogueInstant)
        {
            _instantDialogueCoroutine = StartCoroutine(DisplayInstantDialogue());
        }
        else
        {
            _writingDialogueCoroutine = StartCoroutine(WriteDialogue());
        }

        _dialogueActorText.text = _actualDialogueActor;
    }

    private IEnumerator DisplayInstantDialogue()
    {
        string text = _actualDialogueText;

        _onDialogueTextAnimation = true;

        if (_actualStartScriptsList != null)
        {
            foreach (string script in _actualStartScriptsList)
            {
                if (script[0] == '&')
                {
                    yield return StartCoroutine(_dialogueManager.ExecuteCoroutine(script.Substring(1)));
                }
                else
                {
                    _dialogueManager.ExecuteMethod(script);
                }
            }
        }

        for (int i = 0; i < text.Length; i++)
        {
            if (text[i] == '{')
            {
                int endTag = text.IndexOf('}', i);
                if (endTag != -1)
                {
                    text = text.Remove(i, endTag - i + 1);
                }
            }
        }

        _dialogueText.text = text;
        StartCoroutine(EnableInstantText());

        if (_actualMiddleScriptsList != null)
        {
            _onMiddleScriptRunning = true;
            foreach (string script in _actualMiddleScriptsList)
            {
                if (script[0] == '&')
                {
                    yield return StartCoroutine(_dialogueManager.ExecuteCoroutine(script.Substring(1)));
                }
                else
                {
                    _dialogueManager.ExecuteMethod(script);
                }
            }
            _onMiddleScriptRunning = false;
        }

        onDialogueWriteFinish?.Invoke();
        if (_stopDialogue) StopDialogue();
        yield return null;
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
            if (text[i] == '<')
            {
                int endTag = text.IndexOf('>', i);
                if (endTag != -1)
                {
                    string fullTag = text.Substring(i, endTag - i + 1);
                    _actualTagsList.Add(fullTag);
                    text = text.Remove(i, endTag - i + 1).Insert(i, " ^");
                }
            }
            else if (text[i] == '{')
            {
                int endTag = text.IndexOf('}', i);
                if (endTag != -1)
                {
                    text = text.Remove(i, endTag - i + 1).Insert(i, " ~");
                }
            }
        }

        int writeCursor = 0;
        int tagIndex = 0;
        int scriptIndex = 0;

        while (writeCursor < text.Length)
        {
            if (text[writeCursor] == '^')
            {
                text = text.Remove(writeCursor - 1, 2).Insert(writeCursor - 1, _actualTagsList[tagIndex]);
                tagIndex++;

                int endTag = text.IndexOf('>', writeCursor);
                if (endTag != -1)
                {
                    writeCursor = endTag + 2;
                }

                continue;
            }
            else if (text[writeCursor] == '~')
            {
                text = text.Remove(writeCursor - 1, 2);
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
                continue;
            }

            if (_skipWritingDialogue && !_onMiddleScriptRunning)
            {
                int nextMidScript = text.IndexOf('~', writeCursor);

                if (nextMidScript != -1)
                {
                    for (int i = writeCursor; i < nextMidScript; i++)
                    {
                        if (text[i] == '^')
                        {
                            text = text.Remove(i - 1, 2).Insert(i - 1, _actualTagsList[tagIndex]);
                            tagIndex++;
                        }
                        writeCursor = i;
                    }

                    int endTag = text.IndexOf('>', writeCursor);
                    if (endTag != -1)
                    {
                        writeCursor = endTag + 1;
                    }

                    _skipWritingDialogue = false;
                    continue;
                }
                else
                {
                    for (int i = writeCursor; i < text.Length; i++)
                    {
                        if (text[i] == '^')
                        {
                            text = text.Remove(i - 1, 2).Insert(i - 1, _actualTagsList[tagIndex]);
                            tagIndex++;
                        }
                    }

                    _skipWritingDialogue = false;
                    break;
                }
            }

            _dialogueText.text = text.Insert(writeCursor, _alphaTag);
            if (_stopDialogue) StopDialogue();
            writeCursor++;
            yield return new WaitForSecondsRealtime(_writingTime);
        }

        _dialogueText.text = text;
        _onWritingDialogue = false;
        onDialogueWriteFinish?.Invoke();
    }

    #region Animations
    private IEnumerator OpenDialogue(string key)
    {
        _onDialoguePanelAnimation = true;
        _dialoguePanel.SetActive(true);

        if (_dialogueAnimator != null && _enableDialoguePanel != null)
        {
            _dialogueAnimator.Play(_enableDialoguePanel.name);

            yield return null;

            while (_dialogueAnimator.GetCurrentAnimatorStateInfo(0).shortNameHash == Animator.StringToHash(_enableDialoguePanel.name) && _dialogueAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
            {
                yield return null;
            }
        }

        UpdateDialogue(key);
        _onDialoguePanelAnimation = false;
    }

    private IEnumerator CloseDialogue()
    {
        _onDialoguePanelAnimation = true;

        if (_dialogueAnimator != null && _disableDialoguePanel != null)
        {
            _dialogueAnimator.Play(_disableDialoguePanel.name);

            yield return null;

            while (_dialogueAnimator.GetCurrentAnimatorStateInfo(0).shortNameHash == Animator.StringToHash(_disableDialoguePanel.name) && _dialogueAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
            {
                yield return null;
            }
        }

        ClearDialogue();
        _dialoguePanel.SetActive(false);
        _onDialoguePanelAnimation = false;
        _onDialogue = false;
    }

    private IEnumerator EnableInstantText()
    {
        if (_dialogueAnimator != null && _enableText != null)
        {
            _dialogueAnimator.Play(_enableText.name);

            yield return null;

            while (_dialogueAnimator.GetCurrentAnimatorStateInfo(0).shortNameHash == Animator.StringToHash(_enableText.name) && _dialogueAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
            {
                yield return null;
            }
        }

        _onDialogueTextAnimation = false;
    }
    #endregion
}