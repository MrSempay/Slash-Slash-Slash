using System.Collections;
using System.IO;
using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.RuleTile.TilingRuleOutput;
using System.Text.RegularExpressions;

public class FsmStateEnemy : FsmStateUnit
{

    private float rotationThreshold = 1.0f; // ���������� ���������� �� �������� ����
    private Vector3 baseUpOffsetForStartPointFindingPath = new Vector3 (0, 0.5f, 0); // ��������������� "���������� �����" �������� � ��������� ����� ��� ������ ���� (���� ��� ����� �� ��������������
                                                               // ���� ��� �������� ��������
    private Coroutine constraintTimeFlipXCoroutine;
    private bool canFlipByTimeDeley;

    protected readonly object _lock = new object();

    public Fsm fsmEnemy;
    public Enemy enemy;
    public MeleeEnemy enemy1;
    public NavMeshPath path; // ���������� ��� �������� ����
    public bool onFloor;


    public FsmStateEnemy(Fsm fsm, GameObject gameObject) : base(fsm, gameObject)
    {
        fsmEnemy = fsm;
        enemy = gameObject.GetComponent<Enemy>();

        path = new NavMeshPath();
        enemy.scriptFloorDetector.OnObjGetFloor += OnFloor;
    }

    public override void Update()
    {

    }

    //������������, ������������ ����. ������ ����������� ������� ���������, ������� ��� ���������, ������� �� ��� �.
    public void CalculateDrawPathChangeDirectionAndMove()
    {
        
        FixingFuckingBuggingRotation();
        if (constraintTimeFlipXCoroutine == null)
        {
            constraintTimeFlipXCoroutine = CoroutineManager.Instance.StartManagedCoroutine(this.gameObject, ConstraintTimeFlipX());
        }
        //Debug.Log(enemy.currentTargetTransform.position);
        //Debug.Log(enemy.currentTargetTransform);
            // ��������� ����, ���� ���������� (��������, ���� ����� ������������). �� ���� ���� ��������� ��� � y-������������, �� �� �����...
        if (Mathf.Abs(enemy.transform.position.x - enemy.currentTargetTransform.position.x) > 0.1f)
        {
            //Debug.Log("Emmm???1");
            enemy.isPathValid = NavMesh.CalculatePath(enemy.transform.position + baseUpOffsetForStartPointFindingPath, enemy.currentTargetTransform.position + baseUpOffsetForStartPointFindingPath, NavMesh.AllAreas, path);
            //Debug.Log(path.corners.Length);
            //enemy.agent.destination = enemy.playerTransform.position;
            //path = enemy.agent.path;
            if (path.corners.Length >= 1) enemy.isPathValid = true;

            if (enemy.isPathValid)
            {
                enemy.currentCornerIndex = 1; // ���������� ������ ��� ��������� ����
            }
        }
        //Debug.Log("Emmm???2");
        //��������, ���� ���� ���. ��������, ����� ����
        if (!enemy.isPathValid || path.corners.Length < 2)
        {
            //������������� ��������, ��������
            enemy.rb.linearVelocityX = 0;
            return; // ������ �� ������, ���� ���� �� ������� ��� ������� ��������
        }
        //Debug.Log("Emmm???3");
        // �������� ������� ���� (������ �����)
        if (enemy.currentCornerIndex < path.corners.Length)
        {
            //Debug.Log("Emmm???4");
            enemy.nextPointInPath = path.corners[enemy.currentCornerIndex];
            //Debug.Log(StaticClassForAdditionalFunctions.GetAngle((Vector2)path.corners[0], (Vector2)path.corners[enemy.currentCornerIndex]));
            //Debug.Log(enemy.isGrounded);
            //Debug.Log(StaticClassForAdditionalFunctions.GetAngle((Vector2)path.corners[enemy.currentCornerIndex - 1], (Vector2)path.corners[enemy.currentCornerIndex]));
            if (StaticClassForAdditionalFunctions.GetAngle((Vector2)path.corners[0], (Vector2)path.corners[enemy.currentCornerIndex]) >= 75f &&
                StaticClassForAdditionalFunctions.GetAngle((Vector2)path.corners[0], (Vector2)path.corners[enemy.currentCornerIndex]) <= 165f) Jump(false);
        }
        else
        {
            //Debug.Log("Emmm???5");
            //���� ��� ���� ��������, �� ���������������
            enemy.rb.linearVelocityX = 0;
            return;
        }
        //Debug.Log("Emmm???6");

        // ����������� � ������� �������. C������ �������� ����������� (���� ������, ���� ��������������) � ������ ����� ������������� ������
        //Debug.Log("Emmm???3");
        MoveTowardsTarget();
        //���������� ������� � �������
        if (enemy.lookingRight == enemy.nextPointInPath.x < enemy.transform.position.x)
        {
            ChangeDirectionView();
        }

        //��������� ����
        DrawPath();
    }


