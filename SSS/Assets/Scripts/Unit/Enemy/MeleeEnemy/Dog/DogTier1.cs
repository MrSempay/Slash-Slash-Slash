using UnityEngine;

public class DogTier1 : Dog
{
    protected override void Awake()
    {
        nameOfUnit = C.DK.DogTier1;
        base.Awake();
    }
}
