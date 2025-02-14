using Unity.Android.Gradle.Manifest;
using UnityEngine;

public class FsmStateJumpEnemy : FsmStateEnemy
{
    private bool onFloor;
    public FsmStateJumpEnemy(Fsm fsm, GameObject gameObject) : base(fsm, gameObject)
    {

    }

    public override void Enter()
    {
        Debug.Log("Walk state [ENTER]");
        enemy.rb.linearVelocity = new Vector2(enemy.rb.linearVelocity.x, 0);
        enemy.rb.AddForce(Vector2.up * enemy.jumpForce, ForceMode2D.Impulse);
    }

    public override void Exit()
    {
        Debug.Log("Walk state [EXIT]");
    }

    public override void FixedUpdate()
    {
        if (enemy.rb.linearVelocityY < 0) fsmEnemy.SetState<FsmStateFallEnemy>();
        CalculateDrawPathChangeDirectionAndMove(); // реализован в родительском классе. –ассчитывает, отрисовывает путь. ћен€ет направление взгл€да персонажа, смещает все детекторы.
                                                    // двигает по оси х.
    }

    public bool OnFloor(bool atFloor)
    {
        enemy.isGrounded = atFloor;
        onFloor = atFloor;
        return atFloor;
    }
}
