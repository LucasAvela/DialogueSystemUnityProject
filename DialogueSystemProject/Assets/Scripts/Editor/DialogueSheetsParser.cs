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
        if (GUILayout.Button("Delete All JSON Files"))
        {
            DeleteData();
        }
        if (GUILayout.Button("Parse Dialogue Sheet"))
        {
            StartParser();
        }
    }

    private void StartParser()
    {
        languagesTexts.Clear();
        languagesActors.Clear();
        for (int i = 0; i < _languagesCount; i++)
        {
            languagesTexts.Add(_texts[i]);
            languagesActors.Add(_actors[i]);
        }
        Debug.Log($"🔣 Selected Languages: {string.Join(", ", languagesTexts)}");
        Debug.Log("⏳ Downloading spreadsheet data...");
        try
        {
            EditorUtility.DisplayProgressBar("Parsing Sheets", "Downloading and parsing Dialogue...", 0.25f);
            DialogueParse();
            EditorUtility.DisplayProgressBar("Parsing Sheets", "Parsing Simple Dialogue...", 0.5f);
            SimpleDialogueParser();
            EditorUtility.DisplayProgressBar("Parsing Sheets", "Parsing UI data...", 0.75f);
            UIParser();
            EditorUtility.DisplayProgressBar("Parsing Sheets", "Parsing Questions...", 1f);
            QuestionParser();
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }
        Debug.Log("✅ Download and processing complete!");
    }

    private void DialogueParse()
    {
        string dialogueCsv = Path.Combine(_dialogueSystemFolder, "Dialogue.csv");
        string dialogueJson = Path.Combine(_dialogueSystemFolder, "Dialogue.json");

        if (File.Exists(dialogueCsv)) File.Delete(dialogueCsv);
        if (File.Exists(dialogueJson)) File.Delete(dialogueJson);

        string url = $"https://docs.google.com/spreadsheets/d/{_sheetID}/export?format=csv&gid={_gidDialogue}";
        UnityWebRequest www = UnityWebRequest.Get(url);
        var request = www.SendWebRequest();

        while (!request.isDone) { }

        if (www.result == UnityWebRequest.Result.Success)
        {
            File.WriteAllBytes(dialogueCsv, www.downloadHandler.data);
            Debug.Log($"✅ Downloaded: {Path.GetFileName(dialogueCsv)}");
        }
        else
        {
            Debug.LogError($"❌ Failed to download {Path.GetFileName(dialogueCsv)}: {www.error}");
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

                    if (fields[actorColIndex].Trim() == null || fields[actorColIndex].Trim() == "")
                    {
                        actorDict[languagesTexts[j]] = fields[actorDefaultIndex].Trim();
                    }
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
        if (File.Exists(dialogueCsv)) File.Delete(dialogueCsv);
        File.WriteAllText(dialogueJson, json);
        Debug.Log($"✅ JSON saved: {Path.GetFileName(dialogueJson)}");
        AssetDatabase.Refresh();
    }

    private void SimpleDialogueParser()
    {
        string simpleDialogueCsv = Path.Combine(_dialogueSystemFolder, "SimpleDialogue.csv");
        string simpleDialogueJson = Path.Combine(_dialogueSystemFolder, "SimpleDialogue.json");

        if (File.Exists(simpleDialogueCsv)) File.Delete(simpleDialogueCsv);
        if (File.Exists(simpleDialogueJson)) File.Delete(simpleDialogueJson);

        string url = $"https://docs.google.com/spreadsheets/d/{_sheetID}/export?format=csv&gid={_gidSimpleDialogue}";
        UnityWebRequest www = UnityWebRequest.Get(url);
        var request = www.SendWebRequest();

        while (!request.isDone) { }

        if (www.result == UnityWebRequest.Result.Success)
        {
            File.WriteAllBytes(simpleDialogueCsv, www.downloadHandler.data);
            Debug.Log($"✅ Downloaded: {Path.GetFileName(simpleDialogueCsv)}");
        }
        else
        {
            Debug.LogError($"❌ Failed to download {Path.GetFileName(simpleDialogueCsv)}: {www.error}");
        }

        var emptyDict = new Dictionary<string, object>
        {
            { "Text", new Dictionary<string, object>() },
            { "Scripts", new Dictionary<string, object>
                {
                    { "Insert", new List<object>() }
                }
            }
        };

        string[] lines = File.ReadAllLines(simpleDialogueCsv);
        string[] headers = lines[0].Split(',');

        int keyIndex = Array.IndexOf(headers, "Key");
        int insertIndex = Array.IndexOf(headers, "Insert");

        var simpleDialogueData = new Dictionary<string, object>();

        for (int i = 1; i < lines.Length; i++)
        {
            var fields = lines[i].Split(',');
            if (keyIndex < 0 || keyIndex >= fields.Length) continue;
            string keyValue = fields[keyIndex].Trim();
            if (!string.IsNullOrEmpty(keyValue))
            {
                var clone = new Dictionary<string, object>
                {
                    { "Text", new Dictionary<string, object>() },
                    { "Scripts", new Dictionary<string, object>
                        {
                            { "Insert", new List<object>() }
                        }
                    }
                };

                simpleDialogueData[keyValue] = clone;
            }
        }

        for (int i = 1; i < lines.Length; i++)
        {
            var fields = ParseCsvLine(lines[i]);
            if (keyIndex < 0 || keyIndex >= fields.Count) continue;

            string keyValue = fields[keyIndex].Trim();
            if (!simpleDialogueData.ContainsKey(keyValue)) continue;

            var entry = (Dictionary<string, object>)simpleDialogueData[keyValue];

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

            var scriptsDict = (Dictionary<string, object>)entry["Scripts"];

            if (insertIndex >= 0 && insertIndex < fields.Count)
                scriptsDict["Insert"] = fields[insertIndex].Split('|').Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)).Cast<object>().ToList();
        }

        string json = JsonConvert.SerializeObject(simpleDialogueData, Formatting.Indented);
        if (File.Exists(simpleDialogueCsv)) File.Delete(simpleDialogueCsv);
        File.WriteAllText(simpleDialogueJson, json);
        Debug.Log($"✅ JSON saved: {Path.GetFileName(simpleDialogueJson)}");
        AssetDatabase.Refresh();
    }

    private void UIParser()
    {
        string uiCsv = Path.Combine(_dialogueSystemFolder, "UI.csv");
        string uiJson = Path.Combine(_dialogueSystemFolder, "UI.json");

        if (File.Exists(uiCsv)) File.Delete(uiCsv);
        if (File.Exists(uiJson)) File.Delete(uiJson);

        string url = $"https://docs.google.com/spreadsheets/d/{_sheetID}/export?format=csv&gid={_gidUI}";
        UnityWebRequest www = UnityWebRequest.Get(url);
        var request = www.SendWebRequest();

        while (!request.isDone) { }

        if (www.result == UnityWebRequest.Result.Success)
        {
            File.WriteAllBytes(uiCsv, www.downloadHandler.data);
            Debug.Log($"✅ Downloaded: {Path.GetFileName(uiCsv)}");
        }
        else
        {
            Debug.LogError($"❌ Failed to download {Path.GetFileName(uiCsv)}: {www.error}");
        }

        var emptyDict = new Dictionary<string, object>
        {
            { "Text", new Dictionary<string, object>() },
            { "Scripts", new Dictionary<string, object>
                {
                    { "Insert", new List<object>() }
                }
            }
        };

        string[] lines = File.ReadAllLines(uiCsv);
        string[] headers = lines[0].Split(',');

        int keyIndex = Array.IndexOf(headers, "Key");
        int insertIndex = Array.IndexOf(headers, "Insert");

        var uiData = new Dictionary<string, object>();

        for (int i = 1; i < lines.Length; i++)
        {
            var fields = lines[i].Split(',');
            if (keyIndex < 0 || keyIndex >= fields.Length) continue;
            string keyValue = fields[keyIndex].Trim();
            if (!string.IsNullOrEmpty(keyValue))
            {
                var clone = new Dictionary<string, object>
                {
                    { "Text", new Dictionary<string, object>() },
                    { "Scripts", new Dictionary<string, object>
                        {
                            { "Insert", new List<object>() }
                        }
                    }
                };

                uiData[keyValue] = clone;
            }
        }

        for (int i = 1; i < lines.Length; i++)
        {
            var fields = ParseCsvLine(lines[i]);
            if (keyIndex < 0 || keyIndex >= fields.Count) continue;

            string keyValue = fields[keyIndex].Trim();
            if (!uiData.ContainsKey(keyValue)) continue;

            var entry = (Dictionary<string, object>)uiData[keyValue];

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

            var scriptsDict = (Dictionary<string, object>)entry["Scripts"];

            if (insertIndex >= 0 && insertIndex < fields.Count)
                scriptsDict["Insert"] = fields[insertIndex].Split('|').Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)).Cast<object>().ToList();
        }

        string json = JsonConvert.SerializeObject(uiData, Formatting.Indented);
        if (File.Exists(uiCsv)) File.Delete(uiCsv);
        File.WriteAllText(uiJson, json);
        Debug.Log($"✅ JSON saved: {Path.GetFileName(uiJson)}");
        AssetDatabase.Refresh();
    }

    private void QuestionParser()
    {
        string questionCsv = Path.Combine(_dialogueSystemFolder, "Question.csv");
        string questionJson = Path.Combine(_dialogueSystemFolder, "Question.json");

        if (File.Exists(questionCsv)) File.Delete(questionCsv);
        if (File.Exists(questionJson)) File.Delete(questionJson);

        string url = $"https://docs.google.com/spreadsheets/d/{_sheetID}/export?format=csv&gid={_gidQuestion}";
        UnityWebRequest www = UnityWebRequest.Get(url);
        var request = www.SendWebRequest();

        while (!request.isDone) { }

        if (www.result == UnityWebRequest.Result.Success)
        {
            File.WriteAllBytes(questionCsv, www.downloadHandler.data);
            Debug.Log($"✅ Downloaded: {Path.GetFileName(questionCsv)}");
        }
        else
        {
            Debug.LogError($"❌ Failed to download {Path.GetFileName(questionCsv)}: {www.error}");
            return;
        }

        string[] lines = File.ReadAllLines(questionCsv);
        string[] headers = lines[0].Split(',');

        int keyIndex = Array.IndexOf(headers, "Key");
        int uiKeyIndex = Array.IndexOf(headers, "UIKey");
        int nextKeyIndex = Array.IndexOf(headers, "NextKey");

        var questionData = new Dictionary<string, object>();

        for (int i = 1; i < lines.Length; i++)
        {
            var fields = ParseCsvLine(lines[i]);
            if (keyIndex < 0 || keyIndex >= fields.Count) continue;

            string keyValue = fields[keyIndex].Trim();
            if (string.IsNullOrEmpty(keyValue)) continue;

            var uiKeys = fields[uiKeyIndex].Split('|').Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)).ToList();
            var nextKeys = fields[nextKeyIndex].Split('|').Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)).ToList();

            var questionList = new List<object>();
            int count = Math.Max(uiKeys.Count, nextKeys.Count);

            for (int j = 0; j < count; j++)
            {
                string ui = j < uiKeys.Count ? uiKeys[j] : null;
                string next = j < nextKeys.Count ? nextKeys[j] : null;

                questionList.Add(new Dictionary<string, object>
            {
                { "UIKey", ui },
                { "NextKey", next }
            });
            }

            questionData[keyValue] = questionList;
        }

        if (File.Exists(questionCsv)) File.Delete(questionCsv);

        string json = JsonConvert.SerializeObject(questionData, Formatting.Indented);
        File.WriteAllText(questionJson, json);
        Debug.Log($"✅ JSON saved: {Path.GetFileName(questionJson)}");
        AssetDatabase.Refresh();
    }

    public void DeleteData()
    {
        string dialogueJson = Path.Combine(_dialogueSystemFolder, "Dialogue.json");
        string simpleDialogueJson = Path.Combine(_dialogueSystemFolder, "SimpleDialogue.json");
        string uiJson = Path.Combine(_dialogueSystemFolder, "UI.json");
        string questionJson = Path.Combine(_dialogueSystemFolder, "Question.json");

        if (File.Exists(dialogueJson)) File.Delete(dialogueJson);
        if (File.Exists(simpleDialogueJson)) File.Delete(simpleDialogueJson);
        if (File.Exists(uiJson)) File.Delete(uiJson);
        if (File.Exists(questionJson)) File.Delete(questionJson);

        Debug.Log("✅ Deleted all JSON files.");
        AssetDatabase.Refresh();
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
        Debug.Log($"✅ Configuration saved at: {path}");
    }

    private void LoadConfig()
    {
        string path = Path.Combine(_dialogueSystemFolder, "dialogueParser_config.json");
        if (!File.Exists(path))
        {
            Debug.LogWarning("⚠️ No configuration file found at the specified path.");
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

        Debug.Log("✅ Configuration loaded successfully.");
    }
    #endregion
}
