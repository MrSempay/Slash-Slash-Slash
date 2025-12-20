using System.Collections.Generic;
using UnityEngine;

public static class AdjustUnitParameters
{
    public enum ENEMIES { DogTier1, DogTier2, DogTier3 }
    public static Dictionary<ENEMIES, Enemy> enemiesPrefubs;

    public static void Initialize()
    {
        enemiesPrefubs = new Dictionary<ENEMIES, Enemy>
        {
            { ENEMIES.DogTier1, GameManager.Instance.prefubDogTier1 },
            { ENEMIES.DogTier2, GameManager.Instance.prefubDogTier2 },
            { ENEMIES.DogTier3, GameManager.Instance.prefubDogTier3 },
        };
    }


    public static readonly Dictionary<string, Dictionary<string, object>> unitParameters =
        new Dictionary<string, Dictionary<string, object>>()
        {
        {
            C.DK.Player, new Dictionary<string, object>()
            {
                { C.DK.healthMax, 60 },
                { C.DK.staminaMax, 2 },
                { C.DK.DamageReductionPercentage, 0 }, // Процент блокировки урона! Любого! 0 - дефолт, наносится полный урон
                { C.DK.speed, 2 },
                { C.DK.jumpForce, 12 },
                { C.DK.moneyFromKill, 0 },
                { C.DK.experienceToNextLevel, 150 },
                { C.DK.experienceFromKill, 0 },
                { C.DK.stuneChanceByStandardAttackPercentage, 0 },
                { C.DK.timeStuneByStanartAttack, 2 },
                { C.DK.evasionPercentage, 0 },
                { C.DK.comboOneHitKillMultiplayer, 10f },
                { C.DK.timeRecoverStaminaPoint, 1.55f },
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
                { C.DK.damage, 12 },
                { C.DK.CountAccessToUpInSchool, 1 },
                { C.DK.CurrentMoney, 100 }
            }
        },
        {
            C.DK.DogTier1, new Dictionary<string, object>()
            {
                { C.DK.healthMax, 12 },
                { C.DK.DamageReductionPercentage, 0 },
                { C.DK.speed, 8 },
                { C.DK.stuneChanceByStandardAttackPercentage, 10 }, 
                { C.DK.evasionPercentage, 0 }, 
                { C.DK.timeStuneByStanartAttack, 2 },
                { C.DK.jumpForce, 75 }, // 14 при массе 1
                { C.DK.moneyFromKill, 3 },
                { C.DK.experienceFromKill, 10 },
                { C.DK.scoreFromKill, 30 },
                { C.DK.som, 14 },
                { C.DK.damage, 7 },
                //{ C.DK.nameSoundAttakPeaked, C.MusicSounds.DogMakeDamage },
                { C.DK.nameSoundDeath, C.MusicSounds.DogDeath },
                { C.DK.nameSoundGettingDamage, C.MusicSounds.DogGotDamage }, // проблема в том, что пока что проигрывается вместе со звуком удара
                { C.DK.nameSoundWalk, C.MusicSounds.DogWalk },
                { C.DK.callDownMeleeAttack, 1f },
                { C.DK.IsThroughAble, true }

            }
        },
        {
            C.DK.DogTier2, new Dictionary<string, object>()
            {
                { C.DK.healthMax, 42 },
                { C.DK.DamageReductionPercentage, 20 },
                { C.DK.speed, 12 },
                { C.DK.stuneChanceByStandardAttackPercentage, 10 }, 
                { C.DK.evasionPercentage, 0 }, 
                { C.DK.timeStuneByStanartAttack, 2 },
                { C.DK.jumpForce, 75 }, // 14 при массе 1
                { C.DK.moneyFromKill, 10 },
                { C.DK.experienceFromKill, 20 },
                { C.DK.scoreFromKill, 60 },
                { C.DK.som, 14 },
                { C.DK.damage, 12 },
                //{ C.DK.nameSoundAttakPeaked, C.MusicSounds.DogMakeDamage },
                { C.DK.nameSoundDeath, C.MusicSounds.DogDeath },
                { C.DK.nameSoundGettingDamage, C.MusicSounds.DogGotDamage }, // проблема в том, что пока что проигрывается вместе со звуком удара
                { C.DK.nameSoundWalk, C.MusicSounds.DogWalk },
                { C.DK.callDownMeleeAttack, 0.6f },
                { C.DK.IsThroughAble, true }

            }
        },
        {
            C.DK.DogTier3, new Dictionary<string, object>()
            {
                { C.DK.healthMax, 82 },
                { C.DK.DamageReductionPercentage, 30 },
                { C.DK.speed, 16 },
                { C.DK.stuneChanceByStandardAttackPercentage, 10 }, 
                { C.DK.evasionPercentage, 0 }, 
                { C.DK.timeStuneByStanartAttack, 2 },
                { C.DK.jumpForce, 75 }, // 14 при массе 1
                { C.DK.moneyFromKill, 20 },
                { C.DK.experienceFromKill, 40 },
                { C.DK.scoreFromKill, 90 },
                { C.DK.som, 14 },
                { C.DK.damage, 20 },
                //{ C.DK.nameSoundAttakPeaked, C.MusicSounds.DogMakeDamage },
                { C.DK.nameSoundDeath, C.MusicSounds.DogDeath },
                { C.DK.nameSoundGettingDamage, C.MusicSounds.DogGotDamage }, // проблема в том, что пока что проигрывается вместе со звуком удара
                { C.DK.nameSoundWalk, C.MusicSounds.DogWalk },
                { C.DK.callDownMeleeAttack, 0.3f },
                { C.DK.IsThroughAble, false }

            }
        },
        {
            C.DK.Door, new Dictionary<string, object>()
            {
                { C.DK.healthMax, 1500 }
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
