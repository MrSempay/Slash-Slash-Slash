using System.Collections;
using UnityEngine;


public class FsmStateDiedEnemy : FsmStateEnemy
{
    private Coroutine waitBeforeDisableColliderAndRigidBodyCoroutine; // чтоб все враги успели попадать, если убить их в прыжке, перед деактивацией коллайдера и физ. тела
    private float waitTimeBeforeDisableColliderAndRigidBody = 4f;

    public FsmStateDiedEnemy(Fsm fsm, GameObject gameObject) : base(fsm, gameObject)
    {

    }

    public override void Enter()
    {
        Debug.Log("Died state [ENTER]");
        enemy.animator.Play("EnemyDied");
        enemy.isAlive = false;
        waitBeforeDisableColliderAndRigidBodyCoroutine = CoroutineManager.Instance.StartManagedCoroutine(gameObject, WaitBeforeDisableColliderAndRigidBody());

    }

    public override void Exit()
    {
        Debug.Log("Died state [EXIT]");
        enemy.isAlive = true;
    }


    public override void OnDestroy()
    {
        base.OnDestroy();
    }

    IEnumerator WaitBeforeDisableColliderAndRigidBody()
    {
        yield return new WaitForSeconds(waitTimeBeforeDisableColliderAndRigidBody);
        enemy.selfEnemyCollider.enabled = false;
        enemy.rb.bodyType = RigidbodyType2D.Static;
    }
}

