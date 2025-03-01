using System.Collections.Generic;
using UnityEngine;

public static class AdjustUnitParameters
{
    public static readonly Dictionary<string, Dictionary<string, object>> unitParameters =
        new Dictionary<string, Dictionary<string, object>>()
        {
        {
            C.DK.Player, new Dictionary<string, object>()
            {
                { C.DK.healthMax, 10000 },
                { C.DK.damageReduction, 100 },
                { C.DK.speed, 2 },
                { C.DK.jumpForce, 12 },
                { C.DK.moneyFromKill, 0 },
                { C.DK.experienceToNextLevel, 200 },
                { C.DK.experienceFromKill, 0 },
                { C.DK.increasingGettingExperienceByKillComboTickPercentage, 20f },
                { C.DK.increasingGettingMoneyByKillComboTickPercentage, 20f },
                { C.DK.increasingParametersByLevelUpPercentage, new Dictionary<string, float>()
                    {
                        { C.DK.healthMax, 10f },
                        { C.DK.damage, 10f }
                    }
                },
                { C.DK.damage, 10 },
                { C.DK.CountAccessToUpInSchool, 0 },
                { C.DK.CurrentMoney, 150 }
            }
        },
        {
            C.DK.MeleeEnemy, new Dictionary<string, object>()
            {
                { C.DK.healthMax, 150 },
                { C.DK.damageReduction, 150 },
                { C.DK.speed, 4 },
                { C.DK.jumpForce, 14 },
                { C.DK.moneyFromKill, 50 },
                { C.DK.experienceFromKill, 20 },
                { C.DK.som, 14 },
                { C.DK.damage, 5 }
            }
        },
        {
            C.DK.Door, new Dictionary<string, object>()
            {
                { C.DK.healthMax, 15000 }
            }
        }
        };

    // получаем параметр из словаря по названию юнита и параметра
    public static object GetParameter(string unitName, string parameterName)
    {
        if (unitParameters.ContainsKey(unitName) && unitParameters[unitName].ContainsKey(parameterName))
        {
            return unitParameters[unitName][parameterName];
        }
        return null;
    }

    // получаем весь словарь для отдельного юнита по его имени
    public static object GetSetupOfUnit(string unitName)
    {
        if (unitParameters.ContainsKey(unitName))
        {
            return unitParameters[unitName];
        }
        return null;
    }

}
