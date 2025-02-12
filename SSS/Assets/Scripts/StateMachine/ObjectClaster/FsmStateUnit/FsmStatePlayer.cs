using Unity.VisualScripting;
using UnityEngine;

public class FsmStatePlayer : FsmStateUnit
{
    protected Fsm fsmPlayer;
    protected Player player;

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
            if (fsmPlayer.StateCurrent.GetType() == typeof(FsmStateWalk)) HandleSwipe(player.endTouchPosition - player.startTouchPosition);
            else fsmPlayer.SetState<FsmStateWalk>();
        }
    }



    public virtual void HandleSwipe(Vector3 position) {}
}
