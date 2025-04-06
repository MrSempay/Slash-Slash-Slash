using UnityEngine;
using static FsmStatePlayer;

public class FsmStateBuildingDestroyed : FsmStateBuilding
{
    public FsmStateBuildingDestroyed(Fsm fsm, GameObject gameObject) : base(fsm, gameObject)
    {

    }

    public override void Enter()
    {
        Debug.Log("Building Destroyed state [ENTER]");
        building.gameObject.SetActive(false);

        IMainTarget mainTarget = building as IMainTarget;
        if (mainTarget != null)
        {
            mainTarget.WasDestroyed = true;
        }
    }

    public override void Exit()
    {
        Debug.Log("Building Destroyed state [EXIT]");
    }
}
