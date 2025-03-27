using UnityEngine;
using TMPro;

public class DialogueUIController : MonoBehaviour
{
    [SerializeField] string _key;

    void Start()
    {
        DialogueManager.Instance.onDialogueUpdated += UpdateUI;
        
        UpdateUI();
    }

    void UpdateUI()
    {
        this.gameObject.GetComponent<TextMeshProUGUI>().text = DialogueManager.Instance.GetSimpleText(_key);
    }
}