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
        public const string NameTargetEquipmentPanelPlayer = "nameTargetEquipmentPanelPlayer";
                // значения параметров зданий
                public const string SpellPanel = "SpellPanel";
                public const string AmmunitionPanel = "AmmunitionPanel";


        // пути для файлов/папок
        public const string FolderImagesForSpells = "Images/Spells/";
        public const string FolderImagesForAmmunition = "Images/Ammunition/";
        public const string ImageDoorOpened = "Images/Door/DoorOpened";
        public const string ImageDoorClosed = "Images/Door/DoorClosed";
        public const string PrefabDialogueWindowForPlayer = "Prefabs/UI/DialogueWindowForPlayer";
        public const string PrefabAppearingSprite = "Prefabs/UI/AppearingSprite";
        public const string PathFolderImagesForToggles = "Images/UI/Toggles/";
        public const string PathFolderImagesForAppearingMessages = "Images/UI/AppearingMessages/";
        public const string IboPostProcessProfile = "IboPostProcessProfile";

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
        public const string BigSword = "BigSword";
        public const string Blade = "Blade";
        public const string SaintDragonSword = "SaintDragonSword";
        public const string WitchBlade = "WitchBlade";

        public const string NormalArmor1 = "NormalArmor1";
        public const string NormalArmor2 = "NormalArmor2";
        public const string BigArmor1 = "BigArmor1";
        public const string BigArmor2 = "BigArmor2";
        public const string LegendaryArmor1 = "LegendaryArmor1";
        public const string LegendaryArmor2 = "LegendaryArmor2";

        public const string DeathBook = "DeathBook";
        public const string LifeBook = "LifeBook";
        public const string GreenBook = "GreenBook";
        public const string RedBook = "RedBook";
        public const string MathBook = "MathBook";
        public const string TjanulDedRepku = "TjanulDedRepku";

        // Ключи полей для предметов
        public const string equipmentName = "equipmentName";
        public const string cost = "cost";
        public const string timeCallDown = "timeCallDown";
        public const string amountUpCombo = "amountUpCombo";
        public const string increasingUnitParametersByAmmunitionPercentage = "increasingUnitParametersByAmmunitionPercentage";
        public const string increasingUnitParametersByAmmunitionAbsolute = "increasingUnitParametersByAmmunitionAbsolute";


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
        public const string damageReduction = "damageReduction";
        public const string speed = "speed";
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
        public const string som = "som";

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
        public const string SampleScene = "SampleScene";
        public const string MainMenu = "MainMenu";
    }
    public static class Prefixes
    {
        public const string PrefixTrigger = "Trigger";

    }
}
