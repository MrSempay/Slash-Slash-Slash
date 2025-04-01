using UnityEngine;

public class EnemyNearDetector : MonoBehaviour
{
    public delegate void EnemyNear(bool isNear, Enemy enemy); // шаблон функции
        public event EnemyNear isEnemyNear;         // экземляр(?) функции/сигнала(?)


    private void OnTriggerEnter2D(Collider2D other)
    {
  
        // на данный момент подписаны в FsmStateWalk
        if (other.gameObject.CompareTag("Enemy")) { isEnemyNear?.Invoke(true, other.gameObject.GetComponentInParent<Enemy>()); } // так как коллайдер у врага находится на дочернем элементе Body, а скрипт сам на 
                                                                                                                                        // родителе, получаем компонент родителя
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // на данный момент подписаны в FsmStateWalk
        if (other.gameObject.CompareTag("Enemy")) isEnemyNear?.Invoke(false, other.gameObject.GetComponentInParent<Enemy>());
    }
}