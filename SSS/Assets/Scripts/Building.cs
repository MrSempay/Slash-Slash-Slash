using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class Building : MonoBehaviour
{
    private Fsm _fsm;
    private bool _isAroundBuilding = false;
    private string folderImage = "Spells/";  // Относительный путь к папке с изображениями из папки Assets/Resources

    [NonSerialized] public GameObject entirePanel;
    [NonSerialized] public GameObject buttonEnter;
    [NonSerialized] public RectTransform rectTransformTargetEquipmentPanelPlayer; // чтоб отличать панели магазинов/аммуниции/заклинаний у игрока
    [NonSerialized] public List<GameObject> equipmentInBuilding = new List<GameObject>(); // список всего снаряжения в здании

    public GameObject prefubOfEquipment;
    public bool buttonEnterWasPressedToEnter = false;


    public bool IsAroundBuilding
    {
        get { return _isAroundBuilding; }
        set { _isAroundBuilding = value; }
        /*{
            _isAroundBuilding = value;
            if ( _fsm.StateCurrent?.GetType() != typeof(FsmStateBuildingDestroyed))
            {
                buttonEnter.SetActive(value);
            }
            if (_fsm.StateCurrent?.GetType() == typeof(FsmStateBuildingOpened) && value == false)
            {
                _fsm.SetState<FsmStateBuildingNormal>();
            }
        } */
    }


    protected virtual void Awake()
    {
        entirePanel = transform.Find("EntirePanel")?.gameObject; // Используем ?. для безопасного доступа (если не найдено)
        buttonEnter = transform.Find("CanvasButtonEnter")?.gameObject;
        RectTransform rectTransformEquipmentPlaces = transform.Find("EntirePanel/EquipmentStuffPlaces")?.gameObject.GetComponent<RectTransform>();
        foreach (RectTransform placeForEquipment in rectTransformEquipmentPlaces)
        {
            // СОЗДАЁМ ОБЪЕКТ СНАРЯЖЕНИЯ, ПОЛУЧАЕМ ЕГО ИМЯ, RectTransform, СПАВНИМ У ЗАДАННОГО РОДИТЕЛЯ (МЕСТА СНАРЯЖЕНИЯ)
            GameObject newEquipment = Instantiate(prefubOfEquipment, Vector3.zero, Quaternion.identity);
            RectTransform newEquipmentRectTransform = newEquipment.GetComponent<RectTransform>();
            string randomEquipmentName = AdjustEquipmentParameters.GetRandomSpellName();
            newEquipmentRectTransform.SetParent(placeForEquipment, false); // false - чтобы не сохранять мировые координаты (позицию, масштаб, поворот)

            // НАСТРАИВАЕМ КОМПОНЕНТ RectTransform У ЭКЗЕМПЛЯРА СНАРЯЖЕНИЯ
            newEquipmentRectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            newEquipmentRectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            newEquipmentRectTransform.anchoredPosition = Vector2.zero; // Устанавливаем смещение относительно якорей в (0, 0)
            newEquipmentRectTransform.localPosition = new Vector3(0, -0.5f, -1);
            newEquipmentRectTransform.name = randomEquipmentName;

            // НАСТРАИВАЕМ КОМПОНЕНТ SpriteRenderer У ЭКЗЕМПЛЯРА СНАРЯЖЕНИЯ
            SpriteRenderer spriteRenderer = newEquipment.GetComponent<SpriteRenderer>();
            string fullPath = folderImage + randomEquipmentName;
            Sprite spellSprite = Resources.Load<Sprite>(fullPath);
            spriteRenderer.sprite = spellSprite;

            // НАСТРАИВАЕМ КОМПОНЕНТ Equipment (СОБСНА ЕГО СКРИПТ) У ЭКЗЕМПЛЯРА СНАРЯЖЕНИЯ
            Equipment scriptOfEquipment = newEquipment.GetComponent<Equipment>();
            scriptOfEquipment.equipmentName = randomEquipmentName;
            scriptOfEquipment.isEquipmentASpell = true; // пока что для спелов только
            scriptOfEquipment.startLocalPosition = newEquipmentRectTransform.localPosition;
            scriptOfEquipment.BuildingWhereEquipmentIs = this;
            scriptOfEquipment.rectTransformTargetEquipmentPanelPlayer = rectTransformTargetEquipmentPanelPlayer;
            scriptOfEquipment.transformCurrentEquipmentPlace = placeForEquipment;

            // ИЗМЕНЯЕМ ПАРАМЕТРЫ ЗДАНИЯ ПРИ ДОБАВЛЕНИИ В НЕГО НОВОГО СНАРЯЖЕНИЯ
            equipmentInBuilding.Add(newEquipment);
            PlaceForEquipment scriptOfPlace = placeForEquipment.gameObject.GetComponent<PlaceForEquipment>();
            scriptOfPlace.Equipment = scriptOfEquipment;
            scriptOfPlace.isBuildingPlace = true;
        }
        

        _fsm = new Fsm();

        _fsm.AddState(new FsmStateBuildingNormal(_fsm, gameObject));
        _fsm.AddState(new FsmStateBuildingDestroyed(_fsm, gameObject));
        _fsm.AddState(new FsmStateBuildingOpened(_fsm, gameObject));

    }

    void Start()
    {
        _fsm.SetState<FsmStateBuildingNormal>();
    }

    // Update is called once per frame
    void Update()
    {
        _fsm.Update();
    }

    void FixedUpdate()
    {
        _fsm.FixedUpdate();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player")) { IsAroundBuilding = true; }
        if (other.gameObject.CompareTag("Enemy")) { _fsm.SetState<FsmStateBuildingDestroyed>(); } 
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player")) { IsAroundBuilding = false; }
    }

    public void EnterToBuilding()
    {
        buttonEnterWasPressedToEnter = !buttonEnterWasPressedToEnter;
    }
    public bool HasTargetEnoughMoneyForBuy(Player targetForBuy, Equipment equipment)
    {
        return targetForBuy.CurrentMoney >= equipment.cost;

    }

    public void Sell(Player targetForBuy, Equipment equipment)
    {
        equipment.wasSold = true;
        targetForBuy.CurrentMoney -= equipment.cost;
    }

    
    public bool HasAccessToUpLevelInSchool(Player targetForBuy)
    {
        return targetForBuy.CountAccessToUpInSchool > 0;
    }

    public void TeachByUpLevel(Player targetForBuy, Equipment equipment)
    {
        equipment.wasSold = true;
        targetForBuy.CountAccessToUpInSchool--;

    }
}
