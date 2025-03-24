using UnityEngine;

public class DialogueController : MonoBehaviour
{
    [SerializeField] string _dialoguekey;
    [SerializeField] string _simpleDialoguekey;

    public void StartDialogue()
    {
        DialogueManager.Instance.StartDialogue(_dialoguekey);
    }

    public void ConsumeInput()
    {
        DialogueManager.Instance.ConsumeInput();
    }

    public void StartSimpleDialogue()
    {
        DialogueManager.Instance.StartSimpleDialogue(_simpleDialoguekey);
    }
}
