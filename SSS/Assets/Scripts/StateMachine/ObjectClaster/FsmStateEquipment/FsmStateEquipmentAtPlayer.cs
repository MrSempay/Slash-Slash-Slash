using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class FsmStateEquipmentAtPlayer : FsmStateEquipment
{

    private Coroutine _coroutineDeleyBeforeSelectedState;
    private float _timeDeleyBeforeChangingState = 2f;

    public FsmStateEquipmentAtPlayer(Fsm fsm, GameObject gameObject) : base(fsm, gameObject)
    {
        
    }

    public override void Enter()
    {
        Debug.Log("Equipment At Player state [ENTER]");
        equipment.BuildingWhereEquipmentIs = null; // по умолчанию будем считать, что мы снар€жение вытащили из здани€. ћен€ть этот параметр пока что будем в состо€нии FsmStateEquipmentAtPlayer
        equipment.transform.localPosition = equipment.startLocalPosition;
        if (!equipment.WasSold) // не устанавливаем WasSold в true, если снар€жение уже у нас типа в инвентаре
        {
            equipment.WasSold = true;
        }
    }

    public override void Exit()
    {
        Debug.Log("Equipment At Player state [EXIT]");
        CoroutineManager.Instance.StopManagedCoroutine(this.gameObject, _coroutineDeleyBeforeSelectedState);
        _coroutineDeleyBeforeSelectedState = null;
    }

    public override void Update()
    {
        if (equipment.BuildingWhereEquipmentIs != null) // мен€ем эту переменную в FsmStateEquipmentSelected если хотим помен€ть местами снар€жение у игрока и в здании. ≈сли снар€жение было у
                                                 // игрока то смотрим здание, в которое снар€жение попало и по ссылке здани€ мен€ем его свойство equipmentInBuilding, добавл€€ к списку
                                                 // наше снар€жение. ¬ теории можно было бы мен€ть equipmentInBuilding при входе в FsmStateEquipmentInsideShop, но ведь мы туда можем войти
                                                 // и не из состо€ни€ FsmStateEquipmentAtPlayer, а лишь при создании, в таком случае получитс€ дубликат в списке equipmentInBuilding
        {
            // на самом деле это вообще не работает сейчас, ибо мы разрешили мен€ть местами снар€жение только в целевой панели снар€жений. ћежду панел€ми обмен закрыт на данный момент
            equipment.BuildingWhereEquipmentIs.equipmentInBuilding.Add(equipment);
            fsm.SetState<FsmStateEquipmentInsideShop>();
        }

        if (Input.GetMouseButtonDown(0)) //  огда нажата лева€ кнопка мыши
        {
             if (IsEquipmentPlaceOccupied(Camera.main.ScreenToWorldPoint(Input.mousePosition).x, Camera.main.ScreenToWorldPoint(Input.mousePosition).y))
                _coroutineDeleyBeforeSelectedState = CoroutineManager.Instance.StartManagedCoroutine(this.gameObject, DeleyBeforeSelectedState()); 
        }
        if (Input.GetMouseButtonUp(0)) //  огда нажата лева€ кнопка мыши
        {
            // где бы мы не отпустили кнопку, если мы всЄ ещЄ не выделили наше снар€жение, отмен€ем попытку выделени€ его.
            CoroutineManager.Instance.StopManagedCoroutine(this.gameObject, _coroutineDeleyBeforeSelectedState);
            _coroutineDeleyBeforeSelectedState = null;
            if (IsEquipmentPlaceOccupied(Camera.main.ScreenToWorldPoint(Input.mousePosition).x, Camera.main.ScreenToWorldPoint(Input.mousePosition).y))
            {
                if (equipment.isEquipmentASpell) // у любого снар€жени€ есть флаг, €вл€етс€ ли это снар€жение спелом или аммуницией. ≈сли спел, то вызываем функцию по касту спела
                    AdjustEquipmentParameters.CallSpellByName((Spell) equipment);
            }
              
        }

    }

    private IEnumerator DeleyBeforeSelectedState()
    {
        yield return new WaitForSeconds(_timeDeleyBeforeChangingState);
        fsm.SetState<FsmStateEquipmentSelected>();

    }
}
