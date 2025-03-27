using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using static StaticClassForAdditionalFunctions;
using static GameManager;

public class LocalizationManager
{
    private static LocalizationManager _instance;

    public static LocalizationManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new LocalizationManager(); // Создаем экземпляр при необходимости
            }
            return _instance;
        }
    }

    public ENUM currentLanguage = ENUM.English;

    private Dictionary<string, Dictionary<ENUM, string>> localization = new Dictionary<string, Dictionary<ENUM, string>>();

    private LocalizationManager()
    {
        // Заполняем словарь
        localization.Add("Greeting", new Dictionary<ENUM, string>() {
            { ENUM.English, "Hello!" },
            { ENUM.Russian, "Привет!" },
            { ENUM.Spanish, "?Hola!" }
        });

        localization.Add("Goodbye", new Dictionary<ENUM, string>() {
            { ENUM.English, "Goodbye!" },
            { ENUM.Russian, "До свидания!" },
            { ENUM.Spanish, "?Adi?s!" }
        });
        localization.Add("mda", new Dictionary<ENUM, string>() {
            { ENUM.English, "mda!" },
            { ENUM.Russian, "мда" },
            { ENUM.Spanish, "muda-da?" }
        });
    }

    public string GetText(string key)
    {
        if (localization.ContainsKey(key) && localization[key].ContainsKey(currentLanguage))
        {
            return localization[key][currentLanguage];
        }
        return ""; // Какой-то текст по умолчанию
    }

    public void SetLanguage(ENUM newLanguage)
    {
        currentLanguage = newLanguage;
        // Обновляем весь UI
        UpdateAllText();
    }

    public void UpdateAllText()
    {
        // Находим все компоненты ILocalizableText
        ILocalizableText[] localizableTexts = MonoBehaviour.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).OfType<ILocalizableText>().ToArray();

        foreach (ILocalizableText text in localizableTexts)
        {
            text.UpdateText();
        }
    }
}

