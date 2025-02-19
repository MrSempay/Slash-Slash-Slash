using Unity.Android.Gradle.Manifest;
using UnityEngine;

public class FsmStateJumpEnemy : FsmStateEnemy
{
    public float timeInJumpState;
    public FsmStateJumpEnemy(Fsm fsm, GameObject gameObject) : base(fsm, gameObject)
    {

    }

    public override void Enter()
    {
        Debug.Log("Walk state [ENTER]");
        enemy.rb.linearVelocity = new Vector2(enemy.rb.linearVelocity.x, 0);
        enemy.rb.AddForce(Vector2.up * enemy.jumpForce, ForceMode2D.Impulse);
        timeInJumpState = 0f;
    }

    public override void Exit()
    {
        Debug.Log("Walk state [EXIT]");
    }

    public override void Update()
    {
        base.Update();
    }

    public override void FixedUpdate()
    {
        timeInJumpState += Time.fixedDeltaTime;
        if (enemy.rb.linearVelocityY < 0) fsmEnemy.SetState<FsmStateFallEnemy>();
        CalculateDrawPathChangeDirectionAndMove(); // реализован в родительском классе. Рассчитывает, отрисовывает путь. Меняет направление взгляда персонажа, смещает все детекторы.
                                                    // двигает по оси х.
        if (timeInJumpState >= 2.5) fsmEnemy.SetState<FsmStateFallEnemy>(); // КОРОЧЕ, бывают ситуации, когда мы врезаемся в стену при движении вверх, то есть мгновенная вертикальная
                                                                            // скорость больше 0. В таком случае мы не выходим из состояния прыжка и зависаем. Данное улосвие было доба
                                                                            // влено для лимитирования времени нахождения в прыжке (то есть вертикальном подъёме) 
    }

}
