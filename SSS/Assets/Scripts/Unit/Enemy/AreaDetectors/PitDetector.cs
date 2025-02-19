using UnityEngine;

public class PitDetector : MonoBehaviour
{
    public delegate void DetectedPit(bool jumpOfPit); // передаём в качестве параметра, что прыгаем мы из-за ямы. Втаком случае в методе Jump будет игнорироваться проверка на прямую видимость ГГ
    public event DetectedPit OnDetectedPit;
    private void OnTriggerExit2D(Collider2D other)
    {
        // Подписываемся на событие в состоянии FsmStateWalkEnemy
        if (other.gameObject.CompareTag("Ground")) OnDetectedPit?.Invoke(true);
    }
}
