using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static C;

public class Equipment : MonoBehaviour
{
    private Building _buildingWhereEquipmentIs; // здание, в котором находится снаряжение
    private bool _wasSold = false; // по умолчанию считаем, что все предметы находятся у продавца (в здании) 
    private Image _callDownIcon; // иконка для анимации таймера КД.
    private Coroutine _callDownCoroutine; // иконка для анимации таймера КД.
    private Coroutine _callDownAnimationCoroutine; // иконка для анимации таймера КД.
    private bool _startWasCalledAlready = false; // иконка для анимации таймера КД.
    private bool _awakeWasCalledAlready = false; // иконка для анимации таймера КД.
    [NonSerialized] public AreaDetectEnteringExiting _areaDetectEnteringExiting;
    private EquipmentInfoPanel _equipmentInfoPanel;

    public Animator mdaaa;


    [NonSerialized] public Fsm _fsm; // сделали публичным только для того, чтоб проверять текущее состояние для блядства Input. Ведь Input.GetMouseButtonDown(0) у нас срабатывает для всех
                                     // ебучих объектов при нажатии, а не только на том объекте, на котором мы нажали. Посему придётся проверять состояние для ситуации, когда нам не нужно
                                     // делать проверку на то, по этому ли объекту кликнули, ибо подразумевается что в состоянии FsmStateEquipmentSelected одновременно может быть только один объект
    [NonSerialized] public Transform transformPlaceInfoPanel;
    [NonSerialized] public SpriteRenderer selfSprite; // свой компонент спрайта
    [NonSerialized] public UnityEngine.Sprite sprite; // свой спрайт, назначается в здании при спавне
    [NonSerialized] public Vector3 startLocalPosition;
    [NonSerialized] public RectTransform rectTransformTargetEquipmentPanelPlayer; // чтоб отличать панели магазинов/аммуниции/заклинаний у игрока
    [NonSerialized] public Unit ownerUnit; // если владелец кто-либо из юнитов
    [NonSerialized] public IInventoryStatic ownerStatic; // если владелец что-нибудь статичное (здание, например)
    [NonSerialized] public int cost;
    [NonSerialized] public bool isEquipmentASpell;
    [NonSerialized] public string equipmentName;
    [NonSerialized] public Animator animator;
    [NonSerialized] public RectTransform transformCurrentEquipmentPlace; // компонент RectTransform текущего места нашего снаряжения. Нужно, чтоб задать это же место другому снаряжению при обмене местами
    [NonSerialized] public bool isReady = true;
    [NonSerialized] new public RectTransform transform;
    [NonSerialized] public BoxCollider2D selfCollider;
    [NonSerialized] public Equipment newScriptOfEquipment;

    public int amountUpCombo;
    public bool isActivated; // флаг активированных снаряжений. Типа переключаемой способности
    public bool shouldBeCastedAtStartUnitAnimation; // флаг, определяющий, должен ли эффект каста начать так или иначе работать сразу при начале анимации каста персонажа, или только после её полного завершения
    public float timeCallDown;
    public float durationActiveState = -1; // -1 для бесконечного активного по времени состояния
    public Dictionary<string, float> increasingUnitParametersByAmmunitionPercentageByCast = new Dictionary<string, float>();
    public event Action<string, int> ParametersOfEquipmentWasAssigned;
    public event Action<Equipment> OnEquipmentShouldBeActivate;
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
        if (!_awakeWasCalledAlready)
        {
            transform = GetComponent<RectTransform>();
            //player = GameObject.Find("Player").GetComponent<Player>();
            selfSprite = GetComponent<SpriteRenderer>();
            animator = GetComponent<Animator>();
            selfCollider = GetComponent<BoxCollider2D>();
            mdaaa = animator;

            _callDownIcon = transform.Find(C.NamesObjects.CallDownIcon).GetComponent<Image>();
            _areaDetectEnteringExiting = transform.Find(C.NamesObjects.AreaDetectEnteringExiting).GetComponent<AreaDetectEnteringExiting>();
            transformPlaceInfoPanel = transform.Find(C.NamesObjects.PlaceInfoPanel).GetComponent<Transform>();

            _areaDetectEnteringExiting.somethingEnterExitArea += PlayerEnteredInfoArea;

            //spriteCallDown = callDownIcon.sprite; // на тот случай, если мы не найдём спрайт по имени + Disabled при спавне снаряжения. Спрайт должен быть всегда!
            sprite = selfSprite.sprite; // на тот случай, если мы не найдём спрайт по имени при спавне снаряжения. Спрайт должен быть всегда!
            _awakeWasCalledAlready = true;
        }


