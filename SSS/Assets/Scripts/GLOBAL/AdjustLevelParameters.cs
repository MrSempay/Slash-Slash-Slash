using System.Collections.Generic;
using UnityEngine;

public static class AdjustLevelParameters
{

    public static readonly Dictionary<string, Dictionary<string, object>> levelParameters =
        new Dictionary<string, Dictionary<string, object>>()
        {
            {
                C.NameScene.Level1, new Dictionary<string, object>()
                {
                    { C.DK.percentageIncreaseEnemiesParametersBySpawnIteration, new Dictionary<string, float>() // увеличение параметров на  при спавне врагов %
                            {
                                { "healthMax", 5f },
                                { "DamageReductionPercentage", 0f },
                                { "speed", 0f },
                                { "jumpForce", 0f },
                                //{ C.DK.speed, 20 },
                                { "damage", 5f } } },
                    { C.DK.absoluteIncreaseEnemiesParametersBySpawnIteration, new Dictionary<string, float>() // увеличение параметров на  при спавне врагов, абсолютное значение параметра
                            {
                                { "DamageReductionPercentage", 5f } } },
                }
            },
            {
                C.NameScene.Level2, new Dictionary<string, object>()
                {
                    { C.DK.percentageIncreaseEnemiesParametersBySpawnIteration, new Dictionary<string, float>() // увеличение параметров на  при спавне врагов %
                            {
                                { "healthMax", 5f },
                                { "DamageReductionPercentage", 0f },
                                { "speed", 0f },
                                { "jumpForce", 0f },
                                { "damage", 5f } } },
                    { C.DK.absoluteIncreaseEnemiesParametersBySpawnIteration, new Dictionary<string, float>() // увеличение параметров на  при спавне врагов, абсолютное значение параметра
                            {
                                { "DamageReductionPercentage", 5f } } },
                }
            },
            {
                C.NameScene.Level5, new Dictionary<string, object>()
                {
                    { C.DK.percentageIncreaseEnemiesParametersBySpawnIteration, new Dictionary<string, float>() // увеличение параметров на  при спавне врагов %
                            {
                                { "healthMax", 5f },
                                { "DamageReductionPercentage", 0f },
                                { "speed", 0f },
                                { "jumpForce", 0f },
                                { "damage", 5f } } },
                    { C.DK.absoluteIncreaseEnemiesParametersBySpawnIteration, new Dictionary<string, float>() // увеличение параметров на  при спавне врагов, абсолютное значение параметра
                            {
                                { "DamageReductionPercentage", 5f } } },
                }
            },

        }; 


}
