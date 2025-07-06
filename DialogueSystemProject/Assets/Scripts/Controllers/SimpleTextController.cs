using UnityEngine;
using TMPro;

public class SimpleTextController : MonoBehaviour
{
    [Header("Text Settings")] // Settings for the static text component
    [SerializeField] string _key;

    [Header("Internals")] // Internal state and references for managing dialogue
    private DialogueManager _dialogueManager;

    void Start()
    {
        _dialogueManager = DialogueManager.Instance;

        if (_dialogueManager != null)
        {
            UpdateText();
            _dialogueManager.onDialogueUpdated += UpdateText;
        }
        else
        {
            Debug.LogError("DialogueManager instance not found. Please ensure it is initialized before using SimpleTextController.");
            return;
        }
    }

    private void UpdateText()
    {
        var tmpUGUI = GetComponent<TextMeshProUGUI>();
        if (tmpUGUI != null)
        {
            tmpUGUI.text = _dialogueManager.GetSimpleText(_key);
            return;
        }

        var tmp = GetComponent<TextMeshPro>();
        if (tmp != null)
        {
            tmp.text = _dialogueManager.GetSimpleText(_key);
            return;
        }

        Debug.LogWarning("SimpleTextController: No TextMeshPro or TextMeshProUGUI component found on the GameObject.");
    }
}
