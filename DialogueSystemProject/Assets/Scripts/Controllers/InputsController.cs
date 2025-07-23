using UnityEngine;
using TMPro;

public class InputsController : MonoBehaviour
{
    [SerializeField] private DialogueManager _dialogueManager;
    [SerializeField] private DialogueController _dialogueController;
    [SerializeField] private SimpleDialogueController _simpleDialogueController;
    [SerializeField] private TMP_Dropdown _languageDropdown;

    public void StartDialogue(string key)
    {
        _dialogueController.StartDialogue(key);
    }

    public void ConsumeDialogue()
    {
        _dialogueController.ConsumeInput();
    }
}
