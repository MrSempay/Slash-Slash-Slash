using System.Collections.Generic;
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

    public override void Enter(Dictionary<string, object> initialConditionsEntering)
    {
        Debug.Log("Fall state [ENTER]");

        SubscribeForSignalActivationSomeEquipment();

        string nameAnimation = unit.HasUnitStateAdditional(Unit.UNIT_STATE_ADDITIONAL.Berserker) ?
            C.Animations.PlayerFall + C.StatesAdditional.Berserker :
            C.Animations.PlayerFall;

        player.animator.Play(nameAnimation);

        // animator.Play("fall");
    }

    public override void Exit()
    {
        Debug.Log("Fall state [EXIT]");

        UnsubscribeForSignalActivationSomeEquipment();
    }

    public override void Update()
    {
        MakingSwipe();
        if (GetFloor(onFloor))
        {
            fsm.SetState<FsmStateIdle>();
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