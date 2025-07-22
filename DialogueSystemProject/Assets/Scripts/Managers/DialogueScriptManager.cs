using System;
using System.Collections;
using System.Reflection;
using UnityEngine;

public class DialogueScriptManager : MonoBehaviour, MethodReflection
{
    private DialogueManager _dialogueManager;

    void Start()
    {
        _dialogueManager = DialogueManager.Instance;
    }

    public string InsertText(string insert, string text)
    {
        switch (insert)
        {
            case "PlayerName":
                return text.Replace("{PlayerName}", GameManager.Instance.ReturnPlayerName());

            case "ActionButton":
                return text.Replace("{ActionButton}", GameManager.Instance.ReturnActionButton());

            default:
                return text;
        }
    }
}

public interface MethodReflection
{

}
