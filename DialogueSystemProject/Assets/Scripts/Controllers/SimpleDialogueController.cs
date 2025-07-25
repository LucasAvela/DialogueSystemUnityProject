using UnityEngine;
using TMPro;
using System.Collections;

public class SimpleDialogueController : MonoBehaviour
{
    [Header("Simple Dialogue Settings")] // Settings for the simple dialogue UI
    [SerializeField] private TextMeshProUGUI _simpleDialogueText;
    [SerializeField] private float _dialogueDuration;

    [Header("Animations Settings")]
    [SerializeField] private Animator _dialogueAnimator;
    [SerializeField] private AnimationClip _enableDialogue;
    [SerializeField] private AnimationClip _disableDialogue;

    [Header("Dialogue Runtime Flags")] // Flags to control dialogue state and behavior
    [TextArea(1, 2)][SerializeField] private string _actualSimpleDialogueKey = null;
    [TextArea(3, 9)][SerializeField] private string _actualSimpleDialogueText = null;
    [SerializeField] private bool _onDialogue = false;
    [SerializeField] private bool _onDialogueAnimation = false;

    [Header("Internals")] // Internal state and references for managing dialogue
    private DialogueManager _dialogueManager;

    void Start()
    {
        _dialogueManager = DialogueManager.Instance;

        if (_dialogueManager == null)
        {
            Debug.LogError("DialogueManager instance not found. Please ensure it is initialized before using SimpleDialogueController.");
            return;
        }
    }

    public void StartSimpleDialogue(string key)
    {
        if (_onDialogue || _onDialogueAnimation) return;

        _onDialogue = true;
        _onDialogueAnimation = true;

        _actualSimpleDialogueKey = key;
        _actualSimpleDialogueText = _dialogueManager.GetSimpleDialogue(key);
        _simpleDialogueText.text = _actualSimpleDialogueText; ;

        StartCoroutine(EnableSimpleDialogue());
    }

    private IEnumerator EnableSimpleDialogue()
    {
        _simpleDialogueText.gameObject.SetActive(true);

        if (_dialogueAnimator != null && _enableDialogue != null)
        {
            _dialogueAnimator.Play(_enableDialogue.name);

            yield return null;

            while (_dialogueAnimator.GetCurrentAnimatorStateInfo(0).shortNameHash == Animator.StringToHash(_enableDialogue.name) && _dialogueAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
            {
                yield return null;
            }
        }

        _onDialogueAnimation = false;

        yield return new WaitForSeconds(_dialogueDuration);
        _onDialogueAnimation = true;
        StartCoroutine(DisableSimpleDialogue());
    }

    private IEnumerator DisableSimpleDialogue()
    {
        if (_dialogueAnimator != null && _disableDialogue != null)
        {
            _dialogueAnimator.Play(_disableDialogue.name);

            yield return null;

            while (_dialogueAnimator.GetCurrentAnimatorStateInfo(0).shortNameHash == Animator.StringToHash(_disableDialogue.name) && _dialogueAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
            {
                yield return null;
            }
        }

        _onDialogueAnimation = false;
        _onDialogue = false;

        _simpleDialogueText.gameObject.SetActive(false);
        _actualSimpleDialogueKey = null;
        _actualSimpleDialogueText = null;
        _simpleDialogueText.text = string.Empty;
    }
}
