using System;
using UnityEngine;
public class Player : Unit
{
    private Fsm _fsm;

    [NonSerialized] public Rigidbody2D rb;       // Rigidbody2D ������
    [NonSerialized] public Transform transform;       // Rigidbody2D ������
    public Vector3 startTouchPosition, endTouchPosition; // ��� ������������ �������
    public Vector3 startPositionPlayerBeforeMoving; // ��������� ������� ������ �� ����, ��� �� ����� ��������
    public float differenceXBetweenStartAndEndPositions; // ������� �� ���������� � ����� ������� ������ � ��� ����������
    public float speed = 2f;       // �������� �����������
    public float jumpForce = 5f;  // ���� ������
    public bool isGrounded = true; // ��������, ��������� �� ����� �� �����
    public Camera mainCamera; // ������ �� ������


    private void Awake()
    {
        // ���� �� ��������� ���� ��� ��� ����, ����� ����� OnEnable ��������� ���������, ����� �� �� �������� ���������������� ���� FSM
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
        // _fsm.OnEnable(); �� ���� ��� �� ����, ��� ��� ���� ���������� � ������� ��������� � ���, ��� ��� ����������� �� Monobehavior
    }
    void OnDisable()
    {
        //_fsm.OnDisable(); �� ���� ��� �� ����, ��� ��� ���� ���������� � ������� ��������� � ���, ��� ��� ����������� �� Monobehavior
    }


    // �� ������ ������ ��� ���� - ����. ��� ���� ����� �������� ������� � ��������� ��� ������ ������� � ������������ ������ - isGrounded ����� true. �� ���� ������� ����� ����������.
    // ����� ��������� ������ ����� ������, �� ���� ��������� ��������� � ������ �� ����. 
    void OnCollisionEnter2D(Collision2D collision)
    {
        // ���������, ���������� �� ����� � �������� � ����� "Ground"
        if (collision.gameObject.CompareTag("Ground"))
        {
            if (!isGrounded)
            {
                isGrounded = true; // �������������, ��� ����� ����� �� �����
                if (rb.linearVelocity.x == 0) _fsm.SetState<FsmStateIdle>();
            }
        }
    }

}
