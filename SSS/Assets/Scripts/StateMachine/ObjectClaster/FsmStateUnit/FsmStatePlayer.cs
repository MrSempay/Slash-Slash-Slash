using Unity.VisualScripting;
using UnityEngine;
using static FsmStatePlayer;

public class FsmStatePlayer : FsmStateUnit
{
    protected Fsm fsmPlayer;
    protected Player player;
    public delegate void SwipeEnded();
    public event SwipeEnded OnSwipeEnded;

    public FsmStatePlayer(Fsm fsm, GameObject gameObject) : base(fsm, gameObject)
    {
        fsmPlayer = fsm;
        player = gameObject.GetComponent<Player>();
        
    }

    public override void Update()
    {

    }

    public void MakingSwipe()
    {

        IsAtSpecifiedPosition(); // узнаём, добрался ли игрок до конечной точки пути (по сути его позиция + длина свайпа. Если да, то сбрасываем мгновенную скорость по х в ноль.
        // Для мобильных устройств
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                player.startTouchPosition = touch.position;
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                player.endTouchPosition = touch.position;
                if (fsmPlayer.StateCurrent.GetType() == typeof(FsmStateWalk)) HandleSwipe(player.endTouchPosition - player.startTouchPosition);
                else fsmPlayer.SetState<FsmStateWalk>();
            }
        }
        // Для ПК (с использованием мыши)
        if (Input.GetMouseButtonDown(0)) // Когда нажата левая кнопка мыши
        {
            player.startTouchPosition = player.mainCamera.ScreenToWorldPoint(Input.mousePosition); // тут используем Vector3, поэтому координату z сбрасываем в ноль (для 2D)
            player.startTouchPosition.z = 0;
            player.startPositionPlayerBeforeMoving = player.transform.position; // запоминаем начальную позицию игрока, ибо уже от неё будет рассчитывать расстояние, которое нужно пройти
        }
        else if (Input.GetMouseButtonUp(0)) // Когда отпущена левая кнопка мыши
        {
            player.endTouchPosition = player.mainCamera.ScreenToWorldPoint(Input.mousePosition);
            player.endTouchPosition.z = 0;
            player.differenceXBetweenStartAndEndPositions = player.endTouchPosition.x - player.startTouchPosition.x;
            // Проверяем, является ли текущее состояние уже состоянием перемещения, если да то просто двигаем наш объект через функцию HandleSwipe, если нет, то переходим в
            // состояние FsmStateWalk, в котором у нас функция HandleSwipe вызывается сразу по умолчанию
            OnSwipeEnded?.Invoke(); // на это пока что подпишемся только в состоянии FsmStateIdle 
            HandleSwipe(player.endTouchPosition - player.startTouchPosition);
            fsmPlayer.SetState<FsmStateWalk>();
        }

        if (player.lookingRight == player.rb.linearVelocityX < 0)
        {
            ChangeDirectionView();
        }
    }

    // Обрабатываем свайп
    public void HandleSwipe(Vector3 swipe)
    {
        // Определяем направление свайпа
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
    }

    private void MoveRight()
    {
        //rb.AddForce(Vector2.right * speed, ForceMode2D.Impulse); // в теории можно сделать и импульсом перемещение, но тогда он будет накапливаться при многочисленных свайпах.
        //player.rb.linearVelocity = new Vector3(player.differenceXBetweenStartAndEndPositions * player.speed, 0, 0);
        player.rb.linearVelocityX = player.speed * 10;
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

    void ChangeDirectionView()
    {
        if (player.rb.linearVelocityX != 0) // чтоб во время остановки у нас спрайт не разворачивался влево всегда. Как остановились, так остановились
        {
            
            player.lookingRight = player.rb.linearVelocityX > 0;
            player.selfSprite.flipX = !player.selfSprite.flipX;
            player.attackAreaTransform.localPosition = new Vector3(-1 * player.attackAreaTransform.localPosition.x, player.attackAreaTransform.localPosition.y, player.attackAreaTransform.localPosition.z);
        }
    }


}
