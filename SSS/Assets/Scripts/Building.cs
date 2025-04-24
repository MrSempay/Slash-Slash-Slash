using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class Building : MonoBehaviour
{
    private Fsm _fsm;
    private Coroutine _coroutineUpdateAssortimentInBuilding;
    private bool _isAroundBuilding = false;

    protected string nameOfObject;

    [NonSerialized] public string nameTargetEquipmentPanelPlayer;
    [NonSerialized] public float timeForUpdateAssortiment;
    [NonSerialized] public string folderImagesOfEquipment;  // ќтносительный путь к папке с изображени€ми из папки Assets/Resources
    [NonSerialized] public GameObject entirePanel;
    [NonSerialized] public GameObject buttonEnter;
    [NonSerialized] public RectTransform rectTransformTargetEquipmentPanelPlayer; // чтоб отличать панели магазинов/аммуниции/заклинаний у игрока
    [NonSerialized] public List<Equipment> equipmentInBuilding = new List<Equipment>(); // список всего снар€жени€ в здании

    public GameObject prefubOfEquipment;
    public bool buttonEnterWasPressedToEnter = false;

    public event Action<List<Equipment>, Building> onUpdateAssortment;



    public bool IsAroundBuilding
    {
        get { return _isAroundBuilding; }
        set { _isAroundBuilding = value; }
    }
    
    public List<Equipment> NekoeSvoistvo
    {
        get { return equipmentInBuilding; }
        set { equipmentInBuilding = value;
            Debug.Log("ћџ «ƒ≈———————№№№№№№№№№№№№№№№       " + value);
        }
    }


    protected virtual void Awake()
    {

        StaticClassForAdditionalFunctions.AssignParametersAndProperties(AdjustBuildingParameters.buildingParameters, this, nameOfObject);
        //StaticClassForAdditionalFunctions.AssignPropertyValues(AdjustBuildingParameters.buildingParameters, this, nameOfObject);

        rectTransformTargetEquipmentPanelPlayer = GameObject.Find(nameTargetEquipmentPanelPlayer).GetComponent<RectTransform>();
        entirePanel = transform.Find("EntirePanel")?.gameObject; // »спользуем ?. дл€ безопасного доступа (если не найдено)
        buttonEnter = transform.Find("CanvasButtonEnter")?.gameObject;

  
        

        _fsm = new Fsm();

        _fsm.AddState(new FsmStateBuildingNormal(_fsm, gameObject));
        _fsm.AddState(new FsmStateBuildingDestroyed(_fsm, gameObject));
        _fsm.AddState(new FsmStateBuildingOpened(_fsm, gameObject));

    }

    protected virtual void Start()
    {
        RectTransform rectTransformEquipmentPlaces = transform.Find("EntirePanel/EquipmentStuffPlaces")?.gameObject.GetComponent<RectTransform>();
        _coroutineUpdateAssortimentInBuilding = CoroutineManager.Instance.StartManagedCoroutine(this.gameObject, TimerUpdateAssortmentInBuilding(rectTransformEquipmentPlaces));
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


    IEnumerator TimerUpdateAssortmentInBuilding(RectTransform rectTransformEquipmentPlaces)
    {
        while (true)
        {
            UpdateAssortmentInBuilding(rectTransformEquipmentPlaces);
            yield return new WaitForSeconds(timeForUpdateAssortiment); // ∆дем 15 секунд
        }
    }

    protected virtual void UpdateAssortmentInBuilding(RectTransform rectTransformEquipmentPlaces)
    {
        onUpdateAssortment?.Invoke(null, this); // подписываемс€ на событие в ScenarioScript, чтоб знать, когда отписыватьс€ от прослушивани€ событи€ прошлой партии ассортимента, пока
                                          // та ещЄ не была удалена
        foreach (Equipment equipment in equipmentInBuilding)
        {
            if (equipment) Destroy(equipment.gameObject);
        }
        equipmentInBuilding.Clear();
        foreach (RectTransform placeForEquipment in rectTransformEquipmentPlaces)
        {
            // —ќ«ƒј®ћ ќЅЏ≈ “ —Ќј–я∆≈Ќ»я, ѕќЋ”„ј≈ћ ≈√ќ »ћя, RectTransform, —ѕј¬Ќ»ћ ” «јƒјЌЌќ√ќ –ќƒ»“≈Ћя (ћ≈—“ј —Ќј–я∆≈Ќ»я)
            GameObject newEquipment = Instantiate(prefubOfEquipment, Vector3.zero, Quaternion.identity);
            RectTransform newEquipmentRectTransform = newEquipment.GetComponent<RectTransform>();
            string randomEquipmentName = AdjustEquipmentParameters.GetRandomSpellName();
            newEquipmentRectTransform.SetParent(placeForEquipment, false); // false - чтобы не сохран€ть мировые координаты (позицию, масштаб, поворот)

            // Ќј—“–ј»¬ј≈ћ  ќћѕќЌ≈Ќ“ RectTransform ” Ё «≈ћѕЋя–ј —Ќј–я∆≈Ќ»я
            newEquipmentRectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            newEquipmentRectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            newEquipmentRectTransform.anchoredPosition = Vector2.zero; // ”станавливаем смещение относительно €корей в (0, 0)
            newEquipmentRectTransform.localPosition = new Vector3(0, 0, 0);
            newEquipmentRectTransform.name = randomEquipmentName;

            // Ќј—“–ј»¬ј≈ћ  ќћѕќЌ≈Ќ“ SpriteRenderer ” Ё «≈ћѕЋя–ј —Ќј–я∆≈Ќ»я
            SpriteRenderer spriteRenderer = newEquipment.GetComponent<SpriteRenderer>();
            string fullPath = folderImagesOfEquipment + randomEquipmentName;
            Sprite spellSprite = Resources.Load<Sprite>(fullPath);
            spriteRenderer.sprite = spellSprite;


            // Ќј—“–ј»¬ј≈ћ  ќћѕќЌ≈Ќ“ Equipment (—ќЅ—Ќј ≈√ќ — –»ѕ“) ” Ё «≈ћѕЋя–ј —Ќј–я∆≈Ќ»я
            Equipment scriptOfEquipment = newEquipment.GetComponent<Equipment>();
            scriptOfEquipment.equipmentName = randomEquipmentName;
            scriptOfEquipment.sprite = spellSprite;
            scriptOfEquipment.isEquipmentASpell = true; // пока что дл€ спелов только
            scriptOfEquipment.startLocalPosition = newEquipmentRectTransform.localPosition;
            scriptOfEquipment.BuildingWhereEquipmentIs = this;
            scriptOfEquipment.rectTransformTargetEquipmentPanelPlayer = rectTransformTargetEquipmentPanelPlayer;
            scriptOfEquipment.transformCurrentEquipmentPlace = placeForEquipment;

            // »«ћ≈Ќя≈ћ ѕј–јћ≈“–џ «ƒјЌ»я ѕ–» ƒќЅј¬Ћ≈Ќ»» ¬ Ќ≈√ќ Ќќ¬ќ√ќ —Ќј–я∆≈Ќ»я
            equipmentInBuilding.Add(scriptOfEquipment);
            PlaceForEquipment scriptOfPlace = placeForEquipment.gameObject.GetComponent<PlaceForEquipment>();
            scriptOfPlace.Equipment = scriptOfEquipment;
            scriptOfPlace.isBuildingPlace = true;
        }
        onUpdateAssortment?.Invoke(equipmentInBuilding, this); // подписываемс€ на событие в ScenarioScript, чтоб знать, когда был обновлЄн ассортимент в здании
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
        //equipment.WasSold = true; // присваиваем true в состо€нии AtPlayer
        targetForBuy.CurrentMoney -= equipment.cost;
    }

    
    public bool HasAccessToUpLevelInSchool(Player targetForBuy)
    {
        return targetForBuy.CountAccessToUpInSchool > 0;
    }

    public void TeachByUpLevel(Player targetForBuy, Equipment equipment)
    {
        targetForBuy.CountAccessToUpInSchool--;
    }

    public virtual void OnDestroy()
    {
        _fsm.StateCurrent?.OnDestroy();
        if (_coroutineUpdateAssortimentInBuilding != null)
        {
            CoroutineManager.Instance.StopManagedCoroutine(this.gameObject, _coroutineUpdateAssortimentInBuilding);
            _coroutineUpdateAssortimentInBuilding = null;
        }
    }
}
