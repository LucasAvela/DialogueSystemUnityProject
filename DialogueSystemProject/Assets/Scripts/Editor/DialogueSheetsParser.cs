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
    private int _gidSimpleText;
    private int _gidCharacters;
    private int _gidQuestions;

    private int _languagesCount;
    private bool _showLanguages = true;
    private string[] _languages;

    private string _dialogueSystemFolder = Application.dataPath + "/Resources/DialogueSystem";
    private string _tempDownloadFolder = Application.dataPath + "/Resources/DialogueSystem/Temp";
    private string _configFolder = Application.dataPath + "/Resources/DialogueSystem/Config";
    private string _prefix;

    List<string> _languagesList = new List<string>();

    [MenuItem("Tools/Dialogue")]
    public static void OpenWindow()
    {
        GetWindow<DialogueSheetsParser>("Dialogue");
    }

    #region GUI
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
        _gidSimpleText = EditorGUILayout.IntField("GID - Simple Text", _gidSimpleText);
        _gidCharacters = EditorGUILayout.IntField("GID - Characters", _gidCharacters);
        _gidQuestions = EditorGUILayout.IntField("GID - Question", _gidQuestions);

        EditorGUILayout.Space();

        int newCount = EditorGUILayout.IntField("Languages Count", _languagesCount);
        if (newCount != _languagesCount)
        {
            _languagesCount = newCount;
            _languages = new string[_languagesCount];
        }

        if (_languagesCount <= 0) _languagesCount = 0;

        EditorGUI.indentLevel++;
        _showLanguages = EditorGUILayout.Foldout(_showLanguages, "Languages");

        if (_showLanguages)
        {
            EditorGUI.indentLevel++;
            for (int i = 0; i < _languagesCount; i++)
            {
                _languages[i] = EditorGUILayout.TextField($"Language {i}", _languages[i]);
            }
            EditorGUI.indentLevel--;
        }
        EditorGUI.indentLevel--;

        EditorGUILayout.Space();
        _prefix = EditorGUILayout.TextField("FIles Prefix", _prefix);

        EditorGUILayout.Space();
        if (GUILayout.Button("Delete All DATA Files"))
        {
            //DeleteData();
        }
        if (GUILayout.Button("Parse Dialogue Sheet"))
        {
            StartParser();
        }
    }
    #endregion

    private void StartParser()
    {
        _languagesList.Clear();

        for (int i = 0; i < _languagesCount; i++)
        {
            _languagesList.Add(_languages[i]);
        }

        Debug.Log($"🔣 Selected Languages: {string.Join(", ", _languagesList)}");
        try
        {
            EditorUtility.DisplayProgressBar("⏳ Downloading Sheets", "Fetches data from the Google Sheet using the provided ID and GIDs.", 0.0f);
            DownloadSheet("Dialogue.csv", _gidDialogue);
            EditorUtility.DisplayProgressBar("⏳ Downloading Sheets", "Fetches data from the Google Sheet using the provided ID and GIDs.", 0.2f);
            DownloadSheet("SimpleDialogue.csv", _gidSimpleDialogue);
            EditorUtility.DisplayProgressBar("⏳ Downloading Sheets", "Fetches data from the Google Sheet using the provided ID and GIDs.", 0.4f);
            DownloadSheet("SimpleText.csv", _gidSimpleText);
            EditorUtility.DisplayProgressBar("⏳ Downloading Sheets", "Fetches data from the Google Sheet using the provided ID and GIDs.", 0.6f);
            DownloadSheet("Characters.csv", _gidCharacters);
            EditorUtility.DisplayProgressBar("⏳ Downloading Sheets", "Fetches data from the Google Sheet using the provided ID and GIDs.", 0.8f);
            DownloadSheet("Questions.csv", _gidQuestions);
            EditorUtility.DisplayProgressBar("⏳ Downloading Sheets", "Fetches data from the Google Sheet using the provided ID and GIDs.", 1.0f);
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }
        Debug.Log("✅ Download complete!");

        try
        {
            EditorUtility.DisplayProgressBar("⏳ Processing Dialogue Data", "Parsing Dialogue sheet and building structured JSON.", 0.0f);
            ParseDialogue();
            EditorUtility.DisplayProgressBar("⏳ Processing Simple Dialogue", "Converting simple dialogue rows into JSON format.", 0.2f);
            ParseSimpleDialogue();
            EditorUtility.DisplayProgressBar("⏳ Processing Simple Text", "Transforming simple text entries into localized data.", 0.4f);
            ParseSimpleText();
            EditorUtility.DisplayProgressBar("⏳ Processing Characters", "Loading character information and preparing JSON output.", 0.6f);
            ParseCharacters();
            EditorUtility.DisplayProgressBar("⏳ Processing Questions", "Linking UI options to dialogue flow and exporting questions.", 0.8f);
            ParseQuestions();
            EditorUtility.DisplayProgressBar("✅ All Done", "All dialogue data has been successfully converted to JSON.", 1.0f);
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }
        Debug.Log("✅ All dialogue data has been parsed and saved as JSON.");
        AssetDatabase.Refresh();
    }

    #region Download
    private void DownloadSheet(string path_out, int sheet_gid)
    {
        string csv = Path.Combine(_tempDownloadFolder, _prefix + path_out);
        string url = $"https://docs.google.com/spreadsheets/d/{_sheetID}/export?format=csv&gid={sheet_gid}";
        UnityWebRequest www = UnityWebRequest.Get(url);
        var request = www.SendWebRequest();

        while (!request.isDone) { }

        if (www.result == UnityWebRequest.Result.Success)
        {
            File.WriteAllBytes(csv, www.downloadHandler.data);
        }
        else
        {
            Debug.LogError($"❌ Failed to download {Path.GetFileName(csv)}: {www.error}");
        }
    }
    #endregion

    #region Parse
    private void ParseDialogue()
    {
        string dialogueCsv = Path.Combine(_tempDownloadFolder, _prefix + "Dialogue.csv");
        string dialogueJson = Path.Combine(_dialogueSystemFolder, _prefix + "Dialogue.json");

        if (File.Exists(dialogueJson)) File.Delete(dialogueJson);

        string[] lines = File.ReadAllLines(dialogueCsv);
        string[] headers = lines[0].Split(',');

        int keyIndex = Array.IndexOf(headers, "Key");
        int nextKeyIndex = Array.IndexOf(headers, "Next_Key");
        int insertIndex = Array.IndexOf(headers, "Insert");
        int startIndex = Array.IndexOf(headers, "StartScript");
        int middleIndex = Array.IndexOf(headers, "MiddleScript");
        int endIndex = Array.IndexOf(headers, "EndScript");
        int questionIndex = Array.IndexOf(headers, "Question");
        int actorIndex = Array.IndexOf(headers, "Actor");

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
                    { "Actor", null },
                    { "Text", new Dictionary<string, object>() },
                    { "NextKey", null },
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

            if (actorIndex >= 0 && actorIndex < fields.Count)
            {
                string actor = fields[actorIndex].Trim();
                entry["Actor"] = actor;
            }

            var textDict = (Dictionary<string, object>)entry["Text"];
            for (int j = 0; j < _languagesList.Count; j++)
            {
                string lang = _languagesList[j];
                int textColIndex = Array.FindIndex(headers, h => h.Trim().EndsWith(lang, StringComparison.OrdinalIgnoreCase));
                if (textColIndex >= 0 && textColIndex < fields.Count)
                {
                    textDict[lang] = fields[textColIndex].Trim();
                }
            }

            if (nextKeyIndex >= 0 && nextKeyIndex < fields.Count)
            {
                string nextKey = fields[nextKeyIndex].Trim();
                entry["NextKey"] = string.IsNullOrEmpty(nextKey) ? null : nextKey;
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
    }

    private void ParseSimpleDialogue()
    {
        string simpleDialogueCsv = Path.Combine(_tempDownloadFolder, _prefix + "SimpleDialogue.csv");
        string simpleDialogueJson = Path.Combine(_dialogueSystemFolder, _prefix + "SimpleDialogue.json");

        if (File.Exists(simpleDialogueJson)) File.Delete(simpleDialogueJson);

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
            for (int j = 0; j < _languagesList.Count; j++)
            {
                string lang = _languagesList[j];
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
    }

    private void ParseSimpleText()
    {
        string simpleTextCsv = Path.Combine(_tempDownloadFolder, _prefix + "SimpleText.csv");
        string simpleTextJson = Path.Combine(_dialogueSystemFolder, _prefix + "SimpleText.json");

        if (File.Exists(simpleTextJson)) File.Delete(simpleTextJson);

        string[] lines = File.ReadAllLines(simpleTextCsv);
        string[] headers = lines[0].Split(',');

        int keyIndex = Array.IndexOf(headers, "Key");
        int insertIndex = Array.IndexOf(headers, "Insert");

        var simpleTextData = new Dictionary<string, object>();

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

                simpleTextData[keyValue] = clone;
            }
        }

        for (int i = 1; i < lines.Length; i++)
        {
            var fields = ParseCsvLine(lines[i]);
            if (keyIndex < 0 || keyIndex >= fields.Count) continue;

            string keyValue = fields[keyIndex].Trim();
            if (!simpleTextData.ContainsKey(keyValue)) continue;

            var entry = (Dictionary<string, object>)simpleTextData[keyValue];

            var textDict = (Dictionary<string, object>)entry["Text"];
            for (int j = 0; j < _languagesList.Count; j++)
            {
                string lang = _languagesList[j];
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

        string json = JsonConvert.SerializeObject(simpleTextData, Formatting.Indented);
        if (File.Exists(simpleTextCsv)) File.Delete(simpleTextCsv);
        File.WriteAllText(simpleTextJson, json);
    }

    private void ParseCharacters()
    {
        string charactersCsv = Path.Combine(_tempDownloadFolder, _prefix + "Characters.csv");
        string charactersJson = Path.Combine(_dialogueSystemFolder, _prefix + "Characters.json");

        if (File.Exists(charactersJson)) File.Delete(charactersJson);

        string[] lines = File.ReadAllLines(charactersCsv);
        string[] headers = lines[0].Split(',');

        int keyIndex = Array.IndexOf(headers, "Key");
        int insertIndex = Array.IndexOf(headers, "Insert");
        int actorIndex = Array.IndexOf(headers, "Actor");

        var characterData = new Dictionary<string, object>();

        for (int i = 1; i < lines.Length; i++)
        {
            var fields = lines[i].Split(',');
            if (keyIndex < 0 || keyIndex >= fields.Length) continue;
            string keyValue = fields[keyIndex].Trim();
            if (!string.IsNullOrEmpty(keyValue))
            {
                var clone = new Dictionary<string, object>
                {
                    { "Original", null},
                    { "Actor", new Dictionary<string, object>() },
                    { "Scripts", new Dictionary<string, object>
                        {
                            { "Insert", new List<object>() }
                        }
                    }
                };

                characterData[keyValue] = clone;
            }
        }

        for (int i = 1; i < lines.Length; i++)
        {
            var fields = ParseCsvLine(lines[i]);
            if (keyIndex < 0 || keyIndex >= fields.Count) continue;

            string keyValue = fields[keyIndex].Trim();
            if (!characterData.ContainsKey(keyValue)) continue;

            var entry = (Dictionary<string, object>)characterData[keyValue];

            var actorDict = (Dictionary<string, object>)entry["Actor"];
            for (int j = 0; j < _languagesList.Count; j++)
            {
                string lang = _languagesList[j];
                int textColIndex = Array.FindIndex(headers, h => h.Trim().EndsWith(lang, StringComparison.OrdinalIgnoreCase));
                if (textColIndex >= 0 && textColIndex < fields.Count)
                {
                    if (string.IsNullOrEmpty(fields[textColIndex].Trim()))
                    {
                        actorDict[lang] = fields[actorIndex].Trim();
                    }
                    else
                    {
                        actorDict[lang] = fields[textColIndex].Trim();
                    }
                }
            }

            var scriptsDict = (Dictionary<string, object>)entry["Scripts"];

            if (insertIndex >= 0 && insertIndex < fields.Count)
                scriptsDict["Insert"] = fields[insertIndex].Split('|').Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)).Cast<object>().ToList();

            if (actorIndex >= 0 && actorIndex < fields.Count)
            {
                string actor = fields[actorIndex].Trim();
                entry["Original"] = actor;
            }
        }

        string json = JsonConvert.SerializeObject(characterData, Formatting.Indented);
        if (File.Exists(charactersCsv)) File.Delete(charactersCsv);
        File.WriteAllText(charactersJson, json);
    }

    private void ParseQuestions()
    {
        string questionCsv = Path.Combine(_tempDownloadFolder, _prefix + "Questions.csv");
        string questionJson = Path.Combine(_dialogueSystemFolder, _prefix + "Questions.json");

        if (File.Exists(questionJson)) File.Delete(questionJson);

        string[] lines = File.ReadAllLines(questionCsv);
        string[] headers = lines[0].Split(',');

        int keyIndex = Array.IndexOf(headers, "Key");
        int uiKeyIndex = Array.IndexOf(headers, "TextKey");
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
                { "TextKey", ui },
                { "NextKey", next }
            });
            }

            questionData[keyValue] = questionList;
        }

        if (File.Exists(questionCsv)) File.Delete(questionCsv);

        string json = JsonConvert.SerializeObject(questionData, Formatting.Indented);
        File.WriteAllText(questionJson, json);
    }

    #endregion

    #region Utility
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
    #endregion

    #region Save & Load
    [System.Serializable]
    public class DialogueParseConfig
    {
        public string sheetID;
        public int gidDialogue;
        public int gidSimpleDialogue;
        public int gidSimpleText;
        public int gidCharacters;
        public int gidQuestions;

        public int languagesCount;
        public string[] languages;
        public string prefix;
    }

    private void SaveConfig()
    {
        DialogueParseConfig config = new DialogueParseConfig
        {
            sheetID = _sheetID,
            gidDialogue = _gidDialogue,
            gidSimpleDialogue = _gidSimpleDialogue,
            gidSimpleText = _gidSimpleText,
            gidCharacters = _gidCharacters,
            gidQuestions = _gidQuestions,
            languagesCount = _languagesCount,
            languages = _languages,
            prefix = _prefix
        };

        string json = JsonConvert.SerializeObject(config, Formatting.Indented);
        string path = Path.Combine(_configFolder, "config.json");
        File.WriteAllText(path, json);
        Debug.Log($"💾 Configuration saved at: \n{path}");
    }

    private void LoadConfig()
    {
        string path = Path.Combine(_configFolder, "config.json");
        if (!File.Exists(path))
        {
            Debug.LogWarning("⚠️ No configuration file found at the specified path.");
            return;
        }

        string json = File.ReadAllText(path);
        DialogueParseConfig config = JsonConvert.DeserializeObject<DialogueParseConfig>(json);

        _sheetID = config.sheetID;
        _gidDialogue = config.gidDialogue;
        _gidSimpleDialogue = config.gidSimpleDialogue;
        _gidSimpleText = config.gidSimpleText;
        _gidCharacters = config.gidCharacters;
        _gidQuestions = config.gidQuestions;
        _languagesCount = config.languagesCount;
        _languages = config.languages;
        _prefix = config.prefix;

        Debug.Log("📝 Configuration loaded successfully.");
    }
    #endregion
}
