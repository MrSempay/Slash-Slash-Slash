using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerArea : MonoBehaviour
{
    public delegate void PlayerEnteredTriggerArea();
    public event PlayerEnteredTriggerArea OnPlayerEnteredTriggerArea;
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Подписываемся на событие в состоянии FsmStateIdleEnemy
        if (other.gameObject.CompareTag("Player")) OnPlayerEnteredTriggerArea?.Invoke();
    }
}

