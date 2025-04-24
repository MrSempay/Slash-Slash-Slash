using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Equipment;
using static ScoreManager;
using static AdjustEquipmentParameters;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class Equipment : MonoBehaviour
{
    private Building _buildingWhereEquipmentIs; // здание, в котором находится снаряжение
    private bool _wasSold = false; // по умолчанию считаем, что все предметы находятся у продавца (в здании)


    [NonSerialized] public Fsm _fsm; // сделали публичным только для того, чтоб проверять текущее состояние для блядства Input. Ведь Input.GetMouseButtonDown(0) у нас срабатывает для всех
                                     // ебучих объектов при нажатии, а не только на том объекте, на котором мы нажали. Посему придётся проверять состояние для ситуации, когда нам не нужно
                                     // делать проверку на то, по этому ли объекту кликнули, ибо подразумевается что в состоянии FsmStateEquipmentSelected одновременно может быть только один объект
    [NonSerialized] public SpriteRenderer selfSprite; // свой компонент спрайта
    [NonSerialized] public UnityEngine.Sprite sprite; // свой спрайт, назначается в здании при спавне
    [NonSerialized] public Vector3 startLocalPosition;
    [NonSerialized] public RectTransform rectTransformTargetEquipmentPanelPlayer; // чтоб отличать панели магазинов/аммуниции/заклинаний у игрока
    [NonSerialized] public Player player; // НАДО БУДЕТ КАК-нибудь поменять на Unit onwer
    [NonSerialized] public int cost;
    [NonSerialized] public bool isEquipmentASpell;
    [NonSerialized] public string equipmentName;
    [NonSerialized] public Animator animator;
    [NonSerialized] public RectTransform transformCurrentEquipmentPlace; // компонент RectTransform текущего места нашего снаряжения. Нужно, чтоб задать это же место другому снаряжению при обмене местами
    [NonSerialized] public bool isReady = true;
    [NonSerialized] new public RectTransform transform;

    public BoxCollider2D selfCollider;
    public int amountUpCombo;
    public bool isActivated; // флаг активированных снаряжений. Типа переключаемой способности
    public float timeCallDown;
    public float durationActiveState;
    public event Action<string, int> ParametersOfEquipmentWasAssigned;   
    public event Action<Equipment> onEquipmentWasSold;         // экземляр(?) функции/сигнала(?)
    public event Action<List<Equipment>> onUpdateAssortment;

#region Freshness Mechanic

    private FRESHNESS _currentFreshness;
    private int _currentFreshnessCount;

    public enum FRESHNESS { Fresh, Worn, Dull }
    public float multiplierFreshness = 1;

    [Serializable]
    public struct FreshnessProperties
    {
        public int min;
        public int max;
        public float multiplierFreshness;
    }

    public static Dictionary<FRESHNESS, FreshnessProperties> freshnessProperties = new Dictionary<FRESHNESS, FreshnessProperties>
    {
        { FRESHNESS.Fresh, new FreshnessProperties { min = 0, max = 1, multiplierFreshness = 1f } },
        { FRESHNESS.Worn, new FreshnessProperties { min = 2, max = 3, multiplierFreshness = 0.5f } },
        { FRESHNESS.Dull, new FreshnessProperties { min = 3, max = int.MaxValue, multiplierFreshness = 0.3f } },
    };

    public FRESHNESS CurrentFreshness
    {
        get { return _currentFreshness; }
        set
        {
            _currentFreshness = value;

            multiplierFreshness = freshnessProperties[value].multiplierFreshness;
        }
    }
    public int CurrentFreshnessCount
    {
        get { return _currentFreshnessCount; }
        set
        {
            _currentFreshnessCount = value;
            SetFresh(value);
        }
    }

    private void SetFresh(int value)
    {
        foreach (var properties in freshnessProperties)
        {
            if (value >= properties.Value.min && value <= properties.Value.max)
            {
                if (CurrentFreshness != properties.Key)
                {
                    CurrentFreshness = properties.Key;
                }
                return; // Важно: выходим из цикла, как только нашли подходящий диапазон
            }
        }

        Debug.LogError("Значение вне допустимого диапазона!"); // Если не попали ни в один диапазон
    }
    #endregion


    public bool WasSold // МЕНЯЕМ ЗНАЧЕНИЕ ТОЛЬКО В СОСТОЯНИЯХ InsideShop и AtPlayer!!!!!!!!!!!!!!!!!!!!
    {
        get { return _wasSold; }
        set
        {
            _wasSold = value;
            if (value)
            {
                onEquipmentWasSold?.Invoke(this); // подписываемся в скрипте ScenarioScript чтоб знать, когда игрок купил предмет/заклинание
            }
        }
    }

    // свойство нужно для концептуального определения: где-то там сейчас находится снаряжение (возвращает некий Building, хотя лучше в будущем расширить интерфейсом, ведь снаряжение
    // возможно будет уметь лежать на земле, находиться у торговцев, падать в виде лута с врагов и т.п), или в инвентаре у героя (тогда будет null, его и детектим в Inventory)
    public Building BuildingWhereEquipmentIs // пока что работает мега-ущербно, у нас снаряжение либо в здании, либо null, что означает в инвентаре у игрока. Нужно интерфейсы сделать...
    {
        get { return _buildingWhereEquipmentIs; }
        set
        {
            //if (value == null && _buildingWhereEquipmentIs != null) Sell(); // детектим факт перехода снаряжения из здания в... не здание. Значит продано. Хотя интересно, если оно просто 
            // будет в итоге выпадать из зданий без факта продажи

            if (_buildingWhereEquipmentIs != value)
            {
                _buildingWhereEquipmentIs = value;
                

            }

        }
    }

    public virtual void Awake()
    {

        transform = GetComponent<RectTransform>();
        player = GameObject.Find("Player").GetComponent<Player>();
        selfSprite = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();    

        _fsm = new Fsm();

        _fsm.AddState(new FsmStateEquipmentSelected(_fsm, gameObject));
        _fsm.AddState(new FsmStateEquipmentInsideShop(_fsm, gameObject));
        _fsm.AddState(new FsmStateEquipmentAtPlayer(_fsm, gameObject));


        //animator.Play(equipmentName);
    }
    protected virtual void Start()
    {
        // Короче, план таков: если в списке анимаций есть анимация с именем текущего снаряжения, мы воспроизводим её. Если таковой анимации не было найдено, то мы
        // ищем анимацию для активного состояния данного снаряжения (ибо снаряжение может иметь 2 вида анимации: активное и деактивированное, когда, например, в КД),
        // если нашли - воспроизводим её. Если нет и таковой, то просто устанавливаем спрайт для данного снаряжения. Спрайт по умолчанию назначается в скрипте здания,
        // которое это снаряжение спавнит, находится в поле sprite
        // Предполагается, что не должно быть для указанного снаряжения одновременно и просто анимации по его имени, и анимации с постфиксом "Active", в таком случае
        // будет отдаваться приоритет проигрывания только анимации по имени, которая без постфикса

        if (StaticClassForAdditionalFunctions.AnimationExists(equipmentName, animator))
        {
            animator.Play(equipmentName); // Воспроизводим анимацию
        }
        else
        {
            if (StaticClassForAdditionalFunctions.AnimationExists(equipmentName + "Active", animator))
            {
                animator.Play(equipmentName + "Active"); // Воспроизводим анимацию
            }
            else
            {
                animator.enabled = false;
                //Debug.LogWarning($"Animation '{equipmentName}' not found. Displaying sprite instead.");
                selfSprite.sprite = sprite;
            }
        }

        if (BuildingWhereEquipmentIs) ParametersOfEquipmentWasAssigned?.Invoke(equipmentName, cost); // если снаряжение заспавнилось в здании, то эмулируем вызов сигнала
        _fsm.SetState<FsmStateEquipmentInsideShop>();


    }

    protected virtual void Update()
    {
        _fsm.Update();
        //Debug.Log("Спрайт " + sprite);
        //Debug.Log("Эх " + selfSprite);
        //Debug.Log("Спрайт спрайта " + selfSprite.sprite);
        //Debug.Log("Дичь " + selfSprite.sprite.GetType());
        //Debug.Log("Дичь1 " + selfSprite.sprite.name);
        //selfSprite.sprite = sprite;
    }

    // Функция для проверки существования анимации в AnimatorController


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
            rectTransformTargetPlaceScript.Equipment = this; // у скрипта экземпляра нового места поле Equipment назначаем на текущий экземпляр снаряжения
            // /\ \/ - поменяны местами
            rectTransformTargetPlaceScript.Equipment = null; // обнуляем в любом случае тамошнее снаряжение. Если его нет, то и ладно, а если есть, то оно переместится на место вот 
                                                             // этого текущего. Выше для целевого места назначим снаряжение наше новое (вот это). Сделано для того, чтоб модификаторы
                                                             // снаряжения в ИНВЕНТАРЕ сбросились и назначились корректно
            // 4. Устанавливаем родительский элемент
            transform.SetParent(rectTransformPlace, false); // false - чтобы не сохранять мировые координаты (позицию, масштаб, поворот)

            // 5. Центрируем RectTransform
            transform.anchorMin = new Vector2(0.5f, 0.5f);
            transform.anchorMax = new Vector2(0.5f, 0.5f);
            transform.anchoredPosition = Vector2.zero; // Устанавливаем смещение относительно якорей в (0, 0)
            transform.localPosition = new Vector3(0, 0, 0);
            //Debug.Log(this);
            if (BuildingWhereEquipmentIs) BuildingWhereEquipmentIs.equipmentInBuilding.Remove(this); // собственно удаляем из списка снаряжения в здании это снаряжение только
                                                                                                     // если оно находится в здании

            return true;
        }
        return false;
    }


    public void StartCallDown()
    {
        StartCoroutine(CallDown());
    }

    IEnumerator CallDown() // по идее не должно быть ситуаций, когда данная корутина будет запускаться (но не работать! работать можно!) в здании (вне инвентаря героя)
    {
        //string nameDeactivationFunction = equipmentName + "Deactivate";
        //CallActionFunctionByName(this, 0, player, nameDeactivationFunction); // player надо будет заменить на owner!!! У нас половина механики не доделана!

        isReady = false;

        if (StaticClassForAdditionalFunctions.AnimationExists(equipmentName + "Disable", animator))
        {
            animator.Play(equipmentName + "Disable"); // Воспроизводим анимацию
        }
        else
        {
            animator.enabled = false;
            //Debug.LogWarning($"Animation '{equipmentName}' not found. Displaying sprite instead.");
            selfSprite.sprite = sprite;
        }

        yield return new WaitForSeconds(timeCallDown);

        isReady = true;

        if (StaticClassForAdditionalFunctions.AnimationExists(equipmentName + "Active", animator))
        {
            animator.Play(equipmentName + "Active"); // Воспроизводим анимацию
        }
        else
        {
            animator.enabled = false;
            //Debug.LogWarning($"Animation '{equipmentName}' not found. Displaying sprite instead.");
            selfSprite.sprite = sprite;
        }
    }

    public virtual void OnDestroy()
    {
        _fsm.StateCurrent?.OnDestroy();
    }
    public virtual void OnEnable()
    {
        //Debug.Log(equipmentName);

    }



}
