using System;
using System.Collections;
using System.Reflection;
using UnityEngine;

public class DialogueScriptManager : MonoBehaviour
{
    private DialogueManager _dialogueManager;
    private DialogueController _dialogueController;

    [SerializeField] private MonoBehaviour _scriptsContainer;
    private MethodReflection _methodTarget;

    void Awake()
    {
        if (_scriptsContainer != null)
            _methodTarget = _scriptsContainer as MethodReflection;

        if (_methodTarget == null)
            Debug.LogError("_scriptsContainer não implementa MethodReflection!");
    }

    void Start()
    {
        _dialogueManager = DialogueManager.Instance;
    }

    public void CallMethod(DialogueController dialogueController, string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return;

        _dialogueController = dialogueController;

        int openIndex = input.IndexOf('(');
        int closeIndex = input.LastIndexOf(')');

        if (openIndex < 0 || closeIndex < openIndex)
        {
            Debug.LogWarning("Invalid method format. Use MethodName() or MethodName(argument).");
            return;
        }

        string methodName = input.Substring(0, openIndex).Trim();
        string argument = input.Substring(openIndex + 1, closeIndex - openIndex - 1).Trim();

        Type interfaceType = typeof(MethodReflection);
        MethodInfo method = interfaceType.GetMethod(methodName);

        if (method == null || method.ReturnType != typeof(void))
        {
            Debug.LogWarning($"Method '{methodName}' is not valid for CallMethod.");
            return;
        }

        try
        {
            ParameterInfo[] parameters = method.GetParameters();

            if (parameters.Length == 0)
                method.Invoke(_methodTarget, null);
            else if (parameters.Length == 1)
                method.Invoke(_methodTarget, new object[] { argument });
            else
                Debug.LogWarning($"Method '{methodName}' has more than one parameter — not supported.");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error while invoking '{methodName}': {ex.Message}");
        }
    }

    public IEnumerator CallCoroutine(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) yield break;

        int openIndex = input.IndexOf('(');
        int closeIndex = input.LastIndexOf(')');

        if (openIndex < 0 || closeIndex < openIndex)
        {
            Debug.LogWarning("Invalid coroutine format. Use MethodName() or MethodName(argument).");
            yield break;
        }

        string methodName = input.Substring(0, openIndex).Trim();
        string argument = input.Substring(openIndex + 1, closeIndex - openIndex - 1).Trim();

        IEnumerator coroutine = null;

        try
        {
            Type interfaceType = typeof(MethodReflection);
            MethodInfo method = interfaceType.GetMethod(methodName);

            if (method == null || method.ReturnType != typeof(IEnumerator))
            {
                Debug.LogWarning($"Method '{methodName}' is not valid for CallCoroutine.");
                yield break;
            }

            ParameterInfo[] parameters = method.GetParameters();
            object result = null;

            if (parameters.Length == 0)
                result = method.Invoke(_methodTarget, null);
            else if (parameters.Length == 1)
                result = method.Invoke(_methodTarget, new object[] { argument });
            else
            {
                Debug.LogWarning($"Method '{methodName}' has more than one parameter — not supported.");
                yield break;
            }

            coroutine = result as IEnumerator;
            if (coroutine == null)
            {
                Debug.LogWarning($"Method '{methodName}' did not return a valid IEnumerator.");
                yield break;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error while starting coroutine '{methodName}': {ex.Message}");
            yield break;
        }

        yield return StartCoroutine(coroutine);
    }

    public string InsertText(string insert, string text)
    {
        if (_methodTarget == null)
        {
            Debug.LogWarning("No MethodReflection target set for InsertText.");
            return text;
        }

        return _methodTarget.InsertText(insert, text);
    }
}
