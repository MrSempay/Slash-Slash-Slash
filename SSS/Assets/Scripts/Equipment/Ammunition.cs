using System;
using System.Collections.Generic;
using UnityEngine;
using static AdjustEquipmentParameters; // ОФИГЕТЬ, ОКАЗЫВАЕТ ВОТ ТАК МОЖНО 

public class Ammunition : Equipment
{
    [NonSerialized] public EquipmentChance categoryAndRarityTypesOfEquipment;// структура для того чтоб знать к каким разрезам относится предмет, ибо у нас их много. Присваивается значение при старте
    [NonSerialized] public Dictionary<string, float> increasingUnitParametersByAmmunitionPercentage = new Dictionary<string, float>();
    [NonSerialized] public Dictionary<string, float> increasingUnitParametersByAmmunitionAbsolute = new Dictionary<string, float>();

    public override void Awake()
    {
        base.Awake();
    }
    protected override void Start()
    {
        // увы, присваиваем параметры не в Awake, а тут, ибо в Awake мы ещё не знаем значения полей categoryAndRarityTypesOfEquipment и, соответственно, что это именно за снаряжение
        StaticClassForAdditionalFunctions.AssignParametersAndProperties(ammunitionParameters[categoryAndRarityTypesOfEquipment.equipmentCategory][categoryAndRarityTypesOfEquipment.equipmentRarityType], this, equipmentName);
        base.Start();
    }
}
