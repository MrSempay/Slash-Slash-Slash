using UnityEngine;


public class FsmStateIdle : FsmStatePlayer

{
    public FsmStateIdle(Fsm fsm, GameObject gameObject) : base(fsm, gameObject)
    {
        OnSwipeEnded += SetStateWalk; // эмулирется в MakingSwipe
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
        MakingSwipe(); // здесь эмулируется событие OnSwipeEnded
        if (!player.isGrounded) fsmPlayer.SetState<FsmStateFall>();
    }

    private void SetStateWalk()
    {
        fsmPlayer.SetState<FsmStateWalk>();
    }

    public override void OnDestroy()
    {
        OnSwipeEnded += SetStateWalk;
    }
}


