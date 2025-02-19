using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorDetector : MonoBehaviour
{
    public delegate bool ObjGetFloor(bool atFloor); // шаблон функции
    public event ObjGetFloor OnObjGetFloor;         // экземляр(?) функции/сигнала(?)

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Подписываемся на событие в состоянии FsmStateFall


        if (other.gameObject.CompareTag("Ground")) OnObjGetFloor?.Invoke(true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // Подписываемся на событие в состоянии FsmStateFall
        if (other.gameObject.CompareTag("Ground")) OnObjGetFloor?.Invoke(false);
    }
}