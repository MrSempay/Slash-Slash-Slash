using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public class Fsm
{
    public FsmState StateCurrent { get; private set; } // à ìîæíî áûëî áû íå èçìåíèòü íà public, à äîáàâèòü ıòî:
    // public FsmState stateCurrent { get => StateCurrent; set => StateCurrent = value; }

    private Dictionary<Type, FsmState> _states = new Dictionary<Type, FsmState>();

    public void AddState(FsmState state)
    {
        _states.Add(state.GetType(), state);
    }

    public void SetState<T>(Dictionary<string, object> initialConditionsEntering = null) where T : FsmState
    {
        var type = typeof(T);
        if (StateCurrent?.GetType() == type)
        {
            return;
        }
        //if (StateCurrent?.GetType() == typeof(FsmStateTranslatingEquipment) && type == typeof(FsmStateCastUnit))
        //{
        //    return;
        //}

        if (_states.TryGetValue(type, out var newState))
        {
            StateCurrent?.Exit();
            StateCurrent = newState;
            StateCurrent.Enter(initialConditionsEntering);

        }
        else
        {
            Debug.Log("ÀÀÀÀÀÀÀÀ ÑÓÊÀ ¨ÁÀÍÀß ÑÎÑÒÎßÍÈÅ ÇÀÁÛËËËËËËËËËËËËËËËËËËËËËËËË");
        }
    }

    public void SetStateIdle(Unit unit)
    {
        if (unit is Player)
        {
            Debug.Log("Èãğîê");
            unit._fsm.SetState<FsmStateIdle>();
        }
        else if (unit is Enemy)
        {
            Debug.Log("Âğàã");
            unit._fsm.SetState<FsmStateIdleEnemy>();
        } 

    }


    public void Update()
    {
        StateCurrent?.Update();
    }
    public void FixedUpdate()
    {
        StateCurrent?.FixedUpdate();
    }

    public void OnEnable()
    {
         StateCurrent?.OnEnable();
    }
    public void OnDisable()
    {
        StateCurrent?.OnDisable();
    }



}
