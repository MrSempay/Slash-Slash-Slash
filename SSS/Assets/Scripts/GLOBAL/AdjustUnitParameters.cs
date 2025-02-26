using System.Collections.Generic;
using UnityEngine;

public static class AdjustUnitParameters
{
    public static readonly Dictionary<string, Dictionary<string, object>> unitParameters = new Dictionary<string, Dictionary<string, object>>()
    {
        // »нициализируем словарь при объ€влении
        { "Player", new Dictionary<string, object>() { 
            { "healthMax", 10000 }, 
            { "damageReduction", 100 }, 
            { "speed", 2 }, 
            { "jumpForce", 12 }, 
            { "moneyFromKill", 0 }, 
            { "experienceToNextLevel", 200 }, 
            { "experienceFromKill", 0 }, 
            { "increasingGettingExperienceByKillComboTickPercentage", 20f }, 
            { "increasingGettingMoneyByKillComboTickPercentage", 20f }, 
            { "increasingParametersByLevelUpPercentage",  new Dictionary<string, float>() {
                { "healthMax", 10f },
                { "damage", 10f } } }, 
            { "damage", 10 },


            { "CountAccessToUpInSchool", 0 },
            { "CurrentMoney", 150 } } }, // дл€ свойств потом, возможно, лучше сделать отдельный словарик, а может и нет...
        { "MeleeEnemy", new Dictionary<string, object>() {
            { "healthMax", 150 },
            { "damageReduction", 150 },
            { "speed", 4 },
            { "jumpForce", 14 },
            { "moneyFromKill", 50 },
            { "experienceFromKill", 20 },
            { "som", 14 },
            { "damage", 5 } } },
        { "Door", new Dictionary<string, object>() {
            { "healthMax", 15000 } } }
    };

    // получаем параметр из словар€ по названию юнита и параметра
    public static object GetParameter(string unitName, string parameterName)
    {
        if (unitParameters.ContainsKey(unitName) && unitParameters[unitName].ContainsKey(parameterName))
        {
            return unitParameters[unitName][parameterName];
        }
        return null;
    }

    // получаем весь словарь дл€ отдельного юнита по его имени
    public static object GetSetupOfUnit(string unitName)
    {
        if (unitParameters.ContainsKey(unitName))
        {
            return unitParameters[unitName];
        }
        return null;
    }

}
