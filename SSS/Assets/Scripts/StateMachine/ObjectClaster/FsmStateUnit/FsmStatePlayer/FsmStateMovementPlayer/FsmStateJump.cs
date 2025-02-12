using Unity.VisualScripting;
using UnityEngine;

public class FsmStateJump : FsmStateMovementPlayer
 {

    private LayerMask groundLayer;
    private float distanceToGround;
    private bool onFloor;
    public FsmStateJump(Fsm fsm, GameObject GameObject) : base(fsm, GameObject) {
        // Здесь у нас то, что определяется единожды при создании объекта состояния
        

    }

    public override void Enter()
    {
        Debug.Log("Jump state [ENTER]");
        Jump(); // просто при приземлении переход в FsmStateIdle (реализовано в родительском классе FsmStatePlayer

    }

    public override void Exit()
    {
        Debug.Log("Jump state [EXIT]");
    }


    public override void Update()
    {
        MakingSwipe(); // отсюда потом переход в FsmStateWalk
    }

    void Jump()
    {
        player.rb.AddForce(Vector2.up * player.jumpForce, ForceMode2D.Impulse);
        player.isGrounded = false; // Устанавливаем, что кубик в воздухе
    }



}