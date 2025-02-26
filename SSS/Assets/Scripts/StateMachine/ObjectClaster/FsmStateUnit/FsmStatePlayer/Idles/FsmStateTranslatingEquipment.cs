using Unity.VisualScripting;
using UnityEngine;

public class FsmStateTranslatingEquipment : FsmStatePlayer
{

    // по сути управляем данным состоянием через свойство player.IsTranslatingEquipment из состояния FsmStateEquipmentSelected снаряжения
    public FsmStateTranslatingEquipment(Fsm fsm, GameObject GameObject) : base(fsm, GameObject)
    {
    }

    public override void Enter()
    {
        Debug.Log("Translating Equipment state [ENTER]");
    }

    public override void Exit()
    {
        Debug.Log("Translating Equipment state [EXIT]");
    }
}
