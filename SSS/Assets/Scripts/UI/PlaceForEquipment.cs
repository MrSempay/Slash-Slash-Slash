using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class PlaceForEquipment : MonoBehaviour
{

    [SerializeField] private Equipment _equipment; // снар€жение в данном месте дл€ снар€жени€ // временно [SerializeField]
    private Dictionary<string, float> increasedParametersValuesByEquipmentInThisPlace;

    [SerializeField] private TextEdit nameOfEquipment;
    [SerializeField] private TextEdit costOfEquipment;

    public Inventory inventory; // предполагаетс€, что в дальнейшем инвентарь может быть не один (не только у игрока)
    public Equipment previousEquipment;
    public bool isBuildingPlace = false; // флаг дл€ детекции, находитс€ ли это место в здании
    public Equipment Equipment
    {
        get { return _equipment; }
        set
        {
            //inventory.CheckWasEquipmentAlreadyInInventory(value, this);
            previousEquipment = _equipment; // —охран€ем предыдущее значение
            //Debug.Log(this.ToString() + previousEquipment);
            //Debug.Log(this.ToString() + _equipment);
            if (isBuildingPlace)
            {

                if (!value) // если где-либо значение Equipment устанавливают в null, то мы измен€м UI пол€
                {
                    nameOfEquipment.gameObject.SetActive(false);
                    costOfEquipment.gameObject.SetActive(false);
                    //Debug.Log(_equipment);
                    _equipment.ParametersOfEquipmentWasAssigned -= ChangeNameAndCostEquipment; // если место дл€ снар€жени€ не в здании и снар€жение из этого места исчезло, отписываемс€
                                                                                               // от детекции сигнала о смене его параметров (прошлого экземпл€ра снар€жени€)
                }
                else
                {
                    ChangeNameAndCostEquipment(value.equipmentName, value.cost); // чтоб если в здании вообще по€вилось новое снар€жение, мы обновл€ли его цену и им€
                    // nameOfEquipment.gameObject.SetActive(true); // убрали в рамках расстановки снар€жени€ на полу... 
                    //costOfEquipment.gameObject.SetActive(!value.isEquipmentASpell); // не устанавливаем цену дл€ снар€жени€ типа Spell // убрали в рамках расстановки снар€жени€ на полу...
                    value.ParametersOfEquipmentWasAssigned += ChangeNameAndCostEquipment; // если место дл€ снар€жени€ не в здании и снар€жение по€вилось на данном месте, подписываемс€ на
                                                                                          // изменение его параметров (вот этого нового экземпл€ра снар€жени€)
                }
            }
            else
            {

                if (value != null)
                {
                    if (!value.isEquipmentASpell)
                    {
                        //Ammunition ammunition = (Ammunition)value;
                        //increasedParametersValuesByEquipmentInThisPlace = ammunition.player.ChangeUnitParametersByPercentage(ammunition.increasingUnitParametersByAmmunitionPercentage, true);
                        //ammunition.player.ChangeUnitParametersAndPropertiesByAbsolute(ammunition.increasingUnitParametersByAmmunitionAbsolute, true);
                    }
                }
                else
                {
                    //Debug.Log("Shit Here?");
                    //Debug.Log(previousEquipment);
                    if (previousEquipment)
                    {
                        //Debug.Log("Shit Here?");
                        if (!previousEquipment.isEquipmentASpell)
                        {
                            //Debug.Log("Shit Here?");
                            //Ammunition ammunition = (Ammunition)previousEquipment;
                            /*Dictionary<string, float> decreasingUnitParametersByAmmunition;
                            foreach (var increasedParameter in increasedParametersValuesByEquipmentInThisPlace)
                            {
                                decreasingUnitParametersByAmmunition[increasedParameter.Key] = increasedParameter.Value/ ammunition.player.;
                            }*/
                            //ammunition.player.ChangeUnitParametersByPercentage(ammunition.increasingUnitParametersByAmmunitionPercentage, false);
                            //ammunition.player.ChangeUnitParametersAndPropertiesByAbsolute(ammunition.increasingUnitParametersByAmmunitionAbsolute, false);
                        }
                    }
                }
            }
            _equipment = value;
        }
    }

    void Awake()
    {

    }

    void Start()
    {
        if (!isBuildingPlace)
        {
            //Debug.Log("Mmm?");
            nameOfEquipment.gameObject.SetActive(false);
            costOfEquipment.gameObject.SetActive(false);
            return;
        }
        else if (Equipment.isEquipmentASpell)
        {
            // nameOfEquipment.gameObject.SetActive(true);  // убрали в рамках расстановки снар€жени€ на полу...
            costOfEquipment.gameObject.SetActive(false);
        }

        Equipment.ParametersOfEquipmentWasAssigned += ChangeNameAndCostEquipment;
    }

    // Update is called once per frame
    void Update()
    {
    }

    // хоть на данный момент мы эмулируем сигнал ParametersOfEquipmentWasAssigned только при первичном присвоении параметров дл€ снар€жени€, можно будет доработать скрипт Equipment:
    // создать отдельно пол€ дл€ стоимости и наименовани€ (а можно и просто стоимости) и засунуть туда эмул€цию данного сигнала, чтоб при любом изменении цены у нас вызывалась данна€ функци€
    private void ChangeNameAndCostEquipment(string name, int cost)
    {
        nameOfEquipment.Awake();
        costOfEquipment.Awake();

        nameOfEquipment.Text = name;
        costOfEquipment.SetNotLocalizableText(cost.ToString());
    }

    private void OnDestroy()
    {
        if (Equipment) Equipment.ParametersOfEquipmentWasAssigned -= ChangeNameAndCostEquipment;
    }

}
