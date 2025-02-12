using System.Collections;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;


public class FsmStateWalk : FsmStateMovementPlayer
{
    public FsmStateWalk(Fsm fsm, GameObject GameObject) : base(fsm, GameObject)
    {
    }

    public override void Enter()
    {
        Debug.Log("Walk state [ENTER]");
        HandleSwipe(player.endTouchPosition - player.startTouchPosition);
    }

       public override void Exit()
    {
        Debug.Log("Walk state [EXIT]");
    }

    public override void Update()
    {
        player.transform.rotation = Quaternion.Euler(0, 0, 0);
        IsAtSpecifiedPosition(); // узнаём, добрался ли игрок до конечной точки пути (по сути его позиция + длина свайпа. Если да, то сбрасываем мгновенную скорость по х в ноль.
        MakingSwipe();
        if (player.rb.linearVelocity.x == 0) fsmPlayer.SetState<FsmStateIdle>();
    }

    // Обрабатываем свайп
    void HandleSwipe(Vector3 swipe)
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
                fsmPlayer.SetState<FsmStateJump>();
            }
        }
    }

    void MoveLeft()
    {
        //rb.AddForce(Vector2.left * speed, ForceMode2D.Impulse);
        player.rb.linearVelocity = new Vector3(player.differenceXBetweenStartAndEndPositions * player.speed, 0, 0);
    }

    void MoveRight()
    {
        //rb.AddForce(Vector2.right * speed, ForceMode2D.Impulse); // в теории можно сделать и импульсом перемещение, но тогда он будет накапливаться при многочисленных свайпах.
        player.rb.linearVelocity = new Vector3(player.differenceXBetweenStartAndEndPositions * player.speed, 0, 0);
    }

    void IsAtSpecifiedPosition()
    {
        if (player.transform.position.x >= player.startPositionPlayerBeforeMoving.x + player.differenceXBetweenStartAndEndPositions && player.rb.linearVelocity.x > 0) // для правого движения
        {
            player.rb.linearVelocity = new Vector3(0, player.rb.linearVelocityY, 0);
            Debug.Log("IBOOOO");
        }
        if (player.transform.position.x <= player.startPositionPlayerBeforeMoving.x + player.differenceXBetweenStartAndEndPositions && player.rb.linearVelocity.x < 0) // для левого движения
        {
            player.rb.linearVelocity = new Vector3(0, player.rb.linearVelocityY, 0);
            Debug.Log("Не IBOOOO");
        }
    }

}