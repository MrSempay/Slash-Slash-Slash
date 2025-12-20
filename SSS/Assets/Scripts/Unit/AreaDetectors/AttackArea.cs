using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackArea : MonoBehaviour
{
    public delegate void AllieEnterAttackArea(bool isUnitInArea, Unit unit); // шаблон функции
    public delegate void EnemyEnterAttackArea(bool isUnitInArea, Enemy unit); // шаблон функции
    public event AllieEnterAttackArea isPlayerOrAlliesInAttackArea;         // экземляр(?) функции/сигнала(?)
    public event EnemyEnterAttackArea isEnemyInAttackArea;         // экземляр(?) функции/сигнала(?)


    // Для префабов врагов мы исключим детекцию слоя Enemy (в нём все враги), а для прафаба игрока исключим детекцию слоя Player. Это нужно дабы триггер не срабатывал на самих себя.
    // Как выяснилось далее - сам на себя триггер срабатывать не будет (то есть на любой свой родительский объект). Также для исключения срабатывания триггера для врагов на других
    // объектах Enemy мы в GameManager уже исключили любые столкновения между элементами уровня Enemy. Дополнительно проставлять исключение детекции (exclude) для слоёв в зонах атаки
    // смысла особого в таком случае нет. Для Player также нет смысла проставлять слой Player в exclude, ибо, как ранее было подмечено - родительские объекты не триггерят зону.

    // Надеемся универсализировать скрипт зоны атаки для всех юнитов
    private void OnTriggerEnter2D(Collider2D other)
    {
        // подписываемся в Enemy 
        if (other.gameObject.CompareTag("Player") || other.gameObject.CompareTag("Allies")) { isPlayerOrAlliesInAttackArea?.Invoke(true, other.gameObject.GetComponent<Unit>()); }

        ////Debug.Log(other.gameObject.tag); 
        ////Debug.Log(other.gameObject.CompareTag("Enemy")); 
        // на данный момент подписаны в FsmStateWalk
        if (other.gameObject.CompareTag("Enemy")) 
        { 
            isEnemyInAttackArea?.Invoke(true, other.gameObject.GetComponentInParent<Enemy>());
        }         // так как коллайдер у врага находится на дочернем элементе Body, а скрипт сам на 
                  // родителе, получаем компонент родителя
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // подписываемся в FsmStateWalkEnemy
        if (other.gameObject.CompareTag("Player") || other.gameObject.CompareTag("Allies")) { isPlayerOrAlliesInAttackArea?.Invoke(false, other.gameObject.GetComponent<Unit>()); }
        // на данный момент подписаны в FsmStateWalk
        if (other.gameObject.CompareTag("Enemy")) isEnemyInAttackArea?.Invoke(false, other.gameObject.GetComponentInParent<Enemy>());
    }
}
