using UnityEngine;

public interface IMainTarget
{
    public bool WasDestroyed { get; set; }    
    public bool IsMainTarget { get; set; }
    public Transform targetTransform { get; }
    public void SetLikeAMainTarget() { }
}
