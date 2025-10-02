using System.Collections.Generic;
using UnityEngine;


public class FsmStateIdle : FsmStatePlayer

{
    public FsmStateIdle(Fsm fsm, GameObject gameObject) : base(fsm, gameObject)
    {
        OnSwipeEnded += SetStateWalk; // эмулирется в MakingSwipe
    }

    public override void Enter(Dictionary<string, object> initialConditionsEntering)
    {
        Debug.Log("Idle state [ENTER]");

        SubscribeForSignalActivationSomeEquipment();
        player.OnTranslateEquipment += SomeTranslateEquipment;

        player.rb.linearVelocity = new Vector3(0, player.rb.linearVelocity.y, 0);

        string nameAnimation = unit.HasUnitStateAdditional(Unit.UNIT_STATE_ADDITIONAL.Berserker) ?
            C.Animations.PlayerIdle + C.StatesAdditional.Berserker :
            C.Animations.PlayerIdle;

        player.animator.Play(nameAnimation);
    }

    public override void Exit()
    {
        Debug.Log("Idle state [EXIT]");

        UnsubscribeForSignalActivationSomeEquipment();
        player.OnTranslateEquipment -= SomeTranslateEquipment;
    }

    public override void Update()
    {
        MakingSwipe(); // здесь эмулируется событие OnSwipeEnded
        if (!player.isGrounded) fsmPlayer.SetState<FsmStateFall>();
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

    public override void OnDestroy()
    {
        base.OnDestroy();
        OnSwipeEnded -= SetStateWalk;
    }
}


