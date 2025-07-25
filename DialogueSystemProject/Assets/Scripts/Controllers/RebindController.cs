using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class RebindController : MonoBehaviour
{
    [SerializeField] private InputActionReference _actionReference;
    [SerializeField] private int _bindingIndex = 0;
    [SerializeField] private GameObject _statusText;

    private InputActionRebindingExtensions.RebindingOperation _rebindingOperation;

    public void StartRebind()
    {   
        _actionReference.action.Disable();
        _statusText.SetActive(true);

        _rebindingOperation = _actionReference.action.PerformInteractiveRebinding(_bindingIndex)
            .WithControlsExcluding("Mouse")
            .WithControlsExcluding("<Keyboard>")
            .OnMatchWaitForAnother(0.1f)
            .OnComplete(operation =>
            {
                operation.Dispose();
                _statusText.SetActive(false);
                _actionReference.action.Enable();
            })
            .Start();
    }
}
