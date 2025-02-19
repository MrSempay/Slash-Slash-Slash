using UnityEngine;
using static FsmStatePlayer;

public class FsmStateBuildingDestroyed : FsmStateBuilding
{
    public FsmStateBuildingDestroyed(Fsm fsm, GameObject gameObject) : base(fsm, gameObject)
    {

    }

    public override void Enter()
    {
        Debug.Log("BuildingDestroyed state [ENTER]");
        building.gameObject.SetActive(false);
    }

    public override void Exit()
    {
        Debug.Log("BuildingDestroyed state [EXIT]");
    }
}
