using UnityEngine;
using TMPro;

public class InputsController : MonoBehaviour
{
    [SerializeField] private DialogueController _dialogueController;
    [SerializeField] string _dialogueKey;
    [SerializeField] private SimpleDialogueController _simpleDialogueController;
    [SerializeField] private TMP_Dropdown _languageDropdown;

    public void StartDialogue()
    {
        _dialogueController.StartDialogue(_dialogueKey);
    }

    public void ConsumeDialogue()
    {
        _dialogueController.ConsumeInput();
    }
}
