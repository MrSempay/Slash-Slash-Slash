using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.AI;

[System.Serializable]
public class DataWrapperUnit
{
    public float _healthCurrent; // Начальное здоровье
    public bool lookingRight = true; // Флаг, нужно ли отзеркаливать положение врага (будет выполняться отзеркаливание только если направление изменилось, то есть флаг будет true)
    public bool isAlive = true; // Флаг, жив ли юнит
    public string nameOfUnit;
    public float healthMax; // Начальное здоровье
    public float DamageReductionPercentage; //Поглощение урона
    public float jumpForce; // сила прыжка
    public float speed; // скорость
    public float damage; // урон
    public float moneyFromKill; // деньги за убийство юнита
    public float experienceFromKill; // опыт за убийство юнита


    //private Coroutine _zeroizeKillComboTicksCoroutine;
    //private Coroutine _recoverStaminaPointCoroutine;
    public bool _isTranslatingEquipment; // флаг, маркерующий, переносим ли мы какое-либо снаряжение в инвентарь
    public int _countAccessToUpInSchool; 
    public float _currentExperience;
    public float _currentMoney;
    public int _currentLevel;
    public int _currentKillCombo;
    public int _currentStamina;

    public bool isGrounded = true; // Проверка, находится ли игрок на земле
    public bool areUpdatingFunctionsEnabled = true; // Проверка, находится ли игрок на земле
    public float timeRecoverStaminaPoint; // КД восстановление одного заряда выносливости
    public float timeZeroizeKillComboTicks; // время для сбрасывания текущего комбо за убийства
    public float experienceToNextLevel;
    public float increasingGettingExperienceByKillComboTickPercentage;
    public float increasingGettingMoneyByKillComboTickPercentage;
    public int staminaMax;
    public Dictionary<string, float> increasingParametersByLevelUpPercentage;

    public Vector2 nextPointInPath; // Текущая целевая позиция (вторая точка в пути)
    public Transform currentTargetTransform; // Текущая целевая позиция (вторая точка в пути)
    public float callDownMeleeAttack;
    public int currentCornerIndex; // Индекс текущего угла в пути
    public bool isPathValid; // Флаг, указывающий, что путь валиден
    public bool isInstancedByLevel = false; // Флаг, указывающий, что враг был заспавнен скриптом спавна на уроне, а не добавлен на сцену вручную
    public List<Unit> listOfUnitsInAttackArea = new List<Unit>();





}
