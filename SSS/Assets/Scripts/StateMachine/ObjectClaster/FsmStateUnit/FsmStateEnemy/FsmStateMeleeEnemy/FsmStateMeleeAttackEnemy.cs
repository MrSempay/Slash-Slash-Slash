using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;

public class FsmStateMeleeAttackEnemy : FsmStateEnemy
{
    //private Coroutine attackUnitByTimeCoroutine;

    public FsmStateMeleeAttackEnemy(Fsm fsm, GameObject GameObject) : base(fsm, GameObject)
    {
        enemy.fuck.onAttackAnimationAtRightPointForGetDamage += MakeDamageToUnit;
        enemy.fuck.onAttackAnimationAtRightPointForGetDamage += PlayMakeDamageSound;
    }


    public override void Enter(Dictionary<string, object> initialConditionsEntering)
    {
        Debug.Log("Melee attack state [ENTER]");
        enemy.rb.linearVelocityX = 0;
        enemy.animator.Play("MeleeEnemyAttack", -1, 0f);
        //enemy.animator.Play("MeleeEnemyAttack"); Этот вариант какого-то чёрта не работает. Всегда нужно начинать анимацию атаки с времени 0f, иначе оно багается и переходит в Walk

        //attackUnitByTimeCoroutine = CoroutineManager.Instance.StartManagedCoroutine(this.gameObject, AttackUnitByTime());

    }

    public override void Exit()
    {
        Debug.Log("Melee attack state [EXIT]");
        //CoroutineManager.Instance.StopManagedCoroutine(this.gameObject, attackUnitByTimeCoroutine);
        //attackUnitByTimeCoroutine = null;
    }

    public override void Update()
    {
        base.Update();
    }
    //private IEnumerator AttackUnitByTime()
    //{
    //    while (true)
    //    {
    //        Debug.Log(enemy.animator.GetCurrentAnimatorClipInfo(0)[0].clip.length / 1.5);
    //        Debug.Log(enemy.animator.GetCurrentAnimatorClipInfo(0)[0].clip.name);
    //        yield return new WaitForSeconds(enemy.animator.GetCurrentAnimatorClipInfo(0)[0].clip.length / 2);
    //        List<Unit> unitsToRemove = new List<Unit>(); // Список для удаления юнитов
    //                                                        //lock (_lock)
    //        {
    //            //Debug.Log("IBOOOO " + enemy.listOfUnitsInAttackArea.Count);
    //            for (int i = 0; i < enemy.listOfUnitsInAttackArea.Count; i++)
    //            {
    //                if (i < enemy.listOfUnitsInAttackArea.Count)
    //                {
    //                    if (enemy.listOfUnitsInAttackArea[i]) { enemy.listOfUnitsInAttackArea[i].GetDamage(enemy.damage); }
    //                    else unitsToRemove.Add(enemy.listOfUnitsInAttackArea[i]); // Добавляем в список на удаление
    //                }
    //            }

    //            // Удаляем все юниты, которые нужно удалить, после завершения цикла
    //            foreach (Unit unitToRemove in unitsToRemove)
    //            {
    //                enemy.listOfUnitsInAttackArea.Remove(unitToRemove);
    //            }
    //        }
    //        if (enemy.listOfUnitsInAttackArea.Count == 0) fsmEnemy.SetState<FsmStateWalkEnemy>();

    //        yield return new WaitForSeconds(enemy.animator.GetCurrentAnimatorClipInfo(0)[0].clip.length / 2);
    //    }
    //}

    private void MakeDamageToUnit()
    {
        List<Unit> unitsToRemove = new List<Unit>(); // Список для удаления юнитов
                                                     //lock (_lock)
        {
            //Debug.Log("IBOOOO " + enemy.listOfUnitsInAttackArea.Count);
            for (int i = 0; i < enemy.listOfUnitsInAttackArea.Count; i++)
            {
                if (i < enemy.listOfUnitsInAttackArea.Count)
                {
                    if (enemy.listOfUnitsInAttackArea[i]) { enemy.listOfUnitsInAttackArea[i].GetDamage(enemy.damage, enemy, true); }
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
    }

    private void PlayMakeDamageSound()
    {
        AudioManager.Instance.StartSoundEffectAtSpecifiedObject(enemy.nameSoundAttakPeaked, enemy.gameObject, AudioManager.TYPE_SOUND.AttackPeak, AudioManager.TYPE_AUDIO_SOURCE._3DStandard);
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        enemy.fuck.onAttackAnimationAtRightPointForGetDamage -= MakeDamageToUnit;
        enemy.fuck.onAttackAnimationAtRightPointForGetDamage -= PlayMakeDamageSound;
        //CoroutineManager.Instance.StopManagedCoroutine(this.gameObject, attackUnitByTimeCoroutine);
        //attackUnitByTimeCoroutine = null;
    }
}