using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.SocialPlatforms.Impl;
using static ScoreManager;

public class Player : Unit
{

    private Coroutine _zeroizeKillComboTicksCoroutine;
    private Coroutine _recoverStaminaPointCoroutine;
    private Coroutine _zeroizeComboKillCoroutine = null;
    private bool _isTranslatingEquipment = false; // флаг, маркерующий, переносим ли мы какое-либо снаряжение в инвентарь
    private bool _hasVibratedByFallHealth = false; // флаг, маркерующий, была ли сделана вибрация при пересечении порога ХП в 15%
    private float _fractionCurrentHealthToVibrate = 0.15f; // порог здоровья, при пересечении которого начинаем вибрировать
    private Transform _mainCameraTransform; // компонент Transform камеры игрока
    private int _amountEnemiesWasKilledInCombo = 0; // количество врагов, убитых "одним ударом"
    private float _timeDuringOneHit = 0.2f; // длительность нашего "одного удара"
    private int _currentMinimumAmountCombo; // количество ячеек в инвентаре для заклинаний, пока что... просто константа и не влияет на их количество
    [SerializeField] private float _currentIncreasingStamina; 


    [SerializeField] private int _countAccessToUpInSchool = 0; // флаг, маркерующий, переносим ли мы какое-либо снаряжение в инвентарь 
    [SerializeField] private float _currentExperience = 0;
    [SerializeField] private float _currentMoney = 0;
    [SerializeField] private int _currentScore = 0;
    [SerializeField] private int _currentLevel = 0;
    [SerializeField] private int _currentKillCombo = 0;
    [SerializeField] private int _currentStamina = 0;
    [SerializeField] private RectTransform _rectTransformStaminaBar;
    [SerializeField] private GameObject _prefubOfStaminaPoint;
    
    public static Player instance;

    [NonSerialized] public Rigidbody2D rb;       // Rigidbody2D кубика
    [NonSerialized] public Vector3 startTouchPosition, endTouchPosition = Vector3.zero; // Для отслеживания свайпов
    [NonSerialized] public Vector3 startPositionPlayerBeforeMoving = Vector3.zero; // стартовая позиция игрока до того, как он начал движение
    [NonSerialized] public float differenceXBetweenStartAndEndPositions = 0; // разница по координате х между началом свайпа и его окончанием
    [NonSerialized] public AnimatorClipInfo animatorInfo; // по идее нафиг не нужно. Требуется лишь для отладки
    [NonSerialized] public float comboOneHitKillMultiplayer; // множитель для убийства врагов за "один удар"
    [NonSerialized] public int countAvailableSpellPlaces = 3; // количество ячеек в инвентаре для заклинаний, пока что... просто константа и не влияет на их количество
    [NonSerialized] public int countAvailableAmmunitionPlaces = 3; // количество ячеек в инвентаре для аммуниции, пока что... просто константа и не влияет на их количество

    public RectTransform rectTransformPlaceCustomCombos;
    public InterstitialAds interstitialAds;
    public AttackArea attackAreaScript; // Скрипт зоны для атаки
    public ProgressBar progerssBarStyleRank; // Скрипт зоны для атаки
    public EnemyNearDetector nearAreaDetector; // Скрипт зоны для обнаружения врага и модификации анимации передвижения
    public Transform attackAreaTransform; // Компонент трансформ зоны для атаки (далее при смене направления движения будем позицию менять (отзеркаливать))
    public RectTransform spellPanelTransform; // 
    public RectTransform ammunitionPanelTransform; //
    public RectTransform UI; //
    public List<Spell> playersSpells = new(); // список заклинаний, доступных игроку в инвентаре 
    //[SerializeField] public TextEdit texxt; //   

    public List<Enemy> nearEnemies = new();
    public Vector3 localPositionCamera; // чтоб помнить, где должна быть камере относительно игрока, когда будет возвращать её ему после перемещения
    public bool isEnemyNear; // флаг, идентифицирующий, есть ли какой-либо враг рядом с героем
    public bool wasEnemyDamagedByLastSwipe; // флаг, идентифицирующий, есть ли какой-либо враг рядом с героем
    public float timeRecoverStaminaPoint; // КД восстановление одного заряда выносливости
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
    public event Action<string> OnEnemiesWaveWasDestroyedWithoutLosingMainTargets;  // событие зачистки всей волны врагов без потери основных целей для защиты
    public event Action<string> OnEnemiesWaveWasDestroyed;  // событие зачистки всей волны врагов без потери основных целей для защиты

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

    public int CurrentMinimumAmountCombo
    {
        get { return _currentMinimumAmountCombo; }
        set
        {
            _currentMinimumAmountCombo = value;
            ScoreManager.Instance.CurrentMinimumAmountCombo = value;
        }
    }

    public int CurrentLevel
    {
        get { return _currentLevel; }
        set
        {
            _currentLevel = value;

            if (_currentLevel % 5 == 0) CountAccessToUpInSchool++;
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
            _currentStamina = value > 0 ? value : 0;
            ChangeStaminaBar(_currentStamina);

            if (value < staminaMax) 
            {
                if (_recoverStaminaPointCoroutine == null) // если текущая выносливость меньше максимальной и при этом корутина для её восстановления не запущена
                {
                    _recoverStaminaPointCoroutine = CoroutineManager.Instance.StartManagedCoroutine(this.gameObject, RecoverStaminaPoint());
                }
            }
            else
            {
                if (_recoverStaminaPointCoroutine != null) // если текущая выносливость равна максимальной и при этом корутина для её восстановления всё ещё работает
                {
                    CoroutineManager.Instance.StopManagedCoroutine(gameObject, _recoverStaminaPointCoroutine);
                    _recoverStaminaPointCoroutine = null;
                }
            }
        }
    }

    public float CurrentIncreasingStamina // ввели отдельный контур для увеличения стамины, ибо просто повышать staminaMax не получилось бы из-за слишком маленького шага единицы измерения
    {
        get { return _currentIncreasingStamina; }
        set
        {
            _currentIncreasingStamina = value;
            int ab = 1;
            Debug.Log(ab.GetType());
            Debug.Log((float)ab);
            Debug.Log((baseParametersValues[C.DK.staminaMax]).GetType());
            Debug.Log((baseParametersValues[C.DK.staminaMax]));
            Debug.Log(Convert.ToSingle(baseParametersValues[C.DK.staminaMax]));
            int increasingStamina = Mathf.RoundToInt(Convert.ToSingle(baseParametersValues[C.DK.staminaMax]) * (value/100));
            staminaMax = Convert.ToInt32(baseParametersValues[C.DK.staminaMax]) + increasingStamina;
            CurrentStamina += increasingStamina;
        }
    }
    public override float CurrentHealth
    {
        get { return base.CurrentHealth; }
        set
        {
            base.CurrentHealth = value;
            if (value / healthMax <= _fractionCurrentHealthToVibrate && !_hasVibratedByFallHealth)  // начинаем вибрировать в случае, если ХП меньше 15%
            {
                StaticClassForAdditionalFunctions.Vibrate();
                _hasVibratedByFallHealth = true; // Устанавливаем флаг, чтобы не вибрировать снова
            }
            else if (value / healthMax > _fractionCurrentHealthToVibrate)
            {
                _hasVibratedByFallHealth = false; // Сбрасываем флаг, когда здоровье восстанавливается
            }
        }
    }

    // здесь будем инициализировать те штуки, которые зависят и ссылаются на объект Player, и которые без него работать не смогут
    private void InitializeDependencies()
    {
        ScoreManager.Instance.Initialize(this);
        InventoryPlayer.Instance.Initialize(this);
    }

    protected override void Awake()
    {

        // Сюда мы перенесли этот код для того, чтобы метод OnEnable вызывался корректно, иначе мы не успеваем инициализировать нашу FSM 
        instance = this;
        nameOfUnit = "Player";
        base.Awake();
        CurrentStamina = staminaMax;
        rb = GetComponent<Rigidbody2D>();
        selfSprite = GetComponent<SpriteRenderer>();
        _mainCameraTransform = mainCamera.gameObject.GetComponent<Transform>();
        EventBus.Instance.DoorWasDestroyed.AddListener(DoorDestroyedOrRepeired);

        nearAreaDetector.isEnemyNear += EnemyNear;

        // для простановки начального аддитивного текста в текстовых полях UI
        CurrentExperience = CurrentExperience;
        CurrentKillCombo = CurrentKillCombo;
        CurrentLevel = CurrentLevel;
        CurrentMoney = CurrentMoney;
        CountAccessToUpInSchool = CountAccessToUpInSchool;



        localPositionCamera = _mainCameraTransform.localPosition;
        foreach (RectTransform spellTransform in spellPanelTransform)
        {

        }

        InitializeDependencies();

        _fsm.AddState(new FsmStateIdle(_fsm, gameObject));
        _fsm.AddState(new FsmStateWalk(_fsm, gameObject));
        _fsm.AddState(new FsmStateJump(_fsm, gameObject));
        _fsm.AddState(new FsmStateFall(_fsm, gameObject));
        _fsm.AddState(new FsmStateDied(_fsm, gameObject));
        _fsm.AddState(new FsmStateTranslatingEquipment(_fsm, gameObject));


    }
    protected override void Start()
    {
        //texxt.Text = "Greeting";
        base.Start();
        _fsm.SetState<FsmStateIdle>();
    }


    void Update()
    {
        //Debug.Log(GameManager.Instance.localizationManager.currentLanguage);
        if (areUpdatingFunctionsEnabled) _fsm.Update();
    }
    void asd(bool asd)
    {
     
    }

    private void FixedUpdate()
    {
        if (areUpdatingFunctionsEnabled) _fsm.FixedUpdate();
    }

   
    void OnEnable()
    {
        // _fsm.OnEnable(); По идее это не надо, так как оное вызывается в классах состояний и так, ибо они наследуются от Monobehavior
    }
    void OnDisable()
    {
        //_fsm.OnDisable(); По идее это не надо, так как оное вызывается в классах состояний и так, ибо они наследуются от Monobehavior
    }

    protected override void SomeUnitWasDestroyed(Unit unit)
    {
        Enemy enemyUnit = unit as Enemy; // безопасное приведение, ибо мало ли, вдруг не врага убьём, хотя такого пока что быть не может, ведь атаковать мы можем только тег Enemy

        EventBus.Instance.EnemyWasKilledByPlayer(enemyUnit);

        _amountEnemiesWasKilledInCombo++;
        GetExperienceAndMoneyFromKillingUnit(unit.experienceFromKill, unit.moneyFromKill, unit.comboFromKill, unit.scoreFromKill);
        if (_zeroizeComboKillCoroutine == null)
        {
            _zeroizeComboKillCoroutine = StartCoroutine(ZeroizeComboKill());
        }


        if (enemyUnit != null) 
        {
                        Debug.Log("mda1");
            if (enemyUnit.isInstancedByLevel)
            {
                        Debug.Log("mda2");
                if (LevelBuilder.instance.WasEnemiesWaveDestroyed(enemyUnit)) // подразумевается, что зачищать волну может только герой пока что, если враг сам помрёт, то не засчитается
                {
                        Debug.Log("mda3");
                    if (LevelBuilder.instance.IsAllMainTargetsAlive())
                    {
                        Debug.Log("mda4");
                        OnEnemiesWaveWasDestroyedWithoutLosingMainTargets?.Invoke(LevelBuilder.instance.currentWave); // подписываемся в ScenarioScipt, пока что
                    }
                    OnEnemiesWaveWasDestroyed?.Invoke(LevelBuilder.instance.currentWave);
                }
            }
        }

    }
    protected override void SomeUnitWasHit(Unit unit) // подразумевается, конечно, что толкмо враг игроком может быть ударен (детектим для удара вражеский тэг)
    {
        if (wasEnemyDamagedByLastSwipe == false) // продлеваем комбо за первый удар в свайпе
        {
            wasEnemyDamagedByLastSwipe = true;
            ScoreManager.Instance.UpCombo(1); // пока что магическая константа, но по идее тут должен быть всегда один, и настраивать-то нечего
        }
    }


    protected override void GetExperienceAndMoneyFromKillingUnit(float experience, float money, int comboFromKill, int score)
    {
        CurrentExperience += experience * ScoreManager.Instance.styleMultiplier;
        CurrentMoney += money * ScoreManager.Instance.styleMultiplier;
        ScoreManager.Instance.CurrentScore += score * ScoreManager.Instance.styleMultiplier;
        ScoreManager.Instance.UpCombo(comboFromKill); // по сути набитие комбо на враге не учитывает убийство текущего врага: опыт, злато и очки не скалятся от повышения ранга
    }

    public void GiveRewardScore(int score)
    {
        ScoreManager.Instance.CurrentScore += score;
    }

    IEnumerator ZeroizeComboKill()
    {
        yield return new WaitForSeconds(_timeDuringOneHit);

        if (_amountEnemiesWasKilledInCombo > 1)
        {
            ScoreManager.Instance.UpCombo((int)(_amountEnemiesWasKilledInCombo * comboOneHitKillMultiplayer));
            ScoreManager.InvokeAppearingSprite(ScoreManager.TYPE_APPEARING_MESSAGE.ComboMultyKill);
        }
        _amountEnemiesWasKilledInCombo = 0;
        _zeroizeComboKillCoroutine = null;
    }



    IEnumerator RecoverStaminaPoint()
    {
        while (true)
        {
            yield return new WaitForSeconds(timeRecoverStaminaPoint);
            if (CurrentStamina < staminaMax) CurrentStamina++;

        }

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

    private void DoorDestroyedOrRepeired(bool isDoorDestroyed)
    {
        if (isDoorDestroyed) ShakeCamera(_mainCameraTransform, localPositionCamera);
    }

    public override void GetDamage(float damageSize, Unit unitFromWhoWasGottenDamage = null, bool wasDamageByStandartAttack = true)
    {
        if (unitFromWhoWasGottenDamage)
        {
            if (StaticClassForAdditionalFunctions.CheckChance(evasionPercentage)) // шанс уклониться от урона. Не важно, от какого, главное, что от исходящего от другого юнита
            {
                return;
            }
        }
            ShakeCamera(_mainCameraTransform, localPositionCamera);
        base.GetDamage(damageSize, unitFromWhoWasGottenDamage);
    }


    private void ChangeStaminaBar(int currentStamina)
    {
        if (_rectTransformStaminaBar.childCount != 0)
        {
            // Получаем количество дочерних объектов
            int childCount = _rectTransformStaminaBar.childCount;

            // Итерируемся по дочерним объектам в обратном порядке
            for (int i = childCount - 1; i >= 0; i--)
            {
                // Получаем дочерний объект
                Transform child = _rectTransformStaminaBar.GetChild(i);
                Destroy(child.gameObject); // Используем Destroy во время выполнения игры 
            }
        }
        for (int i = 0; i < currentStamina; i++)
        {
            GameObject newStaminaPoint = Instantiate(_prefubOfStaminaPoint, Vector3.zero, Quaternion.identity);
            newStaminaPoint.GetComponent<RectTransform>().SetParent(_rectTransformStaminaBar, false);
        }
    }

    public void ShakeCamera(Transform mainCameraTransform, Vector3 initialLocalPositionCamera)
    {
        if (GameManager.Instance.currentSettings.cameraShakingOn)
        {
            StartCoroutine(ShakeCoroutine(mainCameraTransform, initialLocalPositionCamera));
        }
    }

    IEnumerator ShakeCoroutine(Transform mainCameraTransform, Vector3 initialLocalPositionCamera)
    {
        float elapsed = 0.0f;


        float shakeDuration = 0.7f; // Длительность тряски
        float shakeMagnitude = 0.1f; // Интенсивность тряски
        float dampingSpeed = 1.0f; // Скорость затухания

        while (elapsed < shakeDuration)
        {
            // Генерируем случайное смещение в пределах сферы
            float x = UnityEngine.Random.Range(-1f, 1f) * shakeMagnitude;
            float y = UnityEngine.Random.Range(-1f, 1f) * shakeMagnitude;

            mainCameraTransform.localPosition = initialLocalPositionCamera + new Vector3(x, y, 0);

            elapsed += Time.deltaTime;

            //Затухание: Уменьшаем величину тряски со временем
            shakeMagnitude = Mathf.Lerp(shakeMagnitude, 0, elapsed / shakeDuration);
            yield return null;
        }

        mainCameraTransform.localPosition = initialLocalPositionCamera; // Возвращаем камеру в исходную позицию
    }

    private void EnemyNear(bool isNear, Enemy enemy)
    {
        if (isNear)
        {
            nearEnemies.Add(enemy);
            isEnemyNear = true;
            if (_fsm.StateCurrent.GetType() == typeof(FsmStateWalk))
            {
                animator.Play("PlayerAttack");
            }
        }
        else
        {
            nearEnemies.Remove(enemy);
            if (nearEnemies.Count == 0)
            {
                isEnemyNear = false;
                if (_fsm.StateCurrent.GetType() == typeof(FsmStateWalk))
                {
                    animator.Play("PlayerWalkAggressive");
                }
            }
        }
    }



    public override void Die(Unit unitFromWhoWasGottenDamage = null)
    {
        base.Die(unitFromWhoWasGottenDamage);
        _fsm.SetState<FsmStateDied>();
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        nearAreaDetector.isEnemyNear -= EnemyNear;
        CoroutineManager.Instance.StopManagedCoroutine(this.gameObject, _recoverStaminaPointCoroutine);
    }


}
