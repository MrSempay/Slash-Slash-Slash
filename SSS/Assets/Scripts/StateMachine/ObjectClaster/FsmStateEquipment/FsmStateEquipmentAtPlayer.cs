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
        equipment.BuildingWhereEquipmentIs = null; // по умолчанию будем считать, что мы снаряжение вытащили из здания. Менять этот параметр пока что будем в состоянии FsmStateEquipmentAtPlayer
        equipment.transform.localPosition = equipment.startLocalPosition;
        equipment.selfSprite.sortingOrder = 21; // выше всех UI-элементов, кроме диалога, по идее
        if (!equipment.WasSold) // не устанавливаем WasSold в true, если снаряжение уже у нас типа в инвентаре
        {
            equipment.WasSold = true;
        }

        if (equipment.isEquipmentASpell)
        {
            if (equipment.player.playersSpells.Contains((Spell)equipment))
            {
                return;
            }
            else
            {
                equipment.player.playersSpells.Add((Spell)equipment);
            }
        }

    }

    public override void Exit()
    {
        Debug.Log("Equipment At Player state [EXIT]");
        CoroutineManager.Instance.StopManagedCoroutine(this.gameObject, _coroutineDeleyBeforeSelectedState);
        _coroutineDeleyBeforeSelectedState = null;

        if (equipment.isEquipmentASpell)
        {
            if (equipment.player.playersSpells.Contains((Spell)equipment))
            {
                equipment.player.playersSpells.Remove((Spell)equipment);
            }
        }

    }

    public override void Update()
    {
        if (equipment.BuildingWhereEquipmentIs != null) // меняем эту переменную в FsmStateEquipmentSelected если хотим поменять местами снаряжение у игрока и в здании. Если снаряжение было у
                                                        // игрока то смотрим здание, в которое снаряжение попало и по ссылке здания меняем его поле equipmentInBuilding, добавляя к списку
                                                        // наше снаряжение. В теории можно было бы менять equipmentInBuilding при входе в FsmStateEquipmentInsideShop, но ведь мы туда можем войти
                                                        // и не из состояния FsmStateEquipmentAtPlayer, а лишь при создании, в таком случае получится дубликат в списке equipmentInBuilding
        {
            // на самом деле это вообще не работает сейчас, ибо мы разрешили менять местами снаряжение только в целевой панели снаряжений. Между панелями обмен закрыт на данный момент
            equipment.BuildingWhereEquipmentIs.equipmentInBuilding.Add(equipment);
            fsm.SetState<FsmStateEquipmentInsideShop>();
        }


        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                if (IsEquipmentPlaceOccupied(Camera.main.ScreenToWorldPoint(touch.position).x, Camera.main.ScreenToWorldPoint(touch.position).y))
                    _coroutineDeleyBeforeSelectedState = CoroutineManager.Instance.StartManagedCoroutine(this.gameObject, DeleyBeforeSelectedState());
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                // где бы мы не отпустили кнопку, если мы всё ещё не выделили наше снаряжение, отменяем попытку выделения его.
                CoroutineManager.Instance.StopManagedCoroutine(this.gameObject, _coroutineDeleyBeforeSelectedState);
                _coroutineDeleyBeforeSelectedState = null;
                if (IsEquipmentPlaceOccupied(Camera.main.ScreenToWorldPoint(touch.position).x, Camera.main.ScreenToWorldPoint(touch.position).y))
                {
                    if (equipment.isReady)
                    {
                        AdjustEquipmentParameters.CallActionByName(equipment, equipment.amountUpCombo, equipment.player);
                    }
                }
            }
            return;
        }


        if (Input.GetMouseButtonDown(0)) // Когда нажата левая кнопка мыши
        {
             if (IsEquipmentPlaceOccupied(Camera.main.ScreenToWorldPoint(Input.mousePosition).x, Camera.main.ScreenToWorldPoint(Input.mousePosition).y))
                _coroutineDeleyBeforeSelectedState = CoroutineManager.Instance.StartManagedCoroutine(this.gameObject, DeleyBeforeSelectedState()); 
        }
        if (Input.GetMouseButtonUp(0)) // Когда нажата левая кнопка мыши
        {
            // где бы мы не отпустили кнопку, если мы всё ещё не выделили наше снаряжение, отменяем попытку выделения его.
            CoroutineManager.Instance.StopManagedCoroutine(this.gameObject, _coroutineDeleyBeforeSelectedState);
            _coroutineDeleyBeforeSelectedState = null;
            if (IsEquipmentPlaceOccupied(Camera.main.ScreenToWorldPoint(Input.mousePosition).x, Camera.main.ScreenToWorldPoint(Input.mousePosition).y))
            {
                if (equipment.isReady) // у любого снаряжения, даже если нет активки, есть кд, по умолчанию равно 0 секундам, задаётся в скрипте Adjust
                {
                    AdjustEquipmentParameters.CallActionByName(equipment, equipment.amountUpCombo, equipment.player);
                }
            }
              
        }

    }

    private IEnumerator DeleyBeforeSelectedState()
    {
        yield return new WaitForSeconds(_timeDeleyBeforeChangingState);
        fsm.SetState<FsmStateEquipmentSelected>();

    }
}
