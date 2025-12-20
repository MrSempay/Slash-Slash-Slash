using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using static UnityEngine.EventSystems.EventTrigger;
using static UnityEngine.Rendering.DebugUI;

public class Enemy : Unit
{

    public CapsuleCollider2D selfEnemyCollider;
    public IMainTarget currentMainTarget;
    public List<Transform> transformTargets;
    public FuckingBuggingRotationForBody fuck;
    public bool isInstancedByLevel = false; // Флаг, указывающий, что враг был заспавнен скриптом спавна на уроне, а не добавлен на сцену вручную
    public bool isTriggered = false; // Флаг, указывающий, что враг затриггерен
    public TriggerArea triggerAreaScript; // Скрипт зоны для погони (триггер)
    public PitDetector pitDetectorScript; // Скрипт зоны детекции ямок 
    public Transform attackAreaTransform; // Компонент трансформ зоны для атаки (далее при смене направления движения будем позицию менять (отзеркаливать))
    public Transform pitDetectorTransform; // Компонент трансформ зоны для детекции ямок (далее при смене направления движения будем позицию менять (отзеркаливать)) 
    public Transform objForRotate;
    public FloorDetector scriptFloorDetector; // Ссылка на скрипт детектора пола
    public AttackArea attackAreaScript; // Скрипт зоны для атаки
    public List<Unit> listOfUnitsInAttackArea = new List<Unit>();
    public bool isInRazbrestisState = false; // изменять ТОЛЬКО В FsmStateEnemy !!!
    public string TEST_Current_State; 
    public bool TEST_MOD;

    [NonSerialized] public Transform playerTransform; // компонент Transform игрока (чтоб позицию его знать для навигации)
    [NonSerialized] public NavMeshAgent agent; // навигационный агент врага (собственный)
    [NonSerialized] public LineRenderer lineRenderer; // ломанная для визуализации пути
    [NonSerialized] public Vector2 nextPointInPath; // Текущая целевая позиция (вторая точка в пути)
    [NonSerialized] public float arrivalThreshold = 0.3f; // Расстояние, при котором считаем, что достигли цели
    [NonSerialized] public float callDownMeleeAttack;
    [NonSerialized] public int currentCornerIndex; // Индекс текущего угла в пути
    [NonSerialized] public bool isPathValid; // Флаг, указывающий, что путь валиден
    [NonSerialized] public AnimatorClipInfo animatorInfo; // по идее нафиг не нужно. Требуется лишь для отладки
    [NonSerialized] public GameObject temporaryTargetForRazbrestis;


    [SerializeField] private Transform _currentTargetTransform; // временно [SerializeField]
    [SerializeField] private Player player;
    
    private bool _isThroughAble;




    public Transform CurrentTargetTransform // Текущая целевая позиция (вторая точка в пути)
    {
        get { return _currentTargetTransform; }
        set
        {
            _currentTargetTransform = value;
            if (!isInRazbrestisState)
            {
                ////Debug.Log(value);
                ////Debug.Log(value.GetComponent<IMainTarget>());
                currentMainTarget = value.GetComponent<IMainTarget>();
                
            }
        }
    }
    public bool IsThroughAble // Текущая целевая позиция (вторая точка в пути)
    {
        get { return _isThroughAble; }
        set
        {
            _isThroughAble = value;
            ////Debug.Log("И что за хрень?");
            // Включаем игнорирование коллизий
            if (Player.instance != null)
            {
                if (selfEnemyCollider != null)
                {
                    //Debug.Log("И что за хрень?");
                    Physics2D.IgnoreCollision(selfEnemyCollider, Player.instance.selfCollider, value);
                }
            }
        }
    }

