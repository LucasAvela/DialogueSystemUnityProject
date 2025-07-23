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
    [SerializeField] private List<string> _actualTagsList = new List<string>();

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
        _actualTagsList.Clear();
    }

    private void DisplayDialogue()
    {
        _dialogueText.text = _actualDialogueText;
        _dialogueActorText.text = _actualDialogueActor;
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