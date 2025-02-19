using UnityEngine;

public class FsmStateBuildingNormal : FsmStateBuilding
{
    public FsmStateBuildingNormal(Fsm fsm, GameObject gameObject) : base(fsm, gameObject)
    {

    }

    public override void Enter()
    {
        Debug.Log("BuildingNormal state [ENTER]");
    }

    public override void Exit()
    {
        Debug.Log("BuildingNormal state [EXIT]");
    }
}
