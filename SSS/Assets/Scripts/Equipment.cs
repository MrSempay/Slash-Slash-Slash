using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class Equipment : MonoBehaviour
{
    private Building _buildingWhereEquipmentIs; // здание, в котором находится снаряжение

    [NonSerialized] public Fsm _fsm; // сделали публичным только для того, чтоб проверять текущее состояние для блядства Input. Ведь Input.GetMouseButtonDown(0) у нас срабатывает для всех
                                     // ебучих объектов при нажатии, а не только на том объекте, на котором мы нажали. Посему придётся проверять состояние для ситуации, когда нам не нужно
                                     // делать проверку на то, по этому ли объекту кликнули, ибо подразумевается что в состоянии FsmStateEquipmentSelected одновременно может быть только один объект
    [NonSerialized] public SpriteRenderer selfSprite; // свой спрайт
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
        selfSprite = GetComponent<SpriteRenderer>();

        _fsm = new Fsm();

        _fsm.AddState(new FsmStateEquipmentSelected(_fsm, gameObject));
        _fsm.AddState(new FsmStateEquipmentInsideShop(_fsm, gameObject));
        _fsm.AddState(new FsmStateEquipmentAtPlayer(_fsm, gameObject));
        
    }
    protected virtual void Start()
    {
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
            transform.parent.gameObject.GetComponent<PlaceForEquipment>().Equipment = null; // у скрипта экземпляра старого места поле Equipment сбрасываем в null (ибо с него убираем)
            PlaceForEquipment rectTransformTargetPlaceScript = rectTransformPlace.gameObject.GetComponent<PlaceForEquipment>(); // получаем скрипт целевого места
            rectTransformTargetPlaceScript.Equipment = null; // обнуляем в любом случае тамошнее снаряжение. Если его нет, то и ладно, а если есть, то оно переместится на место вот 
                                                             // этого текущего. Далее для целевого места назначим снаряжение наше новое (вот это). Сделано для того, чтоб модификаторы
                                                             // снаряжения в ИНВЕНТАРЕ сбросились и назначились корректно
            rectTransformTargetPlaceScript.Equipment = this; // у скрипта экземпляра нового места поле Equipment назначаем на текущий экземпляр снаряжения
            // 4. Устанавливаем родительский элемент
            transform.SetParent(rectTransformPlace, false); // false - чтобы не сохранять мировые координаты (позицию, масштаб, поворот)

            // 5. Центрируем RectTransform
            transform.anchorMin = new Vector2(0.5f, 0.5f);
            transform.anchorMax = new Vector2(0.5f, 0.5f);
            transform.anchoredPosition = Vector2.zero; // Устанавливаем смещение относительно якорей в (0, 0)
            transform.localPosition = new Vector3(0, -0.5f, -1);
            //Debug.Log(this);
            if (BuildingWhereEquipmentIs) BuildingWhereEquipmentIs.equipmentInBuilding.Remove(gameObject); // собственно удаляем из списка снаряжения в здании это снаряжение только
                                                                                                           // если оно находится в здании

            return true;
        }
        return false;
    }



}
