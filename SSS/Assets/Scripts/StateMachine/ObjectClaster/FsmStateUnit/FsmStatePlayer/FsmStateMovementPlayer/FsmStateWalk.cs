using System.Collections;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;


public class FsmStateWalk : FsmStateMovementPlayer
{
    public FsmStateWalk(Fsm fsm, GameObject GameObject) : base(fsm, GameObject)
    {
        player.attackAreaScript.isEnemyInAttackArea += MakeDamageToEnemy;
    }

    public override void Enter()
    {
        Debug.Log("Walk state [ENTER]");
        HandleSwipe(player.endTouchPosition - player.startTouchPosition); // по идее любой вход в данное состояние подразумевает, что свайп был сделан в состоянии покоя и мы
        // далее работает с полями объекта, которые уже были изменены в ходе этого свайпа. Далее в FixedUpdate мы мониторим факт дальнеших свайпов
    }

    public override void Exit()
    {
        Debug.Log("Walk state [EXIT]");
    }

    public override void Update()
    {
        player.transform.rotation = Quaternion.Euler(0, 0, 0);
        
        MakingSwipe();
        if (player.rb.linearVelocity.x == 0) fsmPlayer.SetState<FsmStateIdle>();
        //if (!player.isGrounded) fsmPlayer.SetState<FsmStateFall>();
    }

    private void MakeDamageToEnemy(bool isEnemyInArea, Unit enemy)
    {
        // так как урон можем наносить только во время свайпа, а иметь мгновенную скорость по оси Х также только во время свайпа, проверяем в условии скорость на неравенство нулю.
        if (isEnemyInArea) { enemy.GetDamage(player.damage); }
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        player.attackAreaScript.isEnemyInAttackArea -= MakeDamageToEnemy;

    }

}