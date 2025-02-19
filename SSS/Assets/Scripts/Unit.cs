using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public abstract class Unit : MonoBehaviour
{
    [SerializeField] private Image _healthBarFilling;
    
    [NonSerialized] public SpriteRenderer selfSprite; // ����������� ������

    public void UnitStandart() { }
    public bool lookingRight = true; // ����, ����� �� ������������� ��������� ����� (����� ����������� �������������� ������ ���� ����������� ����������, �� ���� ���� ����� true
    public float healthCurrent; // ��������� ��������
    public event Action<float> HealthChanged;
    public Dictionary<string, object> unitParameters;
    public string nameOfUnit;
    public Fsm _fsm;

    public float healthMax; // ��������� ��������
    public float damageReduction; //���������� �����
    public float jumpForce; // ���� ������
    public float speed; // ��������
    public float damage; // ����
    
    protected virtual void Awake()
    {
        unitParameters = (Dictionary<string, object>) AdjustUnitParameters.GetSetupOfUnit(nameOfUnit);
        AssignParameters(unitParameters);
        healthCurrent = healthMax;
        // �� ������ ������ �� ������, ��� �� ����� ������������ ������� ��� ������� � ���������� �����. ���� ��� ������ �� ���� ����� ���������� ��������� ��������� ������
        // ��� �� ��������. ������ ����� ���� �� �������� ���������� � ����� ������ � AdjustUnitParameters.GetParameter, �� ���� ��� ������� ���� ����� ������� (�� �����)
        /*healthMax = (float) unitParameters["Health"];
        damage = (float) unitParameters["Damage"];
        speed = (float) unitParameters["Speed"];
        jumpForce = (float) unitParameters["JumpPower"];
        damageReduction = (float) unitParameters["damageReduction"]; */
    }

    protected virtual void Start()
    {

    }

    public virtual void GetDamage(float damageSize)
    {
        healthCurrent -= damageSize; // ��������� ��������

        if (healthCurrent <= 0)
        {
            Die(); // �������� ����� ������
        }
        else
        {
            float _currentHealthAsPercantage = (float)healthCurrent / healthMax;
            ChangeHealthBar(_currentHealthAsPercantage);
        }
    }

    void Die()
    {
        Debug.Log(gameObject.name + " ���������!");
        HealthChanged?.Invoke(0);
        Destroy(gameObject); // ���������� ������
    }


    private void ChangeHealthBar(float valueAsPercantage)
    {
        _healthBarFilling.fillAmount = valueAsPercantage;
    }

    void AssignParameters(Dictionary<string, object> objectParameters)
    {
        Type type = this.GetType(); // �������� ��� �������� ������

        foreach (var kvp in objectParameters)
        {
            string parameterName = kvp.Key;
            object parameterValue = kvp.Value;
            
            // �������� ���� � ������, ��������������� ����� �������
            FieldInfo fieldInfo = type.GetField(parameterName);

            if (fieldInfo != null)
            {
                // �������� ������������� �������� � ���� ����
                try
                {
                    object convertedValue = Convert.ChangeType(parameterValue, fieldInfo.FieldType);
                    fieldInfo.SetValue(this, convertedValue); // ����������� �������� ����
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

}
