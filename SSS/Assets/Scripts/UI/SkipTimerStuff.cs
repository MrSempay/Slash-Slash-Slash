using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class SkipTimerStuff : ICleanUp
{
    private GameObject _objButton;
    private GameObject _objHourglass;
    private Coroutine _coroutineExternalTimer;
    private GameObject _ownerCoroutine;

    public SkipTimerStuff(Coroutine coroutineExternalTimer,
                          GameObject coroutineOwner,
                          string textButtonSkip,
                          string timerMarker,
                          float waitTime,
                          Dictionary<string, TaskCompletionSource<bool>> timerTcs,
                          object timerTcsLock)
    {
        CleanupManager.Register(this);

        _coroutineExternalTimer = coroutineExternalTimer;
        _ownerCoroutine = coroutineOwner;

        _objButton = InstanceSkipButton(textButtonSkip, timerMarker, timerTcs, timerTcsLock);

        _objHourglass = GameManager.Instance.InvokeHourglass((int)waitTime, true, Player.instance.scriptUI.rtTopRightPanel).gameObject;
    }

    private GameObject InstanceSkipButton(string textButtonSkip, string timerMarker, Dictionary<string, TaskCompletionSource<bool>> timerTcs, object timerTcsLock)
    {
        // создаЄм кнопку Ч еЄ callback выполн€етс€ на main thread
        GameObject skipButton = GameManager.Instance.InstanceTextButton(
            false,
            Player.instance.scriptUI.rtContainerButtonsUI,
            textButtonSkip,
            () =>
            {
                // main thread: атомарно удалить и завершить tcs как успешное завершение (skip == success)
                TaskCompletionSource<bool> removed = null;
                lock (timerTcsLock)
                {
                    if (timerTcs.TryGetValue(timerMarker, out var tmp))
                    {
                        removed = tmp;
                        timerTcs.Remove(timerMarker);
                    }
                }
                if (removed != null)
                {
                    // помечаем как нормальное завершение Ч await вернЄтс€ и шаг продвинетс€
                    removed.TrySetResult(true);
                }

                // останавливаем корутину (на main thread) Ч безопасно
                try { CoroutineManager.Instance.StopManagedCoroutine(_ownerCoroutine, _coroutineExternalTimer); } catch { }
            }
        );
        return skipButton;
    }


    private IEnumerator UpdateTimeText()
    {
        yield return null;
    }

    private int _disposed = 0;
    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) == 1) return;

        if (_objButton != null)
        {
            GameObject.Destroy(_objButton);
        }
        if (_objHourglass != null)
        {
            GameObject.Destroy(_objHourglass);
        }

        try { CoroutineManager.Instance.StopManagedCoroutine(_ownerCoroutine, _coroutineExternalTimer); } catch { }

    }
}
