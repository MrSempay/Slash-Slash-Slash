using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FsmStateDied : FsmStatePlayer
{
    private Coroutine waitBeforeDisableColliderAndRigidBodyCoroutine; // чтоб все враги успели попадать, если убить их в прыжке, перед деактивацией коллайдера и физ. тела
    private float waitTimeBeforeDisableColliderAndRigidBody = 4f;

    public FsmStateDied(Fsm fsm, GameObject gameObject) : base(fsm, gameObject)
    {

    }

    public override void Enter(Dictionary<string, object> initialConditionsEntering)
    {
        Debug.Log("Died state [ENTER]");

        player.animator.Play("PlayerDied");
        player.isAlive = false;
        player.attackAreaTransform.gameObject.SetActive(false);
        player.areUpdatingFunctionsEnabled = false;
        player.StopAllCoroutines();
        player.WasDestroyed = true;

        waitBeforeDisableColliderAndRigidBodyCoroutine = CoroutineManager.Instance.StartManagedCoroutine(gameObject, WaitBeforeDisableColliderAndRigidBody());

        ScoreManager.Instance.GetAndShowActualLeaderboardAsync(Leaderboard.INSTANTIATION_CONTEXT.PlayerDeath);

    }

    public override void Exit()
    {
        Debug.Log("Died state [EXIT]");
        player.isAlive = true;
    }


    public override void OnDestroy()
    {
        base.OnDestroy();
    }

    IEnumerator WaitBeforeDisableColliderAndRigidBody()
    {
        yield return new WaitForSeconds(waitTimeBeforeDisableColliderAndRigidBody);
        player.gameObject.GetComponent<BoxCollider2D>().enabled = false;
        player.rb.bodyType = RigidbodyType2D.Static;
    }
}

