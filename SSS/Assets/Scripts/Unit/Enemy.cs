using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.EventSystems.EventTrigger;

public class Enemy : Unit
{

    [SerializeField] private Player player;
    [SerializeField] private CircleCollider2D selfEnemyCollider;

    [NonSerialized] public Rigidbody2D rb;       // Rigidbody2D кубика
    [NonSerialized] public Transform playerTransform; // компонент Transform игрока (чтоб позицию его знать для навигации)
    [NonSerialized] public NavMeshAgent agent; // навигационный агент врага (собственный)
    [NonSerialized] public LineRenderer lineRenderer; // ломанная для визуализации пути
    [NonSerialized] public Vector2 nextPointInPath; // Текущая целевая позиция (вторая точка в пути)
    [NonSerialized] public Transform currentTargetTransform; // Текущая целевая позиция (вторая точка в пути)
    [NonSerialized] public float arrivalThreshold = 0.1f; // Расстояние, при котором считаем, что достигли цели
    [NonSerialized] public int currentCornerIndex; // Индекс текущего угла в пути
    [NonSerialized] public bool isPathValid; // Флаг, указывающий, что путь валиден

    public bool isInstancedByLevel = false; // Флаг, указывающий, что враг был заспавнен скриптом спавна на уроне, а не добавлен на сцену вручную
    public bool isGrounded = false; // Проверка, находится ли кубик на земле
    public TriggerArea triggerAreaScript; // Скрипт зоны для погони (триггер)
    public PitDetector pitDetectorScript; // Скрипт зоны детекции ямок 
    public Transform attackAreaTransform; // Компонент трансформ зоны для атаки (далее при смене направления движения будем позицию менять (отзеркаливать))
    public Transform pitDetectorTransform; // Компонент трансформ зоны для детекции ямок (далее при смене направления движения будем позицию менять (отзеркаливать)) 
    public Transform objForRotate;
    public FloorDetector scriptFloorDetector; // Ссылка на скрипт детектора пола
    public string typeOfEnemy; // Ссылка на скрипт детектора пола
    public AttackAreaEnemy attackAreaScript; // Скрипт зоны для атаки
    public List<Unit> listOfUnitsInAttackArea = new List<Unit>();
            




    protected override void Awake()
    {
        base.Awake();
        // Сюда мы перенесли этот код для того, чтобы метод OnEnable вызывался корректно, иначе мы не успеваем инициализировать нашу FSM
        GameObject myObject = GameObject.Find("Player"); //Ищем объект с именем "Player" на сцене
        playerTransform = myObject.GetComponent<Transform>();
        selfSprite = GetComponent<SpriteRenderer>();
        agent = GetComponent<NavMeshAgent>();
        lineRenderer = GetComponent<LineRenderer>(); // Получаем компонент LineRenderer
        rb = GetComponent<Rigidbody2D>();

        // блок ниже - убираем детекцию коллизий между коллайдером каждого экземпляра классов, наследуемых от Enemy и Player
        if (playerTransform != null)
        {
            Collider2D playerCollider = playerTransform.gameObject.GetComponent<BoxCollider2D>();

            if (selfEnemyCollider != null && playerCollider != null)
            {
                Physics2D.IgnoreCollision(selfEnemyCollider, playerCollider);
            }
        }


        attackAreaScript.isPlayerOrAlliesInAttackArea += PlayerOrAlliesInAttackArea;
        triggerAreaScript.OnPlayerEnteredTriggerArea += FollowPlayer;
        // просто заглушка, что если мы сами заспавним врага на уровень, то по умолчанию у него цель будет игрок

        _fsm = new Fsm();


        _fsm.AddState(new FsmStateIdleEnemy(_fsm, gameObject));
        _fsm.AddState(new FsmStateWalkEnemy(_fsm, gameObject));
        _fsm.AddState(new FsmStateJumpEnemy(_fsm, gameObject));
        _fsm.AddState(new FsmStateFallEnemy(_fsm, gameObject));







    }
    protected override void Start()
    {
        base.Start();
       // перенесли переход в первое состояние из Awake в Start для того, чтобы до входа в первое состояние успели измениться все устанавливаемые параметры при автоматическом
       // добавлении врагов на сцену, ибо Awake выполнялся раньше, чем происходило присвоение корректных значений полям экземпляра скриптом LevelBuildScript и вследствии чего
       // заход в первое состояние выполнялся с не актуальными значениями ряда полей.
        if (!isInstancedByLevel)
        {
            currentTargetTransform = playerTransform;
            _fsm.SetState<FsmStateIdleEnemy>();
            return;
        }
        _fsm.SetState<FsmStateWalkEnemy>();
    }

    void Update()
    {

        //Debug.Log(_fsm.StateCurrent);
        //Debug.Log(isGrounded);
        _fsm.Update();
    }

    private void FixedUpdate()
    {
        _fsm.FixedUpdate();
    }

    private void OnDestroy()
    {
        attackAreaScript.isPlayerOrAlliesInAttackArea -= PlayerOrAlliesInAttackArea;
        triggerAreaScript.OnPlayerEnteredTriggerArea -= FollowPlayer;
    }

    private void PlayerOrAlliesInAttackArea(bool isPlayerOrAlliesInArea, Unit alliesOrPlayer)
    {
        //lock (_lock)
        {
            if (isPlayerOrAlliesInArea)
            {
                listOfUnitsInAttackArea.Add(alliesOrPlayer);
                return;
            }
            listOfUnitsInAttackArea.Remove(alliesOrPlayer);
        }
    }

    private void FollowPlayer()
    {
        // Если мы в состоянии покоя, переходим в режим погони. В любом случае ставим целью игрока, предыдущие цели нас более не волнуют
        if (_fsm.StateCurrent?.GetType() == typeof(FsmStateIdleEnemy)) _fsm.SetState<FsmStateWalkEnemy>();
        currentTargetTransform = playerTransform;
    }
}
