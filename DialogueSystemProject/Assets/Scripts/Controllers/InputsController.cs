using UnityEngine;
using TMPro;

public class InputsController : MonoBehaviour
{
    [SerializeField] private DialogueManager _dialogueManager;
    [SerializeField] private DialogueController _dialogueController;
    [SerializeField] private SimpleDialogueController _simpleDialogueController;
    [SerializeField] private TMP_Dropdown _languageDropdown;
    [SerializeField] private GameObject _comsumeInput;

    public void Start()
    {
        _dialogueController.onDialogueWriteFinish += ShowNextButton;
        _dialogueController.onDialogueStop += HideNextButton;
    }

    public void StartDialogue(string key)
    {
        _dialogueController.StartDialogue(key);
    }

    public void ConsumeInput()
    {
        _dialogueController.ConsumeInput();
    }

    public void StartSimpleDialogue(string key)
    {
        _simpleDialogueController.StartSimpleDialogue(key);
    }

    public void ChangeLanguage()
    {
        int index = _languageDropdown.value;
        string lang = _languageDropdown.options[index].text;
        _dialogueManager.ChangeLanguage(lang);
    }

    private void ShowNextButton()
    {
        _comsumeInput.SetActive(true);
    }

    private void HideNextButton()
    {
        _comsumeInput.SetActive(false);
    }
}
