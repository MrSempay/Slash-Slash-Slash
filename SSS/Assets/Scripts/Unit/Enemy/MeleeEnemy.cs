using System;
using System.Collections.Generic;
using UnityEngine;

public class MeleeEnemy : Enemy
{



    protected override void Awake()
    {
        nameOfUnit = "MeleeEnemy";
        base.Awake();
        _fsm.AddState(new FsmStateMeleeAttackEnemy(_fsm, gameObject));
    }
    protected override void Start()
    {
        base.Start();
    }

    
}