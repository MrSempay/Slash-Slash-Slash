using UnityEngine;

public class FsmStateDiedEnemy : FsmStateEnemy
{

    public FsmStateDiedEnemy(Fsm fsm, GameObject gameObject) : base(fsm, gameObject)
    {

    }

    public override void Enter()
    {
        Debug.Log("Died state [ENTER]");
        enemy.animator.Play("EnemyDied");
        enemy.selfEnemyCollider.enabled = false;
        enemy.rb.bodyType = RigidbodyType2D.Static;
    }

    public override void Exit()
    {
        Debug.Log("Died state [EXIT]");
    }


    public override void OnDestroy()
    {
        base.OnDestroy();
    }
}

