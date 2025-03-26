using UnityEngine;

public class DialogueAnswerController : MonoBehaviour
{
    [HideInInspector] public string _nextKey = null;

    public void Choose()
    {
        if (_nextKey != null)
        {
            DialogueManager.Instance.UpdateDialogue(_nextKey);
            DialogueManager.Instance.AnswerInput();
        }
    }
}
