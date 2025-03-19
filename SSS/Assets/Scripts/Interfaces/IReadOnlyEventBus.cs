using UnityEngine;
using UnityEngine.Events;

public interface IReadOnlyEventBus
{
    public UnityEvent<bool> DoorWasDestroyed { get; }
}
