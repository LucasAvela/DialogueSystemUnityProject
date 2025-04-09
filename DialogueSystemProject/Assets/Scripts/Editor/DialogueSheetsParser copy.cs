using UnityEngine;
using UnityEditor;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

public class DialogueSheetsParser2 : EditorWindow
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
            StartParser();
        }
    }

    private void StartParser()
    {
        Debug.Log("⏳ Baixando e processando planilhas...");

        string dialogueCsv = Path.Combine(_dialogueSystemFolder, "Dialogue.csv");
        string dialogueJson = Path.Combine(_dialogueSystemFolder, "Dialogue.json");

        string simpleCsv = Path.Combine(_dialogueSystemFolder, "SimpleDialogue.csv");
        string simpleJson = Path.Combine(_dialogueSystemFolder, "SimpleDialogue.json");

        string uiCsv = Path.Combine(_dialogueSystemFolder, "UI.csv");
        string uiJson = Path.Combine(_dialogueSystemFolder, "UI.json");

        string questionCsv = Path.Combine(_dialogueSystemFolder, "Question.csv");
        string questionJson = Path.Combine(_dialogueSystemFolder, "Question.json");

        DownloadSheet(_gidDialogue, dialogueCsv);
        DownloadSheet(_gidSimpleDialogue, simpleCsv);
        DownloadSheet(_gidUI, uiCsv);
        DownloadSheet(_gidQuestion, questionCsv);

        ParseDialogue(dialogueCsv, dialogueJson);
        ParseSimpleDialogue(simpleCsv, simpleJson);
        ParseUI(uiCsv, uiJson);
        ParseQuestion(questionCsv, questionJson);

        Debug.Log("✅ Arquivos baixados e convertidos com sucesso!");
    }

    private void DownloadSheet(int gid, string filePath)
    {
        string url = $"https://docs.google.com/spreadsheets/d/{_sheetID}/export?format=csv&gid={gid}";
        UnityWebRequest www = UnityWebRequest.Get(url);
        var request = www.SendWebRequest();

        while (!request.isDone) { }

        if (www.result == UnityWebRequest.Result.Success)
        {
            File.WriteAllBytes(filePath, www.downloadHandler.data);
            Debug.Log($"✅ Baixado: {Path.GetFileName(filePath)}");
        }
        else
        {
            Debug.LogError($"❌ Erro ao baixar {Path.GetFileName(filePath)}: {www.error}");
        }
    }

    private int FindIndex(string[] headers, string name)
    {
        for (int i = 0; i < headers.Length; i++)
            if (headers[i].Trim() == name) return i;
        return -1;
    }

    private List<string> SplitScripts(string[] values, int index)
    {
        if (index < 0 || index >= values.Length || string.IsNullOrEmpty(values[index]))
            return new List<string>();
        return new List<string>(values[index].Replace(" ", "").Split('|', System.StringSplitOptions.RemoveEmptyEntries));
    }

    private void ParseDialogue(string csvPath, string jsonPath)
    {
        var dict = new Dictionary<string, object>();
        var lines = File.ReadAllLines(csvPath);
        var headers = lines[0].Split(',');

        int keyIndex = FindIndex(headers, "Key");
        int nextKeyIndex = FindIndex(headers, "Next_Key");
        int insertIndex = FindIndex(headers, "Insert");
        int startIndex = FindIndex(headers, "StartScript");
        int middleIndex = FindIndex(headers, "MiddleScript");
        int endIndex = FindIndex(headers, "EndScript");
        int actorDefaultIndex = FindIndex(headers, "Actor");

        var langIndexes = new List<int>();
        var actorLangIndexes = new List<int>();
        for (int i = 0; i < _languagesCount; i++)
        {
            langIndexes.Add(FindIndex(headers, _texts[i]));
            actorLangIndexes.Add(FindIndex(headers, _actors[i]));
        }

        for (int i = 1; i < lines.Length; i++)
        {
            var values = lines[i].Split(',');

            string key = values[keyIndex].Trim();
            if (string.IsNullOrEmpty(key)) continue;

            var entry = new Dictionary<string, object>
            {
                ["Actor"] = new Dictionary<string, object>(),
                ["Text"] = new Dictionary<string, object>(),
                ["Next_Key"] = string.IsNullOrWhiteSpace(values[nextKeyIndex]) ? null : values[nextKeyIndex].Trim(),
                ["Question"] = null,
                ["Scripts"] = new Dictionary<string, object>
                {
                    ["Insert"] = SplitScripts(values, insertIndex),
                    ["Start"] = SplitScripts(values, startIndex),
                    ["Middle"] = SplitScripts(values, middleIndex),
                    ["End"] = SplitScripts(values, endIndex)
                }
            };

            for (int j = 0; j < _languagesCount; j++)
            {
                string lang = _texts[j];
                string actor = values.Length > actorLangIndexes[j] ? values[actorLangIndexes[j]].Trim() : "";
                if (string.IsNullOrEmpty(actor) && actorDefaultIndex >= 0)
                    actor = values[actorDefaultIndex].Trim();

                ((Dictionary<string, object>)entry["Actor"])[lang] = string.IsNullOrEmpty(actor) ? null : actor;
                ((Dictionary<string, object>)entry["Text"])[lang] = values[langIndexes[j]].Trim();
            }

            dict[key] = entry;
        }

        File.WriteAllText(jsonPath, JsonConvert.SerializeObject(dict, Formatting.Indented));
    }

    private void ParseSimpleDialogue(string csvPath, string jsonPath)
    {
        var dict = new Dictionary<string, object>();
        var lines = File.ReadAllLines(csvPath);
        var headers = lines[0].Split(',');

        int keyIndex = FindIndex(headers, "Key");
        int insertIndex = FindIndex(headers, "Insert");

        var langIndexes = new List<int>();
        for (int i = 0; i < _languagesCount; i++)
            langIndexes.Add(FindIndex(headers, _texts[i]));

        for (int i = 1; i < lines.Length; i++)
        {
            var values = lines[i].Split(',');
            string key = values[keyIndex].Trim();
            if (string.IsNullOrEmpty(key)) continue;

            var entry = new Dictionary<string, object>
            {
                ["Text"] = new Dictionary<string, object>(),
                ["Scripts"] = new Dictionary<string, object>
                {
                    ["Insert"] = SplitScripts(values, insertIndex)
                }
            };

            for (int j = 0; j < _languagesCount; j++)
            {
                string lang = _texts[j];
                ((Dictionary<string, object>)entry["Text"])[lang] = values[langIndexes[j]].Trim();
            }

            dict[key] = entry;
        }

        File.WriteAllText(jsonPath, JsonConvert.SerializeObject(dict, Formatting.Indented));
    }

    private void ParseUI(string csvPath, string jsonPath)
    {
        var dict = new Dictionary<string, object>();
        var lines = File.ReadAllLines(csvPath);
        var headers = lines[0].Split(',');

        int keyIndex = FindIndex(headers, "Key");
        int insertIndex = FindIndex(headers, "Insert");

        var langIndexes = new List<int>();
        for (int i = 0; i < _languagesCount; i++)
            langIndexes.Add(FindIndex(headers, _texts[i]));

        for (int i = 1; i < lines.Length; i++)
        {
            var values = lines[i].Split(',');
            string key = values[keyIndex].Trim();
            if (string.IsNullOrEmpty(key)) continue;

            var entry = new Dictionary<string, object>
            {
                ["Text"] = new Dictionary<string, object>(),
                ["Scripts"] = new Dictionary<string, object>
                {
                    ["Insert"] = SplitScripts(values, insertIndex)
                }
            };

            for (int j = 0; j < _languagesCount; j++)
            {
                string lang = _texts[j];
                ((Dictionary<string, object>)entry["Text"])[lang] = values[langIndexes[j]].Trim();
            }

            dict[key] = entry;
        }

        File.WriteAllText(jsonPath, JsonConvert.SerializeObject(dict, Formatting.Indented));
    }

    private void ParseQuestion(string csvPath, string jsonPath)
    {
        var dict = new Dictionary<string, List<Dictionary<string, string>>>();
        var lines = File.ReadAllLines(csvPath);
        var headers = lines[0].Split(',');

        int keyIndex = FindIndex(headers, "Key");
        int uiKeyIndex = FindIndex(headers, "UIKey");
        int nextKeyIndex = FindIndex(headers, "NextKey");

        for (int i = 1; i < lines.Length; i++)
        {
            var values = lines[i].Split(',');
            string key = values[keyIndex].Trim();
            if (string.IsNullOrEmpty(key)) continue;

            string[] uiKeys = values[uiKeyIndex].Split('|');
            string[] nextKeys = values[nextKeyIndex].Split('|');

            var pairs = new List<Dictionary<string, string>>();
            for (int j = 0; j < Mathf.Min(uiKeys.Length, nextKeys.Length); j++)
            {
                string ui = uiKeys[j].Trim();
                string next = nextKeys[j].Trim();
                if (string.IsNullOrEmpty(ui) || string.IsNullOrEmpty(next)) continue;

                pairs.Add(new Dictionary<string, string>
                {
                    ["UIKey"] = ui,
                    ["NextKey"] = next
                });
            }

            dict[key] = pairs;
        }

        File.WriteAllText(jsonPath, JsonConvert.SerializeObject(dict, Formatting.Indented));
    }
}
