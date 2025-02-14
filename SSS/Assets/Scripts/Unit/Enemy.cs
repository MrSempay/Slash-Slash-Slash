using System;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : Unit
{
    private Fsm _fsm;

    [SerializeField] private Player player;

    [NonSerialized] public Rigidbody2D rb;       // Rigidbody2D ������
    [NonSerialized] public Transform transformmm;       // Rigidbody2D ������
    [NonSerialized] public Transform playerTransform; // ��������� Transform ������ (���� ������� ��� ����� ��� ���������)
    [NonSerialized] public SpriteRenderer selfSprite; // ����������� ������
    [NonSerialized] public NavMeshAgent agent; // ������������� ����� ����� (�����������)
    [NonSerialized] public LineRenderer lineRenderer; // �������� ��� ������������ ����
    [NonSerialized] public float speed;       // �������� �����������
    [NonSerialized] public Vector2 targetPosition; // ������� ������� ������� (������ ����� � ����)
    [NonSerialized] public float arrivalThreshold = 0.1f; // ����������, ��� ������� �������, ��� �������� ����
    [NonSerialized] public int currentCornerIndex; // ������ �������� ���� � ����
    [NonSerialized] public bool isPathValid; // ����, �����������, ��� ���� �������

    public float jumpForce = 12f;  // ���� ������
    public bool isGrounded = true; // ��������, ��������� �� ����� �� �����
    public TriggerArea triggerAreaScript; // ������ ���� ��� ������ (�������)
    public AttackArea attackAreaScript; // ������ ���� ��� �����
    public PitDetector pitDetectorScript; // ������ ���� �������� ���� 
    public Transform attackAreaTransform; // ��������� ��������� ���� ��� ����� (����� ��� ����� ����������� �������� ����� ������� ������ (�������������))
    public Transform pitDetectorTransform; // ��������� ��������� ���� ��� �������� ���� (����� ��� ����� ����������� �������� ����� ������� ������ (�������������)) 
    public Vector3 _attackAreaLocalPosition;
    public Vector3 _pitDetectorLocalPosition;
    public Transform objForRotate;
    public bool lookingRight = true; // ����, ����� �� ������������� ��������� ����� (����� ����������� �������������� ������ ���� ����������� ����������, �� ���� ���� ����� true
    public FloorDetector scriptFloorDetector; // ������ �� ������ ��������� ����



    private void Awake()
    {
        _attackAreaLocalPosition = attackAreaTransform.localPosition;
        _pitDetectorLocalPosition = pitDetectorTransform.localPosition;
        speed = 4f;
        jumpForce = 12f;
        // ���� �� ��������� ���� ��� ��� ����, ����� ����� OnEnable ��������� ���������, ����� �� �� �������� ���������������� ���� FSM
        GameObject myObject = GameObject.Find("Player"); //���� ������ � ������ "Player" �� �����
        playerTransform = myObject.GetComponent<Transform>();
        selfSprite = GetComponent<SpriteRenderer>();
        agent = GetComponent<NavMeshAgent>();
        lineRenderer = GetComponent<LineRenderer>(); // �������� ��������� LineRenderer
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
