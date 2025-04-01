using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using static StaticClassForAdditionalFunctions;
using static GameManager;
using UnityEngine.Events;
using System;

public class LocalizationManager
{
    private static LocalizationManager _instance;

    public event Action<ENUM> OnLanguageWasChanged;

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
        localization.Add("Vibration", new Dictionary<ENUM, string>() {
            { ENUM.English, "Vibration" },
            { ENUM.Russian, "Вибрация" },
            { ENUM.Spanish, "muda-da?" }
        });
        localization.Add("Camera shaking", new Dictionary<ENUM, string>() {
            { ENUM.English, "Camera shaking" },
            { ENUM.Russian, "Шатание камеры" },
            { ENUM.Spanish, "muda-da?" }
        });
        localization.Add("Russian", new Dictionary<ENUM, string>() {
            { ENUM.English, "Russian" },
            { ENUM.Russian, "Русский" },
            { ENUM.Spanish, "muda-da?" }
        });
        localization.Add("English", new Dictionary<ENUM, string>() {
            { ENUM.English, "English" },
            { ENUM.Russian, "Английский" },
            { ENUM.Spanish, "muda-da?" }
        });
        localization.Add("Spanish", new Dictionary<ENUM, string>() {
            { ENUM.English, "Spanish" },
            { ENUM.Russian, "Гишпанский" },
            { ENUM.Spanish, "muda-da?" }
        });
        localization.Add("Volum Effects", new Dictionary<ENUM, string>() {
            { ENUM.English, "Volum Effects" },
            { ENUM.Russian, "Громкость эффектов" },
            { ENUM.Spanish, "muda-da?" }
        });
        localization.Add("Volum Music", new Dictionary<ENUM, string>() {
            { ENUM.English, "Volum Music" },
            { ENUM.Russian, "Громкость музыки" },
            { ENUM.Spanish, "muda-da?" }
        });
        localization.Add("Brightness", new Dictionary<ENUM, string>() {
            { ENUM.English, "Brightness" },
            { ENUM.Russian, "Яркость" },
            { ENUM.Spanish, "muda-da?" }
        });
        localization.Add("Orientation", new Dictionary<ENUM, string>() {
            { ENUM.English, "Orientation" },
            { ENUM.Russian, "Ориентация" },
            { ENUM.Spanish, "muda-da?" }
        });
        localization.Add("Horizontal", new Dictionary<ENUM, string>() {
            { ENUM.English, "Horizontal" },
            { ENUM.Russian, "Горизонтальная" },
            { ENUM.Spanish, "muda-da?" }
        });
        localization.Add("Vertical", new Dictionary<ENUM, string>() {
            { ENUM.English, "Vertical" },
            { ENUM.Russian, "Вертикальная" },
            { ENUM.Spanish, "muda-da?" }
        });
        localization.Add("Money:", new Dictionary<ENUM, string>() {
            { ENUM.English, "Money:" },
            { ENUM.Russian, "Деньги:" },
            { ENUM.Spanish, "muda-da?" }
        });
        localization.Add("Level:", new Dictionary<ENUM, string>() {
            { ENUM.English, "Level:" },
            { ENUM.Russian, "Уровень:" },
            { ENUM.Spanish, "muda-da?" }
        });
        localization.Add("Up lvl:", new Dictionary<ENUM, string>() {
            { ENUM.English, "Up lvl:" },
            { ENUM.Russian, "Up уровня:" },
            { ENUM.Spanish, "muda-da?" }
        });
        localization.Add("Experience:", new Dictionary<ENUM, string>() {
            { ENUM.English, "Experience:" },
            { ENUM.Russian, "Опыт:" },
            { ENUM.Spanish, "muda-da?" }
        });
        localization.Add("Combo:", new Dictionary<ENUM, string>() {
            { ENUM.English, "Combo:" },
            { ENUM.Russian, "Комбо:" },
            { ENUM.Spanish, "muda-da?" }
        });
        localization.Add("SomeSpell1", new Dictionary<ENUM, string>() {
            { ENUM.English, "SomeSpell1" },
            { ENUM.Russian, "Некое заклинание 1" },
            { ENUM.Spanish, "muda-da?" }
        });
        localization.Add("Cost:", new Dictionary<ENUM, string>() {
            { ENUM.English, "Cost:" },
            { ENUM.Russian, "Цена:" },
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
        OnLanguageWasChanged?.Invoke(currentLanguage);
        // Обновляем весь UI
        UpdateAllText();
    }

    public void UpdateAllText()
    {
        // Находим все компоненты ILocalizableText
        //ILocalizableText[] localizableTexts = MonoBehaviour.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).OfType<ILocalizableText>().ToArray();
        TextEdit[] localizableTexts = Resources.FindObjectsOfTypeAll<TextEdit>(); 

        foreach (TextEdit text in localizableTexts)
        {
            text.Awake();
            text.UpdateText();
        }
    }
}

