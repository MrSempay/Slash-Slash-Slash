using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// ѕроста€ "replay one" обЄртка: хранит последний опубликованный T (hasValue),
/// и завершает ожидающие TaskCompletionSource сразу при публикации.
/// Ќовые ждущие, если значение уже было опубликовано Ч получают его мгновенно.
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
                ct.Register(() =>
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
}