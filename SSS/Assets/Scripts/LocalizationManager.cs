using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using static StaticClassForAdditionalFunctions;
using static GameManager;
using UnityEngine.Events;
using System;
using static ScoreManager;

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

    public LANGUAGE currentLanguage = LANGUAGE.Russian;

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
        localization.Add(C.DK.Weapon, new Dictionary<LANGUAGE, string>() {
            { LANGUAGE.English, "Weapon" },
            { LANGUAGE.Russian, "Оружие" },
            { LANGUAGE.Spanish, "Arma" }
        });
        localization.Add(C.DK.Standart, new Dictionary<LANGUAGE, string>() {
            { LANGUAGE.English, "Standard" },
            { LANGUAGE.Russian, "Стандарт" },
            { LANGUAGE.Spanish, "Estándar" }
        });
        localization.Add(C.DK.Spear, new Dictionary<LANGUAGE, string>() {
            { LANGUAGE.English, "Spear" },
            { LANGUAGE.Russian, "Копьё" },
            { LANGUAGE.Spanish, "Lanza" }
        });

        localization.Add(C.DK.damage, new Dictionary<LANGUAGE, string>() {
            { LANGUAGE.English, "Damage" },
            { LANGUAGE.Russian, "Урон" },
            { LANGUAGE.Spanish, "Daño" }
        });
        localization.Add(C.DK.healthHealAmount, new Dictionary<LANGUAGE, string>() {
            { LANGUAGE.English, "Heal HP" },
            { LANGUAGE.Russian, "Исцеление ХП" },
            { LANGUAGE.Spanish, "muda?" }
        });
        localization.Add(C.DK.amountBlockingAttackMax, new Dictionary<LANGUAGE, string>() {
            { LANGUAGE.English, "Amount blocking attack" },
            { LANGUAGE.Russian, "Количество блокируемых атак" },
            { LANGUAGE.Spanish, "muda?" }
        });

        localization.Add(C.DK.CurrentIncreasingStamina, new Dictionary<LANGUAGE, string>() {
            { LANGUAGE.English, "Max stamina change" },
            { LANGUAGE.Russian, "Изменение максимальной выносливости" },
            { LANGUAGE.Spanish, "Cambio de resistencia máxima" }
        });
        localization.Add(C.DK.jumpForce, new Dictionary<LANGUAGE, string>() {
            { LANGUAGE.English, "Jump force" },
            { LANGUAGE.Russian, "Сила прыжка" },
            { LANGUAGE.Spanish, "Fuerza de salto" }
        });
        localization.Add(C.DK.cost, new Dictionary<LANGUAGE, string>() {
            { LANGUAGE.English, "Cost" },
            { LANGUAGE.Russian, "Цена" },
            { LANGUAGE.Spanish, "Costo" }
        });
        localization.Add(C.DK.Knife, new Dictionary<LANGUAGE, string>() {
            { LANGUAGE.English, "Knife" },
            { LANGUAGE.Russian, "Нож" },
            { LANGUAGE.Spanish, "Cuchillo" }
        });
         localization.Add(C.DK.Axe, new Dictionary<LANGUAGE, string>() {
            { LANGUAGE.English, "Axe" },
            { LANGUAGE.Russian, "Топор" },
            { LANGUAGE.Spanish, "Hacha" }
        });
        localization.Add(C.DK.stuneChanceByStandardAttackPercentage, new Dictionary<LANGUAGE, string>() {
            { LANGUAGE.English, "Stun chance (standard attack)" },
            { LANGUAGE.Russian, "Шанс оглушения (обычная атака)" },
            { LANGUAGE.Spanish, "Probabilidad de aturdir (ataque estándar)" }
        });
        localization.Add(C.DK.Sword, new Dictionary<LANGUAGE, string>() {
            { LANGUAGE.English, "Sword" },
            { LANGUAGE.Russian, "Меч" },
            { LANGUAGE.Spanish, "Espada" }
        });
        localization.Add(C.DK.Rare, new Dictionary<LANGUAGE, string>() {
            { LANGUAGE.English, "Rare" },
            { LANGUAGE.Russian, "Редкое" },
            { LANGUAGE.Spanish, "Raro" }
        });
        localization.Add(C.DK.ThunderAxe, new Dictionary<LANGUAGE, string>() {
            { LANGUAGE.English, "Thunder Axe" },
            { LANGUAGE.Russian, "Громовой топор" },
            { LANGUAGE.Spanish, "Hacha del trueno" }
        });
        localization.Add(C.DK.FireSword, new Dictionary<LANGUAGE, string>() {
            { LANGUAGE.English, "Fire Sword" },
            { LANGUAGE.Russian, "Огненный меч" },
            { LANGUAGE.Spanish, "Espada de fuego" }
        });
        localization.Add(C.DK.Legendary, new Dictionary<LANGUAGE, string>() {
            { LANGUAGE.English, "Legendary" },
            { LANGUAGE.Russian, "Легендарное" },
            { LANGUAGE.Spanish, "Legendario" }
        });
        localization.Add(C.DK.ThirstySakura, new Dictionary<LANGUAGE, string>() {
            { LANGUAGE.English, "Thirsty Sakura" },
            { LANGUAGE.Russian, "Жаждущая сакура" },
            { LANGUAGE.Spanish, "Sakura sedienta" }
        });
        localization.Add(C.DK.Armor, new Dictionary<LANGUAGE, string>() {
            { LANGUAGE.English, "Armor" },
            { LANGUAGE.Russian, "Броня" },
            { LANGUAGE.Spanish, "Armadura" }
        });
        localization.Add(C.DK.LeatherArmor, new Dictionary<LANGUAGE, string>() {
            { LANGUAGE.English, "Leather Armor" },
            { LANGUAGE.Russian, "Кожаная броня" },
            { LANGUAGE.Spanish, "Armadura de cuero" }
        });
        localization.Add(C.DK.DamageReductionPercentage, new Dictionary<LANGUAGE, string>() {
            { LANGUAGE.English, "Damage reduction" },
            { LANGUAGE.Russian, "Снижение урона" },
            { LANGUAGE.Spanish, "Reducción de daño" }
        });
        localization.Add(C.DK.PlateArmor, new Dictionary<LANGUAGE, string>() {
            { LANGUAGE.English, "Plate Armor" },
            { LANGUAGE.Russian, "Латная броня" },
            { LANGUAGE.Spanish, "Armadura de placas" }
        });
        localization.Add(C.DK.ChainMail, new Dictionary<LANGUAGE, string>() {
            { LANGUAGE.English, "Chain Mail" },
            { LANGUAGE.Russian, "Кольчуга" },
            { LANGUAGE.Spanish, "Cota de malla" }
        });
        localization.Add(C.DK.ThunderArmor, new Dictionary<LANGUAGE, string>() {
            { LANGUAGE.English, "Thunder Armor" },
            { LANGUAGE.Russian, "Громовая броня" },
            { LANGUAGE.Spanish, "Armadura del trueno" }
        });
        localization.Add(C.DK.DragonArmor, new Dictionary<LANGUAGE, string>() {
            { LANGUAGE.English, "Dragon Armor" },
            { LANGUAGE.Russian, "Драконья броня" },
            { LANGUAGE.Spanish, "Armadura de dragón" }
        });
        localization.Add(C.DK.Accessories, new Dictionary<LANGUAGE, string>() {
            { LANGUAGE.English, "Accessories" },
            { LANGUAGE.Russian, "Аксессуары" },
            { LANGUAGE.Spanish, "Accesorios" }
        });
        localization.Add(C.DK.DexterityBracelet, new Dictionary<LANGUAGE, string>() {
            { LANGUAGE.English, "Dexterity Bracelet" },
            { LANGUAGE.Russian, "Браслет ловкости" },
            { LANGUAGE.Spanish, "Pulsera de destreza" }
        });
        localization.Add(C.DK.RingWarrior, new Dictionary<LANGUAGE, string>() {
            { LANGUAGE.English, "Ring of the Warrior" },
            { LANGUAGE.Russian, "Кольцо воина" },
            { LANGUAGE.Spanish, "Anillo del guerrero" }
        });
        localization.Add(C.DK.healthMax, new Dictionary<LANGUAGE, string>() {
            { LANGUAGE.English, "Max health" },
            { LANGUAGE.Russian, "Максимальное здоровье" },
            { LANGUAGE.Spanish, "Salud máxima" }
        });
        localization.Add(C.DK.MedallionOfLife, new Dictionary<LANGUAGE, string>() {
            { LANGUAGE.English, "Medallion of Life" },
            { LANGUAGE.Russian, "Медальон жизни" },
            { LANGUAGE.Spanish, "Medallón de la vida" }
        });
        localization.Add(C.DK.RingBerserker, new Dictionary<LANGUAGE, string>() {
            { LANGUAGE.English, "Ring of the Berserker" },
            { LANGUAGE.Russian, "Кольцо берсерка" },
            { LANGUAGE.Spanish, "Anillo del berserker" }
        });
        localization.Add(C.DK.RingForesight, new Dictionary<LANGUAGE, string>() {
            { LANGUAGE.English, "Ring of Foresight" },
            { LANGUAGE.Russian, "Кольцо предвидения" },
            { LANGUAGE.Spanish, "Anillo de la previsión" }
        });
        localization.Add(C.DK.evasionPercentage, new Dictionary<LANGUAGE, string>() {
            { LANGUAGE.English, "Evasion chance" },
            { LANGUAGE.Russian, "Шанс уклонения" },
            { LANGUAGE.Spanish, "Probabilidad de evasión" }
        });
        localization.Add(C.DK.Tragicomedy, new Dictionary<LANGUAGE, string>() {
            { LANGUAGE.English, "Tragicomedy" },
            { LANGUAGE.Russian, "Трагикомедия" },
            { LANGUAGE.Spanish, "Tragicomedia" }
        });

        localization.Add(C.DK.timeCallDown, new Dictionary<LANGUAGE, string>() {
            { LANGUAGE.English, "Cooldown time" },
            { LANGUAGE.Russian, "Время перезарядки" },
            { LANGUAGE.Spanish, "Tiempo de reutilización" }
        });
        localization.Add(C.DK.durationActiveState, new Dictionary<LANGUAGE, string>() {
            { LANGUAGE.English, "Active state duration" },
            { LANGUAGE.Russian, "Длительность активного состояния" },
            { LANGUAGE.Spanish, "Duración del estado activo" }
        });
        localization.Add(C.DK.ProtectiveField, new Dictionary<LANGUAGE, string>() {
            { LANGUAGE.English, "Protective Field" },
            { LANGUAGE.Russian, "Защитное поле" },
            { LANGUAGE.Spanish, "Campo protector" }
        });
        localization.Add(C.DK.ArcLightning, new Dictionary<LANGUAGE, string>() {
            { LANGUAGE.English, "Arc Lightning" },
            { LANGUAGE.Russian, "Дуга молнии" },
            { LANGUAGE.Spanish, "Rayo en arco" }
        });
        localization.Add(C.DK.Berserker, new Dictionary<LANGUAGE, string>() {
            { LANGUAGE.English, "Berserker" },
            { LANGUAGE.Russian, "Берсерк" },
            { LANGUAGE.Spanish, "Berserker" }
        });
        localization.Add("SomeSpell2", new Dictionary<LANGUAGE, string>() {
            { LANGUAGE.English, "Some Spell 2" },
            { LANGUAGE.Russian, "Какое-то заклинание 2" },
            { LANGUAGE.Spanish, "Hechizo 2" }
        });
        localization.Add(C.DK.Healing, new Dictionary<LANGUAGE, string>() { 
            { LANGUAGE.English, "Healing" },
            { LANGUAGE.Russian, "Исцеление" },
            { LANGUAGE.Spanish, "Curación" }
        });
        localization.Add(C.Just.Infinite, new Dictionary<LANGUAGE, string>() { 
            { LANGUAGE.English, "Infinite" },
            { LANGUAGE.Russian, "Бесконечное" },
            { LANGUAGE.Spanish, "Mda..." }
        });
        localization.Add(C.DK.PlateArmor + C.Prefixes.Description, new Dictionary<LANGUAGE, string>() { 
            { LANGUAGE.English, "MUdaaaaa-aaaaaaa" },
            { LANGUAGE.Russian, "Ибо мда уж" },
            { LANGUAGE.Spanish, "Mda..." }
        });
        localization.Add(TYPE_APPEARING_MESSAGE.ComboAdded.ToString(), new Dictionary<LANGUAGE, string>() {
            { LANGUAGE.English, "Combo added" },
            { LANGUAGE.Russian, "Добавлено комбо" },
            { LANGUAGE.Spanish, "Combo añadido" }
        });
        localization.Add(TYPE_APPEARING_MESSAGE.SkillUsed.ToString(), new Dictionary<LANGUAGE, string>() {
            { LANGUAGE.English, "Skill used" },
            { LANGUAGE.Russian, "Навык использован" },
            { LANGUAGE.Spanish, "Habilidad usada" }
        });
        localization.Add(TYPE_APPEARING_MESSAGE.ComboMultyKill.ToString(), new Dictionary<LANGUAGE, string>() {
            { LANGUAGE.English, "Combo multi-kill" },
            { LANGUAGE.Russian, "Множественное убийство в комбо" },
            { LANGUAGE.Spanish, "Combo de múltiples bajas" }
        });
        localization.Add(TYPE_APPEARING_MESSAGE.RankImproved.ToString(), new Dictionary<LANGUAGE, string>() {
            { LANGUAGE.English, "Rank improved" },
            { LANGUAGE.Russian, "Ранг повышен" },
            { LANGUAGE.Spanish, "Rango mejorado" }
        });
        localization.Add(TYPE_APPEARING_MESSAGE.SkillCombo.ToString(), new Dictionary<LANGUAGE, string>() {
            { LANGUAGE.English, "Skill combo" },
            { LANGUAGE.Russian, "Комбо навыков" },
            { LANGUAGE.Spanish, "Combo de habilidades" }
        });
        localization.Add(TYPE_APPEARING_MESSAGE.MasterOfSkills.ToString(), new Dictionary<LANGUAGE, string>() {
            { LANGUAGE.English, "Master of skills" },
            { LANGUAGE.Russian, "Мастер навыков" },
            { LANGUAGE.Spanish, "Maestro de habilidades" }
        });
        localization.Add("YourNickname".ToString(), new Dictionary<LANGUAGE, string>() {
            { LANGUAGE.English, "Your Nickname" },
            { LANGUAGE.Russian, "Ваш ник" },
            { LANGUAGE.Spanish, "Muda-dada-da?" }
        });
        localization.Add("DescriptionEmailText".ToString(), new Dictionary<LANGUAGE, string>() {
            { LANGUAGE.English, "You also may link your current ID-account to Email" },
            { LANGUAGE.Russian, "Вы также можете привязать вашу почту к текущему ID-аккаунту" },
            { LANGUAGE.Spanish, "Muda-dada-da?" }
        });
        localization.Add("YourEmail", new Dictionary<LANGUAGE, string>() {
            { LANGUAGE.English, "Your Email" },
            { LANGUAGE.Russian, "Ваш Email" },
            { LANGUAGE.Spanish, "Muda-dada-da?" }
        });
        localization.Add("Login", new Dictionary<LANGUAGE, string>() {
            { LANGUAGE.English, "Login" },
            { LANGUAGE.Russian, "Логин" },
            { LANGUAGE.Spanish, "Loginue?" }
        });
        localization.Add("ChangeNickname", new Dictionary<LANGUAGE, string>() {
            { LANGUAGE.English, "Change nickname" },
            { LANGUAGE.Russian, "Изменить ник" },
            { LANGUAGE.Spanish, "nickue???" }
        });
        localization.Add("Link", new Dictionary<LANGUAGE, string>() {
            { LANGUAGE.English, "Link" },
            { LANGUAGE.Russian, "Привязать" },
            { LANGUAGE.Spanish, "MDAAAAA?" }
        });
        localization.Add(C.Notifications.AccountLinked, new Dictionary<LANGUAGE, string>() {
            { LANGUAGE.English, "Your account was seccessfull linked to Email!" },
            { LANGUAGE.Russian, "Ваш аккаунт был успешно привязан к Email-у!" },
            { LANGUAGE.Spanish, "ughhhhh....?" }
        });
        localization.Add(C.Notifications.SignInEmail, new Dictionary<LANGUAGE, string>() {
            { LANGUAGE.English, "Successfull signing in account by Email!" },
            { LANGUAGE.Russian, "Успешный вход в аккаунт через Email!" },
            { LANGUAGE.Spanish, "ughhhhh....?" }
        });
        localization.Add(C.Notifications.SignInIDMobile, new Dictionary<LANGUAGE, string>() {
            { LANGUAGE.English, "Successfull signing in account by mobile ID!" },
            { LANGUAGE.Russian, "Успешный вход в аккаунт c помощью ID устройства!" },
            { LANGUAGE.Spanish, "ughhhhh....?" }
        });
        localization.Add(C.Notifications.AccountAlreadyLinked, new Dictionary<LANGUAGE, string>() {
            { LANGUAGE.English, "This account is linked already to another Email!" },
            { LANGUAGE.Russian, "Данный аккаунт уже привязан к другом Email-у!" },
            { LANGUAGE.Spanish, "ughhhhh....?" }
        });
        localization.Add(C.Notifications.InvalidEmailAddress, new Dictionary<LANGUAGE, string>() {
            { LANGUAGE.English, "Invalid format of Email address, required format is: 'a@a.a'!" },
            { LANGUAGE.Russian, "Неверный формат электронной почты, требуется формат: 'a@a.a'!" },
            { LANGUAGE.Spanish, "ughhhhh....?" }
        });
        localization.Add(C.Notifications.AccountNotFound, new Dictionary<LANGUAGE, string>() {
            { LANGUAGE.English, "Account not found for a specified Email!" },
            { LANGUAGE.Russian, "Для заданного Email-а аккаунт не найден!" },
            { LANGUAGE.Spanish, "ughhhhh....?" }  
        });
        localization.Add(C.Notifications.AccountAlreadyLinkedToSpecifiedEmail, new Dictionary<LANGUAGE, string>() {
            { LANGUAGE.English, "Account linked ALREADY to specified Email!" },
            { LANGUAGE.Russian, "Текущий аккаунт УЖЕ привязан к заданному Email-у!" },
            { LANGUAGE.Spanish, "ughhhhh....?" }
        });
        localization.Add(C.Notifications.DisplayNameTooShort, new Dictionary<LANGUAGE, string>() {
            { LANGUAGE.English, "Nickname should be no shorter than 3 symbols!" },
            { LANGUAGE.Russian, "Ник должен состоять из не менее 3-ёх символов!" },
            { LANGUAGE.Spanish, "ughhhhh....?" }
        });
        localization.Add(C.Just.NextLevel, new Dictionary<LANGUAGE, string>() {
            { LANGUAGE.English, "Next level" },
            { LANGUAGE.Russian, "Следующий уровень" },
            { LANGUAGE.Spanish, "Uhu-hu" }
        });
        localization.Add(C.Just.ShowLeaderboard, new Dictionary<LANGUAGE, string>() {
            { LANGUAGE.English, "Show leaderboard" },
            { LANGUAGE.Russian, "Показать таблицу лидеров" },
            { LANGUAGE.Spanish, "Uhu-hu" }
        });
        localization.Add("ChooseLevel", new Dictionary<LANGUAGE, string>() {
            { LANGUAGE.English, "Choose level" },
            { LANGUAGE.Russian, "Выберите уровень" },
            { LANGUAGE.Spanish, "Ура!" }
        });
        localization.Add("Level1", new Dictionary<LANGUAGE, string>() {
            { LANGUAGE.English, "Level 1" },
            { LANGUAGE.Russian, "Уровень 1" },
            { LANGUAGE.Spanish, "Ура!" }
        });
        localization.Add("Level2", new Dictionary<LANGUAGE, string>() {
            { LANGUAGE.English, "Level 2" },
            { LANGUAGE.Russian, "Уровень 2" },
            { LANGUAGE.Spanish, "Ура!" }
        });
        localization.Add("Level3", new Dictionary<LANGUAGE, string>() {
            { LANGUAGE.English, "Level 3" },
            { LANGUAGE.Russian, "Уровень 3" },
            { LANGUAGE.Spanish, "Ура!" }
        });
        localization.Add("Level4", new Dictionary<LANGUAGE, string>() {
            { LANGUAGE.English, "Level 4" },
            { LANGUAGE.Russian, "Уровень 4" },
            { LANGUAGE.Spanish, "Ура!" }
        });
        localization.Add("Level5", new Dictionary<LANGUAGE, string>() {
            { LANGUAGE.English, "Level 5" },
            { LANGUAGE.Russian, "Уровень 5" },
            { LANGUAGE.Spanish, "Ура!" }
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

