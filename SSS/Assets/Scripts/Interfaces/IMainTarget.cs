using UnityEngine;

public interface IMainTarget
{
    public bool WasDestroyed { get; set; }    
    public bool IsMainTarget { get; set; }
    public void SetLikeAMainTarget() { }
}
