using UnityEngine;

public class AreaDetectEnteringExiting : MonoBehaviour
{
    public delegate void SomethingEnterExitArea(bool isEnter, GameObject obj, Transform transformArea); // шаблон функции
    public event SomethingEnterExitArea somethingEnterExitArea;         // экземляр(?) функции/сигнала(?)


    private void OnTriggerEnter2D(Collider2D other)
    {
        somethingEnterExitArea?.Invoke(true, other.gameObject, transform);
        ////Debug.Log("УХ КАК ВОШЛИ!");
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        somethingEnterExitArea?.Invoke(false, other.gameObject, transform);
    }
}
