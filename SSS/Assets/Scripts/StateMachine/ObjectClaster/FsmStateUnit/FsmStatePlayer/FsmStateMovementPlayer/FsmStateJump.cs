using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class FsmStateJump : FsmStateMovementPlayer
 {

    private LayerMask groundLayer;
    private float distanceToGround;
    private bool onFloor;
    public FsmStateJump(Fsm fsm, GameObject GameObject) : base(fsm, GameObject) {
        
    }

    public override void Enter(Dictionary<string, object> initialConditionsEntering)
    {
        Debug.Log("Jump state [ENTER]");
        player.rb.linearVelocity = new Vector2(player.rb.linearVelocity.x, 0);
        player.rb.AddForce(Vector2.up * player.jumpForce, ForceMode2D.Impulse);
        player.animator.Play("PlayerJump");

    }

    public override void Exit()
    {
        Debug.Log("Jump state [EXIT]");
    }


    public override void Update()
    {
        MakingSwipe(); // тут эмулирется сигнал для перехода в FsmStateWalk
        if (player.rb.linearVelocity.y < 0) fsm.SetState<FsmStateFall>();
    }





}