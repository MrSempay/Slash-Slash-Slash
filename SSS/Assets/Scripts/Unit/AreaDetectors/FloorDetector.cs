using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorDetector : MonoBehaviour
{
    public delegate bool ObjGetFloor(bool atFloor); // ������ �������
    public event ObjGetFloor OnObjGetFloor;         // ��������(?) �������/�������(?)

    private void OnTriggerEnter2D(Collider2D other)
    {
        // ������������� �� ������� � ��������� FsmStateFall


        if (other.gameObject.CompareTag("Ground")) OnObjGetFloor?.Invoke(true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // ������������� �� ������� � ��������� FsmStateFall
        if (other.gameObject.CompareTag("Ground")) OnObjGetFloor?.Invoke(false);
    }
}