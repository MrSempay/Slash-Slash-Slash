using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AreaDoorClose : MonoBehaviour
{
    public delegate void PlayerEnterAttackArea(Transform transform);
    public event PlayerEnterAttackArea OnPlayerInDoorCloseArea;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // подписываемся в Enemy 
        if (other.gameObject.CompareTag("Player")) { OnPlayerInDoorCloseArea?.Invoke(transform); }

    }
}
