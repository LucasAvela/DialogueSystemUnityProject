using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using UnityEngine.Networking;
using Newtonsoft.Json;

public class DialogueSheetsParser : EditorWindow
{
    // Sheet & GIDs
    private string _sheetID;
    private int _gidDialogue;
    private int _gidSimpleDialogue;
    private int _gidUI;
    private int _gidQuestion;

    // Idiomas
    private int _languagesCount;
    private string[] _texts;
    private string[] _actors;
    private bool[] _languageFoldouts;
    private bool _showLanguages = true;

    // Pasta de destino
    private string _dialogueSystemFolder = Application.dataPath + "/Resources/DialogueSystem";

    // Controle de corrotinas
    private Queue<System.Collections.IEnumerator> _coroutineQueue = new Queue<System.Collections.IEnumerator>();
    private System.Collections.IEnumerator _coroutine;
    private bool _isRunningCoroutine = false;

    [MenuItem("Tools/Dialogue")]
    public static void OpenWindow()
    {
        GetWindow<DialogueSheetsParser>("Dialogue");
    }

    private void OnGUI()
    {
        DrawHeader();
        DrawSheetSettings();
        DrawLanguagesSection();
        DrawFolderSection();
        DrawParseButton();
    }

    private void DrawHeader()
    {
        GUILayout.Label("Dialogue Sheets Parser", EditorStyles.boldLabel);
    }

    private void DrawSheetSettings()
    {
        _sheetID = EditorGUILayout.TextField("Sheet ID", _sheetID);
        _gidDialogue = EditorGUILayout.IntField("GID Dialogue", _gidDialogue);
        _gidSimpleDialogue = EditorGUILayout.IntField("GID Simple Dialogue", _gidSimpleDialogue);
        _gidUI = EditorGUILayout.IntField("GID UI", _gidUI);
        _gidQuestion = EditorGUILayout.IntField("GID Question", _gidQuestion);

        EditorGUILayout.Space();

        int newCount = EditorGUILayout.IntField("Languages Count", _languagesCount);
        if (newCount != _languagesCount)
        {
            _languagesCount = newCount;
            InitializeLanguageArrays();
        }
    }

    private void InitializeLanguageArrays()
    {
        _texts = new string[_languagesCount];
        _actors = new string[_languagesCount];
        _languageFoldouts = new bool[_languagesCount];
    }

    private void DrawLanguagesSection()
    {
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
                    _texts[i] = EditorGUILayout.TextField($"Text {i + 1}", _texts[i]);
                    _actors[i] = EditorGUILayout.TextField($"Actor {i + 1}", _actors[i]);
                    EditorGUI.indentLevel--;
                }
            }
            EditorGUI.indentLevel--;
        }
        EditorGUI.indentLevel--;
    }

    private void DrawFolderSection()
    {
        EditorGUILayout.Space();
        _dialogueSystemFolder = EditorGUILayout.TextField("Dialogue System Folder", _dialogueSystemFolder);

        if (GUILayout.Button("Select Folder"))
        {
            string path = EditorUtility.OpenFolderPanel("Dialogue System Folder", _dialogueSystemFolder, "");
            if (!string.IsNullOrEmpty(path))
            {
                _dialogueSystemFolder = path;
            }
        }
    }

    private void DrawParseButton()
    {
        EditorGUILayout.Space();
        if (GUILayout.Button("Parse Dialogue Sheet"))
        {
            StartCSVDownloads();
        }
    }

    private void StartCSVDownloads()
    {
        _coroutineQueue.Enqueue(DownloadCSV(_gidDialogue, "dialogue_sheet.csv"));
        _coroutineQueue.Enqueue(DownloadCSV(_gidSimpleDialogue, "simple_dialogue_sheet.csv"));
        _coroutineQueue.Enqueue(DownloadCSV(_gidUI, "ui_sheet.csv"));
        _coroutineQueue.Enqueue(DownloadCSV(_gidQuestion, "question_sheet.csv"));
        TryStartNextCoroutine();

        DialogueConvertCSVtoJSON();
    }

    private System.Collections.IEnumerator DownloadCSV(int gid, string fileName)
    {
        string url = $"https://docs.google.com/spreadsheets/d/{_sheetID}/export?format=csv&gid={gid}";
        string savePath = Path.Combine(_dialogueSystemFolder, fileName);

        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            var operation = www.SendWebRequest();
            while (!operation.isDone)
                yield return null;

            if (www.result == UnityWebRequest.Result.Success)
            {
                File.WriteAllText(savePath, www.downloadHandler.text);
                Debug.Log("CSV salvo em: " + savePath);
            }
            else
            {
                Debug.LogError("Erro ao baixar CSV: " + www.error);
            }
        }
    }

    private void TryStartNextCoroutine()
    {
        if (!_isRunningCoroutine && _coroutineQueue.Count > 0)
        {
            _coroutine = _coroutineQueue.Dequeue();
            _isRunningCoroutine = true;
            EditorApplication.update += RunCoroutine;
        }
    }

    private void RunCoroutine()
    {
        if (_coroutine == null || !_coroutine.MoveNext())
        {
            EditorApplication.update -= RunCoroutine;
            _coroutine = null;
            _isRunningCoroutine = false;
            TryStartNextCoroutine();
        }
    }

    // csv to json
    string dialogueSheetFilePath = "Assets/Resources/DialogueSystem/dialogue_sheet.csv";
    string simpleDialogueSheetFilePath = "Assets/Resources/DialogueSystem/simple_dialogue_sheet.csv";
    string uiSheetFilePath = "Assets/Resources/DialogueSystem/ui_sheet.csv";
    string questionSheetFilePath = "Assets/Resources/DialogueSystem/question_sheet.csv";
    
    void DialogueConvertCSVtoJSON()
    {
        var lines = File.ReadAllLines(dialogueSheetFilePath);
        if (lines.Length < 2)
        {
            Debug.LogError("CSV file is empty or has no header.");
            return;
        }

        string[] headers = lines[0].Split(',');
        var dataList = new List<Dictionary<string, string>>();


        for (int i = 1; i < lines.Length; i++)
        {
            string[] values = lines[i].Split(',');
            var entry = new Dictionary<string, string>();

            for (int j = 0; j < headers.Length && j < values.Length; j++)
            {
                entry[headers[j]] = values[j];
            }

            dataList.Add(entry);
        }

        string json = JsonConvert.SerializeObject(dataList, Formatting.Indented);
        File.WriteAllText("Assets/Resources/DialogueSystem/dialogue_sheet.json", json);
        Debug.Log("CSV converted to JSON: " + json);
    }

}
