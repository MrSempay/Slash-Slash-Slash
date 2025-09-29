using System.Collections.Generic;
using UnityEngine;
using static ScoreManager;

public class AdjustScoreManagerParameters
{
    public static Dictionary<string, object> scoreManagerParameters = new Dictionary<string, object>()
    {
        { "timeZeroizeKillComboTicks", 7f }, // время для сбрасывания текущего комбо за убийства. Начальное время при загрузке сцены. Секунды
        { "minTimeZeroizeKillComboTicks", 1f }, // минимальное время для сбрасывания комбо за убийство (меньше данного времени не опустится). Секунды
        { "secondsAdditionalForZeroizeKillComboTicksByTimer", -1f }, // количество секунд прибавляемых ко времени сбрасывания комбо при срабатыванию таймера. Секунды
        { "timeForAddSecondsForZeroizeKillComboTicks", 60f }, // время, через которое ко времени сбрасывания комбо за убийства прибавляется secondsAdditionalForZeroizeKillComboTicksByTimer. Секунды
        { "timeZeroizeActionComboTicks", 2f }, // время, через которое сбрасывается детекция комбо за спелы/активности. Секунды
        { "timeBlockActionCombo", 5f }, // время КД для комбо по скилам/активностям. Секунды
        { "multiplayerActionCombo", 20f }, // дополнительное комбо к основному комбо, за единицу комбо спелов/активностей
        { "thresholdAmountInvalidActionsForStopCombo", 3 }, // номер! неправильной активности/спела прожатых подряд, при котором комбо спелов сбросится и уйдёт на КД (то есть на третье действие)
        { "clastersSynergisticActions", new List<List<string>>() // список цепочек синнергирующих активностей (по названиям спелов/активных предметов). Для простой атаки указать: ""
            {
                //                    10+20          10+40       10+60
                new List<string> { "SomeSpell1", "SomeSpell2", "SomeSpell3" },
            } },        
        { "amountUpKomboMasterOfSkills", 1000 }, // количество дополнительного основного комбо за получение ачивки MasterOfSkills
        { "timeCallDownMasterOfSkills", 10f },  // время КД ачивки MasterOfSkills. Секунды
        { "rankProperties", new Dictionary<STYLE_RANK, RankProperties> // настраиваются границы для каждого ранга, множитель для опыта/денег/очков. Особые ФУНКЦИИ (functionRank) НЕ ДОБАВЛЯТЬ
            {
                { STYLE_RANK.D, new RankProperties { min = 0, max = 10, styleMultiplier = 1 } },
                { STYLE_RANK.C, new RankProperties { min = 11, max = 25, styleMultiplier = 2, functionRank = (isApplying) => { if (ScoreManager.Instance != null) ScoreManager.Instance.RankСReached(isApplying); } } },
                { STYLE_RANK.B, new RankProperties { min = 26, max = 50, styleMultiplier = 3, functionRank = (isApplying) => { if (ScoreManager.Instance != null) ScoreManager.Instance.RankBReached(isApplying); } } },
                { STYLE_RANK.A, new RankProperties { min = 51, max = 100, styleMultiplier = 4, functionRank = (isApplying) => { if (ScoreManager.Instance != null) ScoreManager.Instance.RankAReached(isApplying); } } },
                { STYLE_RANK.S, new RankProperties { min = 101, max = int.MaxValue, styleMultiplier = 5, functionRank = (isApplying) => { if (ScoreManager.Instance != null) ScoreManager.Instance.RankSReached(isApplying); } } } // Для упрощения - до бесконечности
            }
        }, 

        // ПАРАМЕТРЫ ОСОБЫХ БОНУСОВ ПРИ ДОСТИЖЕНИИ РАНГА ВЫШЕ B НАСТРАИВАЮТСЯ В САМОМ КЛАССЕ ScoreManager (не трогать!)


    };
}
