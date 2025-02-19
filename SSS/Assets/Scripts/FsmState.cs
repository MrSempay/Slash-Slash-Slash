
using UnityEngine;

public abstract class FsmState
{
    protected readonly Fsm fsm;
    protected readonly GameObject gameObject;


    public FsmState(Fsm Fsm, GameObject GameObject)
    {
        fsm = Fsm;
        gameObject = GameObject;

    }
    
    public virtual void Enter() { }
    public virtual void Exit() { }
    public virtual void Update() { }
    public virtual void FixedUpdate() { }
    public virtual void OnEnable() { }
    public virtual void OnDisable() { }
    public virtual void OnDestroy() { }


}
