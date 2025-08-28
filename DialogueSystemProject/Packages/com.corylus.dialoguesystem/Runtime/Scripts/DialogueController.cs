using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DialogueController : MonoBehaviour
{
    [Header("Dialogue Settings")] // Settings for dialogue display and behavior
    [SerializeField] private GameObject _dialoguePanel;
    [SerializeField] private TextMeshProUGUI _dialogueText;
    [SerializeField] private TextMeshProUGUI _dialogueActorText;
    [SerializeField] private WriteModes _dialogueMode;
    [SerializeField] private float _writingTime = 0.05f;
    [SerializeField] private string _alphaTag = "<alpha=#00>";
    [SerializeField] private string _animationTag = "<#FF0000>";

    [Header("Animations Settings")] // Settings for dialogue animations
    [SerializeField] private Animator _dialogueAnimator;
    [SerializeField] private AnimationClip _openPanelAnimation;
    [SerializeField] private AnimationClip _closePanelAnimation;
    [SerializeField] private AnimationClip _showTextAnimation;

    [Header("Dialogue Runtime Flags")] // Flags to manage dialogue state and transitions
    private bool _onDialogue = false;
    private bool _onWritingDialogue = false;
    private bool _onMiddleScriptRunning = false;
    private bool _onDialoguePanelAnimation = false;
    private bool _onDialogueTextAnimation = false;
    private bool _skipWritingDialogue = false;
    private bool _stopDialogue = false;
    private bool _onQuestion = false;

    [Header("Dialogue Content")] // Variables to hold current dialogue state and content
    private string _actualDialogueKey = null;
    private string _nextDialogueKey = null;
    private string _actualQuestionKey = null;
    private string _actualDialogueActor = null;
    private string _actualDialogueText = null;
    private List<string> _actualStartScriptsList = new List<string>();
    private List<string> _actualMiddleScriptsList = new List<string>();
    private List<string> _actualEndScriptsList = new List<string>();
    private List<string> _actualSpritesList = new List<string>();
    private List<int> _actualMiddleScriptsListIndex = new List<int>();

    [Header("Internals")] // Internal state and references for managing dialogue
    private DialogueManager _dialogueManager = null;
    private Coroutine _writingDialogueCoroutine = null;
    private Coroutine _instantDialogueCoroutine = null;

    [Header("Modules")] // References to other components and modules
    [SerializeField] private ActorDialogueControllerModule _actorController;
    [SerializeField] private QuestionControllerModule _questionController;

    private enum WriteModes
    {
        LetterByLetter,
        InstantText
    }

    // Events to notify other components about dialogue state changes
    public event System.Action onDialogueStart;
    public event System.Action onDialogueUpdate;
    public event System.Action onDialogueFinish;
    public event System.Action onDialogueWriteFinish;
    public event System.Action onQuestionStarted;
    public event System.Action onQuestionFinished;

    private void Start()
    {
        _dialogueManager = DialogueManager.Instance;

        if (_dialogueManager == null)
        {
            Debug.LogError("DialogueManager instance not found. Please ensure it is initialized before using DialogueController.");
            return;
        }

        ClearDialogue();
    }

    public void StartDialogue(string key) // Method to initiate dialogue with a specific key
    {
        if (!_onDialogue && !_onDialoguePanelAnimation)
        {
            onDialogueStart?.Invoke();
            _onDialogue = true;
            _actualDialogueKey = key;
            StartCoroutine(OpenDialoguePanel());
        }
    }

    public void ConsumeInput() // Method to handle input during dialogue
    {
        if (!_onDialogue || _onDialoguePanelAnimation || _onDialogueTextAnimation || _onMiddleScriptRunning || _onQuestion) return;

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
                    _dialogueManager.ExecuteMethod(this, script);
                }
            }
        }

        if (_questionController._questionMode == QuestionControllerModule.QuestionsModes.OnDialogueAdvance && _actualQuestionKey != null)
        {
            OnQuestionStart();
            return;
        }

        if (_nextDialogueKey != null)
        {
            _actualDialogueKey = _nextDialogueKey;
            UpdateDialogue();
        }
        else
        {
            onDialogueFinish?.Invoke();
            ClearDialogue();
            StartCoroutine(CloseDialogue());
        }
    }

    public void StopDialogue() // Method to stop the current dialogue
    {
        if (!_onDialogue) return;

        _stopDialogue = true;

        if (!_onMiddleScriptRunning)
        {
            if (_writingDialogueCoroutine != null) StopCoroutine(_writingDialogueCoroutine);
            if (_instantDialogueCoroutine != null) StopCoroutine(_instantDialogueCoroutine);

            _onDialoguePanelAnimation = false;
            _onWritingDialogue = false;
            ClearDialogue();
            StartCoroutine(CloseDialogue());
            _onDialogue = false;
            _stopDialogue = false;
            onDialogueFinish?.Invoke();
        }
    }

    private void UpdateDialogue() // Method to update the dialogue content based on the current key
    {
        ClearDialogue();
        DialogueData dialogueData = _dialogueManager.GetDialogueData(_actualDialogueKey);
        onDialogueUpdate?.Invoke();

        _actualDialogueKey = dialogueData.Key;
        _nextDialogueKey = dialogueData.NextKey;
        _actualQuestionKey = dialogueData.Question;
        _actualDialogueActor = (dialogueData.Actor == _dialogueManager.NPCActorKey && _actorController != null) ? _actorController.Name() : dialogueData.Actor;
        _actualDialogueText = dialogueData.Text;
        _actualStartScriptsList = dialogueData.StartScriptsList;
        _actualMiddleScriptsList = dialogueData.MiddleScriptsList;
        _actualEndScriptsList = dialogueData.EndScriptsList;

        DisplayDialogue();
    }

    private void ClearDialogue() // Method to clear the current dialogue state and reset variables
    {
        _dialogueText.text = "";
        _dialogueActorText.text = "";
        _nextDialogueKey = null;
        _actualQuestionKey = null;
        _actualDialogueActor = null;
        _actualDialogueText = null;
        _actualStartScriptsList.Clear();
        _actualMiddleScriptsList.Clear();
        _actualEndScriptsList.Clear();
        _actualMiddleScriptsListIndex.Clear();
    }

    public void ClearTextsUI()
    {
        _dialogueText.text = "";
        _dialogueActorText.text = "";
    }

    private void DisplayDialogue() // Method to display the dialogue text and actor name
    {
        if (_dialogueMode == WriteModes.LetterByLetter)
        {
            _writingDialogueCoroutine = StartCoroutine(WriteDialogue());
        }
        else if (_dialogueMode == WriteModes.InstantText)
        {
            _instantDialogueCoroutine = StartCoroutine(DisplayInstantDialogue());
        }

        _dialogueActorText.text = _actualDialogueActor;
    }

    private IEnumerator DisplayInstantDialogue() // Method to display dialogue text instantly
    {
        string text = _actualDialogueText;
        _onDialogueTextAnimation = true;

        if (_actualStartScriptsList != null)
        {
            var startScriptsCopy = new List<string>(_actualStartScriptsList);
            foreach (string script in startScriptsCopy)
            {
                _dialogueManager.ExecuteMethod(this, script);
            }
        }

        text = text.Replace(_dialogueManager.midScriptChar.ToString(), "");

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
                    _dialogueManager.ExecuteMethod(this, script);
                }
            }
            _onMiddleScriptRunning = false;
        }

        onDialogueWriteFinish?.Invoke();
        if (_stopDialogue) StopDialogue();
        yield return null;
    }

    private IEnumerator WriteDialogue() // Method to write dialogue text letter by letter
    {
        string text = _actualDialogueText;
        _onWritingDialogue = true;

        if (_actualStartScriptsList != null)
        {
            var startScriptsCopy = new List<string>(_actualStartScriptsList);
            foreach (string script in startScriptsCopy)
            {
                _dialogueManager.ExecuteMethod(this, script);
            }
        }

        for (int i = 0; i < text.Length; i++)
        {
            if (text[i] == _dialogueManager.midScriptChar)
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
        int animCursor = 0;
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
                    _dialogueManager.ExecuteMethod(this, _actualMiddleScriptsList[scriptIndex]);
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

                    if (text[i] == '◌')
                    {
                        text = text.Remove(i, 1).Insert(i, _actualSpritesList[spriteIndex]);
                        i += _actualSpritesList[spriteIndex].Length;
                        spriteIndex++;
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

            if (_stopDialogue) StopDialogue();
            if (animCursor >= 0 && animCursor < text.Length)
            {
                _dialogueText.text = text.Insert(writeCursor, _alphaTag).Insert(animCursor, _animationTag);
            }
            else
            {
                _dialogueText.text = text.Insert(writeCursor, _alphaTag);
            }
            writeCursor++;
            animCursor = writeCursor - 1;
            yield return new WaitForSecondsRealtime(_writingTime);
        }

        _dialogueText.text = text;
        _onWritingDialogue = false;
        OnWritingComplete();
    }

    private void OnWritingComplete() // Method called when writing dialogue is complete
    {
        onDialogueWriteFinish?.Invoke();

        if (_questionController._questionMode == QuestionControllerModule.QuestionsModes.OnWriteFinish && _actualQuestionKey != null)
        {
            OnQuestionStart();
        }
    }

    private void OnQuestionStart()
    {
        _questionController.DisplayQuestions(this, _actualQuestionKey);
        _onQuestion = true;
        onQuestionStarted?.Invoke();
    }

    public void OnQuestionComplete(QuestionsEntry question)
    {
        onQuestionFinished?.Invoke();
        _onQuestion = false;
        _actualDialogueKey = question.NextKey;
        _actualQuestionKey = null;
        UpdateDialogue();
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

    private IEnumerator EnableInstantText()
    {
        if (_dialogueAnimator != null && _showTextAnimation != null)
        {
            _dialogueAnimator.Play(_showTextAnimation.name);

            yield return null;

            while (_dialogueAnimator.GetCurrentAnimatorStateInfo(0).shortNameHash == Animator.StringToHash(_showTextAnimation.name) && _dialogueAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
            {
                yield return null;
            }
        }

        _onDialogueTextAnimation = false;
    }
    #endregion
}