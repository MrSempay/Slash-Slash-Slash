using Unity.VisualScripting;
using UnityEngine;

public class FsmStateBuilding : FsmState
{
    protected Building building;

    public FsmStateBuilding(Fsm fsm, GameObject gameObject) : base(fsm, gameObject)
    {
        building = gameObject.GetComponent<Building>();
    }
}