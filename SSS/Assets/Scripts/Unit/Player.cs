using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Player : Unit
{
    private Coroutine zeroizeKillComboTicksCoroutine;

    [SerializeField] private float _currentExperience = 0;
    [SerializeField] private float _currentMoney = 0;
    [SerializeField] private int _currentLevel = 0;

    [NonSerialized] public Rigidbody2D rb;       // Rigidbody2D кубика
    [NonSerialized] public Vector3 startTouchPosition, endTouchPosition = Vector3.zero; // Для отслеживания свайпов
    [NonSerialized] public Vector3 startPositionPlayerBeforeMoving = Vector3.zero; // стартовая позиция игрока до того, как он начал движение
    [NonSerialized] public float differenceXBetweenStartAndEndPositions = 0; // разница по координате х между началом свайпа и его окончанием

    public AttackAreaEnemy attackAreaScript; // Скрипт зоны для атаки
    public Transform attackAreaTransform; // Компонент трансформ зоны для атаки (далее при смене направления движения будем позицию менять (отзеркаливать))

    public bool isGrounded = true; // Проверка, находится ли игрок на земле
    public Camera mainCamera; // Ссылка на камеру
    public FloorDetector scriptFloorDetector; // Ссылка на скрипт детектора пола
    public float experienceToNextLevel;
    public int currentKillCombo = 0;
    public float increasingGettingExperienceByKillComboTickPercentage;
    public float increasingGettingMoneyByKillComboTickPercentage;
    public event Action<float> OnExperienceChanged; // Событие для изменения опыта
    public event Action<float> OnMoneyChanged;     // Событие для изменения денег
    public event Action<int> OnLevelChanged;       // Событие для изменения уровня

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



    protected override void Awake()
    {
        // Сюда мы перенесли этот код для того, чтобы метод OnEnable вызывался корректно, иначе мы не успеваем инициализировать нашу FSM
        nameOfUnit = "Player";
        base.Awake();
        rb = GetComponent<Rigidbody2D>();
        selfSprite = GetComponent<SpriteRenderer>();

        _fsm = new Fsm();

        _fsm.AddState(new FsmStateIdle(_fsm, gameObject));
        _fsm.AddState(new FsmStateWalk(_fsm, gameObject));
        _fsm.AddState(new FsmStateJump(_fsm, gameObject));
        _fsm.AddState(new FsmStateFall(_fsm, gameObject));


        _fsm.SetState<FsmStateIdle>();
    }
    void Start()
    {

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
        if (zeroizeKillComboTicksCoroutine != null)
        {
            CoroutineManager.Instance.StopManagedCoroutine(this.gameObject, zeroizeKillComboTicksCoroutine);
        }

        // Запускаем новую корутину
        zeroizeKillComboTicksCoroutine = CoroutineManager.Instance.StartManagedCoroutine(this.gameObject, ZeroizeKillComboTicks());

        CurrentExperience += experience * (1 + currentKillCombo * (increasingGettingExperienceByKillComboTickPercentage / 100));
        CurrentMoney += money * (1 + currentKillCombo * (increasingGettingMoneyByKillComboTickPercentage / 100));
        currentKillCombo++;
    }

    IEnumerator ZeroizeKillComboTicks()
    {
        yield return new WaitForSeconds(1f); // Ждем 1 секунду

        // Сбрасываем комбо после задержки
        currentKillCombo = 0;
        zeroizeKillComboTicksCoroutine = null; // Сбрасываем ссылку на корутину
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

}
