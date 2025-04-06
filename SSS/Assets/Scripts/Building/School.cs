using UnityEngine;

public class School : Building, IMainTarget
{

#region IMainTarget

    private bool _wasDestroyed;

    [SerializeField] private bool _isMainTarget;

    public bool WasDestroyed { get { return _wasDestroyed; } set { _wasDestroyed = value; } }
    public bool IsMainTarget { get { return _isMainTarget; } set { _isMainTarget = value; } }

    public void SetLikeAMainTarget()
    {
        if (IsMainTarget)
        {
            LevelBuilder.instance.listMainTargets.Add(this);
        }
    }

#endregion

    protected override void Awake()
    {
        nameOfObject = "School";
        base.Awake();
    }

    protected override void Start()
    {
        SetLikeAMainTarget();
        base.Start();
    }


}
