using System.Collections.Generic;
using UnityEngine;

public class FsmStateBuildingNormal : FsmStateBuilding
{
    public FsmStateBuildingNormal(Fsm fsm, GameObject gameObject) : base(fsm, gameObject)
    {

    }

    public override void Enter(Dictionary<string, object> initialConditionsEntering)
    {
        Debug.Log("BuildingNormal state [ENTER]");
        building.selfCollider.enabled = true;
    }

    public override void Exit()
    {
        Debug.Log("BuildingNormal state [EXIT]");
    }

    public override void Update()
    {
        base.Update();

        building.buttonEnter.SetActive(building.IsAroundBuilding);
        if (building.buttonEnterWasPressedToEnter) fsm.SetState<FsmStateBuildingOpened>();

    }
}
