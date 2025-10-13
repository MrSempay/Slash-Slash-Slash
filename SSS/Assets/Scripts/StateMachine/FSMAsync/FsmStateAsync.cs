// AsyncStateBase.cs
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public abstract class AsyncStateBase
{
    protected internal readonly FsmAsync fsm;
    protected internal readonly GameObject owner;

    public AsyncStateBase(FsmAsync fsm, GameObject owner) { this.fsm = fsm; this.owner = owner; }

    public virtual void Enter(Dictionary<string, object> args) { }
    public virtual void Exit() { }
    public abstract Task RunAsync(CancellationToken ct);
}
