using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using UnityEngine.Networking;

public class DialogueSheetsParser : EditorWindow
{
    string _sheetID;
    int _gidDialogue;
    int _gidSimpleDialogue;
    int _gidUI;
    int _gidQuestion;

    int _languagesCount;
    string[] _texts;
    string[] _actors;

    bool[] _languageFoldouts;
    bool _showLanguages = true;
    string _dialogueSystemFolder = Application.dataPath + "/Resources/DialogueSystem";

    System.Collections.IEnumerator _coroutine;
    Queue<System.Collections.IEnumerator> _coroutineQueue = new Queue<System.Collections.IEnumerator>();
    bool _isRunningCoroutine = false;

    [MenuItem("Tools/Dialogue")]
    public static void GetWindow()
    {
        GetWindow<DialogueSheetsParser>("Dialogue");
    }

    void OnGUI()
    {
        GUILayout.Label("Dialogue Sheets Parser", EditorStyles.boldLabel);

        _sheetID = EditorGUILayout.TextField("Sheet ID", _sheetID);
        _gidDialogue = EditorGUILayout.IntField("GID Dialogue", _gidDialogue);
        _gidSimpleDialogue = EditorGUILayout.IntField("GID Simple Dialogue", _gidSimpleDialogue);
        _gidUI = EditorGUILayout.IntField("GID UI", _gidUI);
        _gidQuestion = EditorGUILayout.IntField("GID Question", _gidQuestion);

        EditorGUILayout.Space();

        _languagesCount = EditorGUILayout.IntField("Languages Count", _languagesCount);

        if (_texts == null || _texts.Length != _languagesCount)
        {
            _texts = new string[_languagesCount];
            _actors = new string[_languagesCount];
            _languageFoldouts = new bool[_languagesCount];
        }

        EditorGUI.indentLevel++;
        if (_languagesCount > 0)
        {
            _showLanguages = EditorGUILayout.Foldout(_showLanguages, "Languages");
        }

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

        EditorGUILayout.Space();
        if (GUILayout.Button("Parse Dialogue Sheet"))
        {
            EnqueueDownloads();
        }
    }

    void EnqueueDownloads()
    {
        _coroutineQueue.Enqueue(DownloadCSV(_gidDialogue, "dialogue_sheet.csv"));
        _coroutineQueue.Enqueue(DownloadCSV(_gidSimpleDialogue, "simple_dialogue_sheet.csv"));
        _coroutineQueue.Enqueue(DownloadCSV(_gidUI, "ui_sheet.csv"));
        _coroutineQueue.Enqueue(DownloadCSV(_gidQuestion, "question_sheet.csv"));
        TryStartNextCoroutine();
    }

    System.Collections.IEnumerator DownloadCSV(int gid, string fileName)
    {
        string url = $"https://docs.google.com/spreadsheets/d/{_sheetID}/export?format=csv&gid={gid}";
        string savePath = Path.Combine(_dialogueSystemFolder, fileName);

        Debug.Log("Baixando CSV de: " + url);

        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            var operation = www.SendWebRequest();

            while (!operation.isDone)
                yield return null;

            if (www.result == UnityWebRequest.Result.Success)
            {
                File.WriteAllText(savePath, www.downloadHandler.text);
                Debug.Log("CSV salvo em: " + savePath);
                EditorUtility.DisplayDialog("Sucesso", $"CSV '{fileName}' baixado com sucesso!", "OK");
            }
            else
            {
                Debug.LogError("Erro ao baixar CSV: " + www.error);
                EditorUtility.DisplayDialog("Erro", $"Falha ao baixar o CSV '{fileName}'.\n" + www.error, "OK");
            }
        }
    }

    void TryStartNextCoroutine()
    {
        if (!_isRunningCoroutine && _coroutineQueue.Count > 0)
        {
            _coroutine = _coroutineQueue.Dequeue();
            _isRunningCoroutine = true;
            EditorApplication.update += RunCoroutine;
        }
    }

    void RunCoroutine()
    {
        if (_coroutine == null || !_coroutine.MoveNext())
        {
            EditorApplication.update -= RunCoroutine;
            _coroutine = null;
            _isRunningCoroutine = false;
            TryStartNextCoroutine();
        }
    }
}
