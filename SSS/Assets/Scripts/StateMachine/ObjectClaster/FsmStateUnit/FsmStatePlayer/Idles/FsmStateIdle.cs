using UnityEngine;


public class FsmStateIdle : FsmStatePlayer

{
    public FsmStateIdle(Fsm fsm, GameObject gameObject) : base(fsm, gameObject)
    {

    }

    public override void Enter()
    {
        Debug.Log("Idle state [ENTER]");
        player.rb.linearVelocity = new Vector3(0, player.rb.linearVelocity.y, 0);
    }

    public override void Exit()
    {
        Debug.Log("Idle state [EXIT]");
    }

    public override void Update()
    {
        MakingSwipe();
    }
}


