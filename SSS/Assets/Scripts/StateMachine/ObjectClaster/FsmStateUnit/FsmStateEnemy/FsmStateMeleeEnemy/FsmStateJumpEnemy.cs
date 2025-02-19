using Unity.Android.Gradle.Manifest;
using UnityEngine;

public class FsmStateJumpEnemy : FsmStateEnemy
{
    public float timeInJumpState;
    public FsmStateJumpEnemy(Fsm fsm, GameObject gameObject) : base(fsm, gameObject)
    {

    }

    public override void Enter()
    {
        Debug.Log("Walk state [ENTER]");
        enemy.rb.linearVelocity = new Vector2(enemy.rb.linearVelocity.x, 0);
        enemy.rb.AddForce(Vector2.up * enemy.jumpForce, ForceMode2D.Impulse);
        timeInJumpState = 0f;
    }

    public override void Exit()
    {
        Debug.Log("Walk state [EXIT]");
    }

    public override void Update()
    {
        base.Update();
    }

    public override void FixedUpdate()
    {
        timeInJumpState += Time.fixedDeltaTime;
        if (enemy.rb.linearVelocityY < 0) fsmEnemy.SetState<FsmStateFallEnemy>();
        CalculateDrawPathChangeDirectionAndMove(); // ���������� � ������������ ������. ������������, ������������ ����. ������ ����������� ������� ���������, ������� ��� ���������.
                                                    // ������� �� ��� �.
        if (timeInJumpState >= 2.5) fsmEnemy.SetState<FsmStateFallEnemy>(); // ������, ������ ��������, ����� �� ��������� � ����� ��� �������� �����, �� ���� ���������� ������������
                                                                            // �������� ������ 0. � ����� ������ �� �� ������� �� ��������� ������ � ��������. ������ ������� ���� ����
                                                                            // ����� ��� ������������� ������� ���������� � ������ (�� ���� ������������ �������) 
    }

}
