using UnityEngine;
using TMPro;

public class LanguageChangeController : MonoBehaviour
{   
    [SerializeField] TMP_Dropdown _dropdown;

    public void ChangeLanguage()
    {
        string lang = _dropdown.options[_dropdown.value].text;

        switch (lang)
        {
            case "English (US)":
                DialogueManager.Instance.ChangeLanguage(DialogueManager.Languages.English_US);
                break;
            case "English (UK)":
                DialogueManager.Instance.ChangeLanguage(DialogueManager.Languages.English_GB);
                break;
            case "Português (Brasil)":
                DialogueManager.Instance.ChangeLanguage(DialogueManager.Languages.Portuguese_BR);
                break;
            case "Español":
                DialogueManager.Instance.ChangeLanguage(DialogueManager.Languages.Spanish);
                break;
            case "Italiano":
                DialogueManager.Instance.ChangeLanguage(DialogueManager.Languages.Italian);
                break;
            case "Français":
                DialogueManager.Instance.ChangeLanguage(DialogueManager.Languages.French);
                break;
            case "Deutsch":
                DialogueManager.Instance.ChangeLanguage(DialogueManager.Languages.German);
                break;
            case "Polski":
                DialogueManager.Instance.ChangeLanguage(DialogueManager.Languages.Polish);
                break;
            case "Русский":
                DialogueManager.Instance.ChangeLanguage(DialogueManager.Languages.Russian);
                break;
            case "日本語":
                DialogueManager.Instance.ChangeLanguage(DialogueManager.Languages.Japanese);
                break;
            case "한국어":
                DialogueManager.Instance.ChangeLanguage(DialogueManager.Languages.Korean);
                break;
            case "中文":
                DialogueManager.Instance.ChangeLanguage(DialogueManager.Languages.Chinese);
                break;
            default:
                DialogueManager.Instance.ChangeLanguage(DialogueManager.Languages.English_US);
                break;
        }
    }
}
