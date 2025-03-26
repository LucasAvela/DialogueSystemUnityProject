using UnityEngine;
using UnityEditor;

public class DialogueSheetsParser : EditorWindow
{
    string _key;
    string _nextKey;
    string _InsertInfo;
    string _startScript;
    string _middleScript;
    string _endScript;
    string _originalActor;
    int _languagesCount;
    string[] _texts;
    string[] _actorss;

    bool[] _languageFoldouts;
    bool _showLanguages = true;
    string _downloadLocation = Application.dataPath + "/Resources/DialogueSystem";

    [MenuItem("Tools/Dialogue")]
    public static void GetWindow()
    {
        GetWindow<DialogueSheetsParser>("Dialogue");
    }

    void OnGUI()
    {
        GUILayout.Label("Dialogue Sheets Parser", EditorStyles.boldLabel);

        _key = EditorGUILayout.TextField("Key", _key);
        _nextKey = EditorGUILayout.TextField("Next Key", _nextKey);
        _InsertInfo = EditorGUILayout.TextField("Insert Info", _InsertInfo);
        _startScript = EditorGUILayout.TextField("Start Script", _startScript);
        _middleScript = EditorGUILayout.TextField("Middle Script", _middleScript);
        _endScript = EditorGUILayout.TextField("End Script", _endScript);
        _originalActor = EditorGUILayout.TextField("Original Actor", _originalActor);
        _languagesCount = EditorGUILayout.IntField("Languages Count", _languagesCount);

        if (_texts == null || _texts.Length != _languagesCount)
        {
            _texts = new string[_languagesCount];
            _actorss = new string[_languagesCount];
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
                    _actorss[i] = EditorGUILayout.TextField($"Actor {i + 1}", _actorss[i]);
                    EditorGUI.indentLevel--;
                }
            }

            EditorGUI.indentLevel--;
        }
        EditorGUI.indentLevel--;

        EditorGUILayout.Space();
        _downloadLocation = EditorGUILayout.TextField("Dialogue System Folder", _downloadLocation);

        if (GUILayout.Button("Select Folder"))
        {
            string path = EditorUtility.OpenFolderPanel("Dialogue System Folder", _downloadLocation, "");
            if (!string.IsNullOrEmpty(path))
            {
                _downloadLocation = path;
            }
        }
    }
}
