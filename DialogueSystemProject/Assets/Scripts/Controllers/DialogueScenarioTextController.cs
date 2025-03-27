using UnityEngine;
using TMPro;

public class DialogueScenarioTextController : MonoBehaviour
{
    [SerializeField] string _key;

    void Start()
    {
        DialogueManager.Instance.onDialogueUpdated += UpdateText;
        
        UpdateText();
    }

    void UpdateText()
    {
        this.gameObject.GetComponent<TextMeshPro>().text = DialogueManager.Instance.GetSimpleText(_key);
    }
}
