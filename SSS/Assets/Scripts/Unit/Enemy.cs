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

    [NonSerialized] public Rigidbody2D rb;       // Rigidbody2D ������
    [NonSerialized] public Transform playerTransform; // ��������� Transform ������ (���� ������� ��� ����� ��� ���������)
    [NonSerialized] public NavMeshAgent agent; // ������������� ����� ����� (�����������)
    [NonSerialized] public LineRenderer lineRenderer; // �������� ��� ������������ ����
    [NonSerialized] public Vector2 nextPointInPath; // ������� ������� ������� (������ ����� � ����)
    [NonSerialized] public Transform currentTargetTransform; // ������� ������� ������� (������ ����� � ����)
    [NonSerialized] public float arrivalThreshold = 0.1f; // ����������, ��� ������� �������, ��� �������� ����
    [NonSerialized] public int currentCornerIndex; // ������ �������� ���� � ����
    [NonSerialized] public bool isPathValid; // ����, �����������, ��� ���� �������

    public bool isInstancedByLevel = false; // ����, �����������, ��� ���� ��� ��������� �������� ������ �� �����, � �� �������� �� ����� �������
    public bool isGrounded = false; // ��������, ��������� �� ����� �� �����
    public TriggerArea triggerAreaScript; // ������ ���� ��� ������ (�������)
    public PitDetector pitDetectorScript; // ������ ���� �������� ���� 
    public Transform attackAreaTransform; // ��������� ��������� ���� ��� ����� (����� ��� ����� ����������� �������� ����� ������� ������ (�������������))
    public Transform pitDetectorTransform; // ��������� ��������� ���� ��� �������� ���� (����� ��� ����� ����������� �������� ����� ������� ������ (�������������)) 
    public Transform objForRotate;
    public FloorDetector scriptFloorDetector; // ������ �� ������ ��������� ����
    public string typeOfEnemy; // ������ �� ������ ��������� ����
    public AttackAreaEnemy attackAreaScript; // ������ ���� ��� �����
    public List<Unit> listOfUnitsInAttackArea = new List<Unit>();
            




    protected override void Awake()
    {
        base.Awake();
        // ���� �� ��������� ���� ��� ��� ����, ����� ����� OnEnable ��������� ���������, ����� �� �� �������� ���������������� ���� FSM
        GameObject myObject = GameObject.Find("Player"); //���� ������ � ������ "Player" �� �����
        playerTransform = myObject.GetComponent<Transform>();
        selfSprite = GetComponent<SpriteRenderer>();
        agent = GetComponent<NavMeshAgent>();
        lineRenderer = GetComponent<LineRenderer>(); // �������� ��������� LineRenderer
        rb = GetComponent<Rigidbody2D>();

        // ���� ���� - ������� �������� �������� ����� ����������� ������� ���������� �������, ����������� �� Enemy � Player
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
        // ������ ��������, ��� ���� �� ���� ��������� ����� �� �������, �� �� ��������� � ���� ���� ����� �����

        _fsm = new Fsm();


        _fsm.AddState(new FsmStateIdleEnemy(_fsm, gameObject));
        _fsm.AddState(new FsmStateWalkEnemy(_fsm, gameObject));
        _fsm.AddState(new FsmStateJumpEnemy(_fsm, gameObject));
        _fsm.AddState(new FsmStateFallEnemy(_fsm, gameObject));







    }
    protected override void Start()
    {
        base.Start();
       // ��������� ������� � ������ ��������� �� Awake � Start ��� ����, ����� �� ����� � ������ ��������� ������ ���������� ��� ��������������� ��������� ��� ��������������
       // ���������� ������ �� �����, ��� Awake ���������� ������, ��� ����������� ���������� ���������� �������� ����� ���������� �������� LevelBuildScript � ���������� ����
       // ����� � ������ ��������� ���������� � �� ����������� ���������� ���� �����.
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
        // ���� �� � ��������� �����, ��������� � ����� ������. � ����� ������ ������ ����� ������, ���������� ���� ��� ����� �� �������
        if (_fsm.StateCurrent?.GetType() == typeof(FsmStateIdleEnemy)) _fsm.SetState<FsmStateWalkEnemy>();
        currentTargetTransform = playerTransform;
    }
}
