using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public static class AdjustEquipmentParameters : object
{
    public static readonly Dictionary<string, Dictionary<string, object>> spellParameters = new Dictionary<string, Dictionary<string, object>>()
    {
        // Инициализируем словарь при объявлении
        { "SomeSpell1", new Dictionary<string, object>() {
            { C.DK.equipmentName, "SomeSpell1" },
            { C.DK.cost, 0 },
        } },
        { "SomeSpell2", new Dictionary<string, object>() {
            { C.DK.equipmentName, "SomeSpell2" },
            { C.DK.cost, 0 },
        } },
        { "SomeSpell3", new Dictionary<string, object>() {
            { C.DK.equipmentName, "SomeSpell3" },
            { C.DK.cost, 0 },
        } }
    };

    /*public static readonly Dictionary<string, Dictionary<string, object>> ammunitionParameters = new Dictionary<string, Dictionary<string, object>>()
    {
        // Инициализируем словарь при объявлении
        { "Ammunition1", new Dictionary<string, object>() {
            { C.DK.equipmentName, "Ammunition1" },
            { C.DK.cost, 15 },
        } },
        { "Ammunition2", new Dictionary<string, object>() {
            { C.DK.equipmentName, "Ammunition2" },
            { C.DK.cost, 45 },
        } },
        { "Ammunition3", new Dictionary<string, object>() {
            { C.DK.equipmentName, "Ammunition3" },
            { C.DK.cost, 70 },
        } }
    };*/



    public struct EquipmentChance // структура, описывающая какие предметы в каких разрезах выпадают с какой вероятностью
    {
        public string equipmentCategory; // например Weapon, Armor и т.п
        public string equipmentRarityType; // например Standart, Rare и т.п
        public float chance;    // Вероятность выпадения (в процентах)
    }

    public static List<EquipmentChance> allEquipmentTypesAndCategoriesChance = new List<EquipmentChance>() { 
        new() { equipmentCategory = C.DK.Weapon, equipmentRarityType = C.DK.Standart, chance = 25f } ,
        new() { equipmentCategory = C.DK.Weapon, equipmentRarityType = C.DK.Rare, chance = 8.93f } ,
        new() { equipmentCategory = C.DK.Weapon, equipmentRarityType = C.DK.Legendary, chance = 1.79f } ,
        new() { equipmentCategory = C.DK.Armor, equipmentRarityType = C.DK.Standart, chance = 25f } ,
        new() { equipmentCategory = C.DK.Armor, equipmentRarityType = C.DK.Rare, chance = 8.93f } ,
        new() { equipmentCategory = C.DK.Armor, equipmentRarityType = C.DK.Legendary, chance = 1.79f } ,
        new() { equipmentCategory = C.DK.Accessories, equipmentRarityType = C.DK.Standart, chance = 20f } ,
        new() { equipmentCategory = C.DK.Accessories, equipmentRarityType = C.DK.Rare, chance = 7.14f } ,
        new() { equipmentCategory = C.DK.Accessories, equipmentRarityType = C.DK.Legendary, chance = 1.43f } ,   
    };

    public static readonly Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, object>>>> ammunitionParameters =
        new Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, object>>>>()
        {
            {
                C.DK.Weapon, new Dictionary<string, Dictionary<string, Dictionary<string, object>>>()
                {
                    {
                        C.DK.Standart, new Dictionary<string, Dictionary<string, object>>()
                        {
                            {
                                C.DK.Sword, new Dictionary<string, object>()
                                {
                                    { C.DK.increasingUnitParametersByAmmunition, new Dictionary<string, float>() { { C.DK.damage, 20f } } },
                                    { C.DK.cost, 20 } } },
                            {
                                C.DK.Knife, new Dictionary<string, object>()
                                {
                                    { C.DK.increasingUnitParametersByAmmunition, new Dictionary<string, float>() { { C.DK.damage, 20f } } },
                                    { C.DK.cost, 20 } } } } },
                    {
                        C.DK.Rare, new Dictionary<string, Dictionary<string, object>>()
                        {
                            {
                                C.DK.BigSword, new Dictionary<string, object>()
                                {
                                    { C.DK.increasingUnitParametersByAmmunition, new Dictionary<string, float>() { { C.DK.damage, 50f } } },
                                    { C.DK.cost, 70 } } },
                            {
                                C.DK.Blade, new Dictionary<string, object>()
                                {
                                    { C.DK.increasingUnitParametersByAmmunition, new Dictionary<string, float>() { { C.DK.damage, 50f } } },
                                    { C.DK.cost, 70 } } } } },
                    {
                        C.DK.Legendary, new Dictionary<string, Dictionary<string, object>>()
                        {
                            {
                                C.DK.SaintDragonSword, new Dictionary<string, object>()
                                {
                                    { C.DK.increasingUnitParametersByAmmunition, new Dictionary<string, float>() { { C.DK.damage, 100f } } },
                                    { C.DK.cost, 170 } } },
                            {
                                C.DK.WitchBlade, new Dictionary<string, object>()
                                {
                                    { C.DK.increasingUnitParametersByAmmunition, new Dictionary<string, float>() { { C.DK.damage, 100f } } },
                                    { C.DK.cost, 170 } } } } }, } },
            {
                C.DK.Armor, new Dictionary<string, Dictionary<string, Dictionary<string, object>>>()
                {
                    {
                        C.DK.Standart, new Dictionary<string, Dictionary<string, object>>()
                        {
                            {
                                C.DK.NormalArmor1, new Dictionary<string, object>()
                                {
                                    { C.DK.increasingUnitParametersByAmmunition, new Dictionary<string, float>() { { C.DK.healthMax, 20f } } },
                                    { C.DK.cost, 20 } } },
                            {
                                C.DK.NormalArmor2, new Dictionary<string, object>()
                                {
                                    { C.DK.increasingUnitParametersByAmmunition, new Dictionary<string, float>() { { C.DK.healthMax, 20f } } },
                                    { C.DK.cost, 20 } } } } },
                    {
                        C.DK.Rare, new Dictionary<string, Dictionary<string, object>>()
                        {
                            {
                                C.DK.BigArmor1, new Dictionary<string, object>()
                                {
                                    { C.DK.increasingUnitParametersByAmmunition, new Dictionary<string, float>() { { C.DK.healthMax, 50f } } },
                                    { C.DK.cost, 70 } } },
                            {
                                C.DK.BigArmor2, new Dictionary<string, object>()
                                {
                                    { C.DK.increasingUnitParametersByAmmunition, new Dictionary<string, float>() { { C.DK.healthMax, 50f } } },
                                    { C.DK.cost, 70 } } } } },
                    {
                        C.DK.Legendary, new Dictionary<string, Dictionary<string, object>>()
                        {
                            {
                                C.DK.LegendaryArmor1, new Dictionary<string, object>()
                                {
                                    { C.DK.increasingUnitParametersByAmmunition, new Dictionary<string, float>() { { C.DK.healthMax, 100f } } },
                                    { C.DK.cost, 170 } } },
                            {
                                C.DK.LegendaryArmor2, new Dictionary<string, object>()
                                {
                                    { C.DK.increasingUnitParametersByAmmunition, new Dictionary<string, float>() { { C.DK.healthMax, 100f } } },
                                    { C.DK.cost, 170 } } } } }, } },
            {
                C.DK.Accessories, new Dictionary<string, Dictionary<string, Dictionary<string, object>>>()
                {
                    {
                        C.DK.Standart, new Dictionary<string, Dictionary<string, object>>()
                        {
                            {
                                C.DK.DeathBook, new Dictionary<string, object>()
                                {
                                    { C.DK.equipmentName, "DeathBook" },
                                    { C.DK.cost, 70 } } },
                            {
                                C.DK.LifeBook, new Dictionary<string, object>()
                                {
                                    { C.DK.equipmentName, "LifeBook" },
                                    { C.DK.cost, 70 } } } } },
                    {
                        C.DK.Rare, new Dictionary<string, Dictionary<string, object>>()
                        {
                            {
                                C.DK.RedBook, new Dictionary<string, object>()
                                {
                                    { C.DK.equipmentName, "RedBook" },
                                    { C.DK.cost, 70 } } },
                            {
                                C.DK.GreenBook, new Dictionary<string, object>()
                                {
                                    { C.DK.equipmentName, "GreenBook" },
                                    { C.DK.cost, 70 } } } } },
                    {
                        C.DK.Legendary, new Dictionary<string, Dictionary<string, object>>()
                        {
                            {
                                C.DK.MathBook, new Dictionary<string, object>()
                                {
                                    { C.DK.equipmentName, "MathBook" },
                                    { C.DK.cost, 70 } } },
                            {
                                C.DK.TjanulDedRepku, new Dictionary<string, object>()
                                {
                                    { C.DK.equipmentName, "TjanulDedRepku" },
                                    { C.DK.cost, 70 } } } } }, } }


        };

    // получаем параметр из словаря по названию юнита и параметра
    public static object GetParameter(string spellName, string parameterName)
    {
        if (spellParameters.ContainsKey(spellName) && spellParameters[spellName].ContainsKey(parameterName))
        {
            return spellParameters[spellName][parameterName];
        }
        return null;
    }


    // Метод для получения случайного ключа из словаря spellParameters
    public static string GetRandomSpellName()
    {
        List<string> spellNames = new List<string>(spellParameters.Keys);
        if (spellNames.Count == 0)
        {
            Debug.LogError("No spell names available in unitParameters!");
            return null; // Или какое-то значение по умолчанию
        }
        int randomIndex = UnityEngine.Random.Range(0, spellNames.Count);
        return spellNames[randomIndex];
    }

    public static string GetRandomAmmunitionName(EquipmentChance randomCategoryAndRarityTypesOfEquipment = default)
    {
        List<string> ammunitionNames = new List<string>();
        if (randomCategoryAndRarityTypesOfEquipment.equipmentCategory != null) // Проверяем, что предмет был выбран (randomCategoryAndRarityTypesOfEquipment не остался default)
        {
            foreach (var equipment in ammunitionParameters[randomCategoryAndRarityTypesOfEquipment.equipmentCategory][randomCategoryAndRarityTypesOfEquipment.equipmentRarityType])
            {
                ammunitionNames.Add(equipment.Key);
            }
        }
        else
        {
            foreach (var equipmentCategory in ammunitionParameters)
            {
                foreach (var equipmentRarityType in equipmentCategory.Value)
                {
                    foreach (var equipment in equipmentRarityType.Value)
                    {
                        ammunitionNames.Add(equipment.Key);
                    }
                }
            }
        }
        
        if (ammunitionNames.Count == 0)
           {
               Debug.LogError("No ammunition names available in unitParameters!");
               return null; // Или какое-то значение по умолчанию
           }
        
        int randomIndex = UnityEngine.Random.Range(0, ammunitionNames.Count);
        return ammunitionNames[randomIndex];                
    }

    public static void CallSpellByName(Spell spell)
    {
        // Получаем тип текущего объекта (MyClass)
        Type type = spell.GetType();
        string nameOfSpell = spell.equipmentName;

        // Получаем информацию о методе с указанным именем
        MethodInfo methodInfo = type.GetMethod(nameOfSpell);

        // Проверяем, что метод существует
        if (methodInfo != null)
        {
            // Вызываем метод
            methodInfo.Invoke(spell, null); // this - экземпляр объекта, null - аргументы (если есть)
        }
        else
        {
            Debug.Log($"Функция с именем '{nameOfSpell}' не найдена.");
            return;
        }
    }

}