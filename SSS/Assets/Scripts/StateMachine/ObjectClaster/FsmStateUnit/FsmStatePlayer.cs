using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using static FsmStatePlayer;
using static UnityEngine.Rendering.DebugUI;

public class FsmStatePlayer : FsmStateUnit
{
    private int activeFingerId = -1; // Запоминаем ID активного тача

    protected Fsm fsmPlayer;
    protected Player player;
    public delegate void SwipeEnded();
    public event SwipeEnded OnSwipeEnded;

    public FsmStatePlayer(Fsm fsm, GameObject gameObject) : base(fsm, gameObject)
    {
        fsmPlayer = fsm;
        player = (Player) unit;
        
    }

    public override void Update()
    {

    }

    public void MakingSwipe()
    {

        IsAtSpecifiedPosition(); // узнаём, добрался ли игрок до конечной точки пути (по сути его позиция + длина свайпа. Если да, то сбрасываем мгновенную скорость по х в ноль.
                                 // Для мобильных устройств
                                 //if (Input.touchCount > 0)
                                 //{
                                 //    Touch touch = Input.GetTouch(0);

        //    if (touch.phase == TouchPhase.Began)
        //    {
        //        player.startTouchPosition = player.mainCamera.ScreenToWorldPoint(touch.position);
        //        player.startTouchPosition.z = 0;
        //        player.startPositionPlayerBeforeMoving = player.transform.position; // запоминаем начальную позицию игрока, ибо уже от неё будет рассчитывать расстояние, которое нужно пройти
        //    }
        //    else if (touch.phase == TouchPhase.Ended)
        //    {
        //        if (player.CurrentStamina > 0)
        //        {
        //            player.endTouchPosition = player.mainCamera.ScreenToWorldPoint(touch.position);
        //            player.endTouchPosition.z = 0;
        //            player.differenceXBetweenStartAndEndPositions = player.endTouchPosition.x - player.startTouchPosition.x;
        //            // Проверяем, является ли текущее состояние уже состоянием перемещения, если да то просто двигаем наш объект через функцию HandleSwipe, если нет, то переходим в
        //            // состояние FsmStateWalk, в котором у нас функция HandleSwipe вызывается сразу по умолчанию
        //            if (Mathf.Abs(player.differenceXBetweenStartAndEndPositions) > 0) // чтоб просто при кликаньи на месте выносливость не тратилась
        //            {
        //                HandleSwipe(player.endTouchPosition - player.startTouchPosition);

        //                player.CurrentStamina--;
        //            }
        //        }
        //    }
        //}
        //// Для ПК (с использованием мыши)
        //if (Input.GetMouseButtonDown(0)) // Когда нажата левая кнопка мыши
        //{
        //    player.startTouchPosition = player.mainCamera.ScreenToWorldPoint(Input.mousePosition); // тут используем Vector3, поэтому координату z сбрасываем в ноль (для 2D)
        //    player.startTouchPosition.z = 0;
        //    player.startPositionPlayerBeforeMoving = player.transform.position; // запоминаем начальную позицию игрока, ибо уже от неё будет рассчитывать расстояние, которое нужно пройти
        //}
        //else if (Input.GetMouseButtonUp(0)) // Когда отпущена левая кнопка мыши
        //{
        //    if (player.CurrentStamina > 0)
        //    {
        //        player.endTouchPosition = player.mainCamera.ScreenToWorldPoint(Input.mousePosition);
        //        player.endTouchPosition.z = 0;
        //        player.differenceXBetweenStartAndEndPositions = player.endTouchPosition.x - player.startTouchPosition.x;
        //        // Проверяем, является ли текущее состояние уже состоянием перемещения, если да то просто двигаем наш объект через функцию HandleSwipe, если нет, то переходим в
        //        // состояние FsmStateWalk, в котором у нас функция HandleSwipe вызывается сразу по умолчанию
        //        if (Mathf.Abs(player.differenceXBetweenStartAndEndPositions) > 0) // чтоб просто при кликаньи на месте выносливость не тратилась
        //        {
        //            HandleSwipe(player.endTouchPosition - player.startTouchPosition);

        //            player.CurrentStamina--; 
        //        }
        //    }
        //}

        // === ДЛЯ ТАЧ ===
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            // Проверяем, что касание не на UI
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                return;

            // Начало касания
            if (touch.phase == TouchPhase.Began)
            {
                // Запоминаем fingerId активного касания
                activeFingerId = touch.fingerId;

                player.startTouchPosition = player.mainCamera.ScreenToWorldPoint(touch.position);
                player.startTouchPosition.z = 0;
                player.startPositionPlayerBeforeMoving = player.transform.position; // запоминаем начальную позицию игрока
            }
            // Завершение или отмена касания
            else if (touch.fingerId == activeFingerId &&
                    (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled))
            {
                if (player.CurrentStamina > 0)
                {
                    player.endTouchPosition = player.mainCamera.ScreenToWorldPoint(touch.position);
                    player.endTouchPosition.z = 0;
                    player.differenceXBetweenStartAndEndPositions = player.endTouchPosition.x - player.startTouchPosition.x;

                    if (Mathf.Abs(player.differenceXBetweenStartAndEndPositions) > 0)
                    {
                        HandleSwipe(player.endTouchPosition - player.startTouchPosition);
                        player.CurrentStamina--;
                    }
                }

                // Сбрасываем activeFingerId, чтобы можно было начать новое касание
                activeFingerId = -1;
            }
        }

