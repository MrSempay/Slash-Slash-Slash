using System;
using UnityEngine;
public class Player : Unit
{
    private Fsm _fsm;

    [NonSerialized] public Rigidbody2D rb;       // Rigidbody2D кубика
    [NonSerialized] public Transform transform;       // Rigidbody2D кубика
    public Vector3 startTouchPosition, endTouchPosition; // Для отслеживания свайпов
    public Vector3 startPositionPlayerBeforeMoving; // стартовая позиция игрока до того, как он начал движение
    public float differenceXBetweenStartAndEndPositions; // разница по координате х между началом свайпа и его окончанием
    public float speed = 2f;       // Скорость перемещения
    public float jumpForce = 5f;  // Сила прыжка
    public bool isGrounded = true; // Проверка, находится ли кубик на земле
    public Camera mainCamera; // Ссылка на камеру


    private void Awake()
    {
        // Сюда мы перенесли этот код для того, чтобы метод OnEnable вызывался корректно, иначе мы не успеваем инициализировать нашу FSM
        rb = GetComponent<Rigidbody2D>();
        transform = GetComponent<Transform>();
        _fsm = new Fsm();

        _fsm.AddState(new FsmStateIdle(_fsm, gameObject));
        _fsm.AddState(new FsmStateWalk(_fsm, gameObject));
        _fsm.AddState(new FsmStateJump(_fsm, gameObject));


        _fsm.SetState<FsmStateIdle>();
    }
    void Start()
    {

    }


    void Update()
    {
        Debug.Log(_fsm.StateCurrent);
        _fsm.Update();
    }

    private void FixedUpdate()
    {
        _fsm.FixedUpdate();
    }

   
    void OnEnable()
    {
        // _fsm.OnEnable(); По идее это не надо, так как оное вызывается в классах состояний и так, ибо они наследуются от Monobehavior
    }
    void OnDisable()
    {
        //_fsm.OnDisable(); По идее это не надо, так как оное вызывается в классах состояний и так, ибо они наследуются от Monobehavior
    }


    // на данный момент код ниже - дичь. Ибо если игрок врежется головой в платформу или просто подойдёт к вертикальной стенке - isGrounded будет true. То есть прыгать может бесконечно.
    // Нужно детектить нижнюю часть игрока, то есть отдельный коллайдер и скрипт на него. 
    void OnCollisionEnter2D(Collision2D collision)
    {
        // Проверяем, столкнулся ли кубик с объектом с тегом "Ground"
        if (collision.gameObject.CompareTag("Ground"))
        {
            if (!isGrounded)
            {
                isGrounded = true; // Устанавливаем, что кубик снова на земле
                if (rb.linearVelocity.x == 0) _fsm.SetState<FsmStateIdle>();
            }
        }
    }

}
