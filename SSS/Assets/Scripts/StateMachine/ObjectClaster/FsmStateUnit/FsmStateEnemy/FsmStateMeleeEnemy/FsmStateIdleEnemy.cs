using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class FsmStateIdleEnemy : FsmStateEnemy
{
    
    public FsmStateIdleEnemy(Fsm fsm, GameObject gameObject) : base(fsm, gameObject)
    {

    }

    public override void Enter()
    {
        Debug.Log("Idle Enemy state [ENTER]");

    }

    public override void Exit()
    {
        Debug.Log("Idle Enemy state [EXIT]");
    }

    public override void Update()
    {
        base.Update();
        //if (!enemy.isGrounded) fsmEnemy.SetState<FsmStateFallEnemy>();
        FixingFuckingBuggingRotation();
    }




    public override void OnDestroy()
    {
        base.OnDestroy();
    }
}
