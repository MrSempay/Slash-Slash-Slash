using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FsmStateUnit : FsmState
{
    public readonly Rigidbody2D rigidBody;
    
    public FsmStateUnit(Fsm fsm, GameObject GameObject) : base(fsm, GameObject)
    {
        rigidBody = gameObject.GetComponent<Rigidbody2D>();
    
    }
}
