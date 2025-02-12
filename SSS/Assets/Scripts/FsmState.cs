
using UnityEngine;

public abstract class FsmState
{
    protected readonly Fsm Fsm;
    protected readonly GameObject gameObject;


    public FsmState(Fsm fsm, GameObject GameObject)
    {
        Fsm = fsm;
        gameObject = GameObject;

    }
    
    public virtual void Enter() { }
    public virtual void Exit() { }
    public virtual void Update() { }
    public virtual void FixedUpdate() { }
    public virtual void OnEnable() { }
    public virtual void OnDisable() { }


}
