using UnityEngine;

public class FsmStateDied : FsmStatePlayer
{

    public FsmStateDied(Fsm fsm, GameObject gameObject) : base(fsm, gameObject)
    {

    }

    public override void Enter()
    {
        Debug.Log("Died state [ENTER]");
        player.animator.Play("PlayerDied");
        player.gameObject.GetComponent<BoxCollider2D>().enabled = false;
        player.rb.bodyType = RigidbodyType2D.Static;
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

