using System.Collections.Generic;
using System.Reflection;
using System;
using UnityEngine;
using static AreaButtonEnter;
using static UnityEngine.EventSystems.EventTrigger;

public class Door : Unit
{
    private readonly Vector3 _startPositionOfEnterButton = new Vector3(-0.85f, 1.31f, 0);

    private bool _doorIsOpened;
    private Transform _transformEnterButton;
    
    [SerializeField] private AreaButtonEnter _scriptAreaEnterButton;

    public GameObject enterButton;
    public float health;
    public BoxCollider2D selfCollider; 

    public bool DoorIsOpened
    {
        get { return _doorIsOpened; }
        set
        {
            _doorIsOpened = value;
            if (_fsm.StateCurrent.GetType() != typeof(FsmStateDoorDestroyed))
            {
                if (_doorIsOpened) _fsm.SetState<FsmStateDoorOpened>();
                else _fsm.SetState<FsmStateDoorClosed>();
            }
        }
    }

    protected override void Awake()
    {
        nameOfUnit = "Door";
        base.Awake();

        _scriptAreaEnterButton.onPlayerEnteredEnterButtonArea += SetActiveEnterButton;

        selfCollider = GetComponent<BoxCollider2D>();
        _transformEnterButton = enterButton.GetComponent<Transform>();

        _fsm = new Fsm();

        _fsm.AddState(new FsmStateDoorClosed(_fsm, gameObject));
        _fsm.AddState(new FsmStateDoorOpened(_fsm, gameObject));
        _fsm.AddState(new FsmStateDoorDestroyed(_fsm, gameObject));

        _fsm.SetState<FsmStateDoorClosed>();
    }

    private void SetActiveEnterButton(bool wasPlayerEntered, float positionXOfPlayer)
    {
        if (_fsm.StateCurrent.GetType() != typeof(FsmStateDoorDestroyed))
        {
            if (positionXOfPlayer > transform.position.x) _transformEnterButton.localPosition = new Vector3(-1 * _startPositionOfEnterButton.x, _startPositionOfEnterButton.y, _startPositionOfEnterButton.z);
            else _transformEnterButton.localPosition = new Vector3(_startPositionOfEnterButton.x, _startPositionOfEnterButton.y, _startPositionOfEnterButton.z);

            enterButton.SetActive(wasPlayerEntered);
        }
    }
    public void OpenOrCloseDoor()
    {
        DoorIsOpened = !DoorIsOpened;
    }

}
