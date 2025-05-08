using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;
using static AdjustEquipmentParameters;
using static UnityEngine.Rendering.DebugUI;

public class InventoryPlayer : Inventory
{

    private static InventoryPlayer _instance;
    private int _amountAmmunitionSlotsInInventory; // по умолчанию 3 места в инвентаре под аммуницию
    private int _amountSpellSlotsInInventory; // по умолчанию 3 места в инвентаре под заклинания
    private float _widthSpaceForPlaceForEquipment = 1.33f;
    private Player _player;
    private new RectTransform transform;

    [SerializeField] private RectTransform _rectTransformAmmunitionPanel;
    [SerializeField] private RectTransform _rectTransformSpellPanel;
    [SerializeField] private PlaceForEquipment _prefubPlaceForEquipment;

    public List<Spell> listSpellsInInventory = new(); 
    public List<Ammunition> listAmmunitionInInventory = new();

    public static InventoryPlayer Instance
    {
        get
        {
            if (_instance == null)
            {
                var obj = GameObject.Find("InventoryPlayer");
                _instance = obj.GetComponent<InventoryPlayer>();
            }
            return _instance;
        }
    }

    public void Initialize(Player player)
    {
        _player = player;
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;

        transform = GetComponent<RectTransform>();
    }


    void Start() // пока что только тут спавним места под снаряжение. Не сохраням тут ссылки на них пока что. В итоге надо будет вынести это всё в отдельную вызываемую функцию
    {
        _amountAmmunitionSlotsInInventory = _player.countAvailableAmmunitionPlaces;
        _amountSpellSlotsInInventory = _player.countAvailableSpellPlaces;

        if (_amountAmmunitionSlotsInInventory > 3) // масштабируем панельку с аммуницией, если мест для аммуниции больше 3 (по умолчанию она у нас вмещать 3 штуки должна)
        {
            _rectTransformAmmunitionPanel.sizeDelta = new Vector2(_rectTransformAmmunitionPanel.sizeDelta.x + (_widthSpaceForPlaceForEquipment * (_amountAmmunitionSlotsInInventory - 3)), 
                                                                  _rectTransformAmmunitionPanel.sizeDelta.y);
        }
        if (_amountSpellSlotsInInventory > 3) // масштабируем панельку с заклинаниями, если мест для заклинаний больше 3 (по умолчанию она у нас вмещать 3 штуки должна)
        {
            _rectTransformSpellPanel.sizeDelta = new Vector2(_rectTransformSpellPanel.sizeDelta.x + (_widthSpaceForPlaceForEquipment * (_amountAmmunitionSlotsInInventory - 3)),
                                                             _rectTransformAmmunitionPanel.sizeDelta.y);
        }

        for (int i = 0; i < _amountAmmunitionSlotsInInventory; i++)
        {
            PlaceForEquipment scriptPlaceForEquipment = Instantiate(_prefubPlaceForEquipment, _rectTransformAmmunitionPanel);
            scriptPlaceForEquipment.inventory = this;
        }
        for (int i = 0; i < _amountSpellSlotsInInventory; i++)
        {
            PlaceForEquipment scriptPlaceForEquipment = Instantiate(_prefubPlaceForEquipment, _rectTransformSpellPanel);
            scriptPlaceForEquipment.inventory = this;
        }

    }

    
    public void SetEquipmentToInventory(Equipment equipment)
    {
        if (equipment.isEquipmentASpell)
        {
            Spell spell = (Spell)equipment;
            if (!listSpellsInInventory.Contains(spell))
            {
                listSpellsInInventory.Add(spell);
            }
        }
        else
        {
            Ammunition ammunition = (Ammunition)equipment;
            if (!listAmmunitionInInventory.Contains(ammunition))
            {
                listAmmunitionInInventory.Add(ammunition);
                ammunition.player.ChangeUnitParametersByPercentage(ammunition.increasingUnitParametersByAmmunitionPercentage, true);
                ammunition.player.ChangeUnitParametersAndPropertiesByAbsolute(ammunition.increasingUnitParametersByAmmunitionAbsolute, true);
            }
        }

        equipment.EnteredIntoInventory(_player);

        //string nameEnteredInventoryFunction = equipment.equipmentName + C.Prefixes.EnteredInventory;
        //CallActionFunctionByName(equipment, 0, _player, nameEnteredInventoryFunction);
    }


    public void RemoveEquipmentFromInventory(Equipment equipment)
    {
        if (equipment.isEquipmentASpell)
        {
            Spell spell = (Spell)equipment;
            if (listSpellsInInventory.Contains(spell))
            {
                listSpellsInInventory.Remove(spell);
            }
        }
        else
        {
            Ammunition ammunition = (Ammunition)equipment;
            if (listAmmunitionInInventory.Contains(ammunition))
            {
                listAmmunitionInInventory.Remove(ammunition);
                ammunition.player.ChangeUnitParametersByPercentage(ammunition.increasingUnitParametersByAmmunitionPercentage, false);
                ammunition.player.ChangeUnitParametersAndPropertiesByAbsolute(ammunition.increasingUnitParametersByAmmunitionAbsolute, false);
            }
        }
        // Простое изменение параметров героя через параметры снаряжения отменяется при извлечении снаряжения из инвентаря в PlaceForEquipment (желательно оное также перенести сюда)

        equipment.ExitedFromInventory(_player);

        //string nameExitedInventoryFunction = equipment.equipmentName + C.Prefixes.ExitedInventory;
        //CallActionFunctionByName(equipment, 0, _player, nameExitedInventoryFunction); // отменяем пассивные специфические бонусы (которые не просто параметры юнита увеличивают,
                                                                                      // а вызывались с помощью отдельной функции (Сакура)), если таковые будут найдены.

        //string nameDeactivationFunction = equipment.equipmentName + "Deactivate";
        //CallActionFunctionByName(equipment, 0, _player, nameDeactivationFunction); // отменяем активированные абилки (что-то типа переключаемой способности, то, на что мы нажали и это
                                                                                   // действует длительно. На данный момент механика работает так, что при извлечении из инвентаря
                                                                                   // снаряжения его активные бонусы отменяются (Трагикомедия)), если таковые будут найдены

    }


    // Update is called once per frame   
    void Update()
    {
        
    }
}
