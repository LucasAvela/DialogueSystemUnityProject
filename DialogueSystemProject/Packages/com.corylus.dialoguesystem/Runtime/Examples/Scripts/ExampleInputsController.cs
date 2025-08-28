using UnityEngine;
using TMPro;

public class ExampleInputsController : MonoBehaviour
{
    [SerializeField] private DialogueController _dialogueController;
    [SerializeField] private SimpleDialogueController _simpleDialogueController;
    [SerializeField] private DialogueKeys _dialogueKey;
    [SerializeField] private SimpleDialogueKeys _simpleDialogueKey;
    [SerializeField] private TMP_Dropdown _languageDropdown;

    private enum DialogueKeys
    {
        key_0,
        key_1,
        Player_0,
        npc_0,
        npc_1,
        Americo_0,
        Americo_1,
        Americo_2,
        Americo_3,
        Americo_4,
        Americo_5,
        question_0,
        question_1,
        key_none_0,
        key_none_1,
        key_none_2,
        game_question,
        game_question_mc,
        game_question_cs,
        ActionButton,
        LongSentence,
        ShortSentence,
        LoopSentence,
        StopDialog,
        DestroyDialogue,
        AnswerDialogue
    }

    private enum SimpleDialogueKeys
    {
        SimpleDialogue,
        ClosedDoor,
        AmericoSimple,
        key_player,
        add_bullets,
        Action_button
    }

    public void StartDialogue()
    {
        _dialogueController.StartDialogue(_dialogueKey.ToString());
    }

    public void StartSimpleDialogue()
    {
        _simpleDialogueController.StartSimpleDialogue(_simpleDialogueKey.ToString());
    }

    public void ConsumeDialogue()
    {
        _dialogueController.ConsumeInput();
    }

    public void StopDialogue()
    {
        _dialogueController.StopDialogue();
    }

    public void SetLanguage()
    {
        string lang = _languageDropdown.options[_languageDropdown.value].text;
        
        DialogueManager.Instance.ChangeLanguage(lang);
    }
}
