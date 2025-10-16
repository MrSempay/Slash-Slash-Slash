using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Универсальный хелпер для работы с корутинами как с Task.
/// Позволяет await-ить, отменять, ловить события и управлять вручную.
/// </summary>
/// 

///////////////////////////////////////// ВНИМАНИЕЕЕЕЕЕЕЕЕЕЕЕЕЕЕ !!!!!!!!!!!!!!!!!!!!!!!!!!!! ///////////////////////////////////
// вот эта вся дичь с try/catch около корутины нужна только если корутина может сама эмулировать OperationCanceledException. Из-за этого
// возможен двойной Cancel. Лучше ввести правило, что сами корутины не могут вызывать OperationCanceledException

public static class CoroutineExtensions // Context.Post мы тут ещё не реализовывали (14.10.2025), методы не потокобезопасны!
{
    /// <summary>
    /// Объект, который хранит Task и Coroutine.
    /// Позволяет как await-ить корутину, так и вручную её останавливать.
    /// </summary>
    public class CoroutineHandle
    {
        public Task Task { get; }
        public Coroutine Coroutine { get; }
        public bool IsRunning => !Task.IsCompleted;

        internal CoroutineHandle(Task task, Coroutine coroutine)
        {
            Task = task;
            Coroutine = coroutine;
        }
    }

    /// <summary>
    /// Запускает корутину как Task с полной поддержкой отмены, событий и ручного контроля.
    /// </summary>
    public static CoroutineHandle AsTask(this IEnumerator coroutine,
                                         MonoBehaviour runner,
                                         CancellationToken token = default,
                                         Action onCancel = null,
                                         Action onComplete = null,
                                         Action<Exception> onError = null)
    {
        var tcs = new TaskCompletionSource<bool>();

        var c = runner.StartCoroutine(RunCoroutineSafe(coroutine, tcs, token, onCancel, onComplete, onError));

        // Если токен отменён — прерываем корутину
        CancellationTokenRegistration registration = new();
        if (token.CanBeCanceled)
        {
            registration = token.Register(() =>
            {
                if (runner != null && c != null)
                {
                    runner.StopCoroutine(c);
                }
                onCancel?.Invoke();
                tcs.TrySetCanceled(token);
            });
        }
        tcs.Task.ContinueWith(_ => registration.Dispose());

        return new CoroutineHandle(tcs.Task, c);
    }

    /// <summary>
    /// Обёртка, безопасно запускающая корутину с ловлей исключений.
    /// </summary>
    private static IEnumerator RunCoroutineSafe(IEnumerator coroutine,
                                                TaskCompletionSource<bool> tcs,
                                                CancellationToken token,
                                                Action onCancel,
                                                Action onComplete,
                                                Action<Exception> onError)
    {
        // Перехватываем любые ошибки "вне yield"
        Exception caught = null;

        // Выполняем внутреннюю корутину, но без try/catch с yield внутри
        yield return Inner();

        // После выхода из IEnumerator решаем, чем всё кончилось
        if (caught != null)
        {
            onError?.Invoke(caught);
            tcs.TrySetException(caught);
        }
        else if (token.IsCancellationRequested)
        {
            onCancel?.Invoke();
            tcs.TrySetCanceled(token);
        }
        else
        {
            onComplete?.Invoke();
            tcs.TrySetResult(true);
        }

        // Вложенная корутина без catch вокруг yield
        IEnumerator Inner()
        {
            while (true)
            {
                bool moveNext;
                object current = null;

                try
                {
                    moveNext = coroutine.MoveNext();
                    if (moveNext)
                        current = coroutine.Current;
                }
                catch (OperationCanceledException) // вот эта вся дичь с try/catch около корутины нужна только если корутина может сама эмулировать OperationCanceledException. Из-за этого
                // возможен двойной Cancel. Лучше ввести правило, что сами корутины не могут вызывать OperationCanceledException
                {
                    caught = new TaskCanceledException();
                    yield break;
                }
                catch (Exception ex)
                {
                    caught = ex;
                    yield break;
                }

                if (!moveNext)
                    yield break;

                yield return current;
            }
        }
    }

    /// <summary>
    /// Удобный await-обёртка без ручного доступа к CoroutineHandle.
    /// </summary>
    public static async Task AwaitCoroutine(this IEnumerator coroutine,
                                            MonoBehaviour runner,
                                            CancellationToken token = default,
                                            Action onCancel = null,
                                            Action onComplete = null,
                                            Action<Exception> onError = null)
    {
        var handle = coroutine.AsTask(runner, token, onCancel, onComplete, onError);
        await handle.Task;
    }
}
