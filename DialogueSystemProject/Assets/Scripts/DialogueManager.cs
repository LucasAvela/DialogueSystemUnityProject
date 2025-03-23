using System.Collections;
using TMPro;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    #region Singleton
    private static DialogueManager _instance;
    public static DialogueManager Instance { get { return _instance; } }

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }
    #endregion

    [Header("Dependencies")]
    [SerializeField] DialogueParser _dialogueParser;
    [SerializeField] Languages _language;

    [Header("Action")]
    [SerializeField] GameObject _dialoguePanel;
    [SerializeField] TextMeshProUGUI _dialogueText;
    [SerializeField] TextMeshProUGUI _dialogueActor;
    [SerializeField] float _writingTime = 0.01f;
    [SerializeField] float _animationTime = 0.1f;

    public enum Languages
    {
        English_US,
        English_GB,
        Portuguese_BR,
        Spanish,
        Italian,
        French,
        German,
        Polish,
        Russian,
        Japanese,
        Korean,
        Chinese
    }

    public string ReturnLanguage()
    {
        switch (_language)
        {
            case Languages.English_US:
                return "en_us";
            case Languages.English_GB:
                return "en_gb";
            case Languages.Portuguese_BR:
                return "pt_br";
            case Languages.Spanish:
                return "es";
            case Languages.Italian:
                return "it";
            case Languages.French:
                return "fr";
            case Languages.German:
                return "de";
            case Languages.Polish:
                return "pl";
            case Languages.Russian:
                return "ru";
            case Languages.Japanese:
                return "ja";
            case Languages.Korean:
                return "ko";
            case Languages.Chinese:
                return "zh";
            default:
                return "Unknown Language";
        }
    }

    private IEnumerator OpenDialoguePanel(string key)
    {
        yield return StartCoroutine(AnimationDialoguePanel("Open"));
        UpdateDialogue(key);
    }

    private IEnumerator CloseDialoguePanel()
    {
        yield return StartCoroutine(AnimationDialoguePanel("Close"));
    }

    private IEnumerator AnimationDialoguePanel(string perform)
    {
        _animating = true;

        _dialoguePanel.SetActive(true);
        RectTransform rectTransform = _dialoguePanel.GetComponent<RectTransform>();
        float width = rectTransform.rect.width;

        float target = 0;
        float start = rectTransform.rect.width;
        if (perform == "Open")
        {
            target = rectTransform.rect.width;
            start = 0;
        }

        float elapsedTime = 0f;

        while (elapsedTime < _animationTime)
        {
            float newWidht = Mathf.Lerp(start, target, elapsedTime / _animationTime);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, newWidht);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, target);

        if (perform == "Close")
        {
            _dialoguePanel.SetActive(false);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
        }

        _animating = false;
    }

    private string _actualKey = null;
    private bool _onDialogue = false;
    private bool _writing = false;
    private bool _skipWriting = false;
    private bool _animating = false;

    public void StartDialogue(string key)
    {
        if (!_onDialogue)
        {
            ClearDialogue();
            _onDialogue = true;
            StartCoroutine(OpenDialoguePanel(key));
        }
    }

    public void ConsumeInput()
    {
        if (_onDialogue & !_animating)
        {
            if (!_writing)
            {
                var dialogue = _dialogueParser.GetDialogueByKey(_actualKey);
                if (dialogue.Next_Key != null)
                {
                    UpdateDialogue(dialogue.Next_Key);
                }
                else
                {
                    ClearDialogue();
                    _onDialogue = false;
                    StartCoroutine(CloseDialoguePanel());
                }
            }
            else
            {
                _skipWriting = true;
            }
        }
    }

    public void UpdateDialogue(string key)
    {   
        ClearDialogue();
        var dialogue = _dialogueParser.GetDialogueByKey(key);
        StartCoroutine(WriteDialogue(dialogue.Text[ReturnLanguage()]));
        _dialogueActor.text = dialogue.Actor[ReturnLanguage()];
        _actualKey = key;
    }

    private IEnumerator WriteDialogue(string text)
    {
        _writing = true;
        int i = 0;

        while (i < text.Length)
        {
            if (text[i] == '<')
            {
                int endTag = text.IndexOf('>', i);
                if (endTag != -1)
                {
                    string fullTag = text.Substring(i, endTag - i + 1);
                    _dialogueText.text += fullTag;
                    i = endTag + 1;
                    continue;
                }
            }

            if (_skipWriting)
            {
                _dialogueText.text = text;
                _skipWriting = false;
                break;
            }

            _dialogueText.text += text[i];
            i++;
            yield return new WaitForSeconds(_writingTime);
        }

        _writing = false;
    }

    private void ClearDialogue()
    {
        _dialogueText.text = "";
        _dialogueActor.text = "";
        _actualKey = null;
    }

    public void ChangeLanguage(Languages lang)
    {
        _language = lang;
        if (_onDialogue) { UpdateDialogue(_actualKey); }
        Debug.Log($"Dialogue Manager change actual localization to {_language}");
    }
}
