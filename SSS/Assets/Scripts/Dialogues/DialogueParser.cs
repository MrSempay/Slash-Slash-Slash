using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System;
using UnityEngine.UI;

public class DialogueParser : MonoBehaviour
{
    private string _nameDialogueFolder = "Dialogues/"; // Имя папки с диалогами
    private string _nameIconsUnitFolder = "Dialogues/IconsUnit/"; // Имя папки с диалогами
    private int _currentIndexDialogue = 0; // Имя папки с диалогами
    private string _nameDialogueFileWithParentFolder; // Имя файла без расширения и без папки Dialogues 


    [SerializeField] private Image spriteRendererIconUnit;
    [SerializeField] private TextMeshProUGUI _textMeshProUnitPhrase; // Для отображения диалога
    [SerializeField] private TextMeshProUGUI _textMeshProUnitName; // Для отображения имени персонажа под иконкой
    [SerializeField] private RectTransform rectTransformUnitIcon; // компонент RectTransform иконки персонажа, будем перемещать по ходу диалога
    [SerializeField] private RectTransform rectTransformNameUnit; // компонент RectTransform имени персонажа, будем перемещать по ходу диалога

    public List<DialogueLine> dialogueLines = new List<DialogueLine>();
    public delegate void DialogueWasFinished(string nameDialogue); // шаблон функции
    public event DialogueWasFinished onDialogueWasFinished;         // экземляр(?) функции/сигнала(?)

    [System.Serializable]
    public class DialogueLine
    {
        public string characterName;
        public string dialogueText;
    }


    protected virtual void Awake()
    {
    }

    protected virtual void Start()
    {
       
    }

    protected virtual void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (_currentIndexDialogue < dialogueLines.Count)
            {
                DisplayDialogue(_currentIndexDialogue);
                _currentIndexDialogue++;
                return;
            }
            FinishDialogue();

        }
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                if (_currentIndexDialogue < dialogueLines.Count)
                {
                    DisplayDialogue(_currentIndexDialogue);
                    _currentIndexDialogue++;
                    return;
                }
                FinishDialogue();
            }

        }
    }

    public void LoadAndParseDialogueAndShowFirstPhrase()
    {
        _nameDialogueFileWithParentFolder = GameManager.Instance.nameDialogueCurrent;
        string fullPathToDialogueFile = _nameDialogueFolder + _nameDialogueFileWithParentFolder;
        TextAsset textAsset = Resources.Load<TextAsset>(fullPathToDialogueFile);

        if (textAsset == null)
        {
            // по идее можно просто менять сцену далее, если диалог тут не предусмотрен
            Debug.Log(" Нет ФвйликАААААААААААА  Text file not found in Resources/Dialogues/" + _nameDialogueFileWithParentFolder + ".txt");
            FinishDialogue();
            return;
        }

        string dialogueText = textAsset.text;

        // Разделяем текст на строки
        string[] lines = dialogueText.Split('\n');

        // Парсим каждую строку
        foreach (string line in lines)
        {
            // Используем регулярное выражение для разбора строки
            Match match = Regex.Match(line, @"^(.*?)::\s(.*?)$"); //Регулярка чтобы вытащить данные.
            if (match.Success)
            {
                string characterName = match.Groups[1].Value.Trim(); // Получаем имя персонажа
                string dialogueTextLine = match.Groups[2].Value.Trim(); // Получаем текст реплики

                // Создаем объект DialogueLine
                DialogueLine dialogueLine = new DialogueLine();
                dialogueLine.characterName = characterName;
                dialogueLine.dialogueText = dialogueTextLine;

                // Добавляем объект в список
                dialogueLines.Add(dialogueLine);
            }
            else
            {
                Debug.LogWarning("Invalid dialogue line: " + line);
            }
        }
        DisplayDialogue(0);
        _currentIndexDialogue++;
    }

    // метод для отображения диалога 
    private void DisplayDialogue(int index)
    {
        if (index >= 0 && index < dialogueLines.Count)
        {

            string fullPathToIcon = _nameIconsUnitFolder + dialogueLines[index].characterName;
            Sprite iconUnit = Resources.Load<Sprite>(fullPathToIcon);
            spriteRendererIconUnit.sprite = iconUnit;

            _textMeshProUnitPhrase.text = dialogueLines[index].dialogueText;
            _textMeshProUnitName.text = dialogueLines[index].characterName;
            if (index > 0)
            {
                if (dialogueLines[index].characterName != dialogueLines[index - 1].characterName)
                {
                    rectTransformNameUnit.localPosition = new Vector3(rectTransformNameUnit.localPosition.x * (-1), rectTransformNameUnit.localPosition.y, rectTransformNameUnit.localPosition.z);
                    rectTransformUnitIcon.localPosition = new Vector3(rectTransformUnitIcon.localPosition.x * (-1), rectTransformUnitIcon.localPosition.y, rectTransformUnitIcon.localPosition.z);
                }
            }

        }
        else
        {
            Debug.LogWarning("Dialogue index out of range.");
        }
    }

    protected virtual void FinishDialogue() 
    {
        onDialogueWasFinished?.Invoke(_nameDialogueFileWithParentFolder);
    }


}