using Unity.VisualScripting;
using UnityEngine;

public class FsmStateFall : FsmStateMovementPlayer
{

    private LayerMask groundLayer;
    private float distanceToGround;
    private bool onFloor;
    public FsmStateFall(Fsm fsm, GameObject GameObject) : base(fsm, GameObject)
    {
        // Здесь у нас то, что определяется единожды при создании объекта состояния

        player.scriptFloorDetector.OnObjGetFloor += OnFloor;
        //distanceToGround = scriptGameObject.distanceToGround;
        //groundLayer = scriptGameObject.groundLayer;
    }

    public override void Enter()
    {
        Debug.Log("Fall state [ENTER]");

        // animator.Play("fall");
    }

    public override void Exit()
    {
        Debug.Log("Fall state [EXIT]");
    }

    public override void Update()
    {
        MakingSwipe();
        if (GetFloor(onFloor))
        {
            Fsm.SetState<FsmStateIdle>();
        }
    }

    public override void FixedUpdate()
    {
    }


    public override void OnDestroy()
    {
        player.scriptFloorDetector.OnObjGetFloor -= OnFloor;
    }

    bool OnFloor(bool atFloor)
    {
        player.isGrounded = atFloor;
        onFloor = atFloor;
        return atFloor;
    }

}