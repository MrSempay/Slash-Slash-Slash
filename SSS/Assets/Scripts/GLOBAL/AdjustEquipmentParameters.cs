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
            { "equipmentName", "SomeSpell1" },
            { "cost", 0 },
        } },
        { "SomeSpell2", new Dictionary<string, object>() {
            { "equipmentName", "SomeSpell2" },
            { "cost", 0 },
        } },
        { "SomeSpell3", new Dictionary<string, object>() {
            { "equipmentName", "SomeSpell3" },
            { "cost", 0 },
        } }
    };

    public static readonly Dictionary<string, Dictionary<string, object>> ammunitionParameters = new Dictionary<string, Dictionary<string, object>>()
    {
        // Инициализируем словарь при объявлении
        { "Ammunition1", new Dictionary<string, object>() {
            { "equipmentName", "Ammunition1" },
            { "cost", 15 },
        } },
        { "Ammunition2", new Dictionary<string, object>() {
            { "equipmentName", "Ammunition2" },
            { "cost", 45 },
        } },
        { "Ammunition3", new Dictionary<string, object>() {
            { "equipmentName", "Ammunition3" },
            { "cost", 70 },
        } }
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

/*    // получаем весь словарь для отдельного юнита по его имени
    public static object GetSetupOfSpell(string spellName)
    {
        if (spellParameters.ContainsKey(spellName))
        {
            return spellParameters[spellName];
        }
        return null;
    }*/

    // Метод для получения случайного ключа из словаря unitParameters
    public static string GetRandomSpellName()
    {
        List<string> keys = new List<string>(spellParameters.Keys);
        if (keys.Count == 0)
        {
            Debug.LogError("No spell names available in unitParameters!");
            return null; // Или какое-то значение по умолчанию
        }
        int randomIndex = UnityEngine.Random.Range(0, keys.Count);
        return keys[randomIndex];
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