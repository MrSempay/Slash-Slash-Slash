using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class FsmStateWalk : FsmStateMovementPlayer
{
    public FsmStateWalk(Fsm fsm, GameObject GameObject) : base(fsm, GameObject)
    {
    }

    public override void Enter(Dictionary<string, object> initialConditionsEntering)
    {
        //Debug.Log("Walk state [ENTER]");

        SubscribeForSignalActivationSomeEquipment();
        player.OnTranslateEquipment += SomeTranslateEquipment;
        player.OnTouchWall += WallWasTouched;
        player.OnSetStateIdle += SetStateIdleCallback;
        OnSwipeEnded += SetStateIdle; // эмулирется в StopHorizontalMovement



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
        //Debug.Log("Walk state [EXIT]");

        UnsubscribeForSignalActivationSomeEquipment();

        player.OnTranslateEquipment -= SomeTranslateEquipment;
        player.OnChangeNearEnemyStatus -= CheckNearEnemyStatus;
        player.OnTouchWall -= WallWasTouched;
        player.OnSetStateIdle -= SetStateIdleCallback;

        OnSwipeEnded -= SetStateIdle; // эмулирется в StopHorizontalMovement


        //AudioManager.Instance.StopSomeTypeSoundOnObject(AudioManager.TYPE_SOUND.Walk, gameObject);
        AudioManager.Instance.StopSomeTypeSoundOnEmitter(AudioManager.TYPE_SOUND.Walk, player.audioEmitter);  
    }

    public override void Update()
    {
        player.transform.rotation = Quaternion.Euler(0, 0, 0);

        base.Update();

        //MakingSwipe();

//        HandleTouches();

//        // Обрабатываем мышь только если тач не активен (иначе возможны дубли)
//#if UNITY_STANDALONE || UNITY_EDITOR
//        HandleMouse();
//#endif

        //if (!player.isGrounded) fsmPlayer.SetState<FsmStateFall>();
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
        //if (player.rb.linearVelocityX == 0) fsmPlayer.SetState<FsmStateIdle>(); 
        ////Debug.Log(desiredVelocityX);
        //if (desiredVelocityX == 0) fsmPlayer.SetState<FsmStateIdle>();
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

    private void SetStateIdle()
    {
        fsmPlayer.SetState<FsmStateIdle>();
    }
    private void WallWasTouched()
    {
        StopHorizontalMovement();
        if (player.isGrounded)
        {
            fsmPlayer.SetState<FsmStateIdle>();
        }
        else
        {
            fsmPlayer.SetState<FsmStateFall>();
        }
    }

    public override void OnDestroy()
    {
        base.OnDestroy();

    }

}