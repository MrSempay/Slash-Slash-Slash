using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;
using UnityEngine.AI;
using System.Collections.Generic;

public class FsmStateWalkEnemy : FsmStateEnemy
{
    
    
    public FsmStateWalkEnemy(Fsm fsm, GameObject GameObject) : base(fsm, GameObject)
    {
        // Здесь у нас то, что определяется единожды при создании объекта состояния
        //Debug.Log(enemy);
        //Debug.Log(enemy.agent);
        //Debug.Log(enemy.agent.updatePosition);
        enemy.agent.updatePosition = false;
        enemy.pitDetectorScript.OnDetectedPit += Jump;

    }


    public override void Enter(Dictionary<string, object> initialConditionsEntering)
    {
        Debug.Log("Enemy Walk state [ENTER]");
        enemy.TEST_Current_State = "Walk";
        enemy.currentCornerIndex = 1; // Начинаем со второй точки (индекс 1)
        //enemy.isPathValid = false; // Сбрасываем флаг валидности пути при входе // 03.10.2025 - ваще хз что тут происходит. Из-за этого собаки застряют порою
        enemy.animator.Play("EnemyWalk");
    }

    public override void Exit()
    {
        Debug.Log("Enemy Walk state [EXIT]");
        AudioManager.Instance.StopSomeTypeSoundOnEmitter(AudioManager.TYPE_SOUND.Walk, enemy.audioEmitter);
    }

    public override void Update()
    {
        base.Update();
        if (enemy.listOfUnitsInAttackArea.Count > 0) fsmEnemy.SetState<FsmStateMeleeAttackEnemy>();

    }

    public override void FixedUpdate()
    {
        if (!enemy.isGrounded) fsmEnemy.SetState<FsmStateFallEnemy>();
        CalculateDrawPathChangeDirectionAndMove();
    }




}
