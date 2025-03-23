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
