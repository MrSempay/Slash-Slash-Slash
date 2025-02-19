using Unity.VisualScripting;
using UnityEngine;
using static FsmStatePlayer;

public class FsmStatePlayer : FsmStateUnit
{
    protected Fsm fsmPlayer;
    protected Player player;
    public delegate void SwipeEnded();
    public event SwipeEnded OnSwipeEnded;

    public FsmStatePlayer(Fsm fsm, GameObject gameObject) : base(fsm, gameObject)
    {
        fsmPlayer = fsm;
        player = gameObject.GetComponent<Player>();
        
    }

    public override void Update()
    {

    }

    public void MakingSwipe()
    {

        IsAtSpecifiedPosition(); // �����, �������� �� ����� �� �������� ����� ���� (�� ���� ��� ������� + ����� ������. ���� ��, �� ���������� ���������� �������� �� � � ����.
        // ��� ��������� ���������
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                player.startTouchPosition = touch.position;
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                player.endTouchPosition = touch.position;
                if (fsmPlayer.StateCurrent.GetType() == typeof(FsmStateWalk)) HandleSwipe(player.endTouchPosition - player.startTouchPosition);
                else fsmPlayer.SetState<FsmStateWalk>();
            }
        }
        // ��� �� (� �������������� ����)
        if (Input.GetMouseButtonDown(0)) // ����� ������ ����� ������ ����
        {
            player.startTouchPosition = player.mainCamera.ScreenToWorldPoint(Input.mousePosition); // ��� ���������� Vector3, ������� ���������� z ���������� � ���� (��� 2D)
            player.startTouchPosition.z = 0;
            player.startPositionPlayerBeforeMoving = player.transform.position; // ���������� ��������� ������� ������, ��� ��� �� �� ����� ������������ ����������, ������� ����� ������
        }
        else if (Input.GetMouseButtonUp(0)) // ����� �������� ����� ������ ����
        {
            player.endTouchPosition = player.mainCamera.ScreenToWorldPoint(Input.mousePosition);
            player.endTouchPosition.z = 0;
            player.differenceXBetweenStartAndEndPositions = player.endTouchPosition.x - player.startTouchPosition.x;
            // ���������, �������� �� ������� ��������� ��� ���������� �����������, ���� �� �� ������ ������� ��� ������ ����� ������� HandleSwipe, ���� ���, �� ��������� �
            // ��������� FsmStateWalk, � ������� � ��� ������� HandleSwipe ���������� ����� �� ���������
            OnSwipeEnded?.Invoke(); // �� ��� ���� ��� ���������� ������ � ��������� FsmStateIdle 
            HandleSwipe(player.endTouchPosition - player.startTouchPosition);
            fsmPlayer.SetState<FsmStateWalk>();
        }

        if (player.lookingRight == player.rb.linearVelocityX < 0)
        {
            ChangeDirectionView();
        }
    }

    // ������������ �����
    public void HandleSwipe(Vector3 swipe)
    {
        // ���������� ����������� ������
        if (Mathf.Abs(swipe.x) > Mathf.Abs(swipe.y))
        {
            // ����� ����� ��� ������
            if (swipe.x > 0)
            {
                MoveRight();
            }
            else
            {

                MoveLeft();
            }
        }
        else
        {
            // ����� ����� (������)
            if (swipe.y > 0 && player.isGrounded)
            {
                Jump();
            }
        }
    }

    private void MoveLeft()
    {
        //rb.AddForce(Vector2.left * speed, ForceMode2D.Impulse);
        //player.rb.linearVelocity = new Vector3(player.differenceXBetweenStartAndEndPositions * player.speed, 0, 0);
        player.rb.linearVelocityX = -player.speed * 10;
    }

    private void MoveRight()
    {
        //rb.AddForce(Vector2.right * speed, ForceMode2D.Impulse); // � ������ ����� ������� � ��������� �����������, �� ����� �� ����� ������������� ��� �������������� �������.
        //player.rb.linearVelocity = new Vector3(player.differenceXBetweenStartAndEndPositions * player.speed, 0, 0);
        player.rb.linearVelocityX = player.speed * 10;
    }

    public void IsAtSpecifiedPosition()
    {
        if (player.transform.position.x >= player.startPositionPlayerBeforeMoving.x + player.differenceXBetweenStartAndEndPositions && player.rb.linearVelocity.x > 0) // ��� ������� ��������
        {
            player.rb.linearVelocity = new Vector3(0, player.rb.linearVelocityY, 0);
        }
        if (player.transform.position.x <= player.startPositionPlayerBeforeMoving.x + player.differenceXBetweenStartAndEndPositions && player.rb.linearVelocity.x < 0) // ��� ������ ��������
        {
            player.rb.linearVelocity = new Vector3(0, player.rb.linearVelocityY, 0);
        }
    }

    void Jump()
    {
        fsmPlayer.SetState<FsmStateJump>();
    }

    void ChangeDirectionView()
    {
        if (player.rb.linearVelocityX != 0) // ���� �� ����� ��������� � ��� ������ �� �������������� ����� ������. ��� ������������, ��� ������������
        {
            
            player.lookingRight = player.rb.linearVelocityX > 0;
            player.selfSprite.flipX = !player.selfSprite.flipX;
            player.attackAreaTransform.localPosition = new Vector3(-1 * player.attackAreaTransform.localPosition.x, player.attackAreaTransform.localPosition.y, player.attackAreaTransform.localPosition.z);
        }
    }


}
