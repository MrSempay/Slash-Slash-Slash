using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackAreaEnemy : MonoBehaviour
{
    public delegate void UnitEnterAttackArea(bool isUnitInArea, Unit unit); // ������ �������
    public event UnitEnterAttackArea isPlayerOrAlliesInAttackArea;         // ��������(?) �������/�������(?)
    public event UnitEnterAttackArea isEnemyInAttackArea;         // ��������(?) �������/�������(?)


    // ��� �������� ������ �� �������� �������� ���� Enemy (� �� ��� �����), � ��� ������� ������ �������� �������� ���� Player. ��� ����� ���� ������� �� ���������� �� ����� ����.
    // ��� ���������� ����� - ��� �� ���� ������� ����������� �� ����� (�� ���� �� ����� ���� ������������ ������). ����� ��� ���������� ������������ �������� ��� ������ �� ������
    // �������� Enemy �� � GameManager ��� ��������� ����� ������������ ����� ���������� ������ Enemy. ������������� ����������� ���������� �������� (exclude) ��� ���� � ����� �����
    // ������ ������� � ����� ������ ���. ��� Player ����� ��� ������ ����������� ���� Player � exclude, ���, ��� ����� ���� ��������� - ������������ ������� �� ��������� ����.

    // �������� ������������������ ������ ���� ����� ��� ���� ������
    private void OnTriggerEnter2D(Collider2D other)
    {
        // ������������� � FsmStateWalkEnemy 
        if (other.gameObject.CompareTag("Player") || other.gameObject.CompareTag("Allies")) { isPlayerOrAlliesInAttackArea?.Invoke(true, other.gameObject.GetComponent<Unit>()); }
        // �� ������ ������ ��������� � FsmStateWalk
        if (other.gameObject.CompareTag("Enemy")) { isEnemyInAttackArea?.Invoke(true, other.gameObject.GetComponentInParent<Unit>());} // ��� ��� ��������� � ����� ��������� �� �������� �������� Body, � ������ ��� �� 
                                                                                                                                        // ��������, �������� ��������� ��������
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // ������������� � FsmStateWalkEnemy
        if (other.gameObject.CompareTag("Player") || other.gameObject.CompareTag("Allies")) isPlayerOrAlliesInAttackArea?.Invoke(false, other.gameObject.GetComponent<Unit>());
        // �� ������ ������ ��������� � FsmStateWalk
        if (other.gameObject.CompareTag("Enemy")) isEnemyInAttackArea?.Invoke(false, other.gameObject.GetComponentInParent<Unit>());
    }
}
