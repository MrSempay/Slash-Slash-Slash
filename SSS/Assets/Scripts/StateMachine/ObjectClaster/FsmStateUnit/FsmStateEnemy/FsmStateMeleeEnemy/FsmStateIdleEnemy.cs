using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class FsmStateIdleEnemy : FsmStateEnemy
{
    
    public FsmStateIdleEnemy(Fsm fsm, GameObject gameObject) : base(fsm, gameObject)
    {

    }

    public override void Enter(Dictionary<string, object> initialConditionsEntering)
    {
        Debug.Log("Idle Enemy state [ENTER]");
        enemy.TEST_Current_State = "Idle";
        enemy.animator.Play("EnemyIdle");
        enemy.rb.linearVelocityX = 0;
    }

    public override void Exit()
    {
        Debug.Log("Idle Enemy state [EXIT]");
        enemy.isTriggered = true; // по идее любой факт выхода из состояния idle будет выставлять факт триггера в true
    }

    public override void Update()
    {
        base.Update();
        //if (!enemy.isGrounded) fsmEnemy.SetState<FsmStateFallEnemy>(); 

        FixingFuckingBuggingRotation();
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();

        if (enemy.isTriggered)
        {
            //enemy._fsm.SetState<FsmStateWalkEnemy>(); // по идее триггеримся только во время входа игрока в зону детекции
        }
    }


    public override void OnDestroy()
    {
        base.OnDestroy();
    }
}
