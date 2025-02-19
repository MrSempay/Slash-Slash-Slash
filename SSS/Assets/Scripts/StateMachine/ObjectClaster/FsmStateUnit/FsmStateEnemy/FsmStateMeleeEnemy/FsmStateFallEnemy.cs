using Unity.VisualScripting;
using UnityEngine;

public class FsmStateFallEnemy : FsmStateEnemy
{
    public float mass = 1f; // ����� ������� (� ��)
    public float gravity = 9.81f; // ��������� ���������� ������� (�/�^2)
    private float velocity = 0f; // ���������� ��������
    private float timeElapsed = 0f; // �����, ��������� � ������ �������

    public FsmStateFallEnemy(Fsm fsm, GameObject GameObject) : base(fsm, GameObject)
    {
        // ����� � ��� ��, ��� ������������ �������� ��� �������� ������� ���������

        
    }

    public override void Enter()
    {
        Debug.Log("Fall state [ENTER]");
        Reset();
        // animator.Play("fall");
    }

    public override void Exit()
    {
        Debug.Log("Fall state [EXIT]");
    }

    public override void Update()
    {
        base.Update();
    }

    public override void FixedUpdate()
    {
        // ����������� ����� �������
        timeElapsed += Time.fixedDeltaTime;

        // ������������ ���������� ��������
        velocity = gravity * timeElapsed;

        enemy.rb.linearVelocityY = -velocity;
        CalculateDrawPathChangeDirectionAndMove();
        if (GetFloor(onFloor))
        {
            if (enemy.rb.linearVelocityX != 0)
            {
                fsm.SetState<FsmStateWalkEnemy>();
                return;
            }
            fsm.SetState<FsmStateIdleEnemy>();
        }

    }
    public void Reset()
    {
        velocity = 0f;
        timeElapsed = 0f;
    }



}