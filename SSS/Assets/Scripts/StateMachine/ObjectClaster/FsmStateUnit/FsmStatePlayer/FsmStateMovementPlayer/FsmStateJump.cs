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
        //Debug.Log("Jump state [ENTER]");

        SubscribeForSignalActivationSomeEquipment();
        player.OnSetStateIdle += SetStateIdleCallback;

        player.rb.linearVelocity = new Vector2(player.rb.linearVelocity.x, 0);
        player.rb.AddForce(Vector2.up * player.jumpForce, ForceMode2D.Impulse);

        string nameAnimation = unit.HasUnitStateAdditional(Unit.UNIT_STATE_ADDITIONAL.Berserker) ?
            C.Animations.PlayerJump + C.StatesAdditional.Berserker :
            C.Animations.PlayerJump;

        player.animator.Play(nameAnimation);

    }

    public override void Exit()
    {
        //Debug.Log("Jump state [EXIT]");

        UnsubscribeForSignalActivationSomeEquipment();
        player.OnSetStateIdle -= SetStateIdleCallback;
    }


    public override void Update()
    {
        base.Update();

        //MakingSwipe(); // тут эмулирется сигнал для перехода в FsmStateWalk

//        HandleTouches();

//        // Обрабатываем мышь только если тач не активен (иначе возможны дубли)
//#if UNITY_STANDALONE || UNITY_EDITOR
//        HandleMouse();
//#endif

        if (player.rb.linearVelocity.y < 0) fsm.SetState<FsmStateFall>();
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
    }



}