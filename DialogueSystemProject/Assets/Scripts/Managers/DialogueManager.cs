using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

    public event System.Action onActorSpeaking;
    public event System.Action onDialogueUpdated;

    [Space(10)]
    [Header("Dependencies")]
    [SerializeField] private DialogueParser _dialogueParser;
    [SerializeField] private DialogueScriptManager _dialogueScriptManager;
    [SerializeField] private string _language = "en_us";

    [Space(10)]
    [Header("Dialogue")]
    [SerializeField] private GameObject _dialoguePanel;
    [SerializeField] private TextMeshProUGUI _dialogueText;
    [SerializeField] private TextMeshProUGUI _dialogueActor;
    [SerializeField] private float _writingTime = 0.05f;
    [SerializeField] private float _dialoguePanelAnimationTime = 0.1f;
    private Coroutine _writingDialogueCoroutine = null;

    [Space(10)]
    [Header("Simple Dialogue")]
    [SerializeField] private TextMeshProUGUI _simpleDialogueText;
    [SerializeField] private float _simpleDialogueAnimationTime = 1f;
    [SerializeField] private float _simpleDialogueWaitTime = 2f;
    private Coroutine _simpleDialogueCoroutine = null;

    [Space(10)]
    [Header("Question Dialogue")]
    [SerializeField] private Transform _questionDialogueContainer;
    [SerializeField] private GameObject _questionDialogueButtonPrefab;
    [SerializeField] private Transform _questionDialogueTextContainer;
    [SerializeField] private GameObject _questionDialogueTextPrefab;

    [Space(10)]
    [Header("State")]
    // Dialogue
    [TextArea(1, 2)][SerializeField] private string _actualDialogueKey = null;
    [TextArea(1, 2)][SerializeField] private string _actualDialogueActor = null;
    [TextArea(3, 9)][SerializeField] private string _actualDialogueText = null;
    [SerializeField] private bool _onDialogue = false;
    [SerializeField] private bool _onWritingDialogue = false;
    [SerializeField] private bool _onMiddleScriptRunning = false;
    [SerializeField] private bool _onDialoguePanelAnimation = false;
    [SerializeField] private bool _skipWritingDialogue = false;
    [SerializeField] private bool _stopDialogue = false;
    // Simple Dialogue
    [TextArea(1, 2)][SerializeField] private string _actualSimpleDialogueKey = null;
    [SerializeField] private bool _onSimpleDialogue = false;
    // Question Dialogue
    [SerializeField] private bool _onQuestionDialogue = false;

    public void StartDialogue(string key)
    {
        if (!_onDialogue)
        {
            ClearDialogue();
            _onDialogue = true;
            StartCoroutine(OpenDialoguePanel(key));
        }
    }

    public void UpdateDialogue(string key)
    {
        DialogueEntry dialogue = _dialogueParser.GetDialogueByKey(key);
        ClearDialogue();

        _actualDialogueKey = key;
        _actualDialogueActor = dialogue.Actor[_language];
        _actualDialogueText = dialogue.Text[_language];

        DisplayActor(_actualDialogueActor, dialogue);
        if (_writingDialogueCoroutine != null) StopCoroutine(_writingDialogueCoroutine);
        _writingDialogueCoroutine = StartCoroutine(WriteDialogue(_actualDialogueText, dialogue));
    }

    public void ConsumeInput()
    {
        DialogueEntry dialogue = _dialogueParser.GetDialogueByKey(_actualDialogueKey);

        if (!_onDialogue || _onDialoguePanelAnimation || _onQuestionDialogue || _onMiddleScriptRunning) return;

        if (_onWritingDialogue) { _skipWritingDialogue = true; return; }

        if (dialogue.Question != null)
        {
            DisplayQuestionTexts();
            return;
        }

        if (dialogue.Next_Key != null)
        {
            UpdateDialogue(dialogue.Next_Key);
        }
        else
        {
            EndDialogue(dialogue);
        }
    }

    public void StopDialogue()
    {   
        if (!_onDialogue) return;

        _stopDialogue = true;

        if (_onDialogue & !_onMiddleScriptRunning)
        {
            if (_writingDialogueCoroutine != null) StopCoroutine(_writingDialogueCoroutine);
            if (_onQuestionDialogue) AnswerInput();
            _onDialoguePanelAnimation = false;
            _onWritingDialogue = false;
            ClearDialogue();
            StartCoroutine(CloseDialoguePanel());
            _stopDialogue = false;
            _onDialogue = false;
        }
    }

    private void EndDialogue(DialogueEntry dialogue)
    {
        if (dialogue.Scripts.End != null)
        {
            foreach (string end in dialogue.Scripts.End)
            {
                _dialogueScriptManager.EndScript(end);
            }
        }

        ClearDialogue();
        StartCoroutine(CloseDialoguePanel());
        _onDialogue = false;
    }

    private void DisplayActor(string actor, DialogueEntry dialogue)
    {
        if (dialogue.Actor[_language].Contains("{"))
        {
            _dialogueActor.text = _dialogueScriptManager.InsertActor(dialogue.Actor[_language]);
        }
        else
        {
            _dialogueActor.text = actor;
        }
    }

    private void DisplayQuestionButtons()
    {
        _onQuestionDialogue = true;
        _questionDialogueContainer.gameObject.SetActive(true);

        DialogueEntry dialogue = _dialogueParser.GetDialogueByKey(_actualDialogueKey);
        List<QuestionEntry> question = _dialogueParser.GetQuestionDialogueByKey(dialogue.Question);

        foreach (var answer in question)
        {
            GameObject button = Instantiate(_questionDialogueButtonPrefab, _questionDialogueContainer);
            button.GetComponentInChildren<TextMeshProUGUI>().text = GetSimpleText(answer.UIKey);
            button.GetComponent<DialogueAnswerController>()._nextKey = answer.NextKey;
        }
    }

    private void DisplayQuestionTexts()
    {
        _onQuestionDialogue = true;
        _questionDialogueTextContainer.gameObject.SetActive(true);
        _dialogueText.text = "";

        DialogueEntry dialogue = _dialogueParser.GetDialogueByKey(_actualDialogueKey);
        List<QuestionEntry> question = _dialogueParser.GetQuestionDialogueByKey(dialogue.Question);

        float elementHeight = _dialogueText.GetComponent<RectTransform>().rect.height / question.Count;
        _questionDialogueTextContainer.GetComponent<GridLayoutGroup>().cellSize = new Vector2(_questionDialogueTextContainer.GetComponent<GridLayoutGroup>().cellSize.x, elementHeight);

        foreach (var answer in question)
        {
            GameObject text = Instantiate(_questionDialogueTextPrefab, _questionDialogueTextContainer);
            text.GetComponent<TextMeshProUGUI>().text = GetSimpleText(answer.UIKey);
            text.GetComponent<DialogueAnswerController>()._nextKey = answer.NextKey;
        }
    }

    public void AnswerInput()
    {
        _onQuestionDialogue = false;

        if (_questionDialogueContainer != null)
        {
            _questionDialogueContainer.gameObject.SetActive(false);
            foreach (Transform child in _questionDialogueContainer)
            {
                Destroy(child.gameObject);
            }
        }

        if (_questionDialogueTextContainer != null)
        {
            _questionDialogueTextContainer.gameObject.SetActive(false);
            foreach (Transform child in _questionDialogueTextContainer)
            {
                Destroy(child.gameObject);
            }
        }
    }

    private IEnumerator WriteDialogue(string text, DialogueEntry dialogue)
    {
        _onWritingDialogue = true;

        if (dialogue.Scripts.Insert != null)
        {
            foreach (string insert in dialogue.Scripts.Insert)
            {
                text = _dialogueScriptManager.InsertText(insert, text);
            }
        }

        if (dialogue.Scripts.Start != null)
        {
            foreach (string start in dialogue.Scripts.Start)
            {
                _dialogueScriptManager.StartScript(start);
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
                    string MiddleScriptKey = dialogue.Scripts.Middle[int.Parse(fullMidScriptTag)];
                    _onMiddleScriptRunning = true;
                    yield return _dialogueScriptManager.MiddleScript(MiddleScriptKey);
                    _onMiddleScriptRunning = false;
                    if (_stopDialogue) StopDialogue();
                    i = endMidScriptTag + 1;
                    continue;
                }
            }

            if (_skipWritingDialogue & !_onMiddleScriptRunning)
            {
                int nextMidScriptIndex = text.IndexOf("{", i);
                if (nextMidScriptIndex != -1)
                {
                    string textUntilMidScript = text.Substring(i, nextMidScriptIndex - i);
                    _dialogueText.text += textUntilMidScript;
                    i = nextMidScriptIndex;
                    _skipWritingDialogue = false;
                    continue;
                }
                else
                {
                    _dialogueText.text = System.Text.RegularExpressions.Regex.Replace(text, "\\{\\d+\\}", "");
                    _skipWritingDialogue = false;
                    break;
                }
            }

            _dialogueText.text += text[i];
            i++;
            yield return new WaitForSeconds(_writingTime);
        }

        OnWritingComplete(dialogue);
        _onWritingDialogue = false;
    }

    private void OnWritingComplete(DialogueEntry dialogue)
    {
        if (dialogue.Question != null & _questionDialogueContainer != null) DisplayQuestionButtons();
    }

    private void ClearDialogue()
    {
        _dialogueText.text = "";
        _dialogueActor.text = "";

        _actualDialogueKey = null;
        _actualDialogueActor = null;
        _actualDialogueText = null;
        _skipWritingDialogue = false;
    }

    public void StartSimpleDialogue(string key)
    {
        _actualSimpleDialogueKey = key;
        DialogueEntrySimple dialogue = _dialogueParser.GetSimpleDialogueByKey(_actualSimpleDialogueKey);
        string text = dialogue.Text[_language];

        if (dialogue.Scripts.Insert != null)
        {
            foreach (string insert in dialogue.Scripts.Insert)
            {
                text = _dialogueScriptManager.InsertText(insert, text);
            }
        }

        _simpleDialogueText.text = text;
        if (_simpleDialogueCoroutine != null) StopCoroutine(_simpleDialogueCoroutine);
        _simpleDialogueCoroutine = StartCoroutine(SimpleDialogueAnimation());
    }

    public string GetSimpleText(string key)
    {
        DialogueEntryUI dialogue = _dialogueParser.GetUIDialogueByKey(key);
        string text = dialogue.Text[_language];

        if (dialogue.Scripts.Insert != null)
        {
            foreach (string insert in dialogue.Scripts.Insert)
            {
                text = _dialogueScriptManager.InsertText(insert, text);
            }
        }

        return text;
    }

    public void ChangeLanguage(string language)
    {
        _language = language;
        if (_onDialogue) UpdateDialogue(_actualDialogueKey);
        if (_onSimpleDialogue) StartSimpleDialogue(_actualSimpleDialogueKey);
        onDialogueUpdated?.Invoke();
    }

    #region Animations
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
        _onDialoguePanelAnimation = true;

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

        while (elapsedTime < _dialoguePanelAnimationTime)
        {
            float newWidht = Mathf.Lerp(start, target, elapsedTime / _dialoguePanelAnimationTime);
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

        _onDialoguePanelAnimation = false;
    }

    private IEnumerator SimpleDialogueAnimation()
    {
        _onSimpleDialogue = true;
        _simpleDialogueText.gameObject.SetActive(true);

        float elapsedTime = 0f;
        Color color = _simpleDialogueText.color;
        color.a = 0f;
        _simpleDialogueText.color = color;

        while (elapsedTime < _simpleDialogueAnimationTime)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Clamp01(elapsedTime / _simpleDialogueAnimationTime);
            _simpleDialogueText.color = color;
            yield return null;
        }

        yield return new WaitForSeconds(_simpleDialogueWaitTime);

        elapsedTime = 0f;
        while (elapsedTime < _simpleDialogueAnimationTime)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Clamp01(1f - (elapsedTime / _simpleDialogueAnimationTime));
            _simpleDialogueText.color = color;
            yield return null;
        }

        _simpleDialogueText.gameObject.SetActive(false);
        _simpleDialogueText.text = "";
        _actualSimpleDialogueKey = null;
        _onSimpleDialogue = false;
    }
    #endregion
}
