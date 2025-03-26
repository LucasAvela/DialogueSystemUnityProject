using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DialogueScriptsManager : MonoBehaviour
{
    #region Singleton
    private static DialogueScriptsManager _instance;
    public static DialogueScriptsManager Instance { get { return _instance; } }

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

    [HideInInspector] public bool _waitingInput;
    
    public string InsertText(string insert, string text)
    {
        switch (insert)
        {
            case "PlayerName":
                return text.Replace("{PlayerName}", GameManager.Instance.ReturnPlayerName());
            
            default:
                return text;
        }
    }

    public string InsertActor(string insert)
    {
        name = insert.Trim('{', '}');
        
        switch (name)
        {
            case "Player":
                return GameManager.Instance.ReturnPlayerName();
            
            default:
                return "Actor not found";
        }
    }

    public void StartScript(string script)
    {
        switch (script)
        {
            case "StartScript(0)":
                print("Chamando StartScript(0)");
                break;
            
            default:
                Debug.LogError($"StartScript Error: Key '{script}' not found in the dictionary. Please check if the key is correct or initialized.");
                break;
        }
    }

    public IEnumerator MiddleScript(string script)
    {
        switch (script)
        {
            case "MiddleScript(0)":
                print("Chamando MiddleScript(0)");
                yield return new WaitForSeconds(1f);
                break;
            
            case "MiddleScript(1)":
                print("Chamando MiddleScript(1)");
                yield return new WaitForSeconds(1f);
                break;
            
            case "MiddleScript(2)":
                print("Chamando MiddleScript(2)");
                yield return new WaitForSeconds(1f);
                break;
            
            case "MiddleScriptAmerico":
                DialogueManager.Instance.StartSimpleDialogue("AmericoSimple");
                yield return new WaitForSeconds(2f);
                break;

            default:
                Debug.LogError($"MiddleScript Error: Key '{script}' not found in the dictionary. Please check if the key is correct or initialized.");
                break;
        }
    }

    public void EndScript(string script)
    {
        switch (script)
        {
            case "EndScript(0)":
                print("Chamando EndScript(0)");
                break;
            
            default:
                Debug.LogError($"EndScript Error: Key '{script}' not found in the dictionary. Please check if the key is correct or initialized.");
                break;
        }
    }

    public IEnumerator QuestionScript(List<string> uiKeys, List<string> nextKeys)
    {   
        _waitingInput = true;
        DialogueManager.Instance._questionContainer.gameObject.SetActive(true);

        for (int i = 0; i < uiKeys.Count; i++)
        {
            GameObject question = Instantiate(DialogueManager.Instance._questionPrefab, DialogueManager.Instance._questionContainer);
            question.GetComponent<DialogueAnswerController>()._nextKey = nextKeys[i];
            question.GetComponentInChildren<TextMeshProUGUI>().text = DialogueManager.Instance.TextUI(uiKeys[i]);
        }
        
        yield return new WaitUntil(() => !_waitingInput);
    }

    public void DestroyAllAnswers()
    {
        foreach (Transform child in DialogueManager.Instance._questionContainer)
        {
            Destroy(child.gameObject);
        }
    }
}
