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
    public event Action onEnemyLandingAnimationFinished;  // Событие для определения, закончилась ли анимация преземления в состоянии FsmStateFallEnemy
    public event Action onAttackAnimationAtRightPointForGetDamage;  // Событие для определения, в нужном ли месте анимация атаки и требуется ли наносить урон.    

    private Enemy enemy;

    // Этот метод будет вызван Animation Event в конце анимации
    public void EnemyLandingAnimationFinished()
    {
        onEnemyLandingAnimationFinished?.Invoke(); // подписываемся, вроде, в объекте состояния FsmStateFallEnemy
        //Debug.Log("EnemyLanding animation finished!");
    }
    // Этот метод будет вызван Animation Event в определённом месте анимации атаки, которое посчитали подходящим для нанесения урона.
    public void AttackAnimationAtRightPointForGetDamage()
    {
        onAttackAnimationAtRightPointForGetDamage?.Invoke(); // подписываемся в состоянии FsmStateMeleeAttacklEnemy
        //Debug.Log("MeleeEnemyAttack animation at right point!");
    }

    public virtual void SomeAnimationWasStarted(string nameStartedAnimation) // Когда какая-то анимация началась. Делаем через event в самой анимации
    {
        enemy.SomeAnimationWasStarted(nameStartedAnimation);
    }

    private void Awake()
    {
        enemy = transform.parent.GetComponent<Enemy>();
    }
}
