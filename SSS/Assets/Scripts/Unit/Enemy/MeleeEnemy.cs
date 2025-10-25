using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public class MeleeEnemy : Enemy
{
    protected override void Awake()
    {
        //nameOfUnit = "MeleeEnemy";
        //lookingRight = false; // для псины, по крайней мере
        base.Awake();
        _fsm.AddState(new FsmStateMeleeAttackEnemy(_fsm, gameObject));
    }
    protected override void Start()
    {
        base.Start();
    }

    public override void MakeDamageToUnit(Unit unitWhichIsAttacked)
    {
        List<Unit> unitsToRemove = new List<Unit>(); // Список для удаления юнитов
        //Debug.Log("IBOOOO " + enemy.listOfUnitsInAttackArea.Count);
        for (int i = 0; i < listOfUnitsInAttackArea.Count; i++)
        {
            if (i < listOfUnitsInAttackArea.Count)
            {
                if (listOfUnitsInAttackArea[i]) { listOfUnitsInAttackArea[i].GetDamage(damage, this, true); }
                else unitsToRemove.Add(listOfUnitsInAttackArea[i]); // Добавляем в список на удаление
            }
        }

        // Удаляем все юниты, которые нужно удалить, после завершения цикла
        foreach (Unit unitToRemove in unitsToRemove)
        {
            listOfUnitsInAttackArea.Remove(unitToRemove);
        }
        if (listOfUnitsInAttackArea.Count == 0) _fsm.SetState<FsmStateWalkEnemy>();
    }


}