using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;
using static Unit;
using static UnityEngine.EventSystems.EventTrigger;
using static StaticClassForAdditionalFunctions;

public abstract class Unit : MonoBehaviour, IInventory
{


    [SerializeField] private float _damageReductionPercentage; //Поглощение урона
    [SerializeField] private float _healthCurrent; // Начальное здоровье
    [SerializeField] private Image _healthBarFilling;

    [NonSerialized] public SpriteRenderer selfSprite; // собственный спрайт
    [NonSerialized] public GameObject parametersBars;
    [NonSerialized] public Rigidbody2D rb;       // Rigidbody2D кубика

    public Animator animator; // Флаг, нужно ли отзеркаливать положение врага (будет выполняться отзеркаливание только если направление изменилось, то есть флаг будет true)
    public void UnitStandart() { }
    public bool lookingRight = true; // Флаг, нужно ли отзеркаливать положение врага (будет выполняться отзеркаливание только если направление изменилось, то есть флаг будет true)
    public bool isAlive = true; // Флаг, жив ли юнит 
    public bool areUpdatingFunctionsEnabled = true; // флаг, определяющий, может ли совершать какие-либо действия юнит
    public Dictionary<string, object> unitParameters;
    public string nameOfUnit;
    public string nameSoundGettingDamage;
    public string nameSoundAttakPeaked;
    public string nameSoundDeath;
    public string nameSoundWalk;
    public bool isGrounded = true; // Проверка, находится ли игрок на земле
    public bool isInvicible = false; // неуязвимость к УРОНУ (но не к эффектам, наверное)
    public Fsm _fsm;
    public Transform stunePlace; // место для спрайта эффекта стана
    public Dictionary<string, object> baseParametersValues; // значения из скриптов Adjust
    public event Action<float> OnHealthChanged;       // пока что нигде не подписаны (изменяем ХП-бар тут же через ChangeHealthBar
    public event Action<Unit, Unit> OnThisUnitWasAttacked; // сигнал эмулируется из юнита, которому был нанесён урон. Атакованного и атаковавшего юнитов передаём в качестве параметров
    public event Action<string> OnCastAnimationFinished;  // когда закончился каст какой-то абилки. Передаём название анимации полностью
    public event Action<string> OnCastAnimationPeacked;  // когда достигли кульминации анимации (может быть только одна в рамках одной анимации). Передаём название анимации полностью
    public event Action<bool> OnDirectionViewWasChanged;  //  true - когда смотрим направо
    public event Action OnBerserkerStateDeactivated;
    public delegate void UnitWasKilled(Unit unit); // шаблон функции
    public event UnitWasKilled onUnitWasKilled;         // экземляр(?) функции/сигнала(?)

    public float healthMax; // Начальное здоровье
    public float jumpForce; // сила прыжка
    public float speed; // скорость
    public float stuneChanceByStandartAttackPercentage; // шанс стана при обычной атаки, %. Пока что плевать, какая атака, хоть магическая, хоть дальняя, хоть ближняя
    public float timeStuneByStanartAttack; // время стана дефолтной атаки
    public float damage; // урон
    public float evasionPercentage; // уклонение, проценты
    public float moneyFromKill; // деньги за убийство юнита
    public int comboFromKill = 1; // комбо за убийство юнита. По умолчанию 1. Подразумеваю, что с развитием (???) игры будут добавляться враги, за которых можно дать и по-больше
    public int scoreFromKill; // очки за убийство юнита
    public float experienceFromKill; // опыт за убийство юнита
    public enum UNIT_STATE_ADDITIONAL { Berserker }
    private List<UNIT_STATE_ADDITIONAL> _listCurrentUnitStatesAdditional = new();

    #region IInventory interface

    [SerializeField] private Inventory _inventory; // инвентарий героя, назначаем в инспекторе

    [NonSerialized] private int _countAvailableSpellPlaces = 3; // количество ячеек в инвентаре для заклинаний, пока что... просто константа и не влияет на их количество
    [NonSerialized] private int _countAvailableAmmunitionPlaces = 3; // количество ячеек в инвентаре для аммуниции, пока что... просто константа и не влияет на их количество


    public Inventory Inventory // назначаем _inventory в инспекторе
    {
        get { return _inventory; }
        set
        {
            _inventory = value;
        }
    }
    public float Damage // назначаем _inventory в инспекторе
    {
        get { return damage; }
        set
        {
            damage = value;
        }
    }
    public Unit UnitSelf
    {
        get { return this; }
    }
    
    public virtual Type TypeInventory
    {
        get { return typeof(IInventoryUnit); }
    }

    public int CountAvailableAmmunitionPlaces // пока что _countAvailableAmmunitionPlaces задали явно константой в этом скрипте, позже можно будет вынести в скрипт Adjust
    {
        get { return _countAvailableAmmunitionPlaces; }
        set
        {
            _countAvailableAmmunitionPlaces = value;
        }
    }
    public int CountAvailableSpellPlaces // пока что _countAvailableSpellPlaces задали явно константой в этом скрипте, позже можно будет вынести в скрипт Adjust
    {
        get { return _countAvailableSpellPlaces; }
        set
        {
            _countAvailableSpellPlaces = value;
        }
    }
    public bool IsStaticInventory
    {
        get { return false; }
    }
    public Transform Transform
    {
        get { return transform; }
    }

    #endregion

    public virtual float CurrentHealth
    {
        get { return _healthCurrent; }
        set
        {
            if (value < 0)
            {
                _healthCurrent = 0;
            }
            else if (value > healthMax)
            {
                _healthCurrent = healthMax;
            }
            else
            {
                _healthCurrent = value;
            }

            float _currentHealthAsPercantage = _healthCurrent / healthMax;
            ChangeHealthBar(_currentHealthAsPercantage);
            // Вызываем событие, если есть подписчики
            OnHealthChanged?.Invoke(_healthCurrent); // пока что нигде не подписаны (изменяем ХП-бар тут же через ChangeHealthBar)
        }
    }
    public virtual float DamageReductionPercentage
    {
        get { return _damageReductionPercentage; }
        set
        {
            if (value > 100f)
            {
                _damageReductionPercentage = 100f;
            }
            else
            {
                _damageReductionPercentage = value;
            }
        }
    }



    public virtual void InitializeDependencies()
    {
        if (Inventory != null)
        {
            Inventory.Initialize(this);
        }
    }

    protected virtual void Awake()
    {

        unitParameters = (Dictionary<string, object>)AdjustUnitParameters.GetSetupOfUnit(nameOfUnit);
        //AssignParameters(unitParameters);
        AssignParametersAndProperties(AdjustUnitParameters.unitParameters, this, nameOfUnit);
        //StaticClassForAdditionalFunctions.AssignPropertyValues(AdjustUnitParameters.unitParameters, this, nameOfUnit);
        baseParametersValues = new Dictionary<string, object>(AdjustUnitParameters.unitParameters[nameOfUnit]);
        CurrentHealth = healthMax;
        _fsm = new Fsm();

        _fsm.AddState(new FsmStateStuneUnit(_fsm, gameObject));

        Transform transformParametersBars = transform.Find("ParametersBars");
        if (transformParametersBars != null) parametersBars = transformParametersBars.gameObject;

        selfSprite = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        // на данный момент не уверен, что мы будем пользоваться словарём для доступа к параметрам юнита. Пока что просто по нему будем определять начальные параметры юнитов
        // при их создании. Вообще может было бы напрямую обращаться в таком случае к AdjustUnitParameters.GetParameter, но пока что оставим этот дубль словаря (хз зачем)
        /*healthMax = (float) unitParameters["Health"];
        damage = (float) unitParameters["Damage"];
        speed = (float) unitParameters["Speed"];
        jumpForce = (float) unitParameters["JumpPower"];
        DamageReductionPercentage = (float) unitParameters["DamageReductionPercentage"]; */
    }

    protected virtual void Start()
    {

    }

    public void AddUnitStateAdditional(UNIT_STATE_ADDITIONAL playerState)
    {
        if (!_listCurrentUnitStatesAdditional.Contains(playerState))
        {
            _listCurrentUnitStatesAdditional.Add(playerState);
        }
    }
    public void RemoveUnitStateAdditional(UNIT_STATE_ADDITIONAL playerState)
    {
        if (_listCurrentUnitStatesAdditional.Contains(playerState))
        {
            _listCurrentUnitStatesAdditional.Remove(playerState);
        }
    }
    public bool HasUnitStateAdditional(UNIT_STATE_ADDITIONAL playerState)
    {
        return _listCurrentUnitStatesAdditional.Contains(playerState);
    }

    public virtual void MakeDamageToUnit(Unit unitWhichIsAttacked)
    {

    }

    // делаем unitFromWhoWasGottenDamage по умолчанию null, ибо в теории могут наносить в дальнейшем урон объекты, которые не будут наследоваться от Unit
    // Возвращает bool оттого, что необходимо нам проверять в производной функции, нужно ли завершать её досрочно. Если базовая возвращает true, значит производную функцию нужно прервать
    public virtual bool GetDamage(float damageSize, Unit unitFromWhoWasGottenDamage = null, bool wasDamageByStandartAttack = true)
    {
        if (isAlive)
        {
            ThisUnitWasAttacked(this, unitFromWhoWasGottenDamage);

            if (!isInvicible)
            {
                if (unitFromWhoWasGottenDamage)
                {
                    if (CheckChance(evasionPercentage)) // шанс уклониться от урона. Не важно, от какого, главное, что от исходящего от другого юнита
                    {
                        return false; // уклонились
                    }

                    if (wasDamageByStandartAttack)
                    {
                        if (CheckChance(unitFromWhoWasGottenDamage.stuneChanceByStandartAttackPercentage)) // шанс, что застанили текущей атакой
                        {
                            StuneThisUnit(unitFromWhoWasGottenDamage.timeStuneByStanartAttack);
                        }
                    }

                    unitFromWhoWasGottenDamage.SomeUnitWasHit(this);
                }

                CurrentHealth -= damageSize - (damageSize * DamageReductionPercentage / 100); // Уменьшаем здоровье
                AudioManager.Instance.StartSoundEffectAtSpecifiedObject(nameSoundGettingDamage, gameObject, AudioManager.TYPE_SOUND.GetDamage, AudioManager.TYPE_AUDIO_SOURCE._3DStandard);

                if (CurrentHealth <= 0)
                {
                    Die(unitFromWhoWasGottenDamage); // Вызываем метод смерти
                }
                return true; // получили урон
            }
            return false; // неуязвим
        }
        return false; // мёртв
    }

    public virtual void Heal(float healthHealAmount)
    {
        CurrentHealth += healthHealAmount;
    }

    // по идее надо изменить на private, но оставим так для мгновенной смерти из спела SomeSpell1
    public virtual void Die(Unit unitFromWhoWasGottenDamage = null)
    {
        if (isAlive) // по идее когда помирает юнит, у него коллайдер отключается, но в ряде случаев это происходит не сразу (например, когда юнит умирает в падении и нужно чтоб он корректно приземлился (его останки))
        {
            Debug.Log(gameObject.name + " уничтожен!");

            isAlive = false;

            AudioManager.Instance.StartSoundEffectAtSpecifiedObject(nameSoundDeath, gameObject, AudioManager.TYPE_SOUND.AttackPeak, AudioManager.TYPE_AUDIO_SOURCE._3DStandard);

            if (unitFromWhoWasGottenDamage)
            {
                if (unitFromWhoWasGottenDamage.gameObject.CompareTag("Player")) // пока что только игрок пусть сможет получать что-то за смерть врагов. После это можно будет расширить
                                                                                // с помощью какого-нибудь интерфейса
                {
                    unitFromWhoWasGottenDamage.SomeUnitWasDestroyed(this);
                }
            }
            CurrentHealth = 0;
            parametersBars.SetActive(false); // при смерте отключает все полоски здоровья/стамины прочего

            onUnitWasKilled?.Invoke(this);
            //Destroy(gameObject); // Уничтожаем объект
        }
    }


    private void ChangeHealthBar(float valueAsPercantage)
    {
        _healthBarFilling.fillAmount = valueAsPercantage;
    }

    // все дочерние параметры, относящиеся к производным классам от данного должны быть public

    // функция для увеличения параметров на процент. Процент стакается, то есть увеличиваение идёт от текущего значения параметра, а не от базового
    public Dictionary<string, float> ChangeUnitParametersByPercentageCumilative(Dictionary<string, float> parametersIncreases, bool isIncreasing)
    {

        Type type = this.GetType(); // Получаем тип текущего класса
        Dictionary<string, float> changedParametersValuesAbs = new Dictionary<string, float>();
        foreach (var kvp in parametersIncreases)
        {
            string parameterName = kvp.Key;
            float parameterIncreasing = kvp.Value;

            // Получаем поле с именем, соответствующим ключу словаря
            System.Reflection.FieldInfo fieldInfo = type.GetField(parameterName);

            if (fieldInfo != null)
            {
                // Пытаемся преобразовать значение к типу поля
                try
                {
                    object increasedValue;
                    changedParametersValuesAbs[parameterName] = (float)fieldInfo.GetValue(this);
                    if (isIncreasing) { increasedValue = (float)fieldInfo.GetValue(this) * (1 + (parameterIncreasing / 100)); }
                    else { increasedValue = (float)fieldInfo.GetValue(this) * (1 - (parameterIncreasing / 100)); }
                    fieldInfo.SetValue(this, increasedValue); // Присваиваем значение полю
                    // возвращаем массив с абсолютными значениями изменений наших параметров. Чтоб потом можно было высчитать, сколько это абсолютное значение составляет процентов
                    // от текущего значения параметра. К примеру было 10к ХП, увеличили на 50%, вернули 5к для данного свойства. После, когда так или иначе у нас будет 25к ХП мы
                    // всё равно будем помнить, что вот от данного увеличения параметра на проценты у нас прибавилось 5к ХП. Далее чтоб понять, сколько нужно процентов отнимать, 
                    // можно рассчитать 25к/5к = 20%. 
                    changedParametersValuesAbs[parameterName] = Math.Abs((changedParametersValuesAbs[parameterName] - (float)increasedValue));
                }
                catch (InvalidCastException e)
                {
                    Debug.LogError($"Could not convert value for parameter '{parameterName}' to type '{fieldInfo.FieldType.Name}': {e.Message}");
                }
            }
            else
            {
                Debug.LogWarning($"Field '{parameterName}' not found in class '{type.Name}'.");
            }

            if (parameterName == "healthMax")
            {
                CurrentHealth = CurrentHealth * (1 + (parameterIncreasing / 100));
            }
        }
        return changedParametersValuesAbs;
    }

    // функция, увеличивающая текущий параметр на процент об базового значения данного параметра, то есть увеличение/уменьшение параметров на % будет всегда фиксированным
    public Dictionary<string, float> ChangeUnitParametersByPercentage(Dictionary<string, float> parametersIncreases, bool isIncreasing)
    {
        Type type = this.GetType(); // Получаем тип текущего класса
        Dictionary<string, float> changedParametersValuesAbs = new Dictionary<string, float>();
        foreach (var kvp in parametersIncreases)
        {
            string parameterOrPropertyName = kvp.Key;
            float parameterOrPropertyIncreasing = kvp.Value;
            // Получаем поле с именем, соответствующим ключу словаря
            System.Reflection.FieldInfo fieldInfo = type.GetField(parameterOrPropertyName);

            if (fieldInfo != null)
            {
                // Пытаемся преобразовать значение к типу поля
                try
                {
                    // 1. Получаем базовое значение (предполагаем, что оно типа object)
                    object baseValueObject = baseParametersValues[parameterOrPropertyName];

                    // 2. Преобразуем базовое значение к Double
                    double baseValueDouble = Convert.ToDouble(baseValueObject);
                    double parameterIncreasingDouble = Convert.ToDouble(parameterOrPropertyIncreasing);

                    double increasedValue = isIncreasing
                        ? Convert.ToDouble(fieldInfo.GetValue(this)) + (baseValueDouble * (parameterIncreasingDouble / 100))
                        : Convert.ToDouble(fieldInfo.GetValue(this)) - (baseValueDouble * (parameterIncreasingDouble / 100));

                    // 4. Преобразуем обратно к типу поля
                    object convertedValue = Convert.ChangeType(increasedValue, fieldInfo.FieldType);


                    fieldInfo.SetValue(this, convertedValue);
                    if (parameterOrPropertyName == C.DK.healthMax || parameterOrPropertyName == C.DK.staminaMax) // если значение устанавливаемого параметра подразумевает наличие текущего и максимального
                                                                                                                 // значений, вызываем функцию AdjustCurrentParametersValues, которая правильно настроит 
                                                                                                                 // текущее значение параметра
                        AdjustCurrentParametersValuesPercentage(parameterOrPropertyName, isIncreasing, parameterOrPropertyIncreasing, baseValueDouble);
                    // 5. Присваиваем значение полю
                }
                catch (InvalidCastException e)
                {
                    Debug.Log($"Could not convert value for parameter '{parameterOrPropertyName}' to type '{fieldInfo.FieldType.Name}': {e.Message}");
                }
            }
            else
            {
                PropertyInfo propertyInfo = type.GetProperty(parameterOrPropertyName);

                if (propertyInfo != null)
                {

                    double propertyIncreasingDouble = Convert.ToDouble(parameterOrPropertyIncreasing);

                    double increasedValue = isIncreasing
                        ? Convert.ToDouble(propertyInfo.GetValue(this)) + propertyIncreasingDouble
                        : Convert.ToDouble(propertyInfo.GetValue(this)) - propertyIncreasingDouble;

                    // 4. Преобразуем обратно к типу поля
                    object convertedValue = Convert.ChangeType(increasedValue, propertyInfo.PropertyType);

                    propertyInfo.SetValue(this, convertedValue);
                }
                else
                {
                    Debug.LogWarning($"Field '{parameterOrPropertyName}' not found in class '{type.Name}'.");
                }

            }


        }
        return changedParametersValuesAbs;
    }

    public void ChangeUnitParametersAndPropertiesByAbsolute(Dictionary<string, float> parametersOrPropertyIncreases, bool isIncreasing)
    {
        Type type = this.GetType(); // Получаем тип текущего класса

        foreach (var kvp in parametersOrPropertyIncreases)
        {
            string parameterOrPropertyName = kvp.Key;
            float parameterOrPropertyIncreasing = kvp.Value;
            // Получаем поле с именем, соответствующим ключу словаря
            System.Reflection.FieldInfo fieldInfo = type.GetField(parameterOrPropertyName);

            if (fieldInfo != null)
            {
                // Пытаемся преобразовать значение к типу поля
                try
                {
                    double parameterIncreasingDouble = Convert.ToDouble(parameterOrPropertyIncreasing);

                    double increasedValue = isIncreasing
                        ? Convert.ToDouble(fieldInfo.GetValue(this)) + parameterIncreasingDouble
                        : Convert.ToDouble(fieldInfo.GetValue(this)) - parameterIncreasingDouble;

                    // 4. Преобразуем обратно к типу поля
                    object convertedValue = Convert.ChangeType(increasedValue, fieldInfo.FieldType);


                    fieldInfo.SetValue(this, convertedValue);
                    if (parameterOrPropertyName == C.DK.healthMax || parameterOrPropertyName == C.DK.staminaMax) // если значение устанавливаемого параметра подразумевает наличие текущего и максимального
                                                                                                                 // значений, вызываем функцию AdjustCurrentParametersValues, которая правильно настроит 
                                                                                                                 // текущее значение параметра
                        AdjustCurrentParametersValuesAbsolute(parameterOrPropertyName, isIncreasing, parameterOrPropertyIncreasing);
                    // 5. Присваиваем значение полю
                }
                catch (InvalidCastException e)
                {
                    Debug.Log($"Could not convert value for parameter '{parameterOrPropertyName}' to type '{fieldInfo.FieldType.Name}': {e.Message}");
                }
            }
            else
            {
                PropertyInfo propertyInfo = type.GetProperty(parameterOrPropertyName);

                if (propertyInfo != null)
                {

                    double propertyIncreasingDouble = Convert.ToDouble(parameterOrPropertyIncreasing);

                    double increasedValue = isIncreasing
                        ? Convert.ToDouble(propertyInfo.GetValue(this)) + propertyIncreasingDouble
                        : Convert.ToDouble(propertyInfo.GetValue(this)) - propertyIncreasingDouble;

                    // 4. Преобразуем обратно к типу поля
                    object convertedValue = Convert.ChangeType(increasedValue, propertyInfo.PropertyType);

                    propertyInfo.SetValue(this, convertedValue);
                }
                else
                {
                    Debug.LogWarning($"Field '{parameterOrPropertyName}' not found in class '{type.Name}'.");
                }

            }


        }
    }


    // для работы данной функции необходимо, чтоб значение максимального параметра имело имя по типу: healthMax, а свойства с текущем значением - CurrentHealth.
    // Функция найдёт свойство с текущим значением CurrentHealth отняв от healthMax постфикс Max, увеличив первую букву и добавив перед ней Current
    private void AdjustCurrentParametersValuesPercentage(string nameOfMaxParameter, bool isIncreasing, float increasingValuePercentage, double baseParameterValue)
    {
        string subString = nameOfMaxParameter.Substring(0, nameOfMaxParameter.Length - 3);
        string firstCharToUpper = subString.Substring(0, 1).ToUpper() + subString.Substring(1);
        string nameOfCurrentProperty = "Current" + firstCharToUpper;

        Type type = this.GetType(); // Получаем тип текущего класса
        PropertyInfo currentPropertyInfo = type.GetProperty(nameOfCurrentProperty);
        System.Reflection.FieldInfo maxFieldInfo = type.GetField(nameOfMaxParameter);

        //Debug.Log(nameOfCurrentProperty);
        //Debug.Log(currentPropertyInfo);
        //Debug.Log(currentPropertyInfo.CanWrite);
        if (currentPropertyInfo != null && currentPropertyInfo.CanWrite) //Убедимся, что свойство существует и доступно для записи
        {
            // Пытаемся преобразовать значение к типу свойства 
            try
            {
                double currentPropertyValue = Convert.ToDouble(currentPropertyInfo.GetValue(this));
                double maxFieldValue = Convert.ToDouble(maxFieldInfo.GetValue(this));

                double assigningCurrentPropertyValue = isIncreasing
                        ? currentPropertyValue + (baseParameterValue * (currentPropertyValue / (maxFieldValue - (baseParameterValue * (increasingValuePercentage / 100)))) * (increasingValuePercentage / 100)) * Math.Sign(increasingValuePercentage) // в теории, при формальном увеличении какого-то параметра мы можем фактически его уменьшать. Типа предмет при надевании даёт штраф в 15% к чему-то
                        : currentPropertyValue - (baseParameterValue * (currentPropertyValue / (maxFieldValue + (baseParameterValue * (increasingValuePercentage / 100)))) * (increasingValuePercentage / 100)) * Math.Sign(increasingValuePercentage);

                //Debug.Log(assigningCurrentPropertyValue);
                object convertedValue = Convert.ChangeType(assigningCurrentPropertyValue, currentPropertyInfo.PropertyType);
                currentPropertyInfo.SetValue(this, convertedValue, null); // Присваиваем значение свойству 
            }
            catch (InvalidCastException e)
            {
                Debug.LogError($"Could not convert value for property '{currentPropertyInfo}' to type '{currentPropertyInfo.PropertyType.Name}': {e.Message}");
            }
        }
        else
        {
            if (currentPropertyInfo == null)
            {
                Debug.LogWarning($"Property '{currentPropertyInfo}' not found in class '{type.Name}'.");
            }
            else if (!currentPropertyInfo.CanWrite)
            {
                Debug.LogWarning($"Property '{currentPropertyInfo}' in class '{type.Name}' does not have a setter (is read-only).");
            }
        }
    }
    private void AdjustCurrentParametersValuesAbsolute(string nameOfMaxParameter, bool isIncreasing, float increasingValueAbsolute)
    {
        string subString = nameOfMaxParameter.Substring(0, nameOfMaxParameter.Length - 3);
        string firstCharToUpper = subString.Substring(0, 1).ToUpper() + subString.Substring(1);
        string nameOfCurrentProperty = "Current" + firstCharToUpper;

        Type type = this.GetType(); // Получаем тип текущего класса
        PropertyInfo currentPropertyInfo = type.GetProperty(nameOfCurrentProperty);
        System.Reflection.FieldInfo maxFieldInfo = type.GetField(nameOfMaxParameter);

        //Debug.Log(nameOfCurrentProperty);
        //Debug.Log(currentPropertyInfo);
        //Debug.Log(currentPropertyInfo.CanWrite);
        if (currentPropertyInfo != null && currentPropertyInfo.CanWrite) //Убедимся, что свойство существует и доступно для записи
        {
            // Пытаемся преобразовать значение к типу свойства 
            try
            {
                double currentPropertyValue = Convert.ToDouble(currentPropertyInfo.GetValue(this));

                double assigningCurrentPropertyValue = isIncreasing
                        ? currentPropertyValue + increasingValueAbsolute
                        : currentPropertyValue - increasingValueAbsolute;

                //Debug.Log(assigningCurrentPropertyValue);
                object convertedValue = Convert.ChangeType(assigningCurrentPropertyValue, currentPropertyInfo.PropertyType);
                currentPropertyInfo.SetValue(this, convertedValue, null); // Присваиваем значение свойству 
            }
            catch (InvalidCastException e)
            {
                Debug.LogError($"Could not convert value for property '{currentPropertyInfo}' to type '{currentPropertyInfo.PropertyType.Name}': {e.Message}");
            }
        }
        else
        {
            if (currentPropertyInfo == null)
            {
                Debug.LogWarning($"Property '{currentPropertyInfo}' not found in class '{type.Name}'.");
            }
            else if (!currentPropertyInfo.CanWrite)
            {
                Debug.LogWarning($"Property '{currentPropertyInfo}' in class '{type.Name}' does not have a setter (is read-only).");
            }
        }
    }

    protected virtual void GetExperienceAndMoneyFromKillingUnit(float experience, float money, int comboFromKill, int score) { }
    protected virtual void SomeUnitWasDestroyed(Unit unit) { } // вызывается для (из) того, кто уничтожил какой-то юнит
    protected virtual void SomeUnitWasHit(Unit unit) { } // вызывается для (из) того, кто нанёс урон какому-то юниту
    protected virtual void ThisUnitWasAttacked(Unit unitWhichWasAttacked, Unit attackingUnit) // вызывается в юните, который получил урон
    {
        OnThisUnitWasAttacked?.Invoke(unitWhichWasAttacked, attackingUnit);
    }
    public virtual void DirectionViewWasChanged(bool lookingRight) // вызывается в юните, когда тот сменил направление взора
    {
        OnDirectionViewWasChanged?.Invoke(lookingRight);
    }
    public void CastAnimationFinished(string nameFinishedAnimation)
    {
        OnCastAnimationFinished?.Invoke(nameFinishedAnimation);
    }
    public void CastAnimationPeaked(string namePeackedAnimation)
    {
        OnCastAnimationPeacked?.Invoke(namePeackedAnimation);
    }
    public virtual void SomeAnimationUnitWasFinished(string nameFinishedAnimation) // Когда анимация закончилась
    {
        string lastFourChars = "";
        if (nameFinishedAnimation.Length >= 4) // проверяем, что строка достаточно длинная
        {
            lastFourChars = nameFinishedAnimation.Substring(nameFinishedAnimation.Length - 4);
        }
        switch (lastFourChars) // проверяем анимационные префиксы (постфиксы...)
        {
            case C.Prefixes.Cast:
                CastAnimationFinished(nameFinishedAnimation); // просто обёртка над сигналом, определён в Unit
                break;
        }
    }

    public void BerserkerStateDeactivated() // ещё одна обёртка над методом. Вызываем из Berserker при Deactivate
    {
        OnBerserkerStateDeactivated?.Invoke();
    }
    public virtual void SomeAnimationWasStarted(string nameStartedAnimation) // Когда анимация достигла целевой точки, но не конца. Для Peak такая точка может быть только одна в анимации
    {        
        switch (nameStartedAnimation) // проверяем анимационные префиксы (постфиксы...)
        {
            case C.Animations.Walk:
                //Debug.Log("Пик!");
                AudioManager.Instance.StartSoundEffectAtSpecifiedObject(nameSoundWalk, gameObject, AudioManager.TYPE_SOUND.Walk, AudioManager.TYPE_AUDIO_SOURCE._3DStandard);
                break;
            case C.Animations.Attack: // по идее это та же атака, что и AttackPeaked. Просто звук именно атаки у нас начинается при достижении ею пикового значения, а отменяем прочие
                                      // звуковые эффекты мы по её старту. Делаем это потому, что тяжело найти звуковой эффект, который бы сочетался полностью с началом анимации и действовал
                                      // на протяжении всей её длительности. А, к чёрту, передумал - лучше бахну я тупо для анимации атаки у героя отдельное состояние, а то вся текущая
                                      // проблема заключается в том, что у нас в одном состоянии могут быть сразу 2 анимации, оттого тяжко Герою звуки настроить
                //Debug.Log("Пик!");
                //AudioManager.Instance.StopSomeTypeSoundOnObject(AudioManager.TYPE_SOUND.Walk, gameObject);
                break;
        }
    }
    public virtual void SomeAnimationUnitWasPeaked(string namePeackedAnimation) // Когда анимация достигла целевой точки, но не конца. Для Peak такая точка может быть только одна в анимации
    {
        //Debug.Log("FAISFHJASKJHASK:FAJS:KFAK:FAS:KFASK:F"); 
        string lastFourChars = "";
        if (namePeackedAnimation.Length >= 4) // проверяем, что строка достаточно длинная
        {
            lastFourChars = namePeackedAnimation.Substring(namePeackedAnimation.Length - 4);
        }
        switch (lastFourChars) // проверяем анимационные префиксы (постфиксы...)
        {
            case C.Prefixes.Peak:
                CastAnimationPeaked(namePeackedAnimation); // просто обёртка над сигналом, определён в Unit
                break;
        }
        switch (namePeackedAnimation) // проверяем анимационные префиксы (постфиксы...)
        {
            case C.Animations.AttackPeaked:
                //Debug.Log("Пик!");
                AudioManager.Instance.StartSoundEffectAtSpecifiedObject(nameSoundAttakPeaked, gameObject, AudioManager.TYPE_SOUND.AttackPeak, AudioManager.TYPE_AUDIO_SOURCE._3DStandard);
                break;
        }
    }

    #region Stune mechanic

    private float _currentStuneTimeRemaining;
    private Coroutine _waitStuneTimeCoroutine; // Начальное здоровье
    protected virtual void StuneThisUnit(float timeStune)
    {
        if (stunePlace != null)
        {
            if (timeStune > _currentStuneTimeRemaining)
            {
                _currentStuneTimeRemaining = timeStune;
                if (_waitStuneTimeCoroutine != null) StopCoroutine(_waitStuneTimeCoroutine);
                _waitStuneTimeCoroutine = StartCoroutine(WaitBeforeEndingStune(timeStune));

                _fsm.SetState<FsmStateStuneUnit>();

                GameManager.Instance.InvokeAppearingSprite(C.Other.Stune, stunePlace, timeStune, true); // на самом деле хотелось бы вынести это в состояние FsmStateStuneUnit тоже, но
                                                                                                    // на данный момент мы не можем отсылать параметры в состояние, надо как-нибудь улучшить

                                                                                                    // выше вроде как бред написан. Если мы будем в состоянии стана, мы не сможем в него
                                                                                                    // повторно входить, поэтому спрайт стана надо вызывать тут (по идее). 
            }
        }
    }

    IEnumerator WaitBeforeEndingStune(float timeStune)
    {

        while (_currentStuneTimeRemaining > 0)
        {
            yield return null; // Ждем один кадр

            // Уменьшаем оставшееся время
            _currentStuneTimeRemaining -= Time.deltaTime;

            // Дополнительная проверка, если _currentStuneTimeRemaining случайно станет отрицательным 
            if (_currentStuneTimeRemaining < 0)
            {
                _currentStuneTimeRemaining = 0;
            }
        }
        _fsm.SetStateIdle(this);
    }
# endregion

    public virtual void OnDestroy()
    {
        _fsm.StateCurrent?.OnDestroy();
    }

}
