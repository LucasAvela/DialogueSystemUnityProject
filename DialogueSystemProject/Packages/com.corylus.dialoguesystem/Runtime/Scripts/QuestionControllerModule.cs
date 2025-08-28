using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuestionControllerModule : MonoBehaviour
{
    [Header("Questions Settings")] // Settings for handling questions during dialogue
    [SerializeField] public QuestionsModes _questionMode;
    [SerializeField] private Transform _questionPanel;
    [SerializeField] private GameObject _questionButtonPrefab;

    [Header("Internals")] // Internal state and references for managing dialogue
    private DialogueManager _dialogueManager = null;
    private DialogueController _dialogueController = null;

    public enum QuestionsModes
    {
        OnDialogueAdvance,
        OnWriteFinish
    }

    private void Start()
    {
        _dialogueManager = DialogueManager.Instance;

        if (_dialogueManager == null)
        {
            Debug.LogError("DialogueManager instance not found. Please ensure it is initialized before using QuestionController.");
            return;
        }
    }

    public void DisplayQuestions(DialogueController dialogueController, string actualQuestionKey) // Method to display questions if available
    {
        _dialogueController = dialogueController;

        List<QuestionsEntry> questions = _dialogueManager.GetQuestions(actualQuestionKey);

        foreach (Transform child in _questionPanel)
        {
            Destroy(child.gameObject);
        }

        if (_questionMode == QuestionsModes.OnDialogueAdvance)
        {
            dialogueController.ClearTextsUI();
        }

        _questionPanel.gameObject.SetActive(true);

        foreach (QuestionsEntry question in questions)
        {
            GameObject buttonObj = Instantiate(_questionButtonPrefab, _questionPanel);
            buttonObj.GetComponentInChildren<SimpleTextController>().SetKey(question.TextKey);
            buttonObj.GetComponent<Button>().onClick.AddListener(() => OnQuestionSelected(question));
        }
    }

    public void OnQuestionSelected(QuestionsEntry question) // Method called when a question is selected
    {
        _questionPanel.gameObject.SetActive(false);
        _dialogueController.OnQuestionComplete(question);
    }
}
