using System.Collections;
using UnityEngine;

public interface MethodReflection
{
    void StartScript(string n);
    void MiddleScript(string n);
    void EndScript(string n);
    void StopScript();
    IEnumerator DelayedText();
    IEnumerator DelayedStop();

    string InsertText(string insert, string text);
}

public class ExampleDialogueScripts : MonoBehaviour, MethodReflection
{
    private DialogueController _dialogueController;

    public void StartScript(string n)
    {
        Debug.Log($"StartScript: {n}");
    }

    public void MiddleScript(string n)
    {
        Debug.Log($"MiddleScript: {n}");
    }

    public void EndScript(string n)
    {
        Debug.Log($"EndScript: {n}");
    }

    public void StopScript()
    {
        if (_dialogueController != null)
            _dialogueController.StopDialogue();

        Debug.Log("StopScript: Dialogue stopped.");
    }

    public IEnumerator DelayedText()
    {
        Debug.Log("DelayedText: Waiting...");
        yield return new WaitForSeconds(5f);
        Debug.Log("DelayedText: Done!");
    }

    public IEnumerator DelayedStop()
    {
        Debug.Log("DelayedStop: Waiting...");
        yield return new WaitForSeconds(5f);

        if (_dialogueController != null)
            _dialogueController.StopDialogue();

        Debug.Log("DelayedStop: Dialogue stopped.");
    }

    public string InsertText(string insert, string text)
    {
        switch (insert)
        {
            case "PlayerName":
                return text.Replace("{PlayerName}", ExampleGameManager.Instance.ReturnPlayerName());

            case "ActionButton":
                return text.Replace("{ActionButton}", ExampleGameManager.Instance.ReturnActionButton());

            default:
                return text;
        }
    }

    public void SetController(DialogueController controller)
    {
        _dialogueController = controller;
    }
}
