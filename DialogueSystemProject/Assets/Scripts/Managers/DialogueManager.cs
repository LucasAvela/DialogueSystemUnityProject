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

    [Space(10)][Header("Dependencies")]
    [SerializeField] DialogueParser _dialogueParser;
    [SerializeField] DialogueParserUI _dialogueParserUI;
    [SerializeField] DialogueParserSimple _dialogueParserSimple;
    [SerializeField] Languages _language;

    [Space(10)][Header("Dialogue")]
    [SerializeField] GameObject _dialoguePanel;
    [SerializeField] TextMeshProUGUI _dialogueText;
    [SerializeField] TextMeshProUGUI _dialogueActor;
    [SerializeField] float _writingTime = 0.01f;
    [SerializeField] float _animationTime = 0.1f;
    Coroutine _writingDialogueCoroutine;

    [Space(10)][Header("Simple Dialogue")]
    [SerializeField] TextMeshProUGUI _dialogueSimple;
    [SerializeField] float _simpleDialogueTime = 1f;
    [SerializeField] float _simpleDialogueWaitTime = 2f;
    Coroutine _simpleDialogueCoroutine;

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

    [Space(10)]
    [Header("Debug")]
    [SerializeField] private string _actualKey = null;
    [SerializeField] private string _actualActor = null;
    [SerializeField] private bool _onDialogue = false;
    [SerializeField] private bool _writing = false;
    [SerializeField] private bool _middleScriptRunning = false;
    [SerializeField] private bool _skipWriting = false;
    [SerializeField] private bool _animating = false;

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

                if (dialogue.Scripts.End != null)
                {
                    foreach (string endScript in dialogue.Scripts.End)
                    {
                        DialogueScriptsManager.Instance.EndScript(endScript);
                    }
                }
            }
            else
            {   
                if (!_middleScriptRunning)
                {
                    _skipWriting = true;
                }
            }
        }
    }

    public void UpdateDialogue(string key)
    {
        ClearDialogue();
        _actualKey = key;
        var dialogue = _dialogueParser.GetDialogueByKey(key);
        _writingDialogueCoroutine = StartCoroutine(WriteDialogue(dialogue.Text[ReturnLanguage()]));
        _actualActor = dialogue.Actor[ReturnLanguage()];
        _dialogueActor.text = _actualActor;
    }

    private IEnumerator WriteDialogue(string text)
    {
        _writing = true;

        var dialogue = _dialogueParser.GetDialogueByKey(_actualKey);
        if (dialogue.Scripts.Start != null)
        {
            foreach (string startScript in dialogue.Scripts.Start)
            {
                DialogueScriptsManager.Instance.StartScript(startScript);
            }
        }

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

            if (text[i] == '{')
            {
                int endMidScriptTag = text.IndexOf('}', i);
                if (endMidScriptTag != -1)
                {
                    string fullMidScriptTag = text.Substring(i + 1, endMidScriptTag - i - 1);
                    _middleScriptRunning = true;
                    yield return StartCoroutine(DialogueScriptsManager.Instance.MiddleScript(dialogue.Scripts.Middle[int.Parse(fullMidScriptTag)]));
                    _middleScriptRunning = false;
                    i = endMidScriptTag + 1;
                    continue;
                }
            }

            if (_skipWriting & !_middleScriptRunning)
            {
                int nextMidScriptIndex = text.IndexOf("{", i);
                if (nextMidScriptIndex != -1)
                {
                    string textUntilMidScript = text.Substring(i, nextMidScriptIndex - i);
                    _dialogueText.text += textUntilMidScript;
                    i = nextMidScriptIndex;
                    _skipWriting = false;
                    continue;
                }
                else
                {
                    _dialogueText.text = System.Text.RegularExpressions.Regex.Replace(text, "\\{\\d+\\}", "");
                    _skipWriting = false;
                    break;
                }
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

    public void StartSimpleDialogue(string key)
    {
        var dialogue = _dialogueParserSimple.GetDialogueByKey(key);
        _dialogueSimple.text = dialogue.Text[ReturnLanguage()];

        if (_simpleDialogueCoroutine != null) {StopCoroutine(_simpleDialogueCoroutine);}
        _simpleDialogueCoroutine = StartCoroutine(EnableSimpleDialogue());
    }

    private IEnumerator EnableSimpleDialogue()
    {   
        _dialogueSimple.gameObject.SetActive(true);
        
        float elapsedTime = 0f;
        Color color = _dialogueSimple.color;
        color.a = 0f;
        _dialogueSimple.color = color;

        while (elapsedTime < _simpleDialogueTime)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Clamp01(elapsedTime / _simpleDialogueTime);
            _dialogueSimple.color = color;
            yield return null;
        }

        yield return new WaitForSeconds(_simpleDialogueWaitTime);

        elapsedTime = 0f;
        while (elapsedTime < _simpleDialogueTime)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Clamp01(1f - (elapsedTime / _simpleDialogueTime));
            _dialogueSimple.color = color;
            yield return null;
        }

        _dialogueSimple.gameObject.SetActive(false);
    }

    public string TextUI(string key)
    {
        var dialogue = _dialogueParserUI.GetDialogueByKey(key);
        return dialogue.Text[ReturnLanguage()];
    }

    public string TextSimple(string key)
    {
        var dialogue = _dialogueParserSimple.GetDialogueByKey(key);
        return dialogue.Text[ReturnLanguage()];
    }

    public event System.Action onDialogueUIUpdated;

    public void ChangeLanguage(Languages lang)
    {
        _language = lang;
        onDialogueUIUpdated?.Invoke();

        if (_onDialogue)
        {
            StopCoroutine(_writingDialogueCoroutine);
            UpdateDialogue(_actualKey);
        }
    }
}
