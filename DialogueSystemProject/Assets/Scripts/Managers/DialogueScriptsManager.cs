using System.Collections;
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
}
