// AsyncFsm.cs
using System.Collections.Generic;
using System.Threading;
using System;
using System.Threading.Tasks;
using UnityEngine;

public class FsmAsync : Fsm
{
    private readonly Dictionary<Type, AsyncStateBase> _states = new();
    private AsyncStateBase _current;
    private CancellationTokenSource _stateCts;
    private readonly SemaphoreSlim _transitionGate = new(1, 1); // сериализуем SetStateAsync
    private Task _currentRunTask;

    public void AddState(AsyncStateBase state) => _states[state.GetType()] = state;

    // Request jump: выставляем желаемый тип и отменяем текущее выполнение
    public async Task RequestJumpAsync(Type targetType, Dictionary<string, object> args = null)
    {
        await _transitionGate.WaitAsync();
        try
        {
            _stateCts?.Cancel();
            // SetStateAsync выполнится после отмены и выхода текущего состояния
            await SetStateInternalAsync(targetType, args);
        }
        finally { _transitionGate.Release(); }
    }

    public async Task SetStateAsync<T>(Dictionary<string, object> args = null) where T : AsyncStateBase
        => await SetStateInternalAsync(typeof(T), args);

    private async Task SetStateInternalAsync(Type targetType, Dictionary<string, object> args)
    {
        await _transitionGate.WaitAsync();
        try
        {
            _stateCts?.Cancel();

            var prevTask = _currentRunTask;
            if (prevTask != null)
            {
                try { await prevTask; }          // ← Ключевой момент
                catch (OperationCanceledException) { }
            }

            _current?.Exit();
            _stateCts?.Dispose();

            _stateCts = new CancellationTokenSource();
            if (!_states.TryGetValue(targetType, out var newState)) throw new Exception();
            _current = newState;
            _current.Enter(args);

            _currentRunTask = Task.Run(async () =>
            {
                try { await _current.RunAsync(_stateCts.Token); }
                catch (OperationCanceledException) { }
                catch (Exception ex) { Debug.LogError(ex); }
            });
        }
        finally
        {
            _transitionGate.Release();
        }
    }

    // expose for debugging/inspector
    public string CurrentStateName => _current?.GetType().Name;
}