using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using static AdjustEquipmentParameters; // ОФИГЕТЬ, ОКАЗЫВАЕТ ВОТ ТАК МОЖНО 

public class Ammunition : Equipment
{


    [NonSerialized] public EquipmentChance categoryAndRarityTypesOfEquipment;// структура для того чтоб знать к каким разрезам относится предмет, ибо у нас их много. Присваивается значение при старте
    [NonSerialized] public Dictionary<string, float> increasingUnitParametersByAmmunitionPercentage = new Dictionary<string, float>();
    [NonSerialized] public Dictionary<string, float> increasingUnitParametersByAmmunitionAbsolute = new Dictionary<string, float>();

    // вообще, логику желательно объединить с increasingUnitParametersByAmmunitionPercentage и увеличивать параметры юнитов не просто ссылаясь абсолютно на данное поле, а
    // вызывать какую-нибудь функцию, для пассивного увеличения параметров. Вообще логику PlaceForEquipment в этом плане надо переделать. Пассивное изменение парамтеров
    // юнита-хозяина (пока что только герой, надо тоже исправить) параметрами снаряжения можно перенести в функции EnteredInventory и ExitInventory, пусть в этих функциях
    // вызывается отдельный метод для пассивного увеличения характеристик героя (что делается сейчас в PlaceForEquipment)

    // выше бред написан. Обязательно нужно разделение для увеличения пассивного параметров юнита при попадании предмета в инвентарь и активного, которое получается при прожатии (активации)
    // предмета

    //[NonSerialized] public Dictionary<string, float> increasingUnitParametersByAmmunitionPercentageByCast = new Dictionary<string, float>(); // перенесли в Equipment



    public override void Awake()
    {
        base.Awake();
    }
    public override void Start()
    {
        // увы, присваиваем параметры не в Awake, а тут, ибо в Awake мы ещё не знаем значения полей categoryAndRarityTypesOfEquipment и, соответственно, что это именно за снаряжение
        StaticClassForAdditionalFunctions.AssignParametersAndProperties(ammunitionParameters[categoryAndRarityTypesOfEquipment.equipmentCategory][categoryAndRarityTypesOfEquipment.equipmentRarityType], this, equipmentName);
        base.Start();
    }

    //--------------------------------------------------------- Прожимаемые абилки ---------------------------------------------------------//

    // ПОЛНОСТЬЮ ВСЁ ПОМЕНЯЛИ! Теперь всё активируемое снаряжение или снаряжение со специфической логикой находится в отдельных классах!!!



    //----------------------------- Активности, которые должны срабатывать/прекращать активничать просто при добавлении или изымании аммуниции из инвентаря -----------------------------//
    //----------------------------- то есть функции вызываются при добавлении в инвентарь предмета или при изъятии. Activate и Deactivate соответственно --------------------------------//





 

}
