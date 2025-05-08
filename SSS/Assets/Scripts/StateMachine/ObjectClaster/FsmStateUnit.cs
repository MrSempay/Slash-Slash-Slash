using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FsmStateUnit : FsmState
{
    public readonly Rigidbody2D rigidBody;
    public Unit unit;
    
    public FsmStateUnit(Fsm fsm, GameObject GameObject) : base(fsm, GameObject)
    {
        rigidBody = GameObject.GetComponent<Rigidbody2D>();
        unit = GameObject.GetComponent<Unit>();
    }

    public virtual void ChangeDirectionView(bool? lookingRight)
    {
        unit.DirectionViewWasChanged(lookingRight ?? false);
    }

}
