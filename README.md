# 💬 Sistema de Diálogo e Localização para Unity

Sistema modular para diálogos interativos e localização multilíngue em Unity. Com este sistema, você pode criar narrativas dinâmicas com ramificações, perguntas, scripts customizados e integração com TextMeshPro, permitindo uma experiência rica para o jogador.

<!-- TODO: Inserir um banner ou imagem ilustrativa do sistema em ação -->

---

## ✅ Requisitos

- Unity 6.0 ou superior
- [Newtonsoft.Json](https://github.com/jilleJr/Newtonsoft.Json-for-Unity)
- TextMeshPro (já incluso no Unity por padrão)

---

## 📦 Instalação

1. **Importe os Arquivos**  
   Baixe a release e importe a pasta `DialogueSystem/` para a pasta `Assets` do seu projeto Unity.

   <!-- TODO: Inserir GIF mostrando a importação da pasta no Unity -->

2. **Instale o Pacote Newtonsoft.Json**  
   - Abra: `Window > Package Manager`
   - Clique em `+ > Add package from git URL...`
   - Insira a seguinte URL:
     ```
     https://github.com/jilleJr/Newtonsoft.Json-for-Unity.git
     ```

   <!-- TODO: Inserir imagem ou GIF mostrando a instalação do pacote via Package Manager -->

---

## 💡 Configuração da Planilha (Google Sheets)

Crie uma planilha com **4 abas**. Cada aba representa um conjunto de dados que será convertido para JSON:

1. **Dialogo**  
2. **Dialogo Simples**  
3. **UI**  
4. **Questions**

### 📄 Aba `Dialogo`

Essa aba contém os diálogos com ramificações e scripts.

| Campo         | Descrição                                  |
|---------------|--------------------------------------------|
| **Key**           | Identificador único da fala.            |
| **Next_Key**      | ID da próxima fala (caso haja continuidade). |
| **Question**      | Texto de uma pergunta (para opções, se houver). |
| **Insert**        | Texto base do diálogo.                  |
| **StartScript**   | Script a ser executado antes do diálogo. |
| **MiddleScript**  | Script a ser executado durante o diálogo. |
| **EndScript**     | Script a ser executado ao final do diálogo. |
| **Actor [pt]**    | Nome do personagem em português.        |
| **Actor [en]**    | Nome do personagem em inglês.           |

> **Nota:** Adicione colunas `Insert [idioma]` e `Actor [idioma]` conforme os idiomas suportados.

### 📄 Aba `Dialogo Simples`

Utilizada para diálogos sem ramificações.

| Campo         | Descrição        |
|---------------|-------------------|
| **Key**           | Identificador do diálogo simples. |
| **Insert [pt]**   | Texto em português.     |
| **Insert [en]**   | Texto em inglês.        |

### 📄 Aba `UI`

Contém os textos da interface do usuário (UI).

| Campo         | Descrição     |
|---------------|------------------|
| **Key**           | Identificador do texto UI.   |
| **Insert [pt]**   | Texto em português.    |
| **Insert [en]**   | Texto em inglês.       |

### 📄 Aba `Questions`

Defina as opções de resposta para os diálogos de escolha.

| Campo     | Descrição                              |
|-----------|---------------------------------------------|
| **Key**       | Identificador da opção/pregunta.          |
| **UIKey**     | Chave que referencia o texto na aba UI.   |
| **NextKey**   | ID do diálogo que será ativado ao escolher esta opção. |

<!-- TODO: Inserir imagem com exemplo da planilha no Google Sheets -->

---

## 🧰 Configuração da Ferramenta no Unity

No menu do Unity, acesse: `Tool > Dialogue`
Essa janela foi criada para facilitar a configuração e a geração dos arquivos JSON a partir dos dados da planilha.

### 🔘 Botões da Janela

- **Save Config**: Salva as configurações atuais (Google Sheet ID, GIDs, idiomas, etc.) localmente.
- **Load Config**: Carrega uma configuração previamente salva, evitando reconfiguração manual.

### 📋 Google Sheet ID

- Identificador único da sua planilha, localizado na URL da planilha.  
  Exemplo: `https://docs.google.com/spreadsheets/d/ID_DO_ARQUIVO/edit#gid=0`
  *O trecho `ID_DO_ARQUIVO` é o seu Google Sheet ID.*

### 🔢 GIDs das Abas

Cada aba possui um **GID** único, que pode ser verificado na URL ao clicar na aba:

- **GID Dialogue** → aba `Dialogo`
- **GID Simple Dialogue** → aba `Dialogo Simples`
- **GID UI** → aba `UI`
- **GID Question** → aba `Questions`

### 🌐 Languages Count

Define a quantidade de idiomas que o seu projeto irá suportar. Ao definir este valor, campos adicionais são exibidos para você configurar:
- **Insert Header**: Nome da coluna com o texto localizado.
- **Actor Header**: Nome da coluna com o nome do personagem localizado.

### ▶️ Botão "Parser"

Este botão executa o script que baixa os dados da planilha e gera os arquivos JSON, os quais são salvos em: `Assets/Resources/DialogueSystem/`
<!-- TODO: Inserir um GIF demonstrando o uso dos botões Save Config, Load Config e Parser -->

---

## 📁 Arquivos JSON Gerados

Após o processo de parsing, são gerados quatro arquivos JSON:
- **Dialogue.json**
- **SimpleDialogue.json**
- **UI.json**
- **Question.json**

---

## 🧪 Uso em Cena

### Pré-requisitos

Adicione os 3 managers principais na cena:

- **DialogueManager**
- **DialogueScriptsManager**
- **DialogueParser**

### Componentes e Funções

#### 📘 **DialogueParser**
- Carrega os arquivos `.json` com os diálogos.
- Referencie os arquivos: `Dialogue.json`, `SimpleDialogue.json`, `UI.json` e `Question.json`.

#### 🧠 **DialogueScriptsManager**
- Gerencia e executa os scripts definidos em:
  - `StartScript`
  - `MiddleScript`
  - `EndScript`
  - (Inclusive scripts presentes no campo `Insert`, se necessário)

#### 🧩 **DialogueManager**
- Controla a exibição dos diálogos.
- Requer:
  - Referência ao `DialogueParser`
  - Referência ao `DialogueScriptsManager`
  - Definição da **língua padrão** (ex: `pt`, `en`, etc.)

---

## 🎛️ Configuração do UI na Cena

### 🗨️ **Dialogue (Diálogo Normal)**

| Campo             | Descrição                                                                 |
|-------------------|---------------------------------------------------------------------------|
| **Dialogue Panel**      | Objeto pai que contém todo o painel do diálogo.                         |
| **Dialogue Text**       | Objeto com TextMeshPro para exibir o texto do diálogo.                   |
| **Actor Text**          | Objeto com TextMeshPro para exibir o nome do personagem.                 |
| **Writing Time**        | Intervalo entre caracteres (efeito de digitação).                        |
| **Anim Time**           | Duração da animação de abertura/fechamento do painel.                    |

### ✏️ **Simple Dialogue (Texto Rápido)**

| Campo                         | Descrição                                                       |
|-------------------------------|------------------------------------------------------------------|
| **Simple Dialogue Text**      | Objeto com TextMeshPro para exibir o texto direto.               |
| **Simple Dialogue Animation Time** | Duração da animação de entrada/saída.                      |
| **Simple Dialogue Wait Time** | Tempo que o texto permanecerá visível.                           |

### ❓ **Question Dialogue (Sistema de Escolha)**

| Campo                          | Descrição                                                                                          |
|--------------------------------|-----------------------------------------------------------------------------------------------------|
| **Question Dialogue Container**  | Objeto onde os botões de escolha aparecem imediatamente após a fala ser escrita.                  |
| **Question Dialogue Text Container** | Objeto onde os botões aparecem após o jogador pressionar para continuar.                    |

> ⚠️ **Atenção:** Utilize **apenas um dos dois** campos. Ambos servem para exibir os botões de escolha, mas em momentos distintos da interação.

<!-- TODO: Inserir imagens ou um diagrama explicativo dos elementos do UI (panel, text, containers) -->

---

## ▶️ Teste com Arquivos de Exemplo

O projeto inclui quatro arquivos JSON de exemplo, localizados em: `Assets/Resources/DialogueSystem/Example/`

Esses arquivos simulam os dados de cada aba:
- **Dialogue.json**
- **SimpleDialogue.json**
- **UI.json**
- **Question.json**

### Como Testar

1. No componente `DialogueParser`, referencie os arquivos de exemplo.
2. Verifique se o `DialogueManager` está configurado corretamente, com todos os campos obrigatórios preenchidos.
3. Execute o seguinte comando em um script para iniciar o diálogo de teste:

   ```csharp
   DialogueManager.Instance.StartDialogue("key_0");

<!-- TODO: Inserir GIF demonstrativo da execução do diálogo na cena -->
