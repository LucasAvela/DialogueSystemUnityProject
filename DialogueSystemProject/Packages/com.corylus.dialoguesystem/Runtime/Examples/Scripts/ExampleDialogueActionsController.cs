using UnityEngine;

public class ExampleDialogueActionsController : MonoBehaviour
{
    [SerializeField] private DialogueController _dialogueController;
    private bool _isSubscribed = false;

    [Header("Examples")]
    [SerializeField] private GameObject _nextButton;

    private void Start()
    {
        if (_dialogueController != null && !_isSubscribed)
        {
            _dialogueController.onDialogueStart += OnDialogueStart;
            _dialogueController.onDialogueUpdate += OnDialogueUpdate;
            _dialogueController.onDialogueFinish += OnDialogueFinish;
            _dialogueController.onDialogueWriteFinish += OnDialogueWriteFinish;
            _isSubscribed = true;
        }
    }

    private void OnEnable()
    {
        if (_dialogueController != null && !_isSubscribed)
        {
            _dialogueController.onDialogueStart += OnDialogueStart;
            _dialogueController.onDialogueUpdate += OnDialogueUpdate;
            _dialogueController.onDialogueFinish += OnDialogueFinish;
            _dialogueController.onDialogueWriteFinish += OnDialogueWriteFinish;
            _isSubscribed = true;
        }
    }

    private void OnDestroy()
    {
        if (_dialogueController != null)
        {
            _dialogueController.onDialogueStart -= OnDialogueStart;
            _dialogueController.onDialogueUpdate -= OnDialogueUpdate;
            _dialogueController.onDialogueFinish -= OnDialogueFinish;
            _dialogueController.onDialogueWriteFinish -= OnDialogueWriteFinish;
            _isSubscribed = false;
        }
    }

    private void OnDisable()
    {
        if (_dialogueController != null)
        {
            _dialogueController.onDialogueStart -= OnDialogueStart;
            _dialogueController.onDialogueUpdate -= OnDialogueUpdate;
            _dialogueController.onDialogueFinish -= OnDialogueFinish;
            _dialogueController.onDialogueWriteFinish -= OnDialogueWriteFinish;
            _isSubscribed = false;
        }
    }

    private void OnDialogueStart()
    {
        print("Dialogue Started ▶️");
    }

    private void OnDialogueUpdate()
    {
        print("Dialogue has been Updated 🔄");
        _nextButton?.SetActive(false);
    }

    private void OnDialogueFinish()
    {
        print("Dialogue has finished 🏁");
        _nextButton?.SetActive(false);
    }

    private void OnDialogueWriteFinish()
    {
        print("Dialogue Write has finished ✏️");
        _nextButton?.SetActive(true);
    }
}
