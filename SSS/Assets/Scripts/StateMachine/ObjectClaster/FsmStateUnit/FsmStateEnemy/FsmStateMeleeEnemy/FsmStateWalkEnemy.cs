using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;
using UnityEngine.AI;

public class FsmStateWalkEnemy : FsmStateEnemy
{
    
    
    public FsmStateWalkEnemy(Fsm fsm, GameObject GameObject) : base(fsm, GameObject)
    {
        // ����� � ��� ��, ��� ������������ �������� ��� �������� ������� ���������
        //Debug.Log(enemy);
        //Debug.Log(enemy.agent);
        //Debug.Log(enemy.agent.updatePosition);
        enemy.agent.updatePosition = false;
        enemy.pitDetectorScript.OnDetectedPit += Jump;

    }


    public override void Enter()
    {
        Debug.Log("Walk state [ENTER]");
        enemy.currentCornerIndex = 1; // �������� �� ������ ����� (������ 1)
        enemy.isPathValid = false; // ���������� ���� ���������� ���� ��� �����
    }

    public override void Exit()
    {
        Debug.Log("Walk state [EXIT]");
    }

    public override void Update()
    {
        base.Update();
        if (enemy.listOfUnitsInAttackArea.Count > 0) fsmEnemy.SetState<FsmStateMeleeAttackEnemy>();

    }

    public override void FixedUpdate()
    {
        if (!enemy.isGrounded) fsmEnemy.SetState<FsmStateFallEnemy>();
        CalculateDrawPathChangeDirectionAndMove();
    }




}
