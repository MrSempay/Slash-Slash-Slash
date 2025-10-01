using UnityEngine.UI;
using UnityEngine;
using System.Collections.Generic;
using TMPro;
using System.IO;
using System;

/// <summary>
/// ScrollFadeController — добавлена простая десериализация текстового файла Titles.txt
/// Поддерживается три способа подачи файла:
/// 1) Прямо через TextAsset (перетащи файл в инспектор в поле _titlesTextAsset)
/// 2) Resources/Titles.txt (в Resources положи файл без расширения — Resources/Titles.txt, загружается как Resources.Load<TextAsset>("Titles"))
/// 3) StreamingAssets/Titles.txt или Assets/Titles.txt (подходит для редактора)
///
/// Формат файла:
/// - Блоки, окружённые строками, состоящими только из символов '=' — игнорируются.
/// - Строки вида "H::Some text" — десериализуются как Header
/// - Строки вида "D::Some text" — десериализуются как Description
///
/// Пример в задаче (README + записи H:: / D::).
/// </summary>
public class TitlesParser : MonoBehaviour
{
    public List<DeserializeTextObject> listTextsTitle;

    [Header("Input file (optional)")]
    [Tooltip("Если указать TextAsset, он будет использован. Иначе пытаемся загрузить из Resources/StreamingAssets/Assets.")]
    [SerializeField] private TextAsset _titlesTextAsset = null;
    [SerializeField] private string _resourceFileName = "Titles"; // без расширения для Resources.Load
    [SerializeField] private string _fileName = "Titles.txt"; // fallback имя

    [Header("Debug")]
    [SerializeField] private bool _debugPrintLoaded = false;

    // Список десериализованных объектов

    // Вложенный класс — хранит тип и текст
    [Serializable]
    public class DeserializeTextObject
    {
        public string text;
        public TEXT_TYPE type;
        public enum TEXT_TYPE { Header, Descripton };

        public DeserializeTextObject(string text, TEXT_TYPE type)
        {
            this.text = text;
            this.type = type;
        }

        public override string ToString()
        {
            return $"[{type}] {text}";
        }
    }

    private void Awake()
    {
        // Инициализируем список (в оригинальном коде была ошибка — список не создавался)
        listTextsTitle = new List<DeserializeTextObject>();

        // Загружаем содержимое файла
        string content = LoadFileContent();
        if (string.IsNullOrEmpty(content))
        {
            Debug.LogWarning($"ScrollFadeController: Не удалось загрузить содержимое файла (проверьте _titlesTextAsset / Resources / StreamingAssets). fileName={_fileName}");
            return;
        }

        // Парсим и заполняем список
        ParseContent(content);

        if (_debugPrintLoaded)
        {
            Debug.Log($"ScrollFadeController: Загружено элементов: {listTextsTitle.Count}");
            for (int i = 0; i < listTextsTitle.Count; i++)
                Debug.Log($"#{i}: {listTextsTitle[i].ToString()}");
        }

        // Здесь можно запустить дальнейшую обработку списка (создание UI-элементов и т.д.)
    }

    void Update()
    {

    }

    /// <summary>
    /// Пробуем несколько вариантов загрузки текста: _titlesTextAsset, Resources, StreamingAssets, Assets (editor)
    /// </summary>
    /// <returns>Содержимое файла или null</returns>
    private string LoadFileContent()
    {
        // 1) Явно указанный TextAsset в инспекторе
        if (_titlesTextAsset != null)
        {
            return _titlesTextAsset.text;
        }

        // 2) Resources (без расширения)
        try
        {
            TextAsset ta = Resources.Load<TextAsset>(_resourceFileName);
            if (ta != null)
                return ta.text;
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"ScrollFadeController: Ошибка Resources.Load: {ex.Message}");
        }

