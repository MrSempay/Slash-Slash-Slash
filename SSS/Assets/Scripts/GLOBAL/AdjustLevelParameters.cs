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
                                { "healthMax", 20f },
                                { "DamageReductionPercentage", 2f },
                                { "speed", 1f },
                                { "jumpForce", 0f },
                                //{ C.DK.speed, 20 },
                                { "damage", 5f } } },
                    { C.DK.absoluteIncreaseEnemiesParametersBySpawnIteration, new Dictionary<string, float>() // увеличение параметров на  при спавне врагов, абсолютное значение параметра
                            {
                                { "DamageReductionPercentage", 0.007f } } },
                }
            },
            {
                C.NameScene.Level2, new Dictionary<string, object>()
                {
                    { C.DK.percentageIncreaseEnemiesParametersBySpawnIteration, new Dictionary<string, float>() // увеличение параметров на  при спавне врагов %
                            {
                                { "healthMax", 10f },
                                { "DamageReductionPercentage", 2f },
                                { "speed", 2f },
                                { "jumpForce", 0f },
                                { "damage", 3f } } },
                    { C.DK.absoluteIncreaseEnemiesParametersBySpawnIteration, new Dictionary<string, float>() // увеличение параметров на  при спавне врагов, абсолютное значение параметра
                            {
                                { "DamageReductionPercentage",  0.01f } } },
                }
            },
            {
                C.NameScene.Level3, new Dictionary<string, object>()
                {
                    { C.DK.percentageIncreaseEnemiesParametersBySpawnIteration, new Dictionary<string, float>() // увеличение параметров на  при спавне врагов %
                            {
                                { "healthMax", 10f },
                                { "DamageReductionPercentage", 2f },
                                { "speed", 2f },
                                { "jumpForce", 0f },
                                { "damage", 3f } } },
                    { C.DK.absoluteIncreaseEnemiesParametersBySpawnIteration, new Dictionary<string, float>() // увеличение параметров на  при спавне врагов, абсолютное значение параметра
                            {
                                { "DamageReductionPercentage",  0.01f } } },
                }
            },
             {
                C.NameScene.Level4, new Dictionary<string, object>()
                {
                    { C.DK.percentageIncreaseEnemiesParametersBySpawnIteration, new Dictionary<string, float>() // увеличение параметров на  при спавне врагов %
                            {
                                { "healthMax", 15f },
                                { "DamageReductionPercentage", 2f },
                                { "speed", 2f },
                                { "jumpForce", 0f },
                                { "damage", 5f } } },
                    { C.DK.absoluteIncreaseEnemiesParametersBySpawnIteration, new Dictionary<string, float>() // увеличение параметров на  при спавне врагов, абсолютное значение параметра
                            {
                                { "DamageReductionPercentage",  0.01f } } },
                }
            },
            {
                C.NameScene.Level5, new Dictionary<string, object>()
                {
                    { C.DK.percentageIncreaseEnemiesParametersBySpawnIteration, new Dictionary<string, float>() // увеличение параметров на  при спавне врагов %
                            {
                                { "healthMax", 20f },
                                { "DamageReductionPercentage", 2f },
                                { "speed", 2f },
                                { "jumpForce", 0f },
                                { "damage", 5f } } },
                    { C.DK.absoluteIncreaseEnemiesParametersBySpawnIteration, new Dictionary<string, float>() // увеличение параметров на  при спавне врагов, абсолютное значение параметра
                            {
                                { "DamageReductionPercentage",  0.01f } } },
                }
            },

        }; 


}
