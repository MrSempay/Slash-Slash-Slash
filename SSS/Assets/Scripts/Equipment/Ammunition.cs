using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using static AdjustEquipmentParameters; // ОФИГЕТЬ, ОКАЗЫВАЕТ ВОТ ТАК МОЖНО 

public class Ammunition : Equipment
{
    [SerializeField] private CustomCombo _prefubCustomCombo;

    [NonSerialized] public EquipmentChance categoryAndRarityTypesOfEquipment;// структура для того чтоб знать к каким разрезам относится предмет, ибо у нас их много. Присваивается значение при старте
    [NonSerialized] public Dictionary<string, float> increasingUnitParametersByAmmunitionPercentage = new Dictionary<string, float>();
    [NonSerialized] public Dictionary<string, float> increasingUnitParametersByAmmunitionAbsolute = new Dictionary<string, float>();

    // вообще, логику желательно объединить с increasingUnitParametersByAmmunitionPercentage и увеличивать параметры юнитов не просто ссылаясь абсолютно на данное поле, а
    // вызывать какую-нибудь функцию, для пассивного увеличения параметров. Вообще логику PlaceForEquipment в этом плане надо переделать. Пассивное изменение парамтеров
    // юнита-хозяина (пока что только герой, надо тоже исправить) параметрами снаряжения можно перенести в функции EnteredInventory и ExitInventory, пусть в этих функциях
    // вызывается отдельный метод для пассивного увеличения характеристик героя (что делается сейчас в PlaceForEquipment)

    [NonSerialized] public Dictionary<string, float> increasingUnitParametersByAmmunitionPercentageByCast = new Dictionary<string, float>();



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

    //--------------------------------------------------------- Прожимаемые абилки ---------------------------------------------------------//

# region Tragicomedy

    public void Tragicomedy(Unit whoCastedSpell)
    {
        TragicomedyActivate(whoCastedSpell);
    }

    public void TragicomedyActivate(Unit whoCastedSpell)
    {
        if (!isActivated)
        {
            isActivated = true;
            whoCastedSpell.ChangeUnitParametersByPercentage(increasingUnitParametersByAmmunitionPercentageByCast, true);
            StartCoroutine(DurationTragicomedyActive(whoCastedSpell));
        }
    }

    IEnumerator DurationTragicomedyActive(Unit whoCastedSpell)
    {
        yield return new WaitForSeconds(durationActiveState);

        TragicomedyDeactivate(whoCastedSpell);
    }

    public void TragicomedyDeactivate(Unit whoCastedSpell)
    {
        if (isActivated)
        {
            StartCallDown();
            isActivated = false;
            whoCastedSpell.ChangeUnitParametersByPercentage(increasingUnitParametersByAmmunitionPercentageByCast, false);
        }
    }



# endregion



    //----------------------------- Активности, которые должны срабатывать/прекращать активничать просто при добавлении или изымании аммуниции из инвентаря -----------------------------//
    //----------------------------- то есть функции вызываются при добавлении в инвентарь предмета или при изъятии. Activate и Deactivate соответственно --------------------------------//

# region ThirstySakura

    public void ThirstySakuraEnteredInventory(Unit whoCastedSpell)
    {
        //Debug.Log("mda");


        // если хотим объединить в один счётчик сразу несколько предметов:

        /*
        CustomCombo scriptExistingKillCountCombo = Player.instance.rectTransformPlaceCustomCombos.GetComponentInChildren<CustomCombo>(); // чекаем, есть ли от предыдущей Сакуры комбо уже

        if (scriptExistingKillCountCombo != null)
        {
            UnityAction<int> upCombo1 = scriptExistingKillCountCombo.UpCombo;

            EventBus.Instance.OnOneEnemyWasKilledByPlayer.AddListener(upCombo1); // если да, параллельно вешаем ещё один детектор на OnOneEnemyWasKilledByPlayer
            
            scriptExistingKillCountCombo.AddMethodListenerToDictionary(this, upCombo1);

            return;
        }*/

        CustomCombo scriptCustomCombo = Instantiate(_prefubCustomCombo, Player.instance.rectTransformPlaceCustomCombos);
        scriptCustomCombo.Initialize("KillCount", scriptCustomCombo.IncreaseDamageHeroeByTick, 0); // передаём базовый текст для комбо (оно же будет именем объекта), ссылку на метод,
                                                                                                   // который будет срабатывать при изменении комбо, а также время сбрасывания комбо (0 тут)

        UnityAction<int> upCombo = scriptCustomCombo.UpCombo; // ссылка на метод, который привязываем к событию убийства врага  
        EventBus.Instance.OnOneEnemyWasKilledByPlayer.AddListener(upCombo);

        scriptCustomCombo.AddMethodListenerToDictionary(this, upCombo); // теоретически можно перенести в Initialize. Хотя нет, иногда нам нужно просто добавить новый объект для инду
                                                                        // цирования изменения комбо при этом не создавая его (например комбо одно, а его изменяют несколько объектов)
    }
    public void ThirstySakuraExitedInventory(Unit whoCastedSpell)
    {
        //Debug.Log("mda");

        foreach (RectTransform killCount in Player.instance.rectTransformPlaceCustomCombos)
        {
            CustomCombo scriptCustomCombo = killCount.GetComponent<CustomCombo>();

            if (scriptCustomCombo.DictionaryListenerMethods.ContainsKey(this))
            {
                scriptCustomCombo.CurrentCombo = 0;

                EventBus.Instance.OnOneEnemyWasKilledByPlayer.RemoveListener(scriptCustomCombo.DictionaryListenerMethods[this]);

                Destroy(scriptCustomCombo.gameObject);

                return;
            }
        }

        //CustomCombo scriptCustomCombo = Player.instance.rectTransformPlaceCustomCombos.Find("KillCount").GetComponent<CustomCombo>();
        //scriptCustomCombo.CurrentCombo = 0;


        // если есть несколько источников пополнения комбо, а само комбо должно быть одно:
         
        //if (scriptCustomCombo.DictionaryListenerMethods.Count == 1)
        //{
        //    Destroy(scriptCustomCombo.gameObject);
        //}
    }
# endregion



 

}
