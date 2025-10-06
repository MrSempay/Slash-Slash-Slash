using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Простая "replay one" обёртка: хранит последний опубликованный T (hasValue),
/// и завершает ожидающие TaskCompletionSource сразу при публикации.
/// Новые ждущие, если значение уже было опубликовано — получают его мгновенно.
/// </summary>
public class ReplayableEvent<T>
{
    private readonly object _lock = new object();
    private readonly List<TaskCompletionSource<T>> _waiters = new List<TaskCompletionSource<T>>();
    private bool _hasValue;
    private T _lastValue;

    public bool HasValue
    {
        get { lock (_lock) return _hasValue; }
    }

    public T LastValue
    {
        get { lock (_lock) return _lastValue; }
    }

    /// <summary>
    /// Publish a new value. Completes all current waiters and stores value for future waiters.
    /// </summary>
    public void Publish(T value)
    {
        List<TaskCompletionSource<T>> toComplete;
        lock (_lock)
        {
            _hasValue = true;
            _lastValue = value;
            toComplete = new List<TaskCompletionSource<T>>(_waiters);
            _waiters.Clear();
        }

        foreach (var w in toComplete)
            w.TrySetResult(value);
    }

    /// <summary>
    /// Wait one occurrence. If value is already available, returns completed Task with last value.
    /// Otherwise returns Task that completes on next Publish.
    /// </summary>
    public Task<T> WaitAsync(CancellationToken ct = default)
    {
        lock (_lock)
        {
            if (_hasValue) return Task.FromResult(_lastValue);

            var tcs = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
            _waiters.Add(tcs);

            if (ct != CancellationToken.None)
            {
                ct.Register(() => // по сути, подписываемся на событие отмены
                                  //Пример в лоб
                                  //var cts = new CancellationTokenSource();
                                  //cts.Token.Register(() => Console.WriteLine("Меня отменили!"));
                                  //cts.Cancel(); // сразу выведет: "Меня отменили!"
                                  //То есть Register — это способ подписаться на событие отмены.
                                  //(аналогично someEvent += () => ..., только у CancellationToken нет события, а есть вот такой метод).
                {
                    bool removed = false;
                    lock (_lock)
                    {
                        removed = _waiters.Remove(tcs);
                    }
                    if (removed) tcs.TrySetCanceled();
                });
            }
            return tcs.Task;
        }
    }

    /// <summary>
    /// Clear stored value (optional).
    /// </summary>
    public void Clear()
    {
        lock (_lock)
        {
            _hasValue = false;
            _lastValue = default;
        }
    }

    /***
        * _spellBought — это один объект ReplayableEvent<bool> внутри твоего сценария.
        * Когда ты делаешь:

        var spellTask = _spellBought.WaitAsync(ct);


        ты не создаёшь новый ReplayableEvent.
        Ты просто просишь у него:

        «Дай мне таск, который завершится, когда ты получишь новое значение».

        И внутри этот метод создаёт один новый TaskCompletionSource и кладёт его в общий список _waiters.
    ***/
}