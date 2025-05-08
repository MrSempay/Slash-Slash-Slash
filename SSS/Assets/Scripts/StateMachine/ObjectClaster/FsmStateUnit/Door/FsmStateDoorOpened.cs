using System.Collections.Generic;
using UnityEngine;

public class FsmStateDoorOpened : FsmStateDoor
{
    public FsmStateDoorOpened(Fsm fsm, GameObject GameObject) : base(fsm, GameObject)
    {

    }

    public override void Enter(Dictionary<string, object> initialConditionsEntering)
    {
        Debug.Log("Opened door state [ENTER]");
        door.selfCollider.enabled = false;
        door.selfSprite.sprite = door.spriteDoorOpened;


    }

    public override void Exit()
    {
        Debug.Log("Opened door state [EXIT]");
    }

    public override void Update()
    {
        base.Update();
    }

}
