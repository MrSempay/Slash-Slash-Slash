using UnityEngine;

public class FsmStateBuildingOpened : FsmStateBuilding
{
    public FsmStateBuildingOpened(Fsm fsm, GameObject gameObject) : base(fsm, gameObject)
    {

    }

    public override void Enter()
    {
        Debug.Log("Building Open state [ENTER]");
        building.entirePanel.SetActive(true);
    }

    public override void Exit()
    {
        Debug.Log("Building Open state [EXIT]");
        building.entirePanel.SetActive(false);
        building.buttonEnterWasPressedToEnter = false;
    }

    public override void Update()
    {
        base.Update();
        if (!building.buttonEnterWasPressedToEnter) fsm.SetState<FsmStateBuildingNormal>();
        if (!building.IsAroundBuilding) fsm.SetState<FsmStateBuildingNormal>();
    }
}
