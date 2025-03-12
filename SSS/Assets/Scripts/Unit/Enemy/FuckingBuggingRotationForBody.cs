using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using static UnityEngine.EventSystems.EventTrigger;

public class FuckingBuggingRotationForBody : MonoBehaviour
{
    public event Action onEnemyLandingAnimationFinished;       // Событие для изменения количества прокачки в школе 

    // Этот метод будет вызван Animation Event в конце анимации
    public void EnemyLandingAnimationFinished()
    {
        onEnemyLandingAnimationFinished?.Invoke();
        Debug.Log("EnemyLanding animation finished!");
    }
}
