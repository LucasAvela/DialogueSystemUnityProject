// using System.Collections.Generic;
// using UnityEngine;
// using Newtonsoft.Json;

// // [System.Serializable]
// // public class QuestionEntry
// // {
// //     public string UIKey;
// //     public string NextKey;
// // }

// public class DialogueParserAnswers : MonoBehaviour
// {
//     public TextAsset jsonFile;
//     public Dictionary<string, List<QuestionEntry>> dialogueDictionary;

//     void Awake()
//     {
//         if (jsonFile != null)
//         {
//             dialogueDictionary = ParseJsonToDictionary(jsonFile.text);
//             if (dialogueDictionary == null)
//             {
//                 Debug.LogError("Failed to parse JSON to dictionary.");
//             }
//         }
//         else
//         {
//             Debug.LogError("JSON file is not assigned.");
//         }
//     }

//     public List<QuestionEntry> GetDialogueByKey(string key)
//     {
//         if (dialogueDictionary != null && dialogueDictionary.ContainsKey(key))
//         {
//             return dialogueDictionary[key];
//         }
//         else
//         {
//             Debug.LogError($"Dialogue with key '{key}' not found.");
//             return null;
//         }
//     }

//     Dictionary<string, List<QuestionEntry>> ParseJsonToDictionary(string json)
//     {
//         if (string.IsNullOrEmpty(json))
//         {
//             Debug.LogError("JSON string is null or empty.");
//             return null;
//         }

//         try
//         {
//             return JsonConvert.DeserializeObject<Dictionary<string, List<QuestionEntry>>>(json);
//         }
//         catch (System.Exception ex)
//         {
//             Debug.LogError($"Exception while parsing JSON: {ex.Message}");
//             return null;
//         }
//     }
// }
