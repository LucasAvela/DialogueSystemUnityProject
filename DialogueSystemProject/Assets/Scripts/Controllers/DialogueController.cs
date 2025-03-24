using UnityEngine;

public class DialogueController : MonoBehaviour
{
    [SerializeField] string _key;

    [Header("Simple Dialogue Debug")]
    [SerializeField] GameObject _simpleDialogueDebug;

    public void StartDialogue()
    {
        DialogueManager.Instance.StartDialogue(_key);
    }

    public void ConsumeInput()
    {
        DialogueManager.Instance.ConsumeInput();
    }

    public void StartSimpleDialogue()
    {
        _simpleDialogueDebug.SetActive(true);
    }
}