    protected override void Awake()
    {
        base.Awake();
        // Сюда мы перенесли этот код для того, чтобы метод OnEnable вызывался корректно, иначе мы не успеваем инициализировать нашу FSM
        GameObject playerObject = GameObject.Find("Player"); //Ищем объект с именем "Player" на сцене
        if (playerObject) playerTransform = playerObject.GetComponent<Transform>();
        selfSprite = fuck.gameObject.GetComponent<SpriteRenderer>();
        agent = GetComponent<NavMeshAgent>();
        lineRenderer = GetComponent<LineRenderer>(); // Получаем компонент LineRenderer

        EventBus.Instance.OnPlayerWasInstanced.AddListener(PlayerWasInstanced);

        Transform transformParametersBars = fuck.transform.Find("ParametersBars");
        if (transformParametersBars != null) parametersBars = transformParametersBars.gameObject;

        attackAreaScript.isPlayerOrAlliesInAttackArea += PlayerOrAlliesInAttackArea; // насколько я понимаю, зона атаки будет детектить и вхождение врагов в эту зону, но суть в том, что
                                                                                     // сами враги подписаны на прослушивание только сигнала вхождения игрока в зону, поэтому враги для врагов
                                                                                     // игнорируются
        triggerAreaScript.OnPlayerEnteredTriggerArea += FollowPlayer;
        // просто заглушка, что если мы сами заспавним врага на уровень, то по умолчанию у него цель будет игрок

        _fsm.AddState(new FsmStateIdleEnemy(_fsm, gameObject));
        _fsm.AddState(new FsmStateWalkEnemy(_fsm, gameObject));
        _fsm.AddState(new FsmStateJumpEnemy(_fsm, gameObject));
        _fsm.AddState(new FsmStateFallEnemy(_fsm, gameObject));
        _fsm.AddState(new FsmStateDiedEnemy(_fsm, gameObject));
    }

    protected override void Start()
    {
        base.Start();
       // перенесли переход в первое состояние из Awake в Start для того, чтобы до входа в первое состояние успели измениться все устанавливаемые параметры при автоматическом
       // добавлении врагов на сцену, ибо Awake выполнялся раньше, чем происходило присвоение корректных значений полям экземпляра скриптом LevelBuildScript и вследствии чего
       // заход в первое состояние выполнялся с не актуальными значениями ряда полей.
        if (!isInstancedByLevel)
        {
            CurrentTargetTransform = playerTransform;
            _fsm.SetState<FsmStateIdleEnemy>();
            return;
        }

        _fsm.SetState<FsmStateWalkEnemy>();
    }

    private void Update()
    {

        ////Debug.Log(_fsm.StateCurrent);
        ////Debug.Log(isGrounded);
        if (areUpdatingFunctionsEnabled)
        {
            _fsm.Update();
        }
    }

    private void FixedUpdate()
    {
        if (areUpdatingFunctionsEnabled)
        {
            _fsm.FixedUpdate();
        }
    }

    public override void Die(Unit unitFromWhoWasGottenDamage = null)
    {
        _fsm.SetState<FsmStateDiedEnemy>();
        base.Die(unitFromWhoWasGottenDamage);
        //Destroy(gameObject); // Уничтожаем объект
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
        CurrentTargetTransform = playerTransform;
        currentMainTarget = CurrentTargetTransform.GetComponent<IMainTarget>();
        temporaryTargetForRazbrestis = null;
        if (!transformTargets.Contains(playerTransform))
        {
            transformTargets.Add(playerTransform);
        }
        GameManager.DestroyObject(temporaryTargetForRazbrestis);
        isInRazbrestisState = false;
    }

    private void PlayerWasInstanced()
    {
        IsThroughAble = IsThroughAble;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            var rb = GetComponent<Rigidbody2D>();
            rb.linearVelocityX = 0; // гасим инерцию
        }
    }

    public override void OnDestroy()
    {
        base.OnDestroy();  
        attackAreaScript.isPlayerOrAlliesInAttackArea -= PlayerOrAlliesInAttackArea;
        triggerAreaScript.OnPlayerEnteredTriggerArea -= FollowPlayer;
        EventBus.Instance.OnPlayerWasInstanced.RemoveListener(PlayerWasInstanced);
    }


 

}