        //animator.Play(equipmentName);
    }
    public virtual void Cast(Unit whoCasted) { }
    public virtual void Activate(Unit whoCasted) // в теории можно передавать параметры, которые будут регулировать, на что именно мы подписываемся, но это так запарно...
    {
        whoCasted.OnCastAnimationFinished += UnitCastAnimationFinished;
        whoCasted.OnCastAnimationPeacked += UnitCastAnimationPeacked;
    }
    public virtual void Deactivate(Unit whoCasted)
    {
        whoCasted.OnCastAnimationFinished -= UnitCastAnimationFinished;
        whoCasted.OnCastAnimationPeacked -= UnitCastAnimationPeacked;
    }
    public virtual void EnteredIntoUnitInventory(Unit ownerInventory) { }
    public virtual void ExitedFromUnitInventory(Unit ownerInventory)
    {
        Deactivate(ownerInventory);
    }
    public virtual void EnteredIntoStaticInventory(IInventory ownerInventory) { }
    public virtual void ExitedFromStaticInventory(IInventory ownerInventory) { }

    public virtual void Start()
    {
        if (!_startWasCalledAlready)
        {
            _fsm = new Fsm();

            _fsm.AddState(new FsmStateEquipmentSelected(_fsm, gameObject));
            _fsm.AddState(new FsmStateEquipmentInsideShop(_fsm, gameObject));
            _fsm.AddState(new FsmStateEquipmentAtPlayer(_fsm, gameObject));
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
                    //Debug.Log("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAA MUD-da-daYUUU");
                    animator.enabled = false; // ОЧЕНЬ ВАЖНО! Иначе картинку в SpriteRenderer не даст отображать!
                                              //Debug.LogWarning($"Animation '{equipmentName}' not found. Displaying sprite instead.");
                    selfSprite.sprite = sprite; // по идее sprite никогда не будет null, если в префабе по умолчанию уже стоит хоть какой-то спрайт для снаряжения
                }
            }

            //callDownIcon.sprite = spriteCallDown;


            if (BuildingWhereEquipmentIs)
            {
                ParametersOfEquipmentWasAssigned?.Invoke(equipmentName, cost); // если снаряжение заспавнилось в здании, то эмулируем вызов сигнала 
                _fsm.SetState<FsmStateEquipmentInsideShop>();
            }
            else if (ownerUnit)
            {
                _fsm.SetState<FsmStateEquipmentAtPlayer>();
            }
            _startWasCalledAlready = true;
        }


    }

    protected virtual void Update()
    {
        _fsm.Update();
        //Debug.Log("Снаряжение " + this + " " + gameObject.GetInstanceID());
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
            // /\ \/ - поменяны местами
            rectTransformTargetPlaceScript.Equipment = null; // обнуляем в любом случае тамошнее снаряжение. Если его нет, то и ладно, а если есть, то оно переместится на место вот 
                                                             // этого текущего. Выше для целевого места назначим снаряжение наше новое (вот это). Сделано для того, чтоб модификаторы
                                                             // снаряжения в ИНВЕНТАРЕ сбросились и назначились корректно
            rectTransformTargetPlaceScript.Equipment = this; // у скрипта экземпляра нового места поле Equipment назначаем на текущий экземпляр снаряжения
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

    public void StartTimerActiveState(Unit whoCastedSpell) // хотя, в будущем, логичнее было бы заменить whoCastedSpell на targetOfSpell
    {
        if (durationActiveState != -1f)
        {
            StartCoroutine(DurationActive(whoCastedSpell));
        }
    }

    IEnumerator DurationActive(Unit whoCastedSpell)
    {
        Debug.Log(durationActiveState);
        yield return new WaitForSeconds(durationActiveState);

        Deactivate(whoCastedSpell);
    }

    public void StartCallDown()
    {
        //_callDownCoroutine = StartCoroutine(CallDown());
        //_callDownAnimationCoroutine = StartCoroutine(CallDownIconAnimation());
        _callDownCoroutine = CoroutineManager.Instance.StartManagedCoroutine(gameObject, CallDown());
        _callDownAnimationCoroutine = CoroutineManager.Instance.StartManagedCoroutine(gameObject, CallDownIconAnimation()); 
    }


    public void EquipmentShouldBeActivate(Equipment equipment) // просто обёртка над сигналом, который долен вызываться, когда игрок нажимает на иконку снаряжения
    {
        OnEquipmentShouldBeActivate?.Invoke(equipment); // ну по сути equipment = this
    }
    IEnumerator CallDownIconAnimation()
    {
        _callDownIcon.gameObject.SetActive(true);
        _callDownIcon.fillAmount = 1;
        while (!isReady)
        {
            yield return null;
            _callDownIcon.fillAmount -= 1 / (timeCallDown / Time.deltaTime);
        }
        _callDownIcon.gameObject.SetActive(false);
        _callDownIcon.fillAmount = 0;
    }

    IEnumerator CallDown() // по идее не должно быть ситуаций, когда данная корутина будет запускаться (но не работать! работать можно!) в здании (вне инвентаря героя)
    {
        //string nameDeactivationFunction = equipmentName + "Deactivate";
        //CallActionFunctionByName(this, 0, player, nameDeactivationFunction); // player надо будет заменить на owner!!! У нас половина механики не доделана!

        isReady = false;

        if (StaticClassForAdditionalFunctions.AnimationExists(equipmentName + "Disable", animator))
        {
            animator.Play(equipmentName + "Disable"); // Воспроизводим анимацию. Хотя теперь не понятно, что делать с анимацией таймера КД, где у нас заполяется прозрачный спрайт
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

    #region Animation live-time controlling

    private void UnitCastAnimationFinished(string nameCastAnimation) // юнит может закончить различные анимации каста, мы фильтруем только анимацию каста, подходящую для данного
                                                                     // снаряжения. Шаблон имени анимации: ИМЯ_СНАРЯЖЕНИЯ + "Cast", например: ProtectiveFieldCast
    {
        if (nameCastAnimation == equipmentName + C.Prefixes.Cast)
        {
            UnitCastAnimationFinishedForThisEquipment();
        }
    }

    // для того, чтобы использовать нишеописанную функцию, требуется в конечном скрипте снаряжения в методах Activate и Deactivate вызвать их базовые рализации через base.
    // после этого переопределяем в конечном скрипте функцию UnitCastAnimationFinishedForThisEquipment с той логикой, которая нам нужна
    public virtual void UnitCastAnimationFinishedForThisEquipment() { } // по идее вызывается, когда юнит закончил анимацию каста для данного снаряжения. Может вызваться только если
                                                                        // снаряжение уже в состоянии Active. Нужно, скорее всего, только в том случае, когда переход в состояние Active
                                                                        // происходит сразу при начале каста, но какая-то дополнительная логика отрабатывает, когда анимация каста уже окончена
    private void UnitCastAnimationPeacked(string nameCastAnimation) // юнит может закончить различные анимации каста, мы фильтруем только анимацию каста, подходящую для данного
                                                                    // снаряжения. Шаблон имени анимации: ИМЯ_СНАРЯЖЕНИЯ + "Cast", например: ProtectiveFieldCast
    {
        if (nameCastAnimation == equipmentName + C.Prefixes.Peak)
        {
            UnitCastAnimationPeackedForThisEquipment();
        }
    }

    public virtual void UnitCastAnimationPeackedForThisEquipment() { }

    #endregion




    private void PlayerEnteredInfoArea(bool isEnter, GameObject obj, Transform transformArea)
    {
        if (obj.CompareTag("Player") && !WasSold)
        {
            if (isEnter)
            {
                if (_equipmentInfoPanel)
                {
                    _equipmentInfoPanel.gameObject.SetActive(true);
                }
                else
                {
                    _equipmentInfoPanel = Instantiate(GameManager.Instance.prefubEquipmentInfoPanel, transformPlaceInfoPanel, false);
                    _equipmentInfoPanel.FillInfoForm(this);
                }
            }
            else
            {
                if (_equipmentInfoPanel) // хотя null, по идее, оно тут не может быть, ибо перед тем как из зоны выйти, нужно в неё зайти
                {
                    _equipmentInfoPanel.gameObject.SetActive(false);
                }
            }
        }
    }


    public virtual void OnDestroy()
    {
        //Debug.Log("Уничтожен, низведён до АТОМОВ!!! " + GetInstanceID());

        CoroutineManager.Instance.StopAllCoroutinesFor(gameObject);
        _areaDetectEnteringExiting.somethingEnterExitArea -= PlayerEnteredInfoArea;
        _fsm.StateCurrent.Exit(); // Если снаряжении находится в состоянии Selected, то дабы корректно завершить состояние Translate у Player необходимо выйти из состояния Selected.

        StopAllCoroutines();
        if (_fsm != null)
            _fsm.StateCurrent?.OnDestroy();
    }
    public virtual void OnEnable()
    {
        if (!isReady)
        {
            //(_callDownCoroutine);
            //StartCoroutine(_callDownAnimationCoroutine);            
        }

    }



}
