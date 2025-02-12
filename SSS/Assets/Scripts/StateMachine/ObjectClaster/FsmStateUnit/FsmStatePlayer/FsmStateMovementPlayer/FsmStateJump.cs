using Unity.VisualScripting;
using UnityEngine;

public class FsmStateJump : FsmStateMovementPlayer
 {

    private LayerMask groundLayer;
    private float distanceToGround;
    private bool onFloor;
    public FsmStateJump(Fsm fsm, GameObject GameObject) : base(fsm, GameObject) {
        // ����� � ��� ��, ��� ������������ �������� ��� �������� ������� ���������
        

    }

    public override void Enter()
    {
        Debug.Log("Jump state [ENTER]");
        Jump(); // ������ ��� ����������� ������� � FsmStateIdle (����������� � ������������ ������ FsmStatePlayer

    }

    public override void Exit()
    {
        Debug.Log("Jump state [EXIT]");
    }


    public override void Update()
    {
        MakingSwipe(); // ������ ����� ������� � FsmStateWalk
    }

    void Jump()
    {
        player.rb.AddForce(Vector2.up * player.jumpForce, ForceMode2D.Impulse);
        player.isGrounded = false; // �������������, ��� ����� � �������
    }



}