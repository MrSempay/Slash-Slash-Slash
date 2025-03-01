using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Player : Unit
{
    private Coroutine _zeroizeKillComboTicksCoroutine;
    private bool _isTranslatingEquipment = false; // флаг, маркерующий, переносим ли мы какое-либо снаряжение в инвентарь

    [SerializeField] private int _countAccessToUpInSchool = 0; // флаг, маркерующий, переносим ли мы какое-либо снаряжение в инвентарь
    [SerializeField] private float _currentExperience = 0;
    [SerializeField] private float _currentMoney = 0;
    [SerializeField] private int _currentLevel = 0;
    [SerializeField] private int _currentKillCombo = 0;
    [SerializeField] private int _currentStamina;

    [NonSerialized] public Rigidbody2D rb;       // Rigidbody2D кубика
    [NonSerialized] public Vector3 startTouchPosition, endTouchPosition = Vector3.zero; // Для отслеживания свайпов
    [NonSerialized] public Vector3 startPositionPlayerBeforeMoving = Vector3.zero; // стартовая позиция игрока до того, как он начал движение
    [NonSerialized] public float differenceXBetweenStartAndEndPositions = 0; // разница по координате х между началом свайпа и его окончанием

    public AttackAreaEnemy attackAreaScript; // Скрипт зоны для атаки
    public Transform attackAreaTransform; // Компонент трансформ зоны для атаки (далее при смене направления движения будем позицию менять (отзеркаливать))
    public RectTransform spellPanelTransform; // 
    public RectTransform ammunitionPanelTransform; // 

    public bool isGrounded = true; // Проверка, находится ли игрок на земле
    public Camera mainCamera; // Ссылка на камеру
    public FloorDetector scriptFloorDetector; // Ссылка на скрипт детектора пола
    public float experienceToNextLevel;
    public float increasingGettingExperienceByKillComboTickPercentage;
    public float increasingGettingMoneyByKillComboTickPercentage;
    public int staminaMax;
    public Dictionary<string, float> increasingParametersByLevelUpPercentage;
    public event Action<float> OnExperienceChanged; // Событие для изменения опыта
    public event Action<float> OnMoneyChanged;     // Событие для изменения денег
    public event Action<int> OnLevelChanged;       // Событие для изменения уровня
    public event Action<int> OnKillComboChanged;       // Событие для изменения комбо за убийства 
    public event Action<int> OnLevelUpChanged;       // Событие для изменения количества прокачки в школе 

    public float CurrentExperience
    {
        get { return _currentExperience; }
        set
        {
            _currentExperience = value;

            // Проверяем, достаточно ли опыта для повышения уровня
            while (_currentExperience >= experienceToNextLevel)
            {
                _currentExperience -= experienceToNextLevel; // Вычитаем опыт, необходимый для повышения
                CurrentLevel++; // Повышаем уровень
                if (CurrentLevel % 5 == 0) _countAccessToUpInSchool++;
                ChangeUnitParametersByPercentage(increasingParametersByLevelUpPercentage, true);
            }

            // Вызываем событие, если есть подписчики
            OnExperienceChanged?.Invoke(_currentExperience);
        }
    }

    public float CurrentMoney
    {
        get { return _currentMoney; }
        set
        {
            _currentMoney = value;

            // Вызываем событие, если есть подписчики
            OnMoneyChanged?.Invoke(_currentMoney);
        }
    }

    public int CurrentLevel
    {
        get { return _currentLevel; }
        set
        {
            _currentLevel = value;

            // Вызываем событие, если есть подписчики
            OnLevelChanged?.Invoke(_currentLevel);
        }
    }

    public int CurrentKillCombo
    {
        get { return _currentKillCombo; }
        set
        {
            _currentKillCombo = value;

            // Вызываем событие, если есть подписчики
            OnKillComboChanged?.Invoke(_currentKillCombo);
        }
    }
    
    public int CountAccessToUpInSchool
    {
        get { return _countAccessToUpInSchool; }
        set
        {
            _countAccessToUpInSchool = value;
            OnLevelUpChanged?.Invoke(_countAccessToUpInSchool);
        }
    }

    public bool IsTranslatingEquipment
    {
        get { return _isTranslatingEquipment; }
        set
        {
            _isTranslatingEquipment = value;
            if (value) _fsm.SetState<FsmStateTranslatingEquipment>();
            else _fsm.SetState<FsmStateIdle>();

        }
    }

    public int CurrentStamina
    {
        get { return _currentStamina; }
        set
        {
            _currentStamina = value;
            ChangeStaminaBar(_currentStamina);
        }
    }



    protected override void Awake()
    {
        // Сюда мы перенесли этот код для того, чтобы метод OnEnable вызывался корректно, иначе мы не успеваем инициализировать нашу FSM
        nameOfUnit = "Player";
        base.Awake();
        rb = GetComponent<Rigidbody2D>();
        selfSprite = GetComponent<SpriteRenderer>();

        foreach (RectTransform spellTransform in spellPanelTransform)
        {

        }


        _fsm.AddState(new FsmStateIdle(_fsm, gameObject));
        _fsm.AddState(new FsmStateWalk(_fsm, gameObject));
        _fsm.AddState(new FsmStateJump(_fsm, gameObject));
        _fsm.AddState(new FsmStateFall(_fsm, gameObject));
        _fsm.AddState(new FsmStateTranslatingEquipment(_fsm, gameObject));


    }
    protected override void Start()
    {
        base.Start();
        _fsm.SetState<FsmStateIdle>();
    }


    void Update()
    {
        //Debug.Log(_fsm.StateCurrent);
        _fsm.Update();
    }

    private void FixedUpdate()
    {

        _fsm.FixedUpdate();
    }

   
    void OnEnable()
    {
        // _fsm.OnEnable(); По идее это не надо, так как оное вызывается в классах состояний и так, ибо они наследуются от Monobehavior
    }
    void OnDisable()
    {
        //_fsm.OnDisable(); По идее это не надо, так как оное вызывается в классах состояний и так, ибо они наследуются от Monobehavior
    }

    protected override void GetExperienceAndMoneyFromKillingUnit(float experience, float money)
    {
        // Останавливаем предыдущую корутину (если она существует)
        if (_zeroizeKillComboTicksCoroutine != null)
        {
            CoroutineManager.Instance.StopManagedCoroutine(this.gameObject, _zeroizeKillComboTicksCoroutine);
        }

        // Запускаем новую корутину
        _zeroizeKillComboTicksCoroutine = CoroutineManager.Instance.StartManagedCoroutine(this.gameObject, ZeroizeKillComboTicks());

        CurrentExperience += experience * (1 + CurrentKillCombo * (increasingGettingExperienceByKillComboTickPercentage / 100));
        CurrentMoney += money * (1 + CurrentKillCombo * (increasingGettingMoneyByKillComboTickPercentage / 100));
        CurrentKillCombo++;
    }

    IEnumerator ZeroizeKillComboTicks()
    {
        yield return new WaitForSeconds(1f); // Ждем 1 секунду

        // Сбрасываем комбо после задержки
        CurrentKillCombo = 0;
        _zeroizeKillComboTicksCoroutine = null; // Сбрасываем ссылку на корутину
    }

    // на данный момент код ниже - дичь. Ибо если игрок врежется головой в платформу или просто подойдёт к вертикальной стенке - isGrounded будет true. То есть прыгать может бесконечно.
    // Нужно детектить нижнюю часть игрока, то есть отдельный коллайдер и скрипт на него. 
    /*void OnCollisionEnter2D(Collision2D collision)
    {
        // Проверяем, столкнулся ли кубик с объектом с тегом "Ground"
        if (collision.gameObject.CompareTag("Ground"))
        {
            if (!isGrounded)
            {
                isGrounded = true; // Устанавливаем, что кубик снова на земле
                if (rb.linearVelocity.x == 0) _fsm.SetState<FsmStateIdle>();
            }
        }
    } */

    private void ChangeStaminaBar(int currentStamina)
    {

    }

}
