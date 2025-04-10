using UnityEngine;
using UnityEditor;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using System;
using System.Linq;

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

    List<string> languagesTexts = new List<string>();
    List<string> languagesActors = new List<string>();

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
        for (int i = 0; i < _languagesCount; i++)
        {
            languagesTexts.Add(_texts[i]);
            languagesActors.Add(_actors[i]);
        }

        Debug.Log("⏳ Downloading sheet...");
        DialogueParse();
    }

    private void DialogueParse()
    {
        string dialogueCsv = Path.Combine(_dialogueSystemFolder, "Dialogue.csv");
        string dialogueJson = Path.Combine(_dialogueSystemFolder, "Dialogue.json");

        string url = $"https://docs.google.com/spreadsheets/d/{_sheetID}/export?format=csv&gid={_gidDialogue}";
        UnityWebRequest www = UnityWebRequest.Get(url);
        var request = www.SendWebRequest();

        while (!request.isDone) { }

        if (www.result == UnityWebRequest.Result.Success)
        {
            File.WriteAllBytes(dialogueCsv, www.downloadHandler.data);
            Debug.Log($"✅ Baixado: {Path.GetFileName(dialogueCsv)}");
        }
        else
        {
            Debug.LogError($"❌ Erro ao baixar {Path.GetFileName(dialogueCsv)}: {www.error}");
        }

        var emptyDict = new Dictionary<string, object>
        {
            { "Actor", new Dictionary<string, object>() },
            { "Text", new Dictionary<string, object>() },
            { "Next_Key", null },
            { "Question", null },
            { "Scripts", new Dictionary<string, object>
                {
                    { "Insert", new List<object>() },
                    { "Start", new List<object>() },
                    { "Middle", new List<object>() },
                    { "End", new List<object>() }
                }
            }
        };

        string[] lines = File.ReadAllLines(dialogueCsv);
        string[] headers = lines[0].Split(',');

        int keyIndex = Array.IndexOf(headers, "Key");
        int nextKeyIndex = Array.IndexOf(headers, "Next_Key");
        int insertIndex = Array.IndexOf(headers, "Insert");
        int startIndex = Array.IndexOf(headers, "StartScript");
        int middleIndex = Array.IndexOf(headers, "MiddleScript");
        int endIndex = Array.IndexOf(headers, "EndScript");
        int questionIndex = Array.IndexOf(headers, "Question");
        int actorDefaultIndex = Array.IndexOf(headers, "Actor");

        var dialogueData = new Dictionary<string, object>();

        for (int i = 1; i < lines.Length; i++)
        {
            var fields = lines[i].Split(',');
            if (keyIndex < 0 || keyIndex >= fields.Length) continue;
            string keyValue = fields[keyIndex].Trim();
            if (!string.IsNullOrEmpty(keyValue))
            {
                var clone = new Dictionary<string, object>
                {
                    { "Actor", new Dictionary<string, object>() },
                    { "Text", new Dictionary<string, object>() },
                    { "Next_Key", null },
                    { "Question", null },
                    { "Scripts", new Dictionary<string, object>
                        {
                            { "Insert", new List<object>() },
                            { "Start", new List<object>() },
                            { "Middle", new List<object>() },
                            { "End", new List<object>() }
                        }
                    }
                };

                dialogueData[keyValue] = clone;
            }
        }

        for (int i = 1; i < lines.Length; i++)
        {
            var fields = ParseCsvLine(lines[i]);
            if (keyIndex < 0 || keyIndex >= fields.Count) continue;

            string keyValue = fields[keyIndex].Trim();
            if (!dialogueData.ContainsKey(keyValue)) continue;

            var entry = (Dictionary<string, object>)dialogueData[keyValue];

            var actorDict = (Dictionary<string, object>)entry["Actor"];
            for (int j = 0; j < languagesActors.Count; j++)
            {
                string lang = languagesActors[j];
                int actorColIndex = Array.FindIndex(headers, h => h.Trim().EndsWith(lang, StringComparison.OrdinalIgnoreCase));
                if (actorColIndex >= 0 && actorColIndex < fields.Count)
                {
                    actorDict[languagesTexts[j]] = fields[actorColIndex].Trim();
                }
            }

            var textDict = (Dictionary<string, object>)entry["Text"];
            for (int j = 0; j < languagesTexts.Count; j++)
            {
                string lang = languagesTexts[j];
                int textColIndex = Array.FindIndex(headers, h => h.Trim().EndsWith(lang, StringComparison.OrdinalIgnoreCase));
                if (textColIndex >= 0 && textColIndex < fields.Count)
                {
                    textDict[lang] = fields[textColIndex].Trim();
                }
            }

            if (nextKeyIndex >= 0 && nextKeyIndex < fields.Count)
            {
                string nextKey = fields[nextKeyIndex].Trim();
                entry["Next_Key"] = string.IsNullOrEmpty(nextKey) ? null : nextKey;
            }

            if (questionIndex >= 0 && questionIndex < fields.Count)
            {
                string question = fields[questionIndex].Trim();
                entry["Question"] = string.IsNullOrEmpty(question) ? null : question;
            }

            var scriptsDict = (Dictionary<string, object>)entry["Scripts"];

            if (insertIndex >= 0 && insertIndex < fields.Count)
                scriptsDict["Insert"] = fields[insertIndex].Split('|').Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)).Cast<object>().ToList();

            if (startIndex >= 0 && startIndex < fields.Count)
                scriptsDict["Start"] = fields[startIndex].Split('|').Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)).Cast<object>().ToList();

            if (middleIndex >= 0 && middleIndex < fields.Count)
                scriptsDict["Middle"] = fields[middleIndex].Split('|').Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)).Cast<object>().ToList();

            if (endIndex >= 0 && endIndex < fields.Count)
                scriptsDict["End"] = fields[endIndex].Split('|').Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)).Cast<object>().ToList();

        }

        string json = JsonConvert.SerializeObject(dialogueData, Formatting.Indented);
        File.WriteAllText(dialogueJson, json);
        Debug.Log($"✅ JSON salvo: {Path.GetFileName(dialogueJson)}");
    }

    public static List<string> ParseCsvLine(string line)
    {
        var result = new List<string>();
        bool inQuotes = false;
        string current = "";

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (c == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    current += '"';
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (c == ',' && !inQuotes)
            {
                result.Add(current);
                current = "";
            }
            else
            {
                current += c;
            }
        }

        result.Add(current);
        return result;
    }

    #region save/load
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
    #endregion
}
