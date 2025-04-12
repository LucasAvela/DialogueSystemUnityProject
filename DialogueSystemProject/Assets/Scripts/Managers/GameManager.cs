using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    #region Singleton
    private static GameManager _instance;
    public static GameManager Instance { get { return _instance; } }

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

    [Header("Player Settings")]
    [SerializeField] private string _playerName;

    public string ReturnPlayerName()
    {
        return _playerName;
    }

    [Header("Action Settings")]
    [SerializeField] private string _actionName;
    [SerializeField] private InputActionAsset _inputActions;

    public string ReturnActionButton()
    {
        InputAction action = null;

        foreach (var map in _inputActions.actionMaps)
        {
            action = map.FindAction(_actionName, false);
            if (action != null)
            {
                break;
            }
        }

        string readablePath = null;

        foreach (var binding in action.bindings)
        {
            readablePath = InputControlPath.ToHumanReadableString(binding.effectivePath,InputControlPath.HumanReadableStringOptions.OmitDevice);
        }
        
        return $"<sprite=\"SpriteButtons\" name={readablePath}_button>";
    }

    [ContextMenu("Print Action Button")]
    public void PrintActionButton()
    {
        string actionButton = ReturnActionButton();
        Debug.Log(actionButton);
    }
}
