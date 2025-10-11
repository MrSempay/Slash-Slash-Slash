using System.Collections.Generic;
using UnityEngine;


public class FsmStateIdle : FsmStatePlayer

{
    public FsmStateIdle(Fsm fsm, GameObject gameObject) : base(fsm, gameObject)
    {
    }

    public override void Enter(Dictionary<string, object> initialConditionsEntering)
    {
        Debug.Log("Idle state [ENTER]");

        SubscribeForSignalActivationSomeEquipment();
        player.OnTranslateEquipment += SomeTranslateEquipment;
        player.OnBerserkerStateDeactivated += UpdateIdleAnimation;
        //OnSwipeEnded += SetStateWalk; // эмулирется в MakingSwipe
        //OnSwipeStarted += SetStateWalk; // эмулирется в StopHorizontalMovement

        //player.rb.linearVelocity = new Vector3(0, player.rb.linearVelocity.y, 0);


        string nameAnimation = unit.HasUnitStateAdditional(Unit.UNIT_STATE_ADDITIONAL.Berserker) ?
            C.Animations.PlayerIdle + C.StatesAdditional.Berserker :
            C.Animations.PlayerIdle;

        player.animator.Play(nameAnimation);

        StopHorizontalMovement();
    }

    public override void Exit()
    {
        Debug.Log("Idle state [EXIT]");

        UnsubscribeForSignalActivationSomeEquipment();
        player.OnTranslateEquipment -= SomeTranslateEquipment;
        player.OnBerserkerStateDeactivated -= UpdateIdleAnimation;
        //OnSwipeEnded -= SetStateWalk;
        //OnSwipeStarted -= SetStateWalk; // эмулирется в StopHorizontalMovement
    }

    public override void Update()
    {
        base.Update();
        //MakingSwipe(); // здесь эмулируется событие OnSwipeEnded

//        HandleTouches();

//        // Обрабатываем мышь только если тач не активен (иначе возможны дубли)
//#if UNITY_STANDALONE || UNITY_EDITOR
//        HandleMouse();
//#endif        
        if (!player.isGrounded) fsmPlayer.SetState<FsmStateFall>();
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
    }

    private void SetStateWalk()
    {
        //Debug.Log("Shit ibo");
        if (player.isEnemyNear)
        {
            fsmPlayer.SetState<FsmStateWalkAndAttack>();

        }
        else
        {
            fsmPlayer.SetState<FsmStateWalk>();
        }
    }
    private void UpdateIdleAnimation()
    {
        Debug.Log(" И что за параша?");
        player.animator.Play(C.Animations.PlayerIdle);
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
    }
}


