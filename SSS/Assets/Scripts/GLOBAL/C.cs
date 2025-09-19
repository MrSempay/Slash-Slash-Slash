using System.ComponentModel;
using UnityEngine;

// Класс констант (Const)

public class C
{
    // ключи для словарей (DectionaryKeys)
    public static class DK
    {
        // названия зданий

        public const string School = "School"; 
        public const string Treasury = "Treasury";

        // параметры зданий

        public const string TimeForUpdateAssortiment = "timeForUpdateAssortiment";
        public const string FolderImagesOfEquipment = "folderImagesOfEquipment";
        public const string customScriptsEquipment = "customScriptsEquipment";
        public const string NameTargetEquipmentPanelPlayer = "nameTargetEquipmentPanelPlayer";
                // значения параметров зданий
                public const string SpellPanel = "SpellPanel";
                public const string AmmunitionPanel = "AmmunitionPanel";


        // пути для файлов/папок


        // Категории предметов
        public const string Weapon = "Weapon";
        public const string Armor = "Armor";
        public const string Accessories = "Accessories";

        // Редкость предметов
        public const string Standart = "Standart";
        public const string Rare = "Rare";
        public const string Legendary = "Legendary";

        // Названия предметов
        public const string Sword = "Sword";
        public const string Knife = "Knife";
        public const string Spear = "Spear";
        public const string ThunderAxe = "ThunderAxe";
        public const string Axe = "Axe";
        public const string FireSword = "FireSword";
        public const string SaintDragonSword = "SaintDragonSword";
        public const string ThirstySakura = "ThirstySakura";

        public const string LeatherArmor = "LeatherArmor";
        public const string ChainMail = "ChainMail";
        public const string PlateArmor = "PlateArmor";
        public const string ThunderArmor = "ThunderArmor";
        public const string DragonArmor = "DragonArmor";
        public const string LegendaryArmor1 = "LegendaryArmor1";
        public const string LegendaryArmor2 = "LegendaryArmor2";

        public const string RingBerserker = "RingBerserker";
        public const string DexterityBracelet = "DexterityBracelet";
        public const string MedallionOfLife = "MedallionOfLife";
        public const string RingWarrior = "RingWarrior";
        public const string RingForesight = "RingForesight";
        public const string Tragicomedy = "Tragicomedy";
        public const string RedBook = "RedBook";
        public const string MathBook = "MathBook";
        public const string TjanulDedRepku = "TjanulDedRepku";

        // Ключи полей для предметов

        public const string equipmentName = "equipmentName";
        public const string cost = "cost";
        public const string timeCallDown = "timeCallDown";
        public const string healthHealAmount = "healthHealAmount";
        public const string shouldBeCastedAtStartUnitAnimation = "shouldBeCastedAtStartUnitAnimation";
        public const string amountUpCombo = "amountUpCombo";
        public const string amountBlockingAttackMax = "amountBlockingAttackMax";
        public const string durationActiveState = "durationActiveState";
        public const string increasingUnitParametersByAmmunitionPercentage = "increasingUnitParametersByAmmunitionPercentage";
        public const string increasingUnitParametersByAmmunitionAbsolute = "increasingUnitParametersByAmmunitionAbsolute";
        public const string increasingUnitParametersByAmmunitionPercentageByCast = "increasingUnitParametersByAmmunitionPercentageByCast";

        // названия заклинаний

        public const string ProtectiveField = "ProtectiveField";
        public const string ArcLightning = "ArcLightning";
        public const string Berserker = "Berserker";
        public const string Healing = "Healing";

        // названия юнитов
        public const string Player = "Player";
        public const string MeleeEnemy = "MeleeEnemy";
        public const string Door = "Door";

        // Параметры юнитов
        public const string healthMax = "healthMax";
        public const string callDownMeleeAttack = "callDownMeleeAttack";
        public const string timeRecoverStaminaPoint = "timeRecoverStaminaPoint";
        public const string timeZeroizeKillComboTicks = "timeZeroizeKillComboTicks";
        public const string staminaMax = "staminaMax";
        public const string DamageReductionPercentage = "DamageReductionPercentage";
        public const string speed = "speed";
        public const string evasionPercentage = "evasionPercentage";
        public const string timeStuneByStanartAttack = "timeStuneByStanartAttack";
        public const string stuneChanceByStandardAttackPercentage = "stuneChanceByStandardAttackPercentage";
        public const string jumpForce = "jumpForce";
        public const string moneyFromKill = "moneyFromKill";
        public const string experienceToNextLevel = "experienceToNextLevel";
        public const string experienceFromKill = "experienceFromKill";
        public const string scoreFromKill = "scoreFromKill";
        public const string comboOneHitKillMultiplayer = "comboOneHitKillMultiplayer";
        public const string increasingGettingExperienceByKillComboTickPercentage = "increasingGettingExperienceByKillComboTickPercentage";
        public const string increasingGettingMoneyByKillComboTickPercentage = "increasingGettingMoneyByKillComboTickPercentage";
        public const string increasingParametersByLevelUpPercentage = "increasingParametersByLevelUpPercentage";
        public const string damage = "damage";
        public const string nameSoundGettingDamage = "nameSoundGettingDamage";
        public const string CountAccessToUpInSchool = "CountAccessToUpInSchool"; // Это исключение, так как строка начинается с заглавной (все свойства будут такими)
        public const string CurrentMoney = "CurrentMoney"; // Это исключение, так как строка начинается с заглавной
        public const string CurrentMinimumAmountCombo = "CurrentMinimumAmountCombo"; 
        public const string CurrentIncreasingStamina = "CurrentIncreasingStamina"; 
        public const string som = "som";

        // параметры уровня

        public const string percentageIncreaseEnemiesParametersBySpawnIteration = "percentageIncreaseEnemiesParametersBySpawnIteration";
        public const string absoluteIncreaseEnemiesParametersBySpawnIteration = "absoluteIncreaseEnemiesParametersBySpawnIteration";
        public const string timeBetweenEnemySpawnIteration = "timeBetweenEnemySpawnIteration";



        // названия настроек

        public const string ParameterOrientation = "ParameterOrientation";

        // параметры настроек

        public const string listChosing = "listChosing";

    }

    // строковые значения полей

    public static class Values
    {
        // значения настроек

        public const string orientationHorizontal = "Horizontal";
        public const string orientationVertical = "Vertical";
    }

    public static class NameScene
    {
        public const string SceneDialogue = "SceneDialogue";
        public const string Level1 = "Level1";
        public const string Level2 = "Level2";
        public const string Level5 = "Level5";
        public const string SampleScene = "SampleScene";
        public const string MainMenu = "MainMenu";
    }
    public static class Prefixes
    {
        public const string PrefixTrigger = "Trigger";
        public const string PrefixActivate = "Activate";
        public const string PrefixDeactivate = "Deactivate";
        public const string EnteredInventory = "EnteredInventory";
        public const string ExitedInventory = "ExitedInventory";
        public const string Destroyed = "Destroyed";
        public const string Idle = "Idle";
        public const string Appear = "Appear";
        public const string Disappear = "Disappear";
        public const string Cast = "Cast";
        public const string Peak = "Peak";
        public const string Disabled = "Disabled";
        public const string Description = "Description";

    }
    public static class Paths
    {
        public const string FolderImagesForSpells = "Images/Spells/";
        public const string FolderImagesForAmmunition = "Images/Ammunition/";
        public const string FolderBWImagesForAmmunition = "Images/Ammunition/AmmunitionBW/";
        public const string FolderImagesForRankStyles = "Images/UI/RankStyle/";
        public const string ImageDoorOpened = "Images/Door/DoorOpened";
        public const string ImageDoorClosed = "Images/Door/DoorClosed";
        public const string PrefubDialogueWindowForPlayer = "Prefubs/UI/DialogueWindowForPlayerScaling";
        public const string PrefubAppearingSprite = "Prefubs/UI/AppearingSprite";
        public const string PrefubTextButtonBigScaled = "Prefubs/UI/TextButtonBigScaled";
        public const string PrefubTextButton = "Prefubs/UI/TextButton";
        public const string PrefubAppearingText = "Prefubs/UI/AppearingText";
        public const string PrefubAppearingNotification = "Prefubs/UI/AppearingNotification";
        public const string PrefubCustomCombo = "Prefubs/UI/CustomCombo";
        public const string PrefubTextButtonPanelChoose = "Prefubs/UI/TextButtonPanelChoose";
        public const string PrefubFieldEquipmentInfo = "Prefubs/UI/FieldEquipmentInfo";
        public const string PrefubLeaderboard = "Prefubs/UI/Leaderboard";
        public const string PrefubEquipmentInfoPanel = "Prefubs/UI/EquipmentInfoPanelFixedSize";
        public const string PrefubPlaceForEquipment = "Prefubs/UI/PlaceForEquipment"; 
        public const string PrefubAmmunition = "Prefubs/Ammunition"; 
        public const string PrefubSpell = "Prefubs/Spell"; 
        public const string FieldLeaderboard = "Prefubs/UI/FieldInfo";
        public const string FieldLeaderboardScaled = "Prefubs/UI/FieldInfoScaled";
        public const string PathFolderImagesForToggles = "Images/UI/Toggles/";
        public const string PathFolderImagesForAppearingSprites = "Images/UI/AppearingSprites/";
        public const string IboPostProcessProfile = "IboPostProcessProfile";
        public const string FontMonocraft = "Fonts/Monocraft SDF";
        public const string GeneralLocalDataJSON = "GeneralLocalData.json";
        public const string SyncGeneralDataFOLDER = "SyncGeneralData";
        public const string defaultJSON = "default.json";

    }
    public static class Other
    {
        public const string Stune = "Stune";
        public const string CurrentScore = "CurrentScore";
        public const string maxKillCombo = "maxKillCombo";
        public const string timeFromStartLevel = "timeFromStartLevel";
        public const string currentYear = "currentYear";
        public const string MaxReachedLevel = "MaxReachedLevel";
        public const string currentMonth = "currentMonth";
    }
    public static class NamesObjects
    {
        public const string Stune = "Stune";
        public const string RandomTargetForSplit = "RandomTargetForSplit";
        public const string BoxSplitTargetPointsForEnemies = "BoxSplitTargetPointsForEnemies";
        public const string AreaDetectEnteringExiting = "AreaDetectEnteringExiting";
        public const string CallDownIcon = "CallDownIcon";
        public const string PanelChoose = "PanelChoose";
        public const string PlaceInfoPanel = "PlaceInfoPanel";
    }
    public static class AppSprite
    {
        public const string BerserkerEyes = "BerserkerEyes";
    }
    public static class Notifications
    {
        public const string Success = "Success";
        public const string SignInEmail = "SignInEmail";
        public const string SignInIDMobile = "SignInIDMobile";
        public const string PasswordTooShort = "PasswordTooShort";
        public const string AccountAlreadyLinked = "AccountAlreadyLinked";
        public const string EmailAddressNotAvailable = "EmailAddressNotAvailable";
        public const string ServiceUnavailable = "ServiceUnavailable";
        public const string InvalidEmailAddress = "InvalidEmailAddress";
        public const string CantGetLeaderboard = "CantGetLeaderboard";
        public const string EmailPasswordRecoveyrWasSent = "EmailPasswordRecoveyrWasSent";
        public const string EmailPasswordRecoverFailure = "EmailPasswordRecoverFailure";
        public const string NoInternetConnection = "NoInternetConnection";
        public const string AccountAlreadyLinkedToSpecifiedEmail = "AccountAlreadyLinkedToSpecifiedEmail";
        public const string AccountNotFound = "AccountNotFound";
        public const string InvalidFormatEmailAddressOrPassword = "InvalidFormatEmailAddressOrPassword";
        public const string InvalidEmailAddressOrPassword = "InvalidEmailAddressOrPassword";
        public const string DisplayNameTooShort = "DisplayNameTooShort";
        public const string AccountLinked = "AccountLinked";
    }
    public static class Dilogues
    {
        public const string DialogueStart = "DialogueStart";
        public const string DialogueFinish = "DialogueFinish";
    }
    public static class NameFunc
    {
        public const string TriggerEmailForLinkWasChanged = "TriggerEmailForLinkWasChanged";
        public const string TriggerDisplayNameWasChanged = "TriggerDisplayNameWasChanged";
        public const string TriggerPasswordWasChanged = "TriggerPasswordWasChanged";
        public const string UpdatePlayerStatsNEW = "UpdatePlayerStatsNEW";
        public const string UpdateMaxReachedLevel = "UpdateMaxReachedLevel";
    }
    public static class Just
    {
        public const string Infinite = "Infinite";
        public const string NextLevel = "NextLevel";
        public const string ShowLeaderboard = "ShowLeaderboard";
    }
    public static class Animations
    {
        public const string PlayerDied = "PlayerDied";

    }


}
