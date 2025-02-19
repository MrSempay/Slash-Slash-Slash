using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FsmStateDoor : FsmState
{
    protected Door door;
    protected Fsm fsmDoor;
    public FsmStateDoor(Fsm fsm, GameObject GameObject) : base(fsm, GameObject)
    {
        door = GameObject.GetComponent<Door>();
        fsmDoor = new Fsm();
    }


}
