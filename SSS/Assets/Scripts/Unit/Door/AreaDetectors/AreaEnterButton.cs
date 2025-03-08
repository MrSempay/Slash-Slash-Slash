using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AreaButtonEnter : MonoBehaviour
{
    public delegate void PlayerEnteredEnterButtonArea(bool wasPlayerEntered, float positionXOfPlayer); // позиция нужна чтоб менять расположение кнопки в зависимости от того, с какой
                                                                                                       // стороны подошел к двери игрок
    public event PlayerEnteredEnterButtonArea onPlayerEnteredEnterButtonArea; // чтоб когда игрок в зону зашел - кнопка входа (в дверь) стала активной
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Подписываемся на событие в состоянии FsmStateIdleEnemy
        if (other.gameObject.CompareTag("Player")) onPlayerEnteredEnterButtonArea?.Invoke(true, other.gameObject.GetComponent<Transform>().position.x);
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        // Подписываемся на событие в состоянии FsmStateIdleEnemy
        if (other.gameObject.CompareTag("Player")) onPlayerEnteredEnterButtonArea?.Invoke(false, other.gameObject.GetComponent<Transform>().position.x);
    }
}

