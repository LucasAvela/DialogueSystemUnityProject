using UnityEngine;

public class DialogueController : MonoBehaviour
{
    [SerializeField] string _key;

    public void StartDialogue()
    {
        DialogueManager.Instance.StartDialogue(_key);
    }

    public void ConsumeInput()
    {
        DialogueManager.Instance.ConsumeInput();
    }
}
