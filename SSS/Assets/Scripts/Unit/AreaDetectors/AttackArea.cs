using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackAreaEnemy : MonoBehaviour
{
    public delegate void UnitEnterAttackArea(bool isUnitInArea, Unit unit); // шаблон функции
    public event UnitEnterAttackArea isPlayerOrAlliesInAttackArea;         // экземляр(?) функции/сигнала(?)
    public event UnitEnterAttackArea isEnemyInAttackArea;         // экземляр(?) функции/сигнала(?)


    // Для префабов врагов мы исключим детекцию слоя Enemy (в нём все враги), а для прафаба игрока исключим детекцию слоя Player. Это нужно дабы триггер не срабатывал на самих себя.
    // Как выяснилось далее - сам на себя триггер срабатывать не будет (то есть на любой свой родительский объект). Также для исключения срабатывания триггера для врагов на других
    // объектах Enemy мы в GameManager уже исключили любые столкновения между элементами уровня Enemy. Дополнительно проставлять исключение детекции (exclude) для слоёв в зонах атаки
    // смысла особого в таком случае нет. Для Player также нет смысла проставлять слой Player в exclude, ибо, как ранее было подмечено - родительские объекты не триггерят зону.

    // Надеемся универсализировать скрипт зоны атаки для всех юнитов
    private void OnTriggerEnter2D(Collider2D other)
    {
        // подписываемся в FsmStateWalkEnemy 
        if (other.gameObject.CompareTag("Player") || other.gameObject.CompareTag("Allies")) { isPlayerOrAlliesInAttackArea?.Invoke(true, other.gameObject.GetComponent<Unit>()); }
        // на данный момент подписаны в FsmStateWalk
        if (other.gameObject.CompareTag("Enemy")) { isEnemyInAttackArea?.Invoke(true, other.gameObject.GetComponentInParent<Unit>());} // так как коллайдер у врага находится на дочернем элементе Body, а скрипт сам на 
                                                                                                                                        // родителе, получаем компонент родителя
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // подписываемся в FsmStateWalkEnemy
        if (other.gameObject.CompareTag("Player") || other.gameObject.CompareTag("Allies")) isPlayerOrAlliesInAttackArea?.Invoke(false, other.gameObject.GetComponent<Unit>());
        // на данный момент подписаны в FsmStateWalk
        if (other.gameObject.CompareTag("Enemy")) isEnemyInAttackArea?.Invoke(false, other.gameObject.GetComponentInParent<Unit>());
    }
}
