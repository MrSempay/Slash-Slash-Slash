using UnityEngine;

public class DamageDealer : MonoBehaviour
{
    public float damageAmount = 10f; // Количество урона, наносимого за раз
    public float damageInterval = 1f; // Интервал между нанесением урона (в секундах)

    private float timer; // Таймер для отсчета времени между нанесением урона
    private GameObject currentTarget; //Текущая цель

    void Update()
    {
        if (currentTarget != null)
        {
            timer += Time.deltaTime; // Увеличиваем таймер

            if (timer >= damageInterval)
            {
                timer = 0f; // Сбрасываем таймер
                DamageReceiver damageReceiver = currentTarget.GetComponent<DamageReceiver>();
                if (damageReceiver != null)
                {
                    damageReceiver.TakeDamage(damageAmount); // Наносим урон
                }
                else
                {
                    currentTarget = null; //Обнуляем цель.
                }
            }
        }
    }

    //Метод OnTriggerEnter вызывается при вхождении коллайдера (с установленным флагом Is Trigger) в зону другого коллайдера

    void OnTriggerEnter2D(Collider2D other)
    {
        DamageReceiver damageReceiver = other.GetComponent<DamageReceiver>();
        if (damageReceiver != null)
        {
            currentTarget = other.gameObject;
        }
    }

    //Метод OnTriggerExit вызывается при выходе коллайдера из триггерной зоны
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject == currentTarget)
        {
            currentTarget = null;
            timer = 0;
        }
    }


}