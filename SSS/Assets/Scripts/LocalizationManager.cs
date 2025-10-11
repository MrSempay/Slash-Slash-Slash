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
        localization.Add("YourPassword", new Dictionary<LANGUAGE, string>() {
            { LANGUAGE.English, "Your password" },
            { LANGUAGE.Russian, "Ваш пароль" },
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
        localization.Add(C.Notifications.InvalidFormatEmailAddressOrPassword, new Dictionary<LANGUAGE, string>() {
            { LANGUAGE.English, "Invalid format of Email address or password!" },
            { LANGUAGE.Russian, "Неверный формат электронной почты или пароля!" },
            { LANGUAGE.Spanish, "ughhhhh....?" }
        });
        localization.Add(C.Notifications.InvalidEmailAddressOrPassword, new Dictionary<LANGUAGE, string>() {
            { LANGUAGE.English, "Can't find account with such Email and password pair!" },
            { LANGUAGE.Russian, "Не удаётся найти аккаунт с заданными Email-ом и паролем!" },
            { LANGUAGE.Spanish, "ughhhhh....?" }
        });
        localization.Add(C.Notifications.InvalidEmailAddress, new Dictionary<LANGUAGE, string>() {
            { LANGUAGE.English, "Invalid password!" },
            { LANGUAGE.Russian, "Неверный пароль!" },
            { LANGUAGE.Spanish, "ughhhhh....?" }
        });
        localization.Add(C.Notifications.NoInternetConnection, new Dictionary<LANGUAGE, string>() {
            { LANGUAGE.English, "No internet connection!" },
            { LANGUAGE.Russian, "Нет соединения с интернетом!" },
            { LANGUAGE.Spanish, "ughhhhh....?" }
        });
        localization.Add(C.Notifications.EmailPasswordRecoveyrWasSent, new Dictionary<LANGUAGE, string>() {
            { LANGUAGE.English, "Letter password recovery was sent at specified Email!" },
            { LANGUAGE.Russian, "Письмо для восстановления пароля было отправлено на указанный Email адрес!" },
            { LANGUAGE.Spanish, "ughhhhh....?" }
        });
        localization.Add(C.Notifications.EmailPasswordRecoverFailure, new Dictionary<LANGUAGE, string>() {
            { LANGUAGE.English, "Email password recover wasn't sent by some failure!" },
            { LANGUAGE.Russian, "Email для восстановления пароля был отправлен из-за неведомой ошибки!" },
            { LANGUAGE.Spanish, "ughhhhh....?" }
        });
        localization.Add(C.Notifications.ServiceUnavailable, new Dictionary<LANGUAGE, string>() {
            { LANGUAGE.English, "Server is temporary unavailable or missing internet connection!" },
            { LANGUAGE.Russian, "Сервер временно недоступен либо отсутствует подключение к интернету!" },
            { LANGUAGE.Spanish, "ughhhhh....?" }
        });
        localization.Add(C.Notifications.CantGetLeaderboard, new Dictionary<LANGUAGE, string>() {
            { LANGUAGE.English, "Can't get actual leaderboard, please, check internet connection" },
            { LANGUAGE.Russian, "Не получилось получить актуальную таблицу лидеров, пожалуйста, проверьте соединение с интернетом" },
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
        localization.Add(C.Notifications.PasswordTooShort, new Dictionary<LANGUAGE, string>() {
            { LANGUAGE.English, "Password must contains at least 7 characters!" },
            { LANGUAGE.Russian, "Пароль должен содержать не менее 7 символов!" },
            { LANGUAGE.Spanish, "ughhhhh....?" }
        });
        localization.Add(C.Notifications.EmailAddressNotAvailable, new Dictionary<LANGUAGE, string>() {
            { LANGUAGE.English, "This Email is already linked to some another account!" },
            { LANGUAGE.Russian, "Даннаый Email уже привязан к другому аккаунту!" },
            { LANGUAGE.Spanish, "ughhhhh....?" }
        });
        localization.Add(C.Just.NextLevel, new Dictionary<LANGUAGE, string>() {
            { LANGUAGE.English, "Next level" },
            { LANGUAGE.Russian, "Следующий уровень" },
            { LANGUAGE.Spanish, "Uhu-hu" }
        });
        localization.Add(C.Other.SkipStudy, new Dictionary<LANGUAGE, string>() {
            { LANGUAGE.English, "Skip study" },
            { LANGUAGE.Russian, "Пропустить обучение" },
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
        localization.Add("RecoveryPassword", new Dictionary<LANGUAGE, string>() {
            { LANGUAGE.English, "Recovery password" },
            { LANGUAGE.Russian, "Восстановить пароль" },
            { LANGUAGE.Spanish, "Ура!" }
        });
        localization.Add("ShowNotifications", new Dictionary<LANGUAGE, string>() {
            { LANGUAGE.English, "Show notifications" },
            { LANGUAGE.Russian, "Показывать уведомления" },
            { LANGUAGE.Spanish, "Ура!" }
        });
        localization.Add(C.Other.SkipWaveWait, new Dictionary<LANGUAGE, string>() {
            { LANGUAGE.English, "Волна не ждёт!" },
            { LANGUAGE.Russian, "Wave doesn't wait!" },
            { LANGUAGE.Spanish, "Ура!" }
        });

        localization.Add(C.DK.Spear + C.Prefixes.Description, new Dictionary<LANGUAGE, string>() {
    { LANGUAGE.English, "Ancient spine of Hartgard, wielded by caravan guards. Strikes like lightning, but drains the grip" },
    { LANGUAGE.Russian, "Древний шип Хартгарда, использовался стражниками караванов. Разит молниеносно, но истощает хватку" },
    { LANGUAGE.Spanish, "Espina antigua de Hartgard, usada por guardias de caravanas. Golpea como un rayo, pero agota el agarre" }
});

        localization.Add(C.DK.Sword + C.Prefixes.Description, new Dictionary<LANGUAGE, string>() {
    { LANGUAGE.English, "Simple blade of a castle guard, tempered in Hartgard's forges before the shadow invasion. Reliable in any skirmish" },
    { LANGUAGE.Russian, "Простой клинок стража замка, закаленный в пламени кузниц Хартгарда перед нашествием теней. Надежен в любой стычке" },
    { LANGUAGE.Spanish, "Hoja simple de un guardia del castillo, templada en las forjas de Hartgard antes de la invasión de sombras. Confiable en cualquier escaramuza" }
});

        localization.Add(C.DK.Knife + C.Prefixes.Description, new Dictionary<LANGUAGE, string>() {
    { LANGUAGE.English, "Thin blade from clan warehouses, crafted for shadowy dealings with smugglers" },
    { LANGUAGE.Russian, "Тонкий клинок из складов клана, выточенный для теневых сделок с контрабандистами" },
    { LANGUAGE.Spanish, "Hoja delgada de los almacenes del clan, forjada para tratos sombríos con contrabandistas" }
});

        localization.Add(C.DK.Axe + C.Prefixes.Description, new Dictionary<LANGUAGE, string>() {
    { LANGUAGE.English, "Heavy cleaver of Hartgard's lumberjacks. Crushes with a chance to stun, but devours stamina" },
    { LANGUAGE.Russian, "Тяжелый тесак лесорубов Хартгарда. Крушит с шансом оглушить, но жрет выносливость" },
    { LANGUAGE.Spanish, "Hacha pesada de los leñadores de Hartgard. Aplasta con posibilidad de aturdir, pero devora resistencia" }
});

        localization.Add(C.DK.DexterityBracelet + C.Prefixes.Description, new Dictionary<LANGUAGE, string>() {
    { LANGUAGE.English, "Bracelet woven from silver threads, found in the treasury of ruins. Hastens steps through castle corridors, where every moment is life, without shadows of doubt, like pure barter instinct" },
    { LANGUAGE.Russian, "Плетеный из серебряных нитей браслет, найденный в сокровищнице руин. Ускоряет шаги по коридорам замка, где каждый миг — жизнь, без теней сомнений, как чистый инстинкт бартера." },
    { LANGUAGE.Spanish, "Brazalete tejido con hilos de plata, encontrado en el tesoro de las ruinas. Acelera los pasos por los corredores del castillo, donde cada momento es vida, sin sombras de duda, como puro instinto de trueque" }
});

        localization.Add(C.DK.MedallionOfLife + C.Prefixes.Description, new Dictionary<LANGUAGE, string>() {
    { LANGUAGE.English, "Amulet of Hartgard's lords, imbued with elixirs of ancient healers" },
    { LANGUAGE.Russian, "Амулет лордов Хартгарда, пропитанный эликсирами древних целителей." },
    { LANGUAGE.Spanish, "Amuleto de los señores de Hartgard, impregnado con elixires de sanadores antiguos" }
});

        localization.Add(C.DK.RingWarrior + C.Prefixes.Description, new Dictionary<LANGUAGE, string>() {
    { LANGUAGE.English, "Ring of warrior's honor, engraved with battle runes" },
    { LANGUAGE.Russian, "Кольцо воинской чести, гравированное рунами битв." },
    { LANGUAGE.Spanish, "Anillo del honor guerrero, grabado con runas de batalla" }
});

        localization.Add(C.DK.LeatherArmor + C.Prefixes.Description, new Dictionary<LANGUAGE, string>() {
    { LANGUAGE.English, "Simple hide jerkin, sewn by Hartgard's craftsmen. Perfect for quick maneuvers in ruins, but vulnerable to fangs" },
    { LANGUAGE.Russian, "Простая куртка из шкур, сшитая мастерами Хартгарда. Идеальна для быстрых маневров в руинах, но уязвима к клыкам" },
    { LANGUAGE.Spanish, "Chaqueta simple de pieles, cosida por artesanos de Hartgard. Perfecta para maniobras rápidas en ruinas, pero vulnerable a colmillos" }
});

        localization.Add(C.DK.ChainMail + C.Prefixes.Description, new Dictionary<LANGUAGE, string>() {
    { LANGUAGE.English, "Armor of links, forged from metal of battles with neighbors. Modest in defense, but reliable" },
    { LANGUAGE.Russian, "Доспех из звеньев, собранных из металла битв с соседями. Скромна в обороне, но надежна" },
    { LANGUAGE.Spanish, "Armadura de eslabones, forjada con metal de batallas contra vecinos. Modesta en defensa, pero confiable" }
});

        localization.Add(C.DK.PlateArmor + C.Prefixes.Description, new Dictionary<LANGUAGE, string>() {
    { LANGUAGE.English, "Plate cuirass, forged from metal. Impenetrable against light attacks, but weighs on shoulders, limiting mobility" },
    { LANGUAGE.Russian, "Пластинчатый панцирь, выкованный из металла. Непробиваем пред легкими атаками, но давит на плечи, чем ограничивает подвижность." },
    { LANGUAGE.Spanish, "Coraza de placas, forjada de metal. Impenetrable ante ataques ligeros, pero pesa en los hombros, limitando la movilidad" }
});

        localization.Add(C.DK.ThunderAxe + C.Prefixes.Description, new Dictionary<LANGUAGE, string>() {
    { LANGUAGE.English, "Thunderer's axe, forged in Hartgard's smithies from metal of distant lands" },
    { LANGUAGE.Russian, "Топор громовержца, выкованный в кузницах Хартгарда из металла далеких земель" },
    { LANGUAGE.Spanish, "Hacha del atronador, forjada en las herrerías de Hartgard con metal de tierras lejanas" }
});

        localization.Add(C.DK.ThunderArmor + C.Prefixes.Description, new Dictionary<LANGUAGE, string>() {
    { LANGUAGE.English, "Thunderer's armor, forged in Hartgard's smithies from metal of distant lands" },
    { LANGUAGE.Russian, "Броня громовержца, выкованная в кузницах Хартгарда из металла далеких земель" },
    { LANGUAGE.Spanish, "Armadura del atronador, forjada en las herrerías de Hartgard con metal de tierras lejanas" }
});

        localization.Add(C.DK.FireSword + C.Prefixes.Description, new Dictionary<LANGUAGE, string>() {
    { LANGUAGE.English, "In the hour when the heavens above Hartgard split, the Dragon Defender tore its claw from the flesh of eternity, and the clan lord reforged it into a blade" },
    { LANGUAGE.Russian, "В час, когда небеса над Хартгардом раскололись, Защитник Дракона вырвал свой коготь из плоти вечности, и лорд клана перековал его в клинок" },
    { LANGUAGE.Spanish, "En la hora cuando los cielos sobre Hartgard se partieron, el Defensor Dragón arrancó su garra de la carne de la eternidad, y el señor del clan la reforjó en una hoja" }
});

        localization.Add(C.DK.DragonArmor + C.Prefixes.Description, new Dictionary<LANGUAGE, string>() {
    { LANGUAGE.English, "In the depths of Eldvir's scorching forges, in the flame of the Dragon - guardian of heavens, the scales of this ancient titan were reforged into magnificent armor" },
    { LANGUAGE.Russian, "В недрах раскаленных кузниц Элдвира, в пламени Дракона — стража небес, чешуя сего древнего титана, была перекована в великолепные доспехи." },
    { LANGUAGE.Spanish, "En las profundidades de las forjas ardientes de Eldvir, en la llama del Dragón - guardián de los cielos, las escamas de este antiguo titán fueron reforjadas en armadura magnífica" }
});

        localization.Add(C.DK.RingBerserker + C.Prefixes.Description, new Dictionary<LANGUAGE, string>() {
    { LANGUAGE.English, "Ring of madmen, cast from metal. Allows entering a trance, making attacks reckless" },
    { LANGUAGE.Russian, "Кольцо безумцев, отлитое из металла. Позволяет входить в транс, делая атаки безрасудными." },
    { LANGUAGE.Spanish, "Anillo de locos, fundido en metal. Permite entrar en trance, haciendo los ataques temerarios" }
});

        localization.Add(C.DK.RingForesight + C.Prefixes.Description, new Dictionary<LANGUAGE, string>() {
    { LANGUAGE.English, "Crystal from treasuries, whispering of what's to come" },
    { LANGUAGE.Russian, "Кристалл из сокровищниц, шепчущий о грядущем." },
    { LANGUAGE.Spanish, "Cristal de los tesoros, susurrando sobre el futuro" }
});

        localization.Add(C.DK.Tragicomedy + C.Prefixes.Description, new Dictionary<LANGUAGE, string>() {
    { LANGUAGE.English, "Mask of nameless origin, emerging from shadows of Hartgard's forgotten stages, where whispers in its cracks echoed sagas of a figure in dualistic guise - a shadow that bent the lord to whispers of betrayal, promising triumph over darkness through severing bonds with brother clans, sowing seeds of downfall in ruins of isolation" },
    { LANGUAGE.Russian, "Маска безымянного происхождения, вынырнувшая из теней забытых подмостков Хартгарда, где шепот в ее трещинах эхом повторял саги о фигуре в дуалистском облике — тени, что склонила лорда к шепоту предательства, обещая триумф над тьмой через разрыв уз с братьями-кланами, сея семя падения в руинах изоляции." },
    { LANGUAGE.Spanish, "Máscara de origen anónimo, emergiendo de las sombras de los escenarios olvidados de Hartgard, donde susurros en sus grietas hacían eco de sagas sobre una figura de aspecto dualista - una sombra que inclinó al señor hacia susurros de traición, prometiendo triunfo sobre la oscuridad mediante la ruptura de lazos con clanes hermanos, sembrando semillas de caída en ruinas de aislamiento" }
});

        localization.Add(C.DK.ThirstySakura + C.Prefixes.Description, new Dictionary<LANGUAGE, string>() {
    { LANGUAGE.English, "If you listen closely, the blade whispers sagas of Lord Eldvir - a titan who stood as a wall against waves of darkness until his last breath, drinking in the thirst for enemy blood and pouring it into steel, sowing seeds of untamed fury" },
    { LANGUAGE.Russian, "Если прислушаться, то лезвие шепчет саги о лорде Элдвире — титане, что стоял стеной против волн тьмы до последнего вздоха, впивая в себя жажду крови врагов и вливая её в сталь, сея семя неукротимой ярости." },
    { LANGUAGE.Spanish, "Si escuchas atentamente, la hoja susurra sagas del señor Eldvir - un titán que se mantuvo como un muro contra olas de oscuridad hasta su último aliento, absorbiendo la sed de sangre enemiga y vertiéndola en acero, sembrando semillas de furia indomable" }
});
        
        localization.Add(C.DK.ArcLightning + C.Prefixes.Description, new Dictionary<LANGUAGE, string>() {
    { LANGUAGE.English, "A rune developed by the mages of Clan Hartgard in a desperate attempt to hold back the onslaught of the hyenas. Instead of an instant flash, it creates a crackling wall of lightning before the caster, dealing colossal damage." },
    { LANGUAGE.Russian, "Руна, разработанная магами клана Хартгард в отчаянной попытке сдержать натиск гиен. Вместо мгновенной вспышки, она создает перед заклинателем потрескивающую стену из молний наносящую колосальный урон." },
    { LANGUAGE.Spanish, "Una runa desarrollada por los magos del Clan Hartgard en un intento desesperado por contener la embestida de las hienas. En lugar de un destello instantáneo, crea un crepitante muro de relámpagos ante el lanzador que inflige un daño colosal." }
});

        localization.Add(C.DK.Berserker + C.Prefixes.Description, new Dictionary<LANGUAGE, string>() {
    { LANGUAGE.English, "An instinctive, bestial rune that plunges the warrior into a state of pure madness, reflecting the essence of survival in the wild lands." },
    { LANGUAGE.Russian, "Инстинктивная, звериная руна, погружает воина в состояние чистого безумия, отражающая суть выживания в диких землях." },
    { LANGUAGE.Spanish, "Una runa instintiva y bestial que sumerge al guerrero en un estado de pura locura, reflejando la esencia de la supervivencia en las tierras salvajes." }
});

        localization.Add(C.DK.Healing + C.Prefixes.Description, new Dictionary<LANGUAGE, string>() {
    { LANGUAGE.English, "In every great clan, be it Hartgard or Eldvir, the foundation of any infirmary was the Rune of Minor Healing. This is not the legendary magic that saves heroes on the battlefield, but a low-level, yet indispensable rune for daily work. Young healers and acolytes received it first." },
    { LANGUAGE.Russian, "В каждом великом клане, будь то Хартгард или Элдвир, основой любого лазарета была Руна Малого Исцеления. Молодые целители и аколиты получали её в первую очередь." },
    { LANGUAGE.Spanish, "En cada gran clan, ya fuera Hartgard o Eldvir, la base de cualquier enfermería era la Runa de Sanación Menor. No es la magia legendaria que salva a los héroes en el campo de batalla, sino una runa de bajo nivel, pero indispensable para el trabajo diario. Los jóvenes sanadores y acólitos la recibían en primer lugar." }
});

        localization.Add(C.DK.ProtectiveField + C.Prefixes.Description, new Dictionary<LANGUAGE, string>() {
    { LANGUAGE.English, "The Rune of the Protective Field was standard equipment for the guard, especially for those who stood on the walls or guarded the gates. It was carved on the bracers or the inner side of every guard's shield. This was not a rune for duels, but a tool for holding the formation." },
    { LANGUAGE.Russian, "Руна Защитного Поля была стандартной экипировкой для стражи, особенно для тех, кто стоял на стенах или охранял ворота. Это была не руна для поединков, а инструмент для удержания строя." },
    { LANGUAGE.Spanish, "La Runa del Campo Protector era el equipamiento estándar para la guardia, especialmente para aquellos que vigilaban en las murallas o protegían las puertas. Se grababa en los brazales o en el lado interior del escudo de cada guardia. No era una runa para duelos, sino una herramienta para mantener la formación." }
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

