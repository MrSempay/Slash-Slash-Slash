using System.Collections.Generic;
using System.Reflection;
using System;
using UnityEngine;

public class Door : Unit
{
    

    public float health;
    public BoxCollider2D selfCollider;

    protected override void Awake()
    {
        nameOfUnit = "Door";
        base.Awake();
        selfCollider = GetComponent<BoxCollider2D>();

        _fsm = new Fsm();

        _fsm.AddState(new FsmStateDoorClosed(_fsm, gameObject));
        _fsm.AddState(new FsmStateDoorDestroyed(_fsm, gameObject));

        _fsm.SetState<FsmStateDoorClosed>();
    }
}