        // 3) StreamingAssets (поддерживается в editor/standalone). На Android может потребоваться WWW/UnityWebRequest, но для простоты здесь File.ReadAllText
        string streamingPath = Path.Combine(Application.streamingAssetsPath, _fileName);
        try
        {
            if (File.Exists(streamingPath))
                return File.ReadAllText(streamingPath);
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"ScrollFadeController: Ошибка чтения StreamingAssets ({streamingPath}): {ex.Message}");
        }

        // 4) Пробуем Assets (удобно в Editor, если файл положен рядом с проектом)
        string assetsPath = Path.Combine(Application.dataPath, _fileName);
        try
        {
            if (File.Exists(assetsPath))
                return File.ReadAllText(assetsPath);
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"ScrollFadeController: Ошибка чтения Assets ({assetsPath}): {ex.Message}");
        }

        return null;
    }

    /// <summary>
    /// Парсит содержимое по строчно, игнорирует блоки между строками из '=' и распознаёт маркеры H:: и D::
    /// </summary>
    /// <param name="content"></param>
    private List<DeserializeTextObject> ParseContent(string content)
    {
        using (StringReader reader = new StringReader(content))
        {
            string line;
            bool inExcludedBlock = false;

            while ((line = reader.ReadLine()) != null)
            {
                string trimmed = line.Trim();

                if (string.IsNullOrEmpty(trimmed))
                    continue; // пропускаем пустые строки

                // Если строка состоит только из символов '=' (возможно с пробелами) — переключаем режим исключения
                if (IsEqualsOnly(trimmed))
                {
                    inExcludedBlock = !inExcludedBlock;
                    continue;
                }

                if (inExcludedBlock)
                    continue; // пропускаем строки внутри '=' блока

                // Обработка маркеров H:: и D:: (чувствительна к регистру ключа, но ниже мы допускаем и маленькие буквы)
                if (trimmed.StartsWith("H::"))
                {
                    string txt = trimmed.Substring(3).Trim();
                    if (!string.IsNullOrEmpty(txt))
                        listTextsTitle.Add(new DeserializeTextObject(txt, DeserializeTextObject.TEXT_TYPE.Header));
                    continue;
                }

                if (trimmed.StartsWith("D::"))
                {
                    string txt = trimmed.Substring(3).Trim();
                    if (!string.IsNullOrEmpty(txt))
                        listTextsTitle.Add(new DeserializeTextObject(txt, DeserializeTextObject.TEXT_TYPE.Descripton));
                    continue;
                }

                // Доп. поддержка — если встречается любой ключ вида X::value (например, с пробелами)
                int idx = trimmed.IndexOf("::");
                if (idx > 0)
                {
                    string key = trimmed.Substring(0, idx).Trim();
                    string txt = trimmed.Substring(idx + 2).Trim();
                    if (!string.IsNullOrEmpty(txt))
                    {
                        if (string.Equals(key, "H", System.StringComparison.OrdinalIgnoreCase))
                            listTextsTitle.Add(new DeserializeTextObject(txt, DeserializeTextObject.TEXT_TYPE.Header));
                        else if (string.Equals(key, "D", System.StringComparison.OrdinalIgnoreCase))
                            listTextsTitle.Add(new DeserializeTextObject(txt, DeserializeTextObject.TEXT_TYPE.Descripton));
                        else
                            Debug.LogWarning($"ScrollFadeController: Неизвестный ключ '{key}' в строке: {line}");
                    }
                    continue;
                }

                // Если строка не подошла ни под одно правило — выводим предупреждение (можно убрать)
                Debug.LogWarning($"ScrollFadeController: Игнорирую некорректную строку: {line}");
            }
        }

        return listTextsTitle;
    }

    /// <summary>
    /// Проверяет, состоит ли строка только из символов '=' (и пробелов)
    /// </summary>
    private static bool IsEqualsOnly(string s)
    {
        if (string.IsNullOrEmpty(s))
            return false;
        foreach (char c in s)
        {
            if (c != '=' && !char.IsWhiteSpace(c))
                return false;
        }
        return true;
    }

    // Удобная функция для быстрого вывода результата в редакторе (контекстное меню)
    [ContextMenu("PrintParsedTitles")]
    private void PrintParsedTitles()
    {
        Debug.Log($"ScrollFadeController: Parsed {listTextsTitle.Count} items:");
        for (int i = 0; i < listTextsTitle.Count; i++)
            Debug.Log($"#{i}: {listTextsTitle[i].ToString()}");
    }
}
