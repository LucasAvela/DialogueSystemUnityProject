using UnityEngine;
using TMPro;

public class DialogueLanguageController : MonoBehaviour
{
    [SerializeField] TMP_Dropdown _dropdown;

    public void SetLanguage()
    {
        string lang = _dropdown.options[_dropdown.value].text;
        string language;

        switch (lang)
        {
            case "English (US)":
                language = "en_us";
                break;
            case "English (UK)":
                language = "en_gb";
                break;
            case "Português (Brasil)":
                language = "pt_br";
                break;
            case "Español":
                language = "es";
                break;
            case "Italiano":
                language = "it";
                break;
            case "Français":
                language = "fr";
                break;
            case "Deutsch":
                language = "de";
                break;
            case "Polski":
                language = "pl";
                break;
            case "Русский":
                language = "ru";
                break;
            case "日本語":
                language = "ja";
                break;
            case "한국어":
                language = "ko";
                break;
            case "中文":
                language = "zh";
                break;
            default:
                language = "en_us";
                break;
        }

        DialogueManager.Instance.ChangeLanguage(language);
    }
}
