using System;
using TMPro;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class PlaceForEquipment : MonoBehaviour
{

    private Equipment _equipment; // снар€жение в данном месте дл€ снар€жени€

    [SerializeField] private TextMeshProUGUI nameOfEquipment;
    [SerializeField] private TextMeshProUGUI costOfEquipment;

    public bool isBuildingPlace = false; // флаг дл€ детекции, находитс€ ли это место в здании
    public Equipment Equipment
    {
        get { return _equipment; }
        set
        {
            if (isBuildingPlace)
            {

                if (!value) // если где-либо значение Equipment устанавливают в null, то мы измен€м UI пол€
                {
                    nameOfEquipment.enabled = false;
                    costOfEquipment.enabled = false;
                    _equipment.ParametersOfEquipmentWasAssigned -= ChangeNameAndCostEquipment; // если место дл€ снар€жени€ не в здании и снар€жение из этого места исчезло, отписываемс€
                                                                                               // от детекции сигнала о смене его параметров (прошлого экземпл€ра снар€жени€)
                    return;
                }
                nameOfEquipment.enabled = true;
                costOfEquipment.enabled = !value.isEquipmentASpell; // не устанавливаем цену дл€ снар€жени€ типа Spell
                value.ParametersOfEquipmentWasAssigned += ChangeNameAndCostEquipment; // если место дл€ снар€жени€ не в здании и снар€жение по€вилось на данном месте, подписываемс€ на
                                                                                      // изменение его параметров (вот этого нового экземпл€ра снар€жени€)
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
            nameOfEquipment.enabled = false;
            costOfEquipment.enabled = false;
            return;
        }
        else if (Equipment.isEquipmentASpell)
        {
            nameOfEquipment.enabled = true;
            costOfEquipment.enabled = false;
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
        nameOfEquipment.text = name;
        costOfEquipment.text = "Cost: " + cost;
    }

    private void OnDestroy()
    {
        if (Equipment) Equipment.ParametersOfEquipmentWasAssigned -= ChangeNameAndCostEquipment;
    }

}
