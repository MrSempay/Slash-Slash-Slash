using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class FsmStateDiedEnemy : FsmStateEnemy
{
    private Coroutine waitBeforeDisableColliderAndRigidBodyCoroutine; // чтоб все враги успели попадать, если убить их в прыжке, перед деактивацией коллайдера и физ. тела
    private float waitTimeBeforeDisableColliderAndRigidBody = 4f;

    public FsmStateDiedEnemy(Fsm fsm, GameObject gameObject) : base(fsm, gameObject)
    {

    }

    public override void Enter(Dictionary<string, object> initialConditionsEntering)
    {
        Debug.Log("Died state [ENTER]");
        enemy.TEST_Current_State = "Died";
        enemy.animator.Play("EnemyDied");
        //enemy.isAlive = false; // устанавливаем в методе Die у Unit
        enemy.areUpdatingFunctionsEnabled = false;
        enemy.StopAllCoroutines();

        enemy.gameObject.tag = C.Tags.EnemyDied;
        enemy.fuck.gameObject.tag = C.Tags.EnemyDied;

        Debug.Log("Проверяем тэг в смерти " + enemy.fuck.gameObject.tag);

        waitBeforeDisableColliderAndRigidBodyCoroutine = CoroutineManager.Instance.StartManagedCoroutine(gameObject, WaitBeforeDisableColliderAndRigidBody());

        GameManager.DestroyObject(enemy.temporaryTargetForRazbrestis);

        if (enemy.rb.linearVelocityY > 0)
        {
            enemy.rb.linearVelocityY = 0;
        }
    }

    public override void Exit()
    {
        Debug.Log("Died state [EXIT]");
        enemy.isAlive = true;
    }


    public override void OnDestroy()
    {
        CoroutineManager.Instance.StopCoroutine(waitBeforeDisableColliderAndRigidBodyCoroutine);
        base.OnDestroy();
    }

    IEnumerator WaitBeforeDisableColliderAndRigidBody()
    {
        yield return new WaitForSeconds(waitTimeBeforeDisableColliderAndRigidBody);
        enemy.selfEnemyCollider.enabled = false;
        enemy.rb.bodyType = RigidbodyType2D.Static;
    }
}

