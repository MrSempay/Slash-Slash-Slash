using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class FsmStateDoorDestroyed : FsmStateDoor
{
    public FsmStateDoorDestroyed(Fsm fsm, GameObject GameObject) : base(fsm, GameObject)
    {

    }

    public override void Enter()
    {
        Debug.Log("Destroyed state [ENTER]");
        door.selfCollider.enabled = false;
    }

    public override void Exit()
    {
        Debug.Log("Destroyed state [EXIT]");
    }

}