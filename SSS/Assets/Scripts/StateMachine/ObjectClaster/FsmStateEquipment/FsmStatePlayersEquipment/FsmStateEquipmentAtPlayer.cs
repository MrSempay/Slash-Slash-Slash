using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class FsmStateEquipmentAtPlayer : FsmStatePlayersEquipment // эта должна быть именно для игрока, так как тут проверяем нажатие по снаряжению. Для прочих юнитов другое состояние нужно
{

    private Coroutine _coroutineDeleyBeforeSelectedState;
    private float _timeDeleyBeforeChangingState = 2f;
    //private bool _enteredToStateJustNow = true;

    public FsmStateEquipmentAtPlayer(Fsm fsm, GameObject gameObject) : base(fsm, gameObject)
    {
        
    }

    public override void Enter(Dictionary<string, object> initialConditionsEntering)
    {
        Debug.Log("Equipment At Player state [ENTER]");

        //_enteredToStateJustNow = true;
        if (equipment.BuildingWhereEquipmentIs != null)
        {
            equipment.BuildingWhereEquipmentIs = null; // по умолчанию будем считать, что мы снаряжение вытащили из здания. Менять этот параметр пока что будем в состоянии FsmStateEquipmentAtUnit
            
        }
        equipment.transform.localPosition = equipment.startLocalPosition;
        equipment.selfSprite.sortingOrder = 21; // выше всех UI-элементов, кроме диалога, по идее
        if (!equipment.WasSold) // не устанавливаем WasSold в true, если снаряжение уже у нас типа в инвентаре. По идее WasSold маркирует снаряжение, true только если продан и в инвентаре
        {
            equipment.WasSold = true; // может, стоит WasSold заменить просто на AtPlayer.
            player.Inventory.SetEquipmentToInventory(equipment);
        }

        //if (equipment.isEquipmentASpell)
        //{
        //    if (player.listSpellsInInventory.Contains((Spell)equipment))
        //    {
        //        return;
        //    }
        //    else
        //    {
        //        player.listSpellsInInventory.Add((Spell)equipment);
        //    }
        //}

    }

    public override void Exit()
    {
        Debug.Log("Equipment At Player state [EXIT]");
        CoroutineManager.Instance.StopManagedCoroutine(this.gameObject, _coroutineDeleyBeforeSelectedState);
        _coroutineDeleyBeforeSelectedState = null;

        //if (equipment.isEquipmentASpell)
        //{
        //    if (player.listSpellsInInventory.Contains((Spell)equipment))
        //    {
        //        player.listSpellsInInventory.Remove((Spell)equipment);
        //    }
        //}

    }

    public override void Update()
    {
        if (equipment.BuildingWhereEquipmentIs != null) // меняем эту переменную в FsmStateEquipmentSelected если хотим поменять местами снаряжение у игрока и в здании. Если снаряжение было у
                                                        // игрока то смотрим здание, в которое снаряжение попало и по ссылке здания меняем его поле equipmentInBuilding, добавляя к списку
                                                        // наше снаряжение. В теории можно было бы менять equipmentInBuilding при входе в FsmStateEquipmentInsideShop, но ведь мы туда можем войти
                                                        // и не из состояния FsmStateEquipmentAtUnit, а лишь при создании, в таком случае получится дубликат в списке equipmentInBuilding
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
                //if (_enteredToStateJustNow)
                //{
                //    _enteredToStateJustNow = false;
                //    return;
                //}

                // где бы мы не отпустили кнопку, если мы всё ещё не выделили наше снаряжение, отменяем попытку выделения его.
                CoroutineManager.Instance.StopManagedCoroutine(this.gameObject, _coroutineDeleyBeforeSelectedState);
                _coroutineDeleyBeforeSelectedState = null;
                if (IsEquipmentPlaceOccupied(Camera.main.ScreenToWorldPoint(touch.position).x, Camera.main.ScreenToWorldPoint(touch.position).y))
                {
                    if (equipment.isReady && player.areUpdatingFunctionsEnabled && !equipment.isActivated) // если не КД и действия игрока не заблокированы, и снаряжение сейчас не в активном состоянии!
                    {
                        //player._fsm.SetState<FsmStateCastUnit>(new Dictionary<string, object> { { "equipmentWhatWasPressed", equipment } });
                        equipment.EquipmentShouldBeActivate(equipment);
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
            //Debug.Log("AAAAAAAAAAAAAAAAA МЫ ЧЁ, ТИПА КАСТУЕМ? ЧЕГО ЗА РОФЛЯ?!");

            //if (_enteredToStateJustNow)
            //{
            //    _enteredToStateJustNow = false;
            //    return;
            //}

            // где бы мы не отпустили кнопку, если мы всё ещё не выделили наше снаряжение, отменяем попытку выделения его.
            CoroutineManager.Instance.StopManagedCoroutine(this.gameObject, _coroutineDeleyBeforeSelectedState);
            _coroutineDeleyBeforeSelectedState = null;
            if (IsEquipmentPlaceOccupied(Camera.main.ScreenToWorldPoint(Input.mousePosition).x, Camera.main.ScreenToWorldPoint(Input.mousePosition).y))
            {
                if (equipment.isReady && player.areUpdatingFunctionsEnabled && !equipment.isActivated) // у любого снаряжения, даже если нет активки, есть кд, по умолчанию равно 0 секундам, задаётся в скрипте Adjust
                {
                    //player._fsm.SetState<FsmStateCastUnit>(new Dictionary<string, object> { { "equipmentWhatWasPressed", equipment } });
                    equipment.EquipmentShouldBeActivate(equipment);
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
