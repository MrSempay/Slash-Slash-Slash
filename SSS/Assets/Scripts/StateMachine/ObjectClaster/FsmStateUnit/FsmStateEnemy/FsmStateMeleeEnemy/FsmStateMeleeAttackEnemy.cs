using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public class FsmStateMeleeAttackEnemy : FsmStateEnemy
{
    private Coroutine attackUnitByTimeCoroutine;

    public FsmStateMeleeAttackEnemy(Fsm fsm, GameObject GameObject) : base(fsm, GameObject)
    {
    }


    public override void Enter()
    {
        Debug.Log("Melee attack state [ENTER]");
        attackUnitByTimeCoroutine = CoroutineManager.Instance.StartManagedCoroutine(this.gameObject, AttackUnitByTime());
        enemy.rb.linearVelocityX = 0;
    }

    public override void Exit()
    {
        Debug.Log("Melee attack state [EXIT]");
        CoroutineManager.Instance.StopManagedCoroutine(this.gameObject, attackUnitByTimeCoroutine);
        attackUnitByTimeCoroutine = null;
    }

    public override void Update()
    {
        base.Update();
    }
    private IEnumerator AttackUnitByTime()
    {
        while (true)
        {
            //Debug.Log("WeAre HERE");
            List<Unit> unitsToRemove = new List<Unit>(); // Список для удаления юнитов
            //lock (_lock)
            {
                //Debug.Log("IBOOOO " + enemy.listOfUnitsInAttackArea.Count);
                for (int i = 0; i < enemy.listOfUnitsInAttackArea.Count; i++)
                {
                    if (i < enemy.listOfUnitsInAttackArea.Count)
                    {
                        if (enemy.listOfUnitsInAttackArea[i]) enemy.listOfUnitsInAttackArea[i].GetDamage(enemy.damage);
                        else unitsToRemove.Add(enemy.listOfUnitsInAttackArea[i]); // Добавляем в список на удаление
                    }
                }

                // Удаляем все юниты, которые нужно удалить, после завершения цикла
                foreach (Unit unitToRemove in unitsToRemove)
                {
                    enemy.listOfUnitsInAttackArea.Remove(unitToRemove);
                }
            }
            
            if (enemy.listOfUnitsInAttackArea.Count == 0) fsmEnemy.SetState<FsmStateWalkEnemy>();

            yield return new WaitForSeconds(0.5f);
        }
    }
}