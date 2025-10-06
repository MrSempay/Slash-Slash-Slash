using System.Collections.Generic;
using System.Reflection;
using System;
using UnityEngine;
using static AreaButtonEnter;
using static UnityEngine.EventSystems.EventTrigger;
using System.Linq;

public class Door : Unit
{

    private Vector3 _startPositionOfEnterButton;

    private bool _doorIsOpened;
    private bool _lockDoor;
    private Transform _tAreaDoorClosePlayerHasEntered;
    private Transform _transformEnterButton;
    
    [SerializeField] private AreaButtonEnter _scriptAreaEnterButton;
    [SerializeField] private List<AreaDoorClose> _listAreasDoorClose = new();


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

    public static void LockOrDelockAllDoors(bool lockDoor)
    {
        List<Door> allDoors = FindObjectsByType<Door>(FindObjectsSortMode.None).ToList();

        foreach (Door scriptDoor in allDoors)
        {
            scriptDoor._scriptAreaEnterButton.gameObject.SetActive(!lockDoor); // чтоб кнопка даже не по€вл€лась
            scriptDoor._lockDoor = lockDoor; // и чтоб впринципе не могли дверь трогать. Ќапример, через нашу зону дл€ прохода через дверь
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

        foreach (AreaDoorClose areaDoorClose in _listAreasDoorClose)
        {
            areaDoorClose.OnPlayerInDoorCloseArea += CloseDoor;
        }

        selfCollider = GetComponent<BoxCollider2D>();
        _transformEnterButton = enterButton.GetComponent<Transform>();

        _fsm = new Fsm();

        _fsm.AddState(new FsmStateDoorClosed(_fsm, gameObject));
        _fsm.AddState(new FsmStateDoorOpened(_fsm, gameObject));
        _fsm.AddState(new FsmStateDoorDestroyed(_fsm, gameObject));

        _fsm.SetState<FsmStateDoorClosed>(new Dictionary<string, object>()); // передаЄм пустой словарь просто дл€ маркировки входа в первое состо€ние дл€ двери, чтоб звук не проигрывалс€
                                                                             // при входе в состо€ние таким путЄм
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
        if (_lockDoor) return;
        
        DoorIsOpened = !DoorIsOpened;    
    }
    public void CloseDoor(Transform transform)
    {
        if (_tAreaDoorClosePlayerHasEntered == null)
        {
            _tAreaDoorClosePlayerHasEntered = transform;
            return;
        }
        else if (_tAreaDoorClosePlayerHasEntered != transform)
        {
            if (DoorIsOpened)
            {
                DoorIsOpened = false;
                _tAreaDoorClosePlayerHasEntered = transform;
            }
        }
    }

    public override void Die(Unit unitFromWhoWasGottenDamage = null)
    {
        base.Die(unitFromWhoWasGottenDamage);

        Destroy(gameObject);
    }

}
