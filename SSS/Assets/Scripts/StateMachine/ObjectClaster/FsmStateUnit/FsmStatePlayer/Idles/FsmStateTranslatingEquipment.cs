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
        player.animator.Play("PlayerIdle");
        player.rb.linearVelocityX = 0;
    }

    public override void Exit()
    {
        Debug.Log("Translating Equipment state [EXIT]");
    }
}
