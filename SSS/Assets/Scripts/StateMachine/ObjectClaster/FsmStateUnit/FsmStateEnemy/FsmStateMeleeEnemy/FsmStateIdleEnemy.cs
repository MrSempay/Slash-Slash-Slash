using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class FsmStateIdleEnemy : FsmStateEnemy
{
    
    public FsmStateIdleEnemy(Fsm fsm, GameObject gameObject) : base(fsm, gameObject)
    {
        enemy.triggerAreaScript.OnPlayerEnteredTriggerArea += FollowPlayer;

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
        if (!enemy.isGrounded) fsmEnemy.SetState<FsmStateFallEnemy>();
        FixingFuckingBuggingRotation();
    }

    private void FollowPlayer()
    {
        fsmEnemy.SetState<FsmStateWalkEnemy>();
    }



    public override void OnDestroy()
    {
        enemy.triggerAreaScript.OnPlayerEnteredTriggerArea -= FollowPlayer;
        enemy.scriptFloorDetector.OnObjGetFloor -= OnFloor;
    }
}
