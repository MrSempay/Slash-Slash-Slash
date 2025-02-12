using System;
using UnityEngine;

public class DamageReceiver : MonoBehaviour
{
    public float healthCurrent = 100f; // ��������� ��������
    public float healthMax = 100f; // ��������� ��������
    public float damageReduction = 0f; //���������� �����
    public event Action<float> HealthChanged;
    public void TakeDamage(float damage)
    {
        healthCurrent -= damage; // ��������� ��������
        Debug.Log(gameObject.name + " ������� ����: " + damage + ", �������� ��������: " + healthCurrent);

        if (healthCurrent <= 0)
        {
            Die(); // �������� ����� ������
        }
        else
        {
            float _currentHealthAsPercantage = (float)healthCurrent / healthMax;
            HealthChanged?.Invoke(_currentHealthAsPercantage);
        }
    }

    void Die()
    {
        Debug.Log(gameObject.name + " ���������!");
        HealthChanged?.Invoke(0);
        Destroy(gameObject); // ���������� ������
    }
}