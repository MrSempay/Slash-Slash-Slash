using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System;
using UnityEngine.UI;
using System.Collections;
using static StaticClassForAdditionalFunctions;

public class DialogueParser : MonoBehaviour
{
    private string _nameDialogueFolder = "Dialogues/"; // Имя папки с диалогами
    private string _nameIconsUnitFolder = "Dialogues/IconsUnit/"; // Имя папки с диалогами
    private int _currentIndexDialogue = 0; // Имя папки с диалогами
    private string _nameDialogueFileWithParentFolder; // Имя файла без расширения и без папки Dialogues 
    private Coroutine _slowAppearingTextByCharacterCoroutine; // Имя файла без расширения и без папки Dialogues 
    private float _typeSpeed = 0.05f; // Имя файла без расширения и без папки Dialogues 
    private int _currentCharacterPosition = 0;


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
        LocalizationManager.Instance.OnLanguageWasChanged += UpdateDialogueText;
    }

    protected virtual void Start()
    {
       
    }

    protected virtual void Update()
    {
        if (Time.timeScale > 0) // если не пауза
        {
            //Debug.Log("Обнаружен коллайдер: " + gameObject.name);
            if (Input.GetMouseButtonDown(0))
            {
                Collider2D[] hits = Physics2D.OverlapCircleAll(Camera.main.ScreenToWorldPoint(Input.mousePosition), 0.05f);

                //Визуализация круга обнаружения (отображается только в редакторе)
                DebugExtension.DebugCircle(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector3.forward, Color.red, 0.5f, false, 0.5f);

                // Перебираем все найденные коллайдеры
                foreach (Collider2D hit in hits)
                {
                    //Debug.Log("Обнаружен коллайдер: " + hit.gameObject.name);

                    GameObject placeEquipment = hit.gameObject; // Получаем GameObject
                    if (placeEquipment.name == "ButtonMenu")
                    {
                        Debug.Log("НАШЛИИИИИИИИИИИИИИИИИИИИИИИИИИИИИИИИ " + placeEquipment.name);
                        return;
                        //return placeEquipment.GetComponent<RectTransform>(); // Возвращаем RectTransform, если нашли
                    }
                }
                if (_currentIndexDialogue < dialogueLines.Count)
                {
                    _currentCharacterPosition = 0;
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
                        _currentCharacterPosition = 0;
                        DisplayDialogue(_currentIndexDialogue);
                        _currentIndexDialogue++;
                        return;
                    }
                    FinishDialogue();
                }

            }
        }
    }

    public void LoadAndParseDialogueAndShowPhrase()
    {
        dialogueLines.Clear();

        _nameDialogueFileWithParentFolder = GameManager.Instance.nameDialogueCurrent;
        string fullPathToDialogueFile = _nameDialogueFolder + GameManager.Instance.currentSettings.Language + "/" + _nameDialogueFileWithParentFolder;
        //Debug.Log(fullPathToDialogueFile);
        TextAsset textAsset = Resources.Load<TextAsset>(fullPathToDialogueFile);

        if (textAsset == null)
        {
            // по идее можно просто менять сцену далее, если диалог тут не предусмотрен
            Debug.Log(" Нет ФвйликАААААААААААА  Text file not found in " + fullPathToDialogueFile + ".txt");
            //StartCoroutine(FinishDialogueAfterOneFrame()); // сделано для того, чтоб GameManager успел подписаться на прослушивание завершения данного диалога и подал об этом сигнал прочим.
            // Бо иначе у нас, ввиду того что всё это происходит в Awake диалога, он даже не успевает подписаться на это завершение, диалог спавнится и уничтожается сразу в его Awake

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
        DisplayDialogue(_currentIndexDialogue);
        _currentIndexDialogue++;
    }

    private void UpdateDialogueText(LANGUAGE language)
    {
        _currentIndexDialogue--;
        LoadAndParseDialogueAndShowPhrase();
    }

    // метод для отображения диалога 
    private void DisplayDialogue(int index)
    {
        if (index >= 0 && index < dialogueLines.Count)
        {
            if (_slowAppearingTextByCharacterCoroutine != null)
            {
                CoroutineManager.Instance.StopManagedCoroutine(this.gameObject, _slowAppearingTextByCharacterCoroutine);
            }

            string fullPathToIcon = _nameIconsUnitFolder + dialogueLines[index].characterName;
            Debug.Log("Way: " + fullPathToIcon);
            Sprite iconUnit = Resources.Load<Sprite>(fullPathToIcon);
            spriteRendererIconUnit.sprite = iconUnit; 

            //_textMeshProUnitPhrase.text = dialogueLines[index].dialogueText; 
            //Debug.Log(_currentCharacterPosition);
            _slowAppearingTextByCharacterCoroutine = CoroutineManager.Instance.StartManagedCoroutine(this.gameObject, WriteTextByCharacter(dialogueLines[index].dialogueText));
            _textMeshProUnitName.text = dialogueLines[index].characterName;
            if (Time.timeScale > 0) // чтоб при паузе за зря не менялось расположение иконки персонажа при смене языка
            {
                if (index > 0)
                {
                    if (dialogueLines[index].characterName != dialogueLines[index - 1].characterName)
                    {
                        //Debug.Log("Shit?");
                        rectTransformNameUnit.localPosition = new Vector3(rectTransformNameUnit.localPosition.x * (-1), rectTransformNameUnit.localPosition.y, rectTransformNameUnit.localPosition.z);
                        rectTransformUnitIcon.localPosition = new Vector3(rectTransformUnitIcon.localPosition.x * (-1), rectTransformUnitIcon.localPosition.y, rectTransformUnitIcon.localPosition.z);
                    }
                }
            }
        }
        else
        {
            Debug.LogWarning("Dialogue index out of range.");
        }
    }

    IEnumerator WriteTextByCharacter(string text)
    {
        _textMeshProUnitPhrase.text = ""; // Очищаем текст перед началом
       
        for (int i = 0; i < _currentCharacterPosition; i++)
        {
            if (i < text.Length) // бывают ситуации, когда текст другого языка короче по символам, чем текст на предыдущем языке, чтоб не выйти за рамки
                                                         // строки, добавляем эту проверку
                _textMeshProUnitPhrase.text += text[i]; // Добавляем символ к тексту
        }
        
        for (int i = _currentCharacterPosition; i < text.Length; i++)
        {
            yield return new WaitForSeconds(_typeSpeed); // Ждем заданное время
            _textMeshProUnitPhrase.text += text[i]; // Добавляем символ к тексту
            _currentCharacterPosition = i;
        }
    }

    protected virtual void FinishDialogue() 
    {
        Debug.Log("Закончили диалог: " + _nameDialogueFileWithParentFolder);
        //Debug.Log(_nameDialogueFileWithParentFolder);
        onDialogueWasFinished?.Invoke(_nameDialogueFileWithParentFolder);
    }

    private IEnumerator FinishDialogueAfterOneFrame()
    {
        yield return null;
        FinishDialogue();
    }

    protected virtual void OnDestroy()
    {
        LocalizationManager.Instance.OnLanguageWasChanged -= UpdateDialogueText;
        if (_slowAppearingTextByCharacterCoroutine != null)
        {
            CoroutineManager.Instance.StopManagedCoroutine(this.gameObject, _slowAppearingTextByCharacterCoroutine);
            _slowAppearingTextByCharacterCoroutine = null;
        }
    }

}