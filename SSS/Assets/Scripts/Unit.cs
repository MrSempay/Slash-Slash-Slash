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
    [SerializeField] private Image _healthBarFilling;
    
    [NonSerialized] public SpriteRenderer selfSprite; // собственный спрайт

    public void UnitStandart() { }
    public bool lookingRight = true; // Флаг, нужно ли отзеркаливать положение фрага (будет выполняться отзеркаливание только если направление изменилось, то есть флаг будет true
    public float healthCurrent; // Начальное здоровье
    public event Action<float> HealthChanged;
    public Dictionary<string, object> unitParameters;
    public string nameOfUnit;
    public Fsm _fsm;

    public float healthMax; // Начальное здоровье
    public float damageReduction; //Поглощение урона
    public float jumpForce; // сила прыжка
    public float speed; // скорость
    public float damage; // урон
    public float moneyFromKill; // урон
    public float experienceFromKill; // урон
    
    protected virtual void Awake()
    {
        unitParameters = (Dictionary<string, object>) AdjustUnitParameters.GetSetupOfUnit(nameOfUnit);
        AssignParameters(unitParameters);
        healthCurrent = healthMax;
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
        healthCurrent -= damageSize; // Уменьшаем здоровье

        if (healthCurrent <= 0)
        {
            Die(unitFromWhoWasGottenDamage); // Вызываем метод смерти
        }
        else
        {
            float _currentHealthAsPercantage = (float)healthCurrent / healthMax;
            ChangeHealthBar(_currentHealthAsPercantage);
        }
    }

    void Die(Unit unitFromWhoWasGottenDamage = null)
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
        HealthChanged?.Invoke(0);
        Destroy(gameObject); // Уничтожаем объект
    }


    private void ChangeHealthBar(float valueAsPercantage)
    {
        _healthBarFilling.fillAmount = valueAsPercantage;
    }

    void AssignParameters(Dictionary<string, object> objectParameters)
    {
        Type type = this.GetType(); // Получаем тип текущего класса

        foreach (var kvp in objectParameters)
        {
            string parameterName = kvp.Key;
            object parameterValue = kvp.Value;
            
            // Получаем поле с именем, соответствующим ключу словаря
            FieldInfo fieldInfo = type.GetField(parameterName);

            if (fieldInfo != null)
            {
                // Пытаемся преобразовать значение к типу поля
                try
                {
                    object convertedValue = Convert.ChangeType(parameterValue, fieldInfo.FieldType);
                    fieldInfo.SetValue(this, convertedValue); // Присваиваем значение полю
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
        }
    }

    public void ChangeUnitParametersByPercentage(Dictionary<string, float> parametersIncreases)
    {
        Type type = this.GetType(); // Получаем тип текущего класса

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
                    object increasedValue = (float) fieldInfo.GetValue(this) * (1 + (parameterIncreasing/100));
                    fieldInfo.SetValue(this, increasedValue); // Присваиваем значение полю
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
                healthCurrent = healthCurrent * (1 + (parameterIncreasing / 100));
            }
        }
    }

    protected virtual void GetExperienceAndMoneyFromKillingUnit(float experience, float money) { }


}
