using UnityEngine;

public class DialogueController : MonoBehaviour
{
    [SerializeField] private DialogueManager _dialogueManager;

    [Space(10)][Header("Settings")]
    [SerializeField] private string _dialogueKey;
    [SerializeField] private string _simpleDialogueKey;

    public void StartDialogue()
    {
        _dialogueManager.StartDialogue(_dialogueKey);
    }

    public void StartSimpleDialogue()
    {
        _dialogueManager.StartSimpleDialogue(_simpleDialogueKey);
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
