using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class CameraService : ICleanUp
{
    private class MoveInfo
    {
        public Coroutine Coroutine;
        public GameObject OwnerCoroutine;
        public TaskCompletionSource<bool> Tcs;
        public CancellationTokenRegistration CancelReg;
    }

    private readonly ConcurrentDictionary<string, MoveInfo> _moves = new();
    private int _disposed = 0;
    private static CameraService _instance;
    public static CameraService Instance => _instance ?? throw new InvalidOperationException("CameraService not initialized. Create via CameraService.CreateInstance() in bootstrap.");

    public static CameraService CreateInstance()
    {
        if (_instance != null) throw new InvalidOperationException("CameraService already created for this scene.");
        _instance = new CameraService();
        return _instance;
    }

    // Внутри конструктора регистрируемся
    private CameraService()
    {
        CleanupManager.RegisterDisposeSceneChanged(this);
    }

    public void DelinkCameraPlayer()
    {
        Transform transformCameraPlayer = Player.instance.mainCamera.transform;
        transformCameraPlayer.SetParent(null);
    }

    public Task MoveCameraToTargetAsync(Camera cam, Transform tTraget, float speed, string finishKey, CancellationToken ct, bool moveToPlayer = false)
    {
        if (tTraget == null) throw new ArgumentNullException(nameof(tTraget));
        if (string.IsNullOrEmpty(finishKey)) throw new ArgumentNullException(nameof(finishKey));
        if (IsDisposed) throw new ObjectDisposedException(nameof(CameraService));

        var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var mi = new MoveInfo { Tcs = tcs };

        if (!_moves.TryAdd(finishKey, mi))
            throw new InvalidOperationException($"Move with key '{finishKey}' is already running.");

        if (ct.CanBeCanceled)
        {
            mi.CancelReg = ct.Register(() =>
            {
                if (_moves.TryRemove(finishKey, out var removed))
                {
                    removed.Tcs.TrySetCanceled(ct);
                    MainThreadPost(() =>
                    {
                        //try { if (removed.Coroutine != null) CoroutineManager.Instance.StopManagedCoroutine(removed.OwnerCoroutine, removed.Coroutine); }
                        //catch { }
                    });
                    removed.CancelReg.Dispose();
                }
            });
        }

        try
        {
            Action actionStop = () =>
            {
                if (_moves.TryRemove(finishKey, out var finished))
                {
                    finished.Tcs.TrySetResult(true);
                    try { finished.CancelReg.Dispose(); } catch { }
                }
            };

            Player.instance?.SetStateIdleToPlayerAndBlockAnyUpdateFunctions(true);
            mi.OwnerCoroutine = cam.gameObject;

            if (moveToPlayer)
            {
                mi.Coroutine = CoroutineManager.Instance.StartManagedCoroutine(cam.gameObject,
                    MoveCameraToPlayerCoroutine(cam.transform, tTraget, speed, actionStop));
            }
            else
            {
                mi.Coroutine = CoroutineManager.Instance.StartManagedCoroutine(cam.gameObject,
                    MoveCameraCoroutine(cam.transform, tTraget, speed, actionStop));
            }
        }
        catch (Exception ex)
        {
            if (_moves.TryRemove(finishKey, out var removed))
            {
                try { removed.CancelReg.Dispose(); } catch { }
                removed.Tcs.TrySetException(ex);
            }
            else
            {
                tcs.TrySetException(ex);
            }
        }

        return tcs.Task;
    }

    private IEnumerator MoveCameraToPlayerCoroutine(Transform tCam, Transform tTarget, float speed, Action onFinished)
    {
        if (tCam == null || tTarget == null)
        {
            onFinished?.Invoke();
            yield break;
        }

        yield return null;

        float distanceThreshold = 0.01f;
        Vector3 desired = tTarget.position + Player.instance.localPositionCamera;

        while (Vector3.Distance(tCam.position, desired) > distanceThreshold)
        {
            desired = tTarget.position + Player.instance.localPositionCamera;
            tCam.position = Vector3.MoveTowards(tCam.position, desired, speed * Time.deltaTime);
            yield return null;
        }

        tCam.position = tTarget.position + Player.instance.localPositionCamera;
        tCam.SetParent(Player.instance.transform); // по сути это у меня tTarget
        tCam.localPosition = Player.instance.localPositionCamera;

        Player.instance.SetStateIdleToPlayerAndBlockAnyUpdateFunctions(false);

        onFinished?.Invoke();
    }

    private IEnumerator MoveCameraCoroutine(Transform tCam, Transform tTarget, float speed, Action onFinished)
    {
        if (tCam == null || tTarget == null)
        {
            onFinished?.Invoke();
            yield break;
        }

        yield return null;

        float distanceThreshold = 0.01f;
        Vector3 desired = new Vector3(tTarget.position.x, tTarget.position.y, -10);
        Vector2 distanceXY = new Vector2(tCam.position.x, tCam.position.y) - new Vector2(desired.x, desired.y);
        while (distanceXY.magnitude > distanceThreshold)
        {
            desired = new Vector3(tTarget.position.x, tTarget.position.y, -10);
            tCam.position = Vector3.MoveTowards(tCam.position, desired, speed * Time.deltaTime);

            distanceXY = new Vector2(tCam.position.x, tCam.position.y) - new Vector2(desired.x, desired.y);
            yield return null;
        }

        tCam.position = desired;
        onFinished?.Invoke();
    }

    private static void MainThreadPost(Action a)
    {
        var sync = SynchronizationContext.Current;
        if (sync != null) sync.Post(_ => a(), null);
        else a();
    }

    public bool IsDisposed => Volatile.Read(ref _disposed) == 1;

    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) == 1) return;

        foreach (var kv in _moves)
        {
            var mi = kv.Value;
            try
            {
                MainThreadPost(() =>
                {
                    try { if (mi.Coroutine != null) CoroutineManager.Instance.StopManagedCoroutine(mi.OwnerCoroutine, mi.Coroutine); }
                    catch { }
                });
            }
            catch { }

            try { mi.Tcs.TrySetCanceled(); } catch { }
            try { mi.CancelReg.Dispose(); } catch { }
        }
        _moves.Clear();
        _instance = null;
    }
}