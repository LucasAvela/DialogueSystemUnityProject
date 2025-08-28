using UnityEngine;

public class ActorDialogueControllerModule : MonoBehaviour
{
    [SerializeField] private string _actorKey;
    
    [Header("Internals")] // Internal state and references for managing dialogue
    private DialogueManager _dialogueManager = null;

    private void Start()
    {
        _dialogueManager = DialogueManager.Instance;

        if (_dialogueManager == null)
        {
            Debug.LogError("DialogueManager instance not found. Please ensure it is initialized before using ActorController.");
            return;
        }
    }

    public string Name()
    {
        return _dialogueManager.GetActor(_actorKey);
    }
}
