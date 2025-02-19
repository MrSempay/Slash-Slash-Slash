using UnityEngine;

public class Building : MonoBehaviour
{
    private Fsm _fsm;
    protected virtual void Awake()
    {
        _fsm = new Fsm();

        _fsm.AddState(new FsmStateBuildingNormal(_fsm, gameObject));
        _fsm.AddState(new FsmStateBuildingDestroyed(_fsm, gameObject));

        _fsm.SetState<FsmStateBuildingNormal>();
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player") || other.gameObject.CompareTag("Allies")) {  }
        if (other.gameObject.CompareTag("Enemy")) { _fsm.SetState<FsmStateBuildingDestroyed>(); } 
    }
}
