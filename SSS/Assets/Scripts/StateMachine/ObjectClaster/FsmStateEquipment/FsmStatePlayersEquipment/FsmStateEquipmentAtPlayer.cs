using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FsmStateEquipmentAtPlayer : FsmStatePlayersEquipment // эта должна быть именно для игрока, так как тут проверяем нажатие по снаряжению. Для прочих юнитов другое состояние нужно
{

    private Coroutine _coroutineDeleyBeforeShowChoosePanel;
    private Equipment _equipmentAtWhatPressed; // необходимо для того, чтобы мы активировали только то снаряжение, на иконку которого нажали и отпустили. Иначе у нас можно нажать на одом
    // снаряжении, отпустить на другом - и кастанётся второе. Или вообще нажать незнамо где, далее отпустить на снаряжении, игрок вместо свайпа кастовать начнёт. Это выглядит плохо
    private float _timeDeleyBeforeChangingState = 2f;
    private bool _showingChoosePanel = false;
    private RectTransform _rectTransformPanelInfo;
    private Vector3 _enterPivot = new Vector2(0.5f, 0f);
    private Vector3 _exitPivot = new Vector2(0.5f, 0.5f);
    //private bool _enteredToStateJustNow = true;

    public FsmStateEquipmentAtPlayer(Fsm fsm, GameObject gameObject) : base(fsm, gameObject)
    {
        _rectTransformPanelInfo = (RectTransform)equipment.equipmentInfoPanel.transform;
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

        ControlPositionPanelInfo(true);

        equipment.OnMoveEquipment += MoveEquipment;

    }
    public override void Exit()
    {
        Debug.Log("Equipment At Player state [EXIT]");
        CoroutineManager.Instance.StopManagedCoroutine(this.gameObject, _coroutineDeleyBeforeShowChoosePanel);
        _coroutineDeleyBeforeShowChoosePanel = null;

        ControlPositionPanelInfo(false);

        equipment.OnMoveEquipment -= MoveEquipment;

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
                _equipmentAtWhatPressed = IsEquipmentPlaceOccupied(Camera.main.ScreenToWorldPoint(touch.position).x, Camera.main.ScreenToWorldPoint(touch.position).y);
                Debug.Log(_equipmentAtWhatPressed);
                if (_equipmentAtWhatPressed == equipment)
                    _coroutineDeleyBeforeShowChoosePanel = CoroutineManager.Instance.StartManagedCoroutine(this.gameObject, DeleyBeforeShowChoosePanel());
                else
                {
                    _equipmentAtWhatPressed = null;
                }
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                //if (_enteredToStateJustNow)
                //{
                //    _enteredToStateJustNow = false;
                //    return;
                //}

                if (_showingChoosePanel) // чтоб предмет не прожался, если мы только что активировали панель выбора
                {
                    _showingChoosePanel = false;
                    return;
                }

                // где бы мы не отпустили кнопку, если мы всё ещё не выделили наше снаряжение, отменяем попытку выделения его. 

                CoroutineManager.Instance.StopManagedCoroutine(this.gameObject, _coroutineDeleyBeforeShowChoosePanel);
                _coroutineDeleyBeforeShowChoosePanel = null;
                Equipment equipmentEndedTouch = IsEquipmentPlaceOccupied(Camera.main.ScreenToWorldPoint(touch.position).x, Camera.main.ScreenToWorldPoint(touch.position).y);
                Debug.Log(_equipmentAtWhatPressed);
                Debug.Log(equipmentEndedTouch);
                if (equipmentEndedTouch && equipmentEndedTouch == _equipmentAtWhatPressed)
                {
                    _equipmentAtWhatPressed = null;
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
            _equipmentAtWhatPressed = IsEquipmentPlaceOccupied(Camera.main.ScreenToWorldPoint(Input.mousePosition).x, Camera.main.ScreenToWorldPoint(Input.mousePosition).y);
            Debug.Log(_equipmentAtWhatPressed);
            if (IsEquipmentPlaceOccupied(Camera.main.ScreenToWorldPoint(Input.mousePosition).x, Camera.main.ScreenToWorldPoint(Input.mousePosition).y))
                _coroutineDeleyBeforeShowChoosePanel = CoroutineManager.Instance.StartManagedCoroutine(this.gameObject, DeleyBeforeShowChoosePanel());
            else
            {
                _equipmentAtWhatPressed = null;
            }
        }
        if (Input.GetMouseButtonUp(0)) // Когда нажата левая кнопка мыши
        {
            //Debug.Log("AAAAAAAAAAAAAAAAA МЫ ЧЁ, ТИПА КАСТУЕМ? ЧЕГО ЗА РОФЛЯ?!");

            //if (_enteredToStateJustNow)
            //{
            //    _enteredToStateJustNow = false;
            //    return;
            //}

            if (_showingChoosePanel)
            {
                _showingChoosePanel = false;
                return;
            }

            // где бы мы не отпустили кнопку, если мы всё ещё не выделили наше снаряжение, отменяем попытку выделения его.
            CoroutineManager.Instance.StopManagedCoroutine(this.gameObject, _coroutineDeleyBeforeShowChoosePanel);
            _coroutineDeleyBeforeShowChoosePanel = null;
            Equipment equipmentEndedTouch = IsEquipmentPlaceOccupied(Camera.main.ScreenToWorldPoint(Input.mousePosition).x, Camera.main.ScreenToWorldPoint(Input.mousePosition).y);
            if (equipmentEndedTouch && equipmentEndedTouch == _equipmentAtWhatPressed)
            {
                if (equipment.isReady && player.areUpdatingFunctionsEnabled && !equipment.isActivated) // у любого снаряжения, даже если нет активки, есть кд, по умолчанию равно 0 секундам, задаётся в скрипте Adjust
                {
                    //player._fsm.SetState<FsmStateCastUnit>(new Dictionary<string, object> { { "equipmentWhatWasPressed", equipment } });
                    equipment.EquipmentShouldBeActivate(equipment);
                }
            }
              
        }

    }

    private void MoveEquipment()
    {
        fsm.SetState<FsmStateEquipmentSelected>();
    }
    private void ControlPositionPanelInfo(bool isEnter)
    {
        equipment.transformPlaceInfoPanel.localPosition = isEnter? new Vector3(0f, 0.75f, 0f) : baseLocalPositionInfoPanel;
        _rectTransformPanelInfo.pivot = isEnter? new Vector2(0.5f, 0f) : new Vector2(0.5f, 0.5f);
    }

    private IEnumerator DeleyBeforeShowChoosePanel()
    {
        yield return new WaitForSeconds(_timeDeleyBeforeChangingState);

        _showingChoosePanel = true;

        InventoryPlayer inventoryPlayer = (InventoryPlayer)Player.instance.Inventory;
        inventoryPlayer.ShowPanelChoose((RectTransform)equipment.panelChoose.gameObject.transform);
    }
}
