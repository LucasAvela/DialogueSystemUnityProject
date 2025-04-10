using UnityEngine;

public class DialogueController : MonoBehaviour
{
    [SerializeField] private string[] _dialogueKeys;
    [SerializeField] private string[] _simpleDialogueKeys;
    private int _dialogueKeyIndex = 0;
    private int _simpleDialogueKeyIndex = 0;
    private DialogueManager _dialogueManager;

    [Header("Controller")]
    [SerializeField] string _actualDialogueKey = "";
    [SerializeField] string _actualSimpleDialogueKey = "";
    [SerializeField] bool _nextDialogue = false;
    [SerializeField] bool _nextSimpleDialogue = false;

    void Start()
    {
        if (_dialogueManager == null)
        {
            _dialogueManager = DialogueManager.Instance;
        }

        _actualDialogueKey = _dialogueKeys[_dialogueKeyIndex];
        _actualSimpleDialogueKey = _simpleDialogueKeys[_simpleDialogueKeyIndex];
    }

    void Update()
    {
        if (_nextDialogue)
        {
            _nextDialogue = false;
            _dialogueKeyIndex = (_dialogueKeyIndex + 1) % _dialogueKeys.Length;
            _actualDialogueKey = _dialogueKeys[_dialogueKeyIndex];
        }

        if (_nextSimpleDialogue)
        {
            _nextSimpleDialogue = false;
            _simpleDialogueKeyIndex = (_simpleDialogueKeyIndex + 1) % _simpleDialogueKeys.Length;
            _actualSimpleDialogueKey = _simpleDialogueKeys[_simpleDialogueKeyIndex];
        }
    }

    public void StartDialogue()
    {
        _dialogueManager.StartDialogue(_actualDialogueKey);
    }

    public void StartSimpleDialogue()
    {
        _dialogueManager.StartSimpleDialogue(_actualSimpleDialogueKey);
    }

    public void ConsumeInput()
    {
        _dialogueManager.ConsumeInput();
    }

    public void StopDialogue()
    {
        _dialogueManager.StopDialogue();
    }
}
