using UnityEngine;

public class DialogueManagerUI : MonoBehaviour
{
    #region Singleton
    private static DialogueManagerUI _instance;
    public static DialogueManagerUI Instance { get { return _instance; } }

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

    [Space(10)]
    [Header("Dependencies")]
    [SerializeField] DialogueParserUI _dialogueParserUI;

    public void Start()
    {
           
    }

    public string TextUI(string key)
    {
        var dialogue = _dialogueParserUI.GetDialogueByKey(key);
        return dialogue.Text[DialogueManager.Instance.ReturnLanguage()];
    }
}
