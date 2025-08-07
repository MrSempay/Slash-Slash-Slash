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

    public event Action<LANGUAGE> OnLanguageWasChanged;

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

    public LANGUAGE currentLanguage = LANGUAGE.English;

    private Dictionary<string, Dictionary<LANGUAGE, string>> localization = new Dictionary<string, Dictionary<LANGUAGE, string>>();

    private LocalizationManager()
    {
        // Заполняем словарь
        localization.Add("Greeting", new Dictionary<LANGUAGE, string>() {
            { LANGUAGE.English, "Hello!" },
            { LANGUAGE.Russian, "Привет!" },
            { LANGUAGE.Spanish, "?Hola!" }
        });

        localization.Add("Goodbye", new Dictionary<LANGUAGE, string>() {
            { LANGUAGE.English, "Goodbye!" },
            { LANGUAGE.Russian, "До свидания!" },
            { LANGUAGE.Spanish, "?Adi?s!" }
        });
        localization.Add("mda", new Dictionary<LANGUAGE, string>() {
            { LANGUAGE.English, "mda!" },
            { LANGUAGE.Russian, "мда" },
            { LANGUAGE.Spanish, "muda-da?" }
        });
        localization.Add("Vibration", new Dictionary<LANGUAGE, string>() {
            { LANGUAGE.English, "Vibration" },
            { LANGUAGE.Russian, "Вибрация" },
            { LANGUAGE.Spanish, "muda-da?" }
        });
        localization.Add("Camera shaking", new Dictionary<LANGUAGE, string>() {
            { LANGUAGE.English, "Camera shaking" },
            { LANGUAGE.Russian, "Шатание камеры" },
            { LANGUAGE.Spanish, "muda-da?" }
        });
        localization.Add("Russian", new Dictionary<LANGUAGE, string>() {
            { LANGUAGE.English, "Russian" },
            { LANGUAGE.Russian, "Русский" },
            { LANGUAGE.Spanish, "muda-da?" }
        });
        localization.Add("English", new Dictionary<LANGUAGE, string>() {
            { LANGUAGE.English, "English" },
            { LANGUAGE.Russian, "Английский" },
            { LANGUAGE.Spanish, "muda-da?" }
        });
        localization.Add("Spanish", new Dictionary<LANGUAGE, string>() {
            { LANGUAGE.English, "Spanish" },
            { LANGUAGE.Russian, "Гишпанский" },
            { LANGUAGE.Spanish, "muda-da?" }
        });
        localization.Add("Volum Effects", new Dictionary<LANGUAGE, string>() {
            { LANGUAGE.English, "Volum Effects" },
            { LANGUAGE.Russian, "Громкость эффектов" },
            { LANGUAGE.Spanish, "muda-da?" }
        });
        localization.Add("Volum Music", new Dictionary<LANGUAGE, string>() {
            { LANGUAGE.English, "Volum Music" },
            { LANGUAGE.Russian, "Громкость музыки" },
            { LANGUAGE.Spanish, "muda-da?" }
        });
        localization.Add("Brightness", new Dictionary<LANGUAGE, string>() {
            { LANGUAGE.English, "Brightness" },
            { LANGUAGE.Russian, "Яркость" },
            { LANGUAGE.Spanish, "muda-da?" }
        });
        localization.Add("Orientation", new Dictionary<LANGUAGE, string>() {
            { LANGUAGE.English, "Orientation" },
            { LANGUAGE.Russian, "Ориентация" },
            { LANGUAGE.Spanish, "muda-da?" }
        });
        localization.Add("Horizontal", new Dictionary<LANGUAGE, string>() {
            { LANGUAGE.English, "Horizontal" },
            { LANGUAGE.Russian, "Горизонтальная" },
            { LANGUAGE.Spanish, "muda-da?" }
        });
        localization.Add("Vertical", new Dictionary<LANGUAGE, string>() {
            { LANGUAGE.English, "Vertical" },
            { LANGUAGE.Russian, "Вертикальная" },
            { LANGUAGE.Spanish, "muda-da?" }
        });
        localization.Add("Money:", new Dictionary<LANGUAGE, string>() {
            { LANGUAGE.English, "Money:" },
            { LANGUAGE.Russian, "Деньги:" },
            { LANGUAGE.Spanish, "muda-da?" }
        });
        localization.Add("Level:", new Dictionary<LANGUAGE, string>() {
            { LANGUAGE.English, "Level:" },
            { LANGUAGE.Russian, "Уровень:" },
            { LANGUAGE.Spanish, "muda-da?" }
        });
        localization.Add("Up lvl:", new Dictionary<LANGUAGE, string>() {
            { LANGUAGE.English, "Up lvl:" },
            { LANGUAGE.Russian, "Up уровня:" },
            { LANGUAGE.Spanish, "muda-da?" }
        });
        localization.Add("Name:", new Dictionary<LANGUAGE, string>() {
            { LANGUAGE.English, "Name:" },
            { LANGUAGE.Russian, "Название:" },
            { LANGUAGE.Spanish, "muda-da?" }
        });
        localization.Add("Experience:", new Dictionary<LANGUAGE, string>() {
            { LANGUAGE.English, "Experience:" },
            { LANGUAGE.Russian, "Опыт:" },
            { LANGUAGE.Spanish, "muda-da?" }
        });
        localization.Add("Combo:", new Dictionary<LANGUAGE, string>() {
            { LANGUAGE.English, "Combo:" },
            { LANGUAGE.Russian, "Комбо:" },
            { LANGUAGE.Spanish, "muda-da?" }
        });
        localization.Add("SomeSpell1", new Dictionary<LANGUAGE, string>() {
            { LANGUAGE.English, "SomeSpell1" },
            { LANGUAGE.Russian, "Некое заклинание 1" },
            { LANGUAGE.Spanish, "muda-da?" }
        });
        localization.Add("Cost:", new Dictionary<LANGUAGE, string>() {
            { LANGUAGE.English, "Cost:" },
            { LANGUAGE.Russian, "Цена:" },
            { LANGUAGE.Spanish, "muda-da?" }
        });
        localization.Add("Style Rank:", new Dictionary<LANGUAGE, string>() {
            { LANGUAGE.English, "Style rank:" },
            { LANGUAGE.Russian, "Ранг стиля:" },
            { LANGUAGE.Spanish, "muda-da?" }
        });
        localization.Add("Score:", new Dictionary<LANGUAGE, string>() {
            { LANGUAGE.English, "Score:" },
            { LANGUAGE.Russian, "Очки:" },
            { LANGUAGE.Spanish, "muda-da?" }
        });
        localization.Add("Leaderboard", new Dictionary<LANGUAGE, string>() { 
            { LANGUAGE.English, "Leaderboard" },
            { LANGUAGE.Russian, "Доска почёта" },
            { LANGUAGE.Spanish, "muda-da?" }
        });
        localization.Add("KillCount", new Dictionary<LANGUAGE, string>() {
            { LANGUAGE.English, "Kill count" },
            { LANGUAGE.Russian, "Счётчик убийств" },
            { LANGUAGE.Spanish, "muda-da?" }
        });
        localization.Add(C.DK.PlateArmor, new Dictionary<LANGUAGE, string>() {
            { LANGUAGE.English, "Plate Armor" },
            { LANGUAGE.Russian, "Пластинчатый Доспех" },
            { LANGUAGE.Spanish, "muda-da?" }
        });
    }

    public string GetText(string key)
    {
        if (localization.ContainsKey(key))
        {
            if (localization[key].ContainsKey(currentLanguage))
            {
                return localization[key][currentLanguage];
            }
        }
        return ""; // Какой-то текст по умолчанию
    }

    public void SetLanguage(LANGUAGE newLanguage)
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