        // === ДЛЯ ПК (мышь) ===
        if (Input.GetMouseButtonDown(0))
        {
            // Проверяем, что курсор не над UI
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            player.startTouchPosition = player.mainCamera.ScreenToWorldPoint(Input.mousePosition);
            player.startTouchPosition.z = 0;
            player.startPositionPlayerBeforeMoving = player.transform.position;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            if (player.CurrentStamina > 0)
            {
                player.endTouchPosition = player.mainCamera.ScreenToWorldPoint(Input.mousePosition);
                player.endTouchPosition.z = 0;
                player.differenceXBetweenStartAndEndPositions = player.endTouchPosition.x - player.startTouchPosition.x;

                if (Mathf.Abs(player.differenceXBetweenStartAndEndPositions) > 0)
                {
                    HandleSwipe(player.endTouchPosition - player.startTouchPosition);
                    player.CurrentStamina--;
                }
            }
        }

        if ((player.lookingRight == player.rb.linearVelocityX < 0) && Mathf.Abs(player.rb.linearVelocityX) > 0.01) // добавили некоторый treshhold для нивелирования небольшого заноса от CompositeCollider
        {
            ChangeDirectionView(null);
        }
    }

    // Обрабатываем свайп
    public void HandleSwipe(Vector3 swipe)
    {
        player.wasEnemyDamagedByLastSwipe = false;

        player.OnMove();

        ScoreManager.Instance.UpActionCombo();

        if (Mathf.Abs(swipe.x) > Mathf.Abs(swipe.y))
        {
            // Свайп влево или вправо
            if (swipe.x > 0)
            {
                MoveRight();
            }
            else
            {

                MoveLeft();
            }
        }
        else
        {
            // Свайп вверх (прыжок)
            if (swipe.y > 0 && player.isGrounded)
            {
                Jump();
            }
        }
    }

    private void MoveLeft()
    {
        //rb.AddForce(Vector2.left * speed, ForceMode2D.Impulse);
        //player.rb.linearVelocity = new Vector3(player.differenceXBetweenStartAndEndPositions * player.speed, 0, 0);
        player.rb.linearVelocityX = -player.speed * 10;
        OnSwipeEnded?.Invoke(); // на это пока что подпишемся только в состоянии FsmStateIdle 
        if (player.isEnemyNear)
        {
            fsmPlayer.SetState<FsmStateWalkAndAttack>();

        }
        else
        {
            fsmPlayer.SetState<FsmStateWalk>();
        }
    }

    private void MoveRight()
    {
        //rb.AddForce(Vector2.right * speed, ForceMode2D.Impulse); // в теории можно сделать и импульсом перемещение, но тогда он будет накапливаться при многочисленных свайпах.
        //player.rb.linearVelocity = new Vector3(player.differenceXBetweenStartAndEndPositions * player.speed, 0, 0);
        player.rb.linearVelocityX = player.speed * 10;
        OnSwipeEnded?.Invoke(); // на это пока что подпишемся только в состоянии FsmStateIdle 
        if (player.isEnemyNear)
        {
            fsmPlayer.SetState<FsmStateWalkAndAttack>();

        }
        else
        {
            fsmPlayer.SetState<FsmStateWalk>();
        }
    }

    public void IsAtSpecifiedPosition()
    {
        if (player.transform.position.x >= player.startPositionPlayerBeforeMoving.x + player.differenceXBetweenStartAndEndPositions && player.rb.linearVelocity.x > 0) // для правого движения
        {
            player.rb.linearVelocity = new Vector3(0, player.rb.linearVelocityY, 0);
        }
        if (player.transform.position.x <= player.startPositionPlayerBeforeMoving.x + player.differenceXBetweenStartAndEndPositions && player.rb.linearVelocity.x < 0) // для левого движения
        {
            player.rb.linearVelocity = new Vector3(0, player.rb.linearVelocityY, 0);
        }
    }

    void Jump()
    {
        fsmPlayer.SetState<FsmStateJump>();
    } 

    public void SubscribeForSignalActivationSomeEquipment()
    {
        player.OnSomeEquipmentShouldBeActivate += SomeEquipmentShouldBeActivate;
    }

    public void UnsubscribeForSignalActivationSomeEquipment()
    {
        player.OnSomeEquipmentShouldBeActivate -= SomeEquipmentShouldBeActivate;
    }

    private void SomeEquipmentShouldBeActivate(Equipment equipment)
    {
        player._fsm.SetState<FsmStateCastUnit>(new Dictionary<string, object> { { "equipmentWhatWasPressed", equipment } });
    }

    public void SomeTranslateEquipment(bool isTranslating) // если честно, совершенно не ясно, на кой чёрт нам нужно вот это состояние: FsmStateTranslatingEquipment. Если мы хотим
                                                           // хотим привязать его к перетаскиванию снаряжение, то, например, во время анимации каста, нам нельзя перетаскивать снаряжение,
                                                           // иначе состояние каста прервётся. Есть смысл оставлять просто текущее состояние при перемещении снаряжения. Или пусть тогда
                                                           // при клацании на снаряжение оно не переходит сразу в состояние Selected, а, если текущее состояние позволяет, игрок переходит
                                                           // в FsmStateTranslatingEquipment и только тогда уже состояние снаряжения переводится в Selected
    {
        if (isTranslating) player._fsm.SetState<FsmStateTranslatingEquipment>();
        else player._fsm.SetState<FsmStateIdle>();
    }

    public override void ChangeDirectionView(bool? lookingRight)
    {

        if (player.rb.linearVelocityX != 0) // чтоб во время остановки у нас спрайт не разворачивался влево всегда. Как остановились, так остановились
        {
            
            player.lookingRight = player.rb.linearVelocityX > 0;

            base.ChangeDirectionView(player.lookingRight);

            player.selfSprite.flipX = !player.selfSprite.flipX;
            player.attackAreaTransform.localPosition = new Vector3(-1 * player.attackAreaTransform.localPosition.x, player.attackAreaTransform.localPosition.y, player.attackAreaTransform.localPosition.z);
        }
    }

}
