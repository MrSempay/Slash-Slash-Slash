using System;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : Unit
{
    private Fsm _fsm;

    [SerializeField] private Player player;

    [NonSerialized] public Rigidbody2D rb;       // Rigidbody2D кубика
    [NonSerialized] public Transform transformmm;       // Rigidbody2D кубика
    [NonSerialized] public Transform playerTransform; // компонент Transform игрока (чтоб позицию его знать для навигации)
    [NonSerialized] public SpriteRenderer selfSprite; // собственный спрайт
    [NonSerialized] public NavMeshAgent agent; // навигационный агент врага (собственный)
    [NonSerialized] public LineRenderer lineRenderer; // ломанная для визуализации пути
    [NonSerialized] public float speed;       // Скорость перемещения
    [NonSerialized] public Vector2 targetPosition; // Текущая целевая позиция (вторая точка в пути)
    [NonSerialized] public float arrivalThreshold = 0.1f; // Расстояние, при котором считаем, что достигли цели
    [NonSerialized] public int currentCornerIndex; // Индекс текущего угла в пути
    [NonSerialized] public bool isPathValid; // Флаг, указывающий, что путь валиден

    public float jumpForce = 12f;  // Сила прыжка
    public bool isGrounded = true; // Проверка, находится ли кубик на земле
    public TriggerArea triggerAreaScript; // Скрипт зоны для погони (триггер)
    public AttackArea attackAreaScript; // Скрипт зоны для атаки
    public PitDetector pitDetectorScript; // Скрипт зоны детекции ямок 
    public Transform attackAreaTransform; // Компонент трансформ зоны для атаки (далее при смене направления движения будем позицию менять (отзеркаливать))
    public Transform pitDetectorTransform; // Компонент трансформ зоны для детекции ямок (далее при смене направления движения будем позицию менять (отзеркаливать)) 
    public Vector3 _attackAreaLocalPosition;
    public Vector3 _pitDetectorLocalPosition;
    public Transform objForRotate;
    public bool lookingRight = true; // Флаг, нужно ли отзеркаливать положение фрага (будет выполняться отзеркаливание только если направление изменилось, то есть флаг будет true
    public FloorDetector scriptFloorDetector; // Ссылка на скрипт детектора пола



    private void Awake()
    {
        _attackAreaLocalPosition = attackAreaTransform.localPosition;
        _pitDetectorLocalPosition = pitDetectorTransform.localPosition;
        speed = 4f;
        jumpForce = 12f;
        // Сюда мы перенесли этот код для того, чтобы метод OnEnable вызывался корректно, иначе мы не успеваем инициализировать нашу FSM
        GameObject myObject = GameObject.Find("Player"); //Ищем объект с именем "Player" на сцене
        playerTransform = myObject.GetComponent<Transform>();
        selfSprite = GetComponent<SpriteRenderer>();
        agent = GetComponent<NavMeshAgent>();
        lineRenderer = GetComponent<LineRenderer>(); // Получаем компонент LineRenderer
        rb = GetComponent<Rigidbody2D>();
        transformmm = GetComponent<Transform>();

        _fsm = new Fsm();

        _fsm.AddState(new FsmStateIdleEnemy(_fsm, gameObject));
        _fsm.AddState(new FsmStateWalkEnemy(_fsm, gameObject));
        _fsm.AddState(new FsmStateJumpEnemy(_fsm, gameObject));
        _fsm.AddState(new FsmStateFallEnemy(_fsm, gameObject));


        _fsm.SetState<FsmStateIdleEnemy>();
    }
    void Start()
    {

    }

    void Update()
    {
        Debug.Log(_fsm.StateCurrent);
        Debug.Log(isGrounded);
        _fsm.Update();
    }

    private void FixedUpdate()
    {
        _fsm.FixedUpdate();
    }
}
