using Unity.VisualScripting;
using UnityEngine;

public class FsmStateFallEnemy : FsmStateEnemy
{
    public float mass = 1f; // Масса объекта (в кг)
    public float gravity = 9.81f; // Ускорение свободного падения (м/с^2)
    private float velocity = 0f; // Мгновенная скорость
    private float timeElapsed = 0f; // Время, прошедшее с начала падения

    public FsmStateFallEnemy(Fsm fsm, GameObject GameObject) : base(fsm, GameObject)
    {
        // Здесь у нас то, что определяется единожды при создании объекта состояния

        
    }

    public override void Enter()
    {
        Debug.Log("Fall state [ENTER]");
        Reset();
        // animator.Play("fall");
    }

    public override void Exit()
    {
        Debug.Log("Fall state [EXIT]");
    }

    public override void Update()
    {
        base.Update();
    }

    public override void FixedUpdate()
    {
        // Увеличиваем время падения
        timeElapsed += Time.fixedDeltaTime;

        // Рассчитываем мгновенную скорость
        velocity = gravity * timeElapsed;

        enemy.rb.linearVelocityY = -velocity;
        CalculateDrawPathChangeDirectionAndMove();
        if (GetFloor(onFloor))
        {
            if (enemy.rb.linearVelocityX != 0)
            {
                fsm.SetState<FsmStateWalkEnemy>();
                return;
            }
            fsm.SetState<FsmStateIdleEnemy>();
        }

    }
    public void Reset()
    {
        velocity = 0f;
        timeElapsed = 0f;
    }



}