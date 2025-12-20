using System.Collections.Generic;
using UnityEngine;

public class FsmStateBuildingOpened : FsmStateBuilding
{
    public FsmStateBuildingOpened(Fsm fsm, GameObject gameObject) : base(fsm, gameObject)
    {

    }

    public override void Enter(Dictionary<string, object> initialConditionsEntering)
    {
        //Debug.Log("Building Open state [ENTER]");
        //building.entirePanel.SetActive(true); // убрали в рамках расстановки снар€жени€ на полу...
    }

    public override void Exit()
    {
        //Debug.Log("Building Open state [EXIT]");
        //building.entirePanel.SetActive(false); // убрали в рамках расстановки снар€жени€ на полу...
        //building.buttonEnterWasPressedToEnter = false; // убрали в рамках расстановки снар€жени€ на полу...
    }

    public override void Update()
    {
        base.Update();
        // if (!building.buttonEnterWasPressedToEnter) fsm.SetState<FsmStateBuildingNormal>(); // убрали в рамках расстановки снар€жени€ на полу...
        // if (!building.IsAroundBuilding) fsm.SetState<FsmStateBuildingNormal>(); // убрали в рамках расстановки снар€жени€ на полу...
    }
}
