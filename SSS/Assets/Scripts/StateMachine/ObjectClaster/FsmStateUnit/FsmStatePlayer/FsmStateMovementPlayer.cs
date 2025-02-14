using Unity.VisualScripting;
using UnityEngine;

public class FsmStateMovementPlayer : FsmStatePlayer

{
 
    public FsmStateMovementPlayer(Fsm fsm, GameObject GameObject) : base(fsm, GameObject) {
    }

    public override void Enter()
    {
        Debug.Log($"Move???ment({this.GetType().Name}) state [ENTER]");
    }

    public override void Exit()
    {
        Debug.Log($"Move???ment({this.GetType().Name}) state [Exit]");
    }

    public bool GetFloor(bool onFloor)
    {
        return (onFloor && player.rb.linearVelocity.y <= 0); // ��� ����: hit.collider != null. � ��� �� ����������, ��� �������� �� ����� � ��� ���� �� ������ ����������� (��� ����� �������
                                                       // ��, ��� �������� ������ 0
    }



}