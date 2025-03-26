using UnityEngine;

public class DialogueAnswerController : MonoBehaviour
{
    public string _nextKey = null;

    public void Choose()
    {
        if (_nextKey != null)
        {
            DialogueManager.Instance.UpdateDialogue(_nextKey);
            DialogueScriptsManager.Instance.DestroyAllAnswers();
            DialogueScriptsManager.Instance._waitingInput = false;
        }
    }
}
