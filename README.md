[[PT-BR](README.pt.md) ]
# 💬 Dialogue and Localization System for Unity

Modular system for interactive dialogues and multilingual localization in Unity. With this system, you can create dynamic narratives with branches, questions, custom scripts, and TextMeshPro integration, offering a rich experience for the player.

<img src="/docs/img/header.png" width="500"/>

---

## ✅ Requirements

- Unity 6.0 or higher
- [Newtonsoft.Json](https://github.com/jilleJr/Newtonsoft.Json-for-Unity)
- TextMeshPro (included by default in Unity)

---

## 📦 Installation

1. **Import the Files**  
   Download the release and import the `DialogueSystem/` folder into your Unity project's `Assets` folder.

2. **Install the Newtonsoft.Json Package**  
   - Go to: `Window > Package Manager`
   - Click `+ > Add package from git URL...`
   - Paste the following URL:
     ```
     https://github.com/jilleJr/Newtonsoft.Json-for-Unity.git
     ```

      <img src="/docs/img/packageManager.png" width="400"/>

---

## 💡 Google Sheets Setup

Create a spreadsheet with **4 tabs**. Each tab represents a data set that will be converted to JSON:

1. **Dialogue**  
2. **Simple Dialogue**  
3. **UI**  
4. **Questions**

### 📄 `Dialogue` Tab

This tab contains the dialogues with branching and scripts.

<img src="/docs/img/dialogueSheet.png" height="80"/>

| Field         | Description                                  |
|---------------|----------------------------------------------|
| **Key**           | Unique identifier of the dialogue.          |
| **Next_Key**      | ID of the next dialogue (if there's continuity). |
| **Question**      | Question text (for choice options, if any). |
| **Insert**        | Base text of the dialogue.                  |
| **StartScript**   | Script to execute before the dialogue.      |
| **MiddleScript**  | Script to execute during the dialogue.      |
| **EndScript**     | Script to execute at the end of the dialogue. |
| **en_us**         | Localized text in English.                  |
| **Actor_en_us**   | Character name in English.                  |

> **Note:** Add `language` and `Actor_language` columns according to the supported languages.

### 📄 `Simple Dialogue` Tab

Used for dialogues without branching.

<img src="/docs/img/simpleDialogueSheet.png" height="80"/>

| Field     | Description             |
|-----------|-------------------------|
| **Key**       | Identifier of the dialogue. |
| **en_us**     | Text in English.             |
| **pt_br**     | Text in Portuguese.          |

### 📄 `UI` Tab

Contains UI text strings.

<img src="/docs/img/uiSheet.png" height="80"/>

| Field     | Description              |
|-----------|--------------------------|
| **Key**       | UI text identifier.       |
| **en_us**     | Text in English.          |
| **pt_br**     | Text in Portuguese.       |

### 📄 `Questions` Tab

Defines response options for choice-based dialogues.

<img src="/docs/img/questionSheet.png" height="80"/>

| Field     | Description                                  |
|-----------|----------------------------------------------|
| **Key**       | Identifier of the option/question.         |
| **UIKey**     | References the UI text from the UI tab.    |
| **NextKey**   | ID of the dialogue triggered by this option. |

---

## 🧰 Unity Tool Setup

In Unity's menu, go to: `Tool > Dialogue`  
This window helps configure and generate the JSON files from the spreadsheet data.

<img src="/docs/img/dialogueWindow.png" width="400"/>

### 🔘 Window Buttons

- **Save Config**: Saves current configuration (Google Sheet ID, GIDs, languages, etc.) locally.
- **Load Config**: Loads a previously saved configuration to avoid reconfiguration.

### 📋 Google Sheet ID

- Unique identifier of your spreadsheet, located in the spreadsheet URL.  
  Example: `https://docs.google.com/spreadsheets/d/SPREADSHEET_ID/edit#gid=0`  
  *The `SPREADSHEET_ID` is your Google Sheet ID.*

### 🔢 Tab GIDs

Each tab has a unique **GID**, found in the URL when clicking the tab:

- **GID Dialogue** → `Dialogue` tab
- **GID Simple Dialogue** → `Simple Dialogue` tab
- **GID UI** → `UI` tab
- **GID Question** → `Questions` tab

### 🌐 Languages Count

Defines how many languages your project will support. Once set, additional fields will appear for configuration:
- **Insert Header**: Column name for localized dialogue text.
- **Actor Header**: Column name for localized character name.

### ▶️ "Parser" Button

This button runs the script that downloads data from the spreadsheet and generates JSON files, saved to:  
`Assets/Resources/DialogueSystem/`

---

## 📁 Generated JSON Files

After parsing, four JSON files are created:
- **Dialogue.json**
- **SimpleDialogue.json**
- **UI.json**
- **Question.json**

---

## 🧪 Using in Scene

### Prerequisites

Add the 3 main managers to your scene:

- **DialogueManager**
- **DialogueScriptsManager**
- **DialogueParser**

### Components & Functions

#### 📘 **DialogueParser**
- Loads the `.json` dialogue files.
- Assign the following files: `Dialogue.json`, `SimpleDialogue.json`, `UI.json`, and `Question.json`.

#### 🧠 **DialogueScriptsManager**
- Manages and executes scripts defined in:
  - `StartScript`
  - `MiddleScript`
  - `EndScript`
  - (Including scripts within the `Insert` field, if needed)

#### 🧩 **DialogueManager**
- Controls dialogue display.
- Requires:
  - Reference to `DialogueParser`
  - Reference to `DialogueScriptsManager`
  - **Default language** setting (e.g. `pt`, `en`, etc.)

---

## 🎛️ UI Configuration in Scene

### 🗨️ **Dialogue (Standard Dialogue)**

| Field              | Description                                                        |
|--------------------|--------------------------------------------------------------------|
| **Dialogue Panel**       | Parent object containing the dialogue UI panel.                  |
| **Dialogue Text**        | TextMeshPro object displaying the dialogue text.                 |
| **Actor Text**           | TextMeshPro object displaying the character name.                |
| **Writing Time**         | Character typing interval (typing effect).                      |
| **Anim Time**            | Duration of panel open/close animation.                         |

### ✏️ **Simple Dialogue (Quick Text)**

| Field                          | Description                                                   |
|--------------------------------|---------------------------------------------------------------|
| **Simple Dialogue Text**       | TextMeshPro object for direct simple text display.            |
| **Simple Dialogue Animation Time** | Entry/exit animation duration.                           |
| **Simple Dialogue Wait Time**  | Duration the text remains visible.                            |

### ❓ **Question Dialogue (Choice System)**

| Field                                | Description                                                                 |
|--------------------------------------|-----------------------------------------------------------------------------|
| **Question Dialogue Container**         | Object where choice buttons appear immediately after the dialogue.          |
| **Question Dialogue Text Container**   | Object where buttons appear after the player presses to continue.           |

> ⚠️ **Note:** Use **only one** of the two fields. Both are used to display choice buttons but at different moments.

<!-- TODO: Insert images or a diagram showing UI elements (panel, text, containers) -->

---

## ▶️ Test with Sample Files

The project includes four sample JSON files, located in:  
`Assets/Resources/DialogueSystem/Example/`

These files simulate data from each spreadsheet tab:
- **Dialogue.json**
- **SimpleDialogue.json**
- **UI.json**
- **Question.json**

### How to Test

1. In the `DialogueParser` component, assign the example files.
2. Ensure `DialogueManager` is correctly configured with all required fields.
3. Run the following command in a script to start the test dialogue:

   ```csharp
   DialogueManager.Instance.StartDialogue("key_0");

<!-- TODO: Insert GIF showing dialogue execution in scene -->
