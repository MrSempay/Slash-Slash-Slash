using Unity.VisualScripting;
using UnityEngine;

public class FsmStatePlayer : FsmStateUnit
{
    protected Fsm fsmPlayer;
    protected Player player;

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
            if (fsmPlayer.StateCurrent.GetType() == typeof(FsmStateWalk)) HandleSwipe(player.endTouchPosition - player.startTouchPosition);
            else fsmPlayer.SetState<FsmStateWalk>();
        }
    }



    public virtual void HandleSwipe(Vector3 position) {}
}
