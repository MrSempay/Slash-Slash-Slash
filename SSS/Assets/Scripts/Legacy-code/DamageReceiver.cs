using System;
using UnityEngine;

public class DamageReceiver : MonoBehaviour
{
    public float healthCurrent = 100f; // Начальное здоровье
    public float healthMax = 100f; // Начальное здоровье
    public float damageReduction = 0f; //Поглощение урона
    public event Action<float> HealthChanged;
    public void TakeDamage(float damage)
    {
        healthCurrent -= damage; // Уменьшаем здоровье
        Debug.Log(gameObject.name + " получил урон: " + damage + ", осталось здоровья: " + healthCurrent);

        if (healthCurrent <= 0)
        {
            Die(); // Вызываем метод смерти
        }
        else
        {
            float _currentHealthAsPercantage = (float)healthCurrent / healthMax;
            HealthChanged?.Invoke(_currentHealthAsPercantage);
        }
    }

    void Die()
    {
        Debug.Log(gameObject.name + " уничтожен!");
        HealthChanged?.Invoke(0);
        Destroy(gameObject); // Уничтожаем объект
    }
}