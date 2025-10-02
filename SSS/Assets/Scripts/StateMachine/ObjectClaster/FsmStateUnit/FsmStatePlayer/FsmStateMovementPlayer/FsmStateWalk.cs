using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static C;
using static UnityEngine.RuleTile.TilingRuleOutput;


public class FsmStateWalk : FsmStateMovementPlayer
{
    public FsmStateWalk(Fsm fsm, GameObject GameObject) : base(fsm, GameObject)
    {
    }

    public override void Enter(Dictionary<string, object> initialConditionsEntering)
    {
        Debug.Log("Walk state [ENTER]");

        SubscribeForSignalActivationSomeEquipment();
        player.OnTranslateEquipment += SomeTranslateEquipment;

        HandleSwipe(player.endTouchPosition - player.startTouchPosition); // по идее любой вход в данное состояние подразумевает, что свайп был сделан в состоянии покоя и мы
                                                                          // далее работает с полями объекта, которые уже были изменены в ходе этого свайпа. Далее в FixedUpdate
                                                                          // мы мониторим факт дальнеших свайпов
        player.OnChangeNearEnemyStatus += CheckNearEnemyStatus;

        string nameAnimation = unit.HasUnitStateAdditional(Unit.UNIT_STATE_ADDITIONAL.Berserker) ?
            C.Animations.PlayerWalkAggressive + C.StatesAdditional.Berserker :
            C.Animations.PlayerWalkAggressive;
        player.animator.Play(nameAnimation);
    }

    public override void Exit()
    {
        Debug.Log("Walk state [EXIT]");

        UnsubscribeForSignalActivationSomeEquipment();
        player.OnTranslateEquipment -= SomeTranslateEquipment;
        player.OnChangeNearEnemyStatus -= CheckNearEnemyStatus;

        AudioManager.Instance.StopSomeTypeSoundOnObject(AudioManager.TYPE_SOUND.Walk, gameObject);
    }

    public override void Update()
    {
        player.transform.rotation = Quaternion.Euler(0, 0, 0);
        
        MakingSwipe();
        if (player.rb.linearVelocity.x == 0) fsmPlayer.SetState<FsmStateIdle>();
        //if (!player.isGrounded) fsmPlayer.SetState<FsmStateFall>();
    }

    private void CheckNearEnemyStatus()
    {
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

    }

}