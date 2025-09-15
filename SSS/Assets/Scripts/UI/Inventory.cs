using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    protected IInventory infoAboutInventory;
    protected Unit unitSelf;
    protected int amountAmmunitionSlotsInInventory; // по умолчанию 3 места в инвентаре под аммуницию
    protected int amountSpellSlotsInInventory; // по умолчанию 3 места в инвентаре под заклинания
    protected float widthSpaceForPlaceForEquipment = 1.33f;
    protected new RectTransform transform;


    public RectTransform rectTransformAmmunitionPanel;
    public RectTransform rectTransformSpellPanel;
    public List<Spell> listSpellsInInventory = new();
    public List<Ammunition> listAmmunitionInInventory = new();

    public virtual void Initialize(IInventory infoAboutInventory)
    {
        this.infoAboutInventory = infoAboutInventory;
        unitSelf = infoAboutInventory.UnitSelf;
    }

    public virtual void Awake()
    {
        transform = (RectTransform)gameObject.GetComponent<Transform>();
    }

    public virtual void Start() // пока что только тут спавним места под снаряжение. Не сохраням тут ссылки на них пока что. В итоге надо будет вынести это всё в отдельную вызываемую функцию
    {
        amountAmmunitionSlotsInInventory = infoAboutInventory.CountAvailableAmmunitionPlaces;
        amountSpellSlotsInInventory = infoAboutInventory.CountAvailableSpellPlaces;
        //Debug.Log(owner.CountAvailableSpellPlaces);
        //Debug.Log(owner.CountAvailableAmmunitionPlaces);

        if (amountAmmunitionSlotsInInventory > 3) // масштабируем панельку с аммуницией, если мест для аммуниции больше 3 (по умолчанию она у нас вмещать 3 штуки должна)
        {
            rectTransformAmmunitionPanel.sizeDelta = new Vector2(rectTransformAmmunitionPanel.sizeDelta.x + (widthSpaceForPlaceForEquipment * (amountAmmunitionSlotsInInventory - 3)),
                                                                  rectTransformAmmunitionPanel.sizeDelta.y);
        }
        if (amountSpellSlotsInInventory > 3) // масштабируем панельку с заклинаниями, если мест для заклинаний больше 3 (по умолчанию она у нас вмещать 3 штуки должна)
        {
            rectTransformSpellPanel.sizeDelta = new Vector2(rectTransformSpellPanel.sizeDelta.x + (widthSpaceForPlaceForEquipment * (amountAmmunitionSlotsInInventory - 3)),
                                                             rectTransformSpellPanel.sizeDelta.y);
        }

        InstantiatePlacesForEquipment(rectTransformAmmunitionPanel, amountAmmunitionSlotsInInventory);

        InstantiatePlacesForEquipment(rectTransformSpellPanel, amountSpellSlotsInInventory);
    }

    private void InstantiatePlacesForEquipment(RectTransform rectTransformEquipmentPanel, int amountPlaces)
    {
        for (int i = 0; i < amountPlaces; i++)
        {
            PlaceForEquipment scriptPlaceForEquipment = Instantiate(GameManager.Instance.prefubPlaceForEquipment, rectTransformEquipmentPanel);
            scriptPlaceForEquipment.inventory = this;
            if (unitSelf)
            {
                //Debug.Log("sadasdasdas");
                scriptPlaceForEquipment.isBuildingPlace = false;
            }
        }
    }

    public virtual bool SetEquipmentToInventory(Equipment equipment)
    {
        if (equipment.isEquipmentASpell)
        {
            Spell spell = (Spell)equipment;
            if (!listSpellsInInventory.Contains(spell))
            {
                listSpellsInInventory.Add(spell);
            }
            else
            {
                return false;
            }
        }
        else
        {
            Ammunition ammunition = (Ammunition)equipment;
            if (!listAmmunitionInInventory.Contains(ammunition))
            {
                listAmmunitionInInventory.Add(ammunition);
            }
            else
            {
                return false;
            }
        }
        //Debug.Log(infoAboutInventory.IsStaticInventory);
        if (infoAboutInventory.IsStaticInventory)
        {
            equipment.EnteredIntoStaticInventory(infoAboutInventory); // впрочем, вот это явное приведение можно было бы вынести в дочерние классы, ну да ладно
            equipment.ownerUnit = null; // по идее можно убрать, не будет такого, чтоб при входе в статичный инвентарь данное поле не равнялось null
        }
        else
        {
            equipment.ownerUnit = unitSelf;
            equipment.EnteredIntoUnitInventory(infoAboutInventory.UnitSelf);
        }

        return true;

        //string nameEnteredInventoryFunction = equipment.equipmentName + C.Prefixes.EnteredInventory;
        //CallActionFunctionByName(equipment, 0, _player, nameEnteredInventoryFunction);
    }


    public virtual bool RemoveEquipmentFromInventory(Equipment equipment)
    {
        if (equipment.isEquipmentASpell)
        {
            Spell spell = (Spell)equipment;
            if (listSpellsInInventory.Contains(spell))
            {
                listSpellsInInventory.Remove(spell);
            }
            else
            {
                return false;
            }
        }
        else
        {
            Ammunition ammunition = (Ammunition)equipment;
            if (listAmmunitionInInventory.Contains(ammunition))
            {
                listAmmunitionInInventory.Remove(ammunition);
            }
            else
            {
                return false;
            }
        }
        // Простое изменение параметров героя через параметры снаряжения отменяется при извлечении снаряжения из инвентаря в PlaceForEquipment (желательно оное также перенести сюда)

        if (infoAboutInventory.IsStaticInventory)
        {
            equipment.ExitedFromStaticInventory(infoAboutInventory); // впрочем, вот это явное приведение можно было бы вынести в дочерние классы, ну да ладно
            equipment.ownerUnit = null; // по идее можно убрать, не будет такого, чтоб при выходе из статичного инвентаря данное поле не равнялось null
        }
        else
        {
            equipment.ExitedFromUnitInventory(infoAboutInventory.UnitSelf);
            equipment.ownerUnit = null;
        }

        return true;

        //string nameExitedInventoryFunction = equipment.equipmentName + C.Prefixes.ExitedInventory;
        //CallActionFunctionByName(equipment, 0, _player, nameExitedInventoryFunction); // отменяем пассивные специфические бонусы (которые не просто параметры юнита увеличивают,
        // а вызывались с помощью отдельной функции (Сакура)), если таковые будут найдены.

        //string nameDeactivationFunction = equipment.equipmentName + "Deactivate";
        //CallActionFunctionByName(equipment, 0, _player, nameDeactivationFunction); // отменяем активированные абилки (что-то типа переключаемой способности, то, на что мы нажали и это
        // действует длительно. На данный момент механика работает так, что при извлечении из инвентаря
        // снаряжения его активные бонусы отменяются (Трагикомедия)), если таковые будут найдены

    }





}




