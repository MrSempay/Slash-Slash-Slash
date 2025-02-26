using System;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class Equipment : MonoBehaviour
{
    private Building _buildingWhereEquipmentIs; // здание, в котором находится снаряжение

    [NonSerialized] public Fsm _fsm; // сделали публичным только для того, чтоб проверять текущее состояние для блядства Input. Ведь Input.GetMouseButtonDown(0) у нас срабатывает для всех
                                     // ебучих объектов при нажатии, а не только на том объекте, на котором мы нажали. Посему придётся проверять состояние для ситуации, когда нам не нужно
                                     // делать проверку на то, по этому ли объекту кликнули, ибо подразумевается что в состоянии FsmStateEquipmentSelected одновременно может быть только один объект
    [NonSerialized] public Vector3 startLocalPosition;
    [NonSerialized] public RectTransform rectTransformTargetEquipmentPanelPlayer; // чтоб отличать панели магазинов/аммуниции/заклинаний у игрока
    [NonSerialized] public Player player;
    [NonSerialized] public bool wasSold = false;
    [NonSerialized] public int cost;
    [NonSerialized] public bool isEquipmentASpell;
    [NonSerialized] public string equipmentName;
    [NonSerialized] public RectTransform transformCurrentEquipmentPlace; // компонент RectTransform текущего места нашего снаряжения. Нужно, чтоб задать это же место другому снаряжению при обмене местами

    public BoxCollider2D selfCollider;
    public event Action<string, int> ParametersOfEquipmentWasAssigned;       // Событие для изменения комбо за убийства 

    new public RectTransform transform;

    public Building BuildingWhereEquipmentIs
    {
        get { return _buildingWhereEquipmentIs; }
        set
        {
            //if (value == null && _buildingWhereEquipmentIs != null) Sell(); // детектим факт перехода снаряжения из здания в... не здание. Значит продано. Хотя интересно, если оно просто 
                                                                                  // будет в итоге выпадать из зданий без факта продажи
            _buildingWhereEquipmentIs = value;

        }
    }

    protected virtual void Awake()
    {

        transform = GetComponent<RectTransform>();
        player = GameObject.Find("Player").GetComponent<Player>();

        _fsm = new Fsm();

        _fsm.AddState(new FsmStateEquipmentSelected(_fsm, gameObject));
        _fsm.AddState(new FsmStateEquipmentInsideShop(_fsm, gameObject));
        _fsm.AddState(new FsmStateEquipmentAtPlayer(_fsm, gameObject));
        
    }
    protected virtual void Start()
    {
        if (isEquipmentASpell) StaticClassForAdditionalFunctions.AssignParameters(AdjustEquipmentParameters.spellParameters, this, equipmentName);
        else StaticClassForAdditionalFunctions.AssignParameters(AdjustEquipmentParameters.ammunitionParameters, this, equipmentName);
        if (BuildingWhereEquipmentIs) ParametersOfEquipmentWasAssigned?.Invoke(equipmentName, cost); // если снаряжение заспавнилось в здании, то эмулируем вызов сигнала
        _fsm.SetState<FsmStateEquipmentInsideShop>();
    }


    private void Update()
    {
        _fsm.Update();
    }

    private void FixedUpdate()
    {
        _fsm.FixedUpdate();
    }

    public bool SetEquipmentToPlaceIfNotNull(RectTransform rectTransformPlace)
    {
        if (rectTransformPlace)
        {
            // 4. Устанавливаем родительский элемент
            transform.SetParent(rectTransformPlace, false); // false - чтобы не сохранять мировые координаты (позицию, масштаб, поворот)

            // 5. Центрируем RectTransform
            transform.anchorMin = new Vector2(0.5f, 0.5f);
            transform.anchorMax = new Vector2(0.5f, 0.5f);
            transform.anchoredPosition = Vector2.zero; // Устанавливаем смещение относительно якорей в (0, 0)
            transform.localPosition = new Vector3(0, -0.5f, -1);
            transform.parent.gameObject.GetComponent<PlaceForEquipment>().Equipment = null; // у скрипта экземпляра старого места поле Equipment сбрасываем в null (ибо с него убираем)
            rectTransformPlace.gameObject.GetComponent<PlaceForEquipment>().Equipment = this; // у скрипта экземпляра нового места поле Equipment назначаем на текущий экземпляр снаряжения
            if (BuildingWhereEquipmentIs) BuildingWhereEquipmentIs.equipmentInBuilding.Remove(gameObject); // собственно удаляем из списка снаряжения в здании это снаряжение только
                                                                                                           // если оно находится в здании

            return true;
        }
        return false;
    }



}
