using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class FsmStateTranslatingEquipment : FsmStatePlayer
{

    // по сути управляем данным состоянием через свойство player.IsTranslatingEquipment из состояния FsmStateEquipmentSelected снаряжения
    public FsmStateTranslatingEquipment(Fsm fsm, GameObject GameObject) : base(fsm, GameObject)
    {
    }

    public override void Enter(Dictionary<string, object> initialConditionsEntering)
    {
        Debug.Log("Translating Equipment state [ENTER]");

        player.OnTranslateEquipment += SomeTranslateEquipment;
        player.OnSetStateIdle += SetStateIdleCallback;

        string nameAnimation = unit.HasUnitStateAdditional(Unit.UNIT_STATE_ADDITIONAL.Berserker) ?
            C.Animations.PlayerIdle + C.StatesAdditional.Berserker :
            C.Animations.PlayerIdle;
        player.rb.linearVelocityX = 0;
    }

    public override void Exit()
    {
        Debug.Log("Translating Equipment state [EXIT]");

        player.OnTranslateEquipment -= SomeTranslateEquipment;
        player.OnSetStateIdle -= SetStateIdleCallback;
    }

}
