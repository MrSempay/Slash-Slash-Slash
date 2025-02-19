using UnityEngine;

public class FsmStateDoorClosed : FsmStateDoor
{
    public FsmStateDoorClosed(Fsm fsm, GameObject GameObject) : base(fsm, GameObject)
    {

    }

    public override void Enter()
    {
        Debug.Log("Closed state [ENTER]");


    }

    public override void Exit()
    {
        Debug.Log("Closed state [EXIT]");
    }

    public override void Update()
    {
        base.Update();
        if (door.health <= 0)
        {
            fsmDoor.SetState<FsmStateDoorDestroyed>();
        }
    }

}
