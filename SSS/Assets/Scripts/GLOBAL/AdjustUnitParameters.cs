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
                { C.DK.healthMax, 50 },
                { C.DK.staminaMax, 2 },
                { C.DK.DamageReductionPercentage, 0 }, // Процент блокировки урона! Любого! 0 - дефолт, наносится полный урон
                { C.DK.speed, 2 },
                { C.DK.jumpForce, 12 },
                { C.DK.moneyFromKill, 0 },
                { C.DK.experienceToNextLevel, 200 },
                { C.DK.experienceFromKill, 0 },
                { C.DK.stuneChanceByStandardAttackPercentage, 0 },
                { C.DK.timeStuneByStanartAttack, 2 },
                { C.DK.evasionPercentage, 0 },
                { C.DK.comboOneHitKillMultiplayer, 10f },
                { C.DK.timeRecoverStaminaPoint, 0.5f },
                //{ C.DK.timeZeroizeKillComboTicks, 1f },
                { C.DK.nameSoundGettingDamage, C.MusicSounds.PlayerGotDamage }, // и какого хрена я это константой не делаю... шиза. 20.09 - Делаю!
                { C.DK.nameSoundDeath, C.MusicSounds.PlayerDeath },
                { C.DK.nameSoundAttakPeaked, C.MusicSounds.PlayerAttackPeak },
                { C.DK.nameSoundWalk, C.MusicSounds.PlayerWalk },
                { C.DK.increasingGettingExperienceByKillComboTickPercentage, 20f },
                { C.DK.increasingGettingMoneyByKillComboTickPercentage, 20f },
                { C.DK.increasingParametersByLevelUpPercentage, new Dictionary<string, float>()
                    {
                        { C.DK.healthMax, 10f },
                        { C.DK.damage, 10f }
                    }
                },
                { C.DK.damage, 10 },
                { C.DK.CountAccessToUpInSchool, 12210 },
                { C.DK.CurrentMoney, 15000 }
            }
        },
        {
            C.DK.MeleeEnemy, new Dictionary<string, object>()
            {
                { C.DK.healthMax, 25 },
                { C.DK.DamageReductionPercentage, 0 },
                { C.DK.speed, 8 },
                { C.DK.stuneChanceByStandardAttackPercentage, 10 }, 
                { C.DK.evasionPercentage, 0 }, 
                { C.DK.timeStuneByStanartAttack, 2 },
                { C.DK.jumpForce, 75 }, // 14 при массе 1
                { C.DK.moneyFromKill, 50 },
                { C.DK.experienceFromKill, 20 },
                { C.DK.scoreFromKill, 50 },
                { C.DK.som, 14 },
                { C.DK.damage, 5 },
                //{ C.DK.nameSoundAttakPeaked, C.MusicSounds.DogMakeDamage },
                { C.DK.nameSoundDeath, C.MusicSounds.DogDeath },
                { C.DK.nameSoundGettingDamage, C.MusicSounds.DogGotDamage }, // проблема в том, что пока что проигрывается вместе со звуком удара
                { C.DK.nameSoundWalk, C.MusicSounds.DogWalk },
                { C.DK.callDownMeleeAttack, 0.5f }

            }
        },
        {
            C.DK.Door, new Dictionary<string, object>()
            {
                { C.DK.healthMax, 500 }
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
