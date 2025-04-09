using UnityEngine;
using UnityEditor;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

public class DialogueSheetsParser : EditorWindow
{
    private string _sheetID;
    private int _gidDialogue;
    private int _gidSimpleDialogue;
    private int _gidUI;
    private int _gidQuestion;

    private int _languagesCount;
    private string[] _texts;
    private string[] _actors;
    private bool[] _languageFoldouts;
    private bool _showLanguages = true;

    private string _dialogueSystemFolder = Application.dataPath + "/Resources/DialogueSystem";

    [MenuItem("Tools/Dialogue")]
    public static void OpenWindow()
    {
        GetWindow<DialogueSheetsParser>("Dialogue");
    }

    private void OnGUI()
    {
        GUILayout.Label("Dialogue Sheets Parser", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Save Config"))
        {
            SaveConfig();
        }
        if (GUILayout.Button("Load Config"))
        {
            LoadConfig();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        _sheetID = EditorGUILayout.TextField("Google Sheet ID", _sheetID);
        _gidDialogue = EditorGUILayout.IntField("GID - Dialogue", _gidDialogue);
        _gidSimpleDialogue = EditorGUILayout.IntField("GID - Simple Dialogue", _gidSimpleDialogue);
        _gidUI = EditorGUILayout.IntField("GID - UI", _gidUI);
        _gidQuestion = EditorGUILayout.IntField("GID - Question", _gidQuestion);

        EditorGUILayout.Space();

        int newCount = EditorGUILayout.IntField("Languages Count", _languagesCount);
        if (newCount != _languagesCount)
        {
            _languagesCount = newCount;
            _texts = new string[_languagesCount];
            _actors = new string[_languagesCount];
            _languageFoldouts = new bool[_languagesCount];
        }

        if (_languagesCount <= 0) return;

        EditorGUI.indentLevel++;
        _showLanguages = EditorGUILayout.Foldout(_showLanguages, "Languages");

        if (_showLanguages)
        {
            EditorGUI.indentLevel++;
            for (int i = 0; i < _languagesCount; i++)
            {
                _languageFoldouts[i] = EditorGUILayout.Foldout(_languageFoldouts[i], $"Language {i + 1}");
                if (_languageFoldouts[i])
                {
                    EditorGUI.indentLevel++;
                    _texts[i] = EditorGUILayout.TextField($"Text {i + 1} Label", _texts[i]);
                    _actors[i] = EditorGUILayout.TextField($"Actor {i + 1} Label", _actors[i]);
                    EditorGUI.indentLevel--;
                }
            }
            EditorGUI.indentLevel--;
        }
        EditorGUI.indentLevel--;

        EditorGUILayout.Space();
        _dialogueSystemFolder = EditorGUILayout.TextField("Output Folder", _dialogueSystemFolder);

        if (GUILayout.Button("Select Folder"))
        {
            string path = EditorUtility.OpenFolderPanel("Select Output Folder", _dialogueSystemFolder, "");
            if (!string.IsNullOrEmpty(path))
            {
                _dialogueSystemFolder = path;
            }
        }

        EditorGUILayout.Space();
        if (GUILayout.Button("Parse Dialogue Sheet"))
        {
            StartParser();
        }
    }

    private void StartParser()
    {
        List<string> languagesTexts = new List<string>();
        List<string> languagesActors = new List<string>();

        for (int i = 0; i < _languagesCount; i++)
        {
            languagesTexts.Add(_texts[i]);
            languagesActors.Add(_actors[i]);
        }

        Debug.Log("⏳ Downloading sheet...");
        Debug.Log("Texts: " + string.Join(", ", languagesTexts));
        Debug.Log("Actors: " + string.Join(", ", languagesActors));
    }

    [System.Serializable]
    public class DialogueSheetConfig
    {
        public string sheetID;
        public int gidDialogue;
        public int gidSimpleDialogue;
        public int gidUI;
        public int gidQuestion;

        public int languagesCount;
        public string[] texts;
        public string[] actors;
        public string dialogueSystemFolder;
    }

    private void SaveConfig()
    {
        DialogueSheetConfig config = new DialogueSheetConfig
        {
            sheetID = _sheetID,
            gidDialogue = _gidDialogue,
            gidSimpleDialogue = _gidSimpleDialogue,
            gidUI = _gidUI,
            gidQuestion = _gidQuestion,
            languagesCount = _languagesCount,
            texts = _texts,
            actors = _actors,
            dialogueSystemFolder = _dialogueSystemFolder
        };

        string json = JsonConvert.SerializeObject(config, Formatting.Indented);
        string path = Path.Combine(_dialogueSystemFolder, "dialogueParser_config.json");
        File.WriteAllText(path, json);
        Debug.Log($"✅ Config saved at: {path}");
    }

    private void LoadConfig()
    {
        string path = Path.Combine(_dialogueSystemFolder, "dialogueParser_config.json");
        if (!File.Exists(path))
        {
            Debug.LogWarning("⚠️ No config file found.");
            return;
        }

        string json = File.ReadAllText(path);
        DialogueSheetConfig config = JsonConvert.DeserializeObject<DialogueSheetConfig>(json);

        _sheetID = config.sheetID;
        _gidDialogue = config.gidDialogue;
        _gidSimpleDialogue = config.gidSimpleDialogue;
        _gidUI = config.gidUI;
        _gidQuestion = config.gidQuestion;
        _languagesCount = config.languagesCount;
        _texts = config.texts;
        _actors = config.actors;
        _dialogueSystemFolder = config.dialogueSystemFolder;

        _languageFoldouts = new bool[_languagesCount];

        Debug.Log("✅ Config loaded successfully.");
    }
}
