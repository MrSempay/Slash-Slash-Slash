using System.Collections.Generic;
using System.Reflection;
using System;
using UnityEngine;
using static AreaButtonEnter;
using static UnityEngine.EventSystems.EventTrigger;

public class Door : Unit
{

    private Vector3 _startPositionOfEnterButton;

    private bool _doorIsOpened;
    private Transform _transformEnterButton;
    
    [SerializeField] private AreaButtonEnter _scriptAreaEnterButton;


    [SerializeField] public Sprite spriteDoorOpened;
    [SerializeField] public Sprite spriteDoorClosed;

    public GameObject enterButton;
    public BoxCollider2D selfCollider;


    public override float CurrentHealth
    {
        get { return base.CurrentHealth; }
        set
        {
            base.CurrentHealth = value;
            if (value <= 0)
            {
                _fsm.SetState<FsmStateDoorDestroyed>();
            }
        }
    }

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

        //Sprite spriteDoorOpened = Resources.LoadAll<Sprite>("Images/Door/TrsMain")[2];
        //Sprite spriteDoorClosed = Resources.Load<Sprite>(C.DK.ImageDoorClosed);

        selfSprite.sprite = spriteDoorOpened;

        _startPositionOfEnterButton = enterButton.transform.localPosition;

        _scriptAreaEnterButton.onPlayerEnteredEnterButtonArea += SetActiveEnterButton;

        selfCollider = GetComponent<BoxCollider2D>();
        _transformEnterButton = enterButton.GetComponent<Transform>();

        _fsm = new Fsm();

        _fsm.AddState(new FsmStateDoorClosed(_fsm, gameObject));
        _fsm.AddState(new FsmStateDoorOpened(_fsm, gameObject));
        _fsm.AddState(new FsmStateDoorDestroyed(_fsm, gameObject));

        _fsm.SetState<FsmStateDoorClosed>(new Dictionary<string, object>()); // передаём пустой словарь просто для маркировки входа в первое состояние для двери, чтоб звук не проигрывался
                                                                             // при входе в состояние таким путём
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

    public override void Die(Unit unitFromWhoWasGottenDamage = null)
    {
        base.Die(unitFromWhoWasGottenDamage);

        Destroy(gameObject);
    }

}