    // ������� ��������� �� ����
    void MoveTowardsTarget()
    {
        //Debug.Log("Emmm???4");
        float direction = Mathf.Sign(enemy.nextPointInPath.x - enemy.transform.position.x);
        enemy.rb.linearVelocityX = direction * enemy.speed;
        // �������� ���������� ����
        if (Mathf.Abs(enemy.transform.position.x - enemy.nextPointInPath.x) <= enemy.arrivalThreshold)
        {
            //Debug.Log("Emmm???5");
            // ��������� � ��������� �����
            enemy.currentCornerIndex++;
            if (enemy.currentCornerIndex >= path.corners.Length)
            {
                //Debug.Log("Emmm???6");
                // �������� �������� �����
                enemy.rb.linearVelocityX = 0; //���������������
                enemy.isPathValid = false; //���������� ����
                return;
            }
        }
    }

    // ������������ ����
    void DrawPath()
    {
        if (enemy.lineRenderer == null || path == null) return;
        // ������������� ���������� ����� LineRenderer
        enemy.lineRenderer.positionCount = path.corners.Length;
        // ������������� ������� ����� LineRenderer
        enemy.lineRenderer.SetPositions(path.corners);
    }

    // ������������� ��������� � ��� ��������� ��� ��������
    void ChangeDirectionView()
    {
        if (canFlipByTimeDeley)
        {
            enemy.lookingRight = enemy.nextPointInPath.x > enemy.transform.position.x;
            enemy.selfSprite.flipX = !enemy.selfSprite.flipX;
            enemy.attackAreaTransform.localPosition = new Vector3(-1 * enemy.attackAreaTransform.localPosition.x, enemy.attackAreaTransform.localPosition.y, enemy.attackAreaTransform.localPosition.z);
            enemy.pitDetectorTransform.localPosition = new Vector3(-1 * enemy.pitDetectorTransform.localPosition.x, enemy.pitDetectorTransform.localPosition.y, enemy.pitDetectorTransform.localPosition.z);
            canFlipByTimeDeley = false;
        }
    }

    // �������. ���������� ���� ���� ���� ������� ������ ���� ������ 45 �������� ��� ���� ���������� ��� (����������� ������ �� PitDetector)
    protected void Jump(bool jumpOfPit)
    {
        if (enemy.isGrounded) { // ���� �� �� �����
            if (path.corners.Length > 1 && enemy.currentCornerIndex < path.corners.Length) { // ��� �� ������ ����� ��� ������-�� ��� ����� 0 ������ 0_0
                //Debug.Log(enemy.currentCornerIndex - 1);
                //Debug.Log(enemy.currentCornerIndex);
                //Debug.Log(path.corners.Length);
                if (!(StaticClassForAdditionalFunctions.GetAngle((Vector2)path.corners[enemy.currentCornerIndex - 1], (Vector2)path.corners[enemy.currentCornerIndex]) <= -5f
                && StaticClassForAdditionalFunctions.GetAngle((Vector2)path.corners[enemy.currentCornerIndex - 1], (Vector2)path.corners[enemy.currentCornerIndex]) >= -175f))
                { // ���� ��� �� ����
                    if (path.corners.Length != 2) fsmEnemy.SetState<FsmStateJumpEnemy>(); //���� Player �� � ������ ��������� (���� �� ���� �� ������ ���� �� �������, ����� ���������� ����� ���� = 2)
                    if (jumpOfPit) fsmEnemy.SetState<FsmStateJumpEnemy>(); // ���������� ������ ��������� ����� � ��� ������, ���� ���� ������������ ����� ��������
                }

            }     
        }
    }

    private IEnumerator ConstraintTimeFlipX()
    {
        while (true)
        {
            canFlipByTimeDeley = true;
            yield return new WaitForSeconds(0.5f);
        }
    }

    protected void FixingFuckingBuggingRotation()
    {
        float xRotation = enemy.transform.rotation.eulerAngles.x;
        //Debug.Log(xRotation);

        // ����������� ���� � ��������� 0-360
        xRotation = Mathf.Repeat(xRotation, 360.0f);

        // �������� � ��������� -180 - 180
        if (xRotation > 180.0f)
            xRotation -= 360.0f;

        if (Mathf.Abs(xRotation - 90f) < rotationThreshold)
        {
            enemy.objForRotate.transform.localRotation = Quaternion.Euler(-90, 0, 0);
            //Debug.Log("������������ �������� ������ �� -90 ��������");
        }
        else if (Mathf.Abs(xRotation + 90f) < rotationThreshold)
        {
            enemy.objForRotate.transform.localRotation = Quaternion.Euler(90, 0, 0);
            //Debug.Log("������������ �������� ������ �� 90 ��������");
        }
        else if (Mathf.Abs(xRotation - 0f) < rotationThreshold)
        {
            enemy.objForRotate.transform.localRotation = Quaternion.Euler(0, 0, 0);
            //Debug.Log("������������� ��������� ������� ������� �������");
        }

        // �� ����� �������� ������� �������� �����!
        // enemy.transform.rotation = Quaternion.Euler(0, 0, 0);
    }

    public bool GetFloor(bool onFloor)
    {
        return (onFloor && enemy.rb.linearVelocity.y <= 0); 
    }

    protected bool OnFloor(bool atFloor)
    {
        enemy.isGrounded = atFloor;
        onFloor = atFloor;
        return atFloor;
    }

    public override void OnDestroy()
    {
        enemy.scriptFloorDetector.OnObjGetFloor -= OnFloor;

    }





}
