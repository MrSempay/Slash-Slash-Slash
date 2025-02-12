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
        IsAtSpecifiedPosition(); // �����, �������� �� ����� �� �������� ����� ���� (�� ���� ��� ������� + ����� ������. ���� ��, �� ���������� ���������� �������� �� � � ����.
        MakingSwipe();
        if (player.rb.linearVelocity.x == 0) fsmPlayer.SetState<FsmStateIdle>();
    }

    // ������������ �����
    void HandleSwipe(Vector3 swipe)
    {
        // ���������� ����������� ������
        if (Mathf.Abs(swipe.x) > Mathf.Abs(swipe.y))
        {
            // ����� ����� ��� ������
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
            // ����� ����� (������)
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
        //rb.AddForce(Vector2.right * speed, ForceMode2D.Impulse); // � ������ ����� ������� � ��������� �����������, �� ����� �� ����� ������������� ��� �������������� �������.
        player.rb.linearVelocity = new Vector3(player.differenceXBetweenStartAndEndPositions * player.speed, 0, 0);
    }

    void IsAtSpecifiedPosition()
    {
        if (player.transform.position.x >= player.startPositionPlayerBeforeMoving.x + player.differenceXBetweenStartAndEndPositions && player.rb.linearVelocity.x > 0) // ��� ������� ��������
        {
            player.rb.linearVelocity = new Vector3(0, player.rb.linearVelocityY, 0);
            Debug.Log("IBOOOO");
        }
        if (player.transform.position.x <= player.startPositionPlayerBeforeMoving.x + player.differenceXBetweenStartAndEndPositions && player.rb.linearVelocity.x < 0) // ��� ������ ��������
        {
            player.rb.linearVelocity = new Vector3(0, player.rb.linearVelocityY, 0);
            Debug.Log("�� IBOOOO");
        }
    }

}