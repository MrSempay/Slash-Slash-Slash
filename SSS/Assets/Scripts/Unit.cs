using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public abstract class Unit : MonoBehaviour
{
    private float _healthCurrent; // Начальное здоровье

    [SerializeField] private Image _healthBarFilling;
    
    [NonSerialized] public SpriteRenderer selfSprite; // собственный спрайт

    public void UnitStandart() { }
    public bool lookingRight = true; // Флаг, нужно ли отзеркаливать положение фрага (будет выполняться отзеркаливание только если направление изменилось, то есть флаг будет true
    public Dictionary<string, object> unitParameters;
    public string nameOfUnit;
    public Fsm _fsm;
    public Dictionary<string, object> baseParametersValues; // значения из скриптов Adjust
    public event Action<float> OnHealthChanged;       // пока что нигде не подписаны (изменяем ХП-бар тут же через ChangeHealthBar

    public float healthMax; // Начальное здоровье
    public float damageReduction; //Поглощение урона
    public float jumpForce; // сила прыжка
    public float speed; // скорость
    public float damage; // урон
    public float moneyFromKill; // урон
    public float experienceFromKill; // урон

    public float HealthCurrent
    {
        get { return _healthCurrent; }
        set
        {
            _healthCurrent = value;
            float _currentHealthAsPercantage = (float)HealthCurrent / healthMax;
            ChangeHealthBar(_currentHealthAsPercantage);
            // Вызываем событие, если есть подписчики
            OnHealthChanged?.Invoke(_healthCurrent); // пока что нигде не подписаны (изменяем ХП-бар тут же через ChangeHealthBar
        }
    }

    protected virtual void Awake()
    {

        unitParameters = (Dictionary<string, object>) AdjustUnitParameters.GetSetupOfUnit(nameOfUnit);
        //AssignParameters(unitParameters);
        StaticClassForAdditionalFunctions.AssignParametersAndProperties(AdjustUnitParameters.unitParameters, this, nameOfUnit);
        //StaticClassForAdditionalFunctions.AssignPropertyValues(AdjustUnitParameters.unitParameters, this, nameOfUnit);
        baseParametersValues = new Dictionary<string, object>(AdjustUnitParameters.unitParameters[nameOfUnit]);
        HealthCurrent = healthMax;

        _fsm = new Fsm();
        // на данный момент не уверен, что мы будем пользоваться словарём для доступа к параметрам юнита. Пока что просто по нему будем определять начальные параметры юнитов
        // при их создании. Вообще может было бы напрямую обращаться в таком случае к AdjustUnitParameters.GetParameter, но пока что оставим этот дубль словаря (хз зачем)
        /*healthMax = (float) unitParameters["Health"];
        damage = (float) unitParameters["Damage"];
        speed = (float) unitParameters["Speed"];
        jumpForce = (float) unitParameters["JumpPower"];
        damageReduction = (float) unitParameters["damageReduction"]; */
    }

    protected virtual void Start()
    {

    }

    // делаем unitFromWhoWasGottenDamage по умолчанию null, ибо в теории могут наносить в дальнейшем урон объекты, которые не будут наследоваться от Unit
    public virtual void GetDamage(float damageSize, Unit unitFromWhoWasGottenDamage = null)
    {
        HealthCurrent -= damageSize; // Уменьшаем здоровье

        if (HealthCurrent <= 0)
        {
            Die(unitFromWhoWasGottenDamage); // Вызываем метод смерти
        }
    }

    // по идее надо изменить на private, но оставим так для мгновенной смерти из спела SomeSpell1
    public void Die(Unit unitFromWhoWasGottenDamage = null)
    {
        Debug.Log(gameObject.name + " уничтожен!");
        if (unitFromWhoWasGottenDamage)
        {
            if (unitFromWhoWasGottenDamage.gameObject.CompareTag("Player")) // пока что только игрок пусть сможет получать что-то за смерть врагов. После это можно будет расширить
                                                                            // с помощью какого-нибудь интерфейса
            {
                unitFromWhoWasGottenDamage.GetExperienceAndMoneyFromKillingUnit(moneyFromKill, experienceFromKill);
            }
        }
        HealthCurrent = 0;
        Destroy(gameObject); // Уничтожаем объект
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
            FieldInfo fieldInfo = type.GetField(parameterName);

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
                HealthCurrent = HealthCurrent * (1 + (parameterIncreasing / 100));
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
            string parameterName = kvp.Key;
            float parameterIncreasing = kvp.Value;
            // Получаем поле с именем, соответствующим ключу словаря
            FieldInfo fieldInfo = type.GetField(parameterName);

            if (fieldInfo != null)
            {
                // Пытаемся преобразовать значение к типу поля
                try
                {
                    // 1. Получаем базовое значение (предполагаем, что оно типа object)
                    object baseValueObject = baseParametersValues[parameterName];

                    // 2. Преобразуем базовое значение к float
                    float baseValue = Convert.ToSingle(baseValueObject);

                    if (parameterName == "healthMax")
                    {
                        HealthCurrent = isIncreasing
                            ? HealthCurrent + (baseValue * (HealthCurrent / healthMax) * (parameterIncreasing / 100))
                            : HealthCurrent - (baseValue * (HealthCurrent / healthMax) * (parameterIncreasing / 100));
                    }

                    // 3. Вычисляем новое значение
                    float increasedValue = isIncreasing
                        ? (float)fieldInfo.GetValue(this) + (baseValue * (parameterIncreasing / 100))
                        : (float)fieldInfo.GetValue(this) - (baseValue * (parameterIncreasing / 100));

                    // 4. Преобразуем новое значение к типу поля
                    object convertedValue = Convert.ChangeType(increasedValue, fieldInfo.FieldType);

                    // 5. Присваиваем значение полю
                    fieldInfo.SetValue(this, convertedValue);

                }
                catch (InvalidCastException e)
                {
                    Debug.Log($"Could not convert value for parameter '{parameterName}' to type '{fieldInfo.FieldType.Name}': {e.Message}");
                }
            }
            else
            {
                Debug.LogWarning($"Field '{parameterName}' not found in class '{type.Name}'.");
            }


        }
        return changedParametersValuesAbs;
    }



    protected virtual void GetExperienceAndMoneyFromKillingUnit(float experience, float money) { }


}
