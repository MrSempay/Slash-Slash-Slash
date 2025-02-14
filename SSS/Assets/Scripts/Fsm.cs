using System;
using System.Collections;
using System.Collections.Generic;
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

    public void SetState<T>() where T : FsmState
    {
        var type = typeof(T);
        if (StateCurrent?.GetType() == type)
        {
            return;
        }

        if (_states.TryGetValue(type, out var newState))
        {
            StateCurrent?.Exit();
            StateCurrent = newState;
            StateCurrent.Enter();

        }
        else
        {
            Debug.Log("ÀÀÀÀÀÀÀÀ ÑÓÊÀ ¨ÁÀÍÀß ÑÎÑÒÎßÍÈÅ ÇÀÁÛËËËËËËËËËËËËËËËËËËËËËËËË");
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
