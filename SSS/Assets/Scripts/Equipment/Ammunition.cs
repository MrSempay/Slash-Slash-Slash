using System;
using System.Collections.Generic;
using UnityEngine;
using static AdjustEquipmentParameters; // ОФИГЕТЬ, ОКАЗЫВАЕТ ВОТ ТАК МОЖНО

public class Ammunition : Equipment
{
    [NonSerialized] public EquipmentChance categoryAndRarityTypesOfEquipment;// структура для того чтоб знать к каким разрезам относится предмет, ибо у нас их много. Присваивается значение при старте
    [NonSerialized] public Dictionary<string, float> increasingUnitParametersByAmmunition = new Dictionary<string, float>();

    protected override void Awake()
    {
        base.Awake();
    }
    protected override void Start()
    {
        StaticClassForAdditionalFunctions.AssignParametersAndProperties(ammunitionParameters[categoryAndRarityTypesOfEquipment.equipmentCategory][categoryAndRarityTypesOfEquipment.equipmentRarityType], this, equipmentName);
        base.Start();
    }
}
