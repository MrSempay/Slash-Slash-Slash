using System.Collections.Generic;
using UnityEngine;

public class FsmStateStuneUnit : FsmStateUnit
{
 
    public FsmStateStuneUnit(Fsm fsm, GameObject gameObject) : base(fsm, gameObject)
    {

    }

    public override void Enter(Dictionary<string, object> initialConditionsEntering)
    {
        Debug.Log("Stune state [ENTER]");
        unit.areUpdatingFunctionsEnabled = false;
        rigidBody.linearVelocityX = 0;
        if (unit.animator != null)
        {
            if (StaticClassForAdditionalFunctions.AnimationExists(C.Other.Stune, unit.animator))
            {
                unit.animator.Play(C.Other.Stune); // Воспроизводим анимацию
            }
            else
            {
                Debug.Log("--- НЕТ АНИМАЦИИ СТАНА !!! ---" +
                          "--- Используем ПРЕДЫДУЩУЮ анимацию! ---");
            }
        }
    }

    public override void Exit()
    {
        Debug.Log("Died state [EXIT]");
        unit.areUpdatingFunctionsEnabled = true;
    }


    public override void OnDestroy()
    {
        base.OnDestroy();
    }
}
