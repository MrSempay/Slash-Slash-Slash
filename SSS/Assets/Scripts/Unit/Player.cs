using System;
using System.Collections.Generic;
using UnityEngine;
public class Player : Unit
{

    [NonSerialized] public Rigidbody2D rb;       // Rigidbody2D ������
    [NonSerialized] public Vector3 startTouchPosition, endTouchPosition = Vector3.zero; // ��� ������������ �������
    [NonSerialized] public Vector3 startPositionPlayerBeforeMoving = Vector3.zero; // ��������� ������� ������ �� ����, ��� �� ����� ��������
    [NonSerialized] public float differenceXBetweenStartAndEndPositions = 0; // ������� �� ���������� � ����� ������� ������ � ��� ����������

    public AttackAreaEnemy attackAreaScript; // ������ ���� ��� �����
    public Transform attackAreaTransform; // ��������� ��������� ���� ��� ����� (����� ��� ����� ����������� �������� ����� ������� ������ (�������������))

    public bool isGrounded = true; // ��������, ��������� �� ����� �� �����
    public Camera mainCamera; // ������ �� ������
    public FloorDetector scriptFloorDetector; // ������ �� ������ ��������� ����



    protected override void Awake()
    {
        // ���� �� ��������� ���� ��� ��� ����, ����� ����� OnEnable ��������� ���������, ����� �� �� �������� ���������������� ���� FSM
        nameOfUnit = "Player";
        base.Awake();
        rb = GetComponent<Rigidbody2D>();
        selfSprite = GetComponent<SpriteRenderer>();

        _fsm = new Fsm();

        _fsm.AddState(new FsmStateIdle(_fsm, gameObject));
        _fsm.AddState(new FsmStateWalk(_fsm, gameObject));
        _fsm.AddState(new FsmStateJump(_fsm, gameObject));
        _fsm.AddState(new FsmStateFall(_fsm, gameObject));


        _fsm.SetState<FsmStateIdle>();
    }
    void Start()
    {

    }


    void Update()
    {

        //Debug.Log(_fsm.StateCurrent);
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
    /*void OnCollisionEnter2D(Collision2D collision)
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
    } */

}
