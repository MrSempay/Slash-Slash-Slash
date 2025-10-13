using System;
using System.Collections;
using UnityEngine;

public class Hourglass : MonoBehaviour
{
    public TextEdit text;

    private int _timeRemained;
    private Coroutine _countdownCoroutine;

    public void Initialize(int waitTime, bool showCountdown = true) // точка входа
    {
        _countdownCoroutine = CoroutineManager.Instance.StartManagedCoroutine(gameObject, SecondCallDown(waitTime, showCountdown));
    }

    private IEnumerator SecondCallDown(int waitTime, bool showCountdown)
    {
        _timeRemained = waitTime;
        while (_timeRemained > 0)
        {
            if (showCountdown)
            {
                text.SetNotLocalizableText(_timeRemained.ToString());
            }
            _timeRemained--;

            yield return new WaitForSeconds(1f);
        }
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        // Останавливаем корутину если объект уничтожается раньше времени
        if (_countdownCoroutine != null)
        {
            CoroutineManager.Instance.StopManagedCoroutine(gameObject, _countdownCoroutine);
        }
    }
}
