using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class CameraService : ICleanUp
{

    public AnimationCurve customCurve = AnimationCurve.Linear(0, 0, 1, 1);
    public enum CameraMoveType
    {
        Linear,
        SmoothStep,
        CustomCurve
    }
    public static CameraService Instance => _instance ?? throw new InvalidOperationException("CameraService not initialized. Create via CameraService.CreateInstance() in bootstrap.");

    private class MoveInfo
    {
        public Coroutine Coroutine;
        public GameObject OwnerCoroutine;
        public TaskCompletionSource<bool> Tcs;
        public CancellationTokenRegistration CancelReg;
    }
    public class CameraMoveParams
    {
        public Transform Target { get; set; } = null;
        public string FinishKey { get; set; } = string.Empty;
        public Camera Camera { get; set; } = null;
        public CancellationToken CancellationToken { get; set; } = CancellationToken.None;
        public bool EnableUpdateFuncAfter { get; set; } = true;

        // движение
        public bool MoveToPlayer { get; set; } = false;
        public float Speed { get; set; } = 0f;
        public float Time { get; set; } = 0f;
        public CameraService.CameraMoveType MoveType { get; set; } = CameraService.CameraMoveType.Linear;
    }

    private readonly ConcurrentDictionary<string, MoveInfo> _moves = new();
    private int _disposed = 0;
    private static CameraService _instance;

    // Внутри конструктора регистрируемся
    private CameraService()
    {
        CleanupManager.RegisterDisposeSceneChanged(this);
    }
    public static CameraService CreateInstance()
    {
        if (_instance != null) throw new InvalidOperationException("CameraService already created for this scene.");
        _instance = new CameraService();
        return _instance;
    }

    public void DelinkCameraPlayer()
    {
        Transform transformCameraPlayer = Player.instance.mainCamera.transform;
        transformCameraPlayer.SetParent(null);
    }

    public Task MoveCameraToTargetAsync(CameraMoveParams param)
    {
        if (param.Target == null) throw new ArgumentNullException(nameof(param.Target));
        if (string.IsNullOrEmpty(param.FinishKey)) throw new ArgumentNullException(nameof(param.FinishKey));
        if (IsDisposed) throw new ObjectDisposedException(nameof(CameraService));
        Debug.Log(1231);
        //param.Target = null;
        Debug.Log(param.Target.position);
        Debug.Log(12312412);
        var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var mi = new MoveInfo { Tcs = tcs };

        if (!_moves.TryAdd(param.FinishKey, mi))
            throw new InvalidOperationException($"Move with key '{param.FinishKey}' is already running.");
        Debug.Log("Чё за хрень?");
        if (param.CancellationToken.CanBeCanceled)
        {
            mi.CancelReg = param.CancellationToken.Register(() =>
            {
                Debug.Log(1);
                if (_moves.TryRemove(param.FinishKey, out var removed))
                {
                    Debug.Log(2);
                    removed.Tcs.TrySetCanceled(param.CancellationToken);
                    MainThreadPost(() =>
                    {
                        Debug.Log(3);
                        try { if (removed.Coroutine != null) CoroutineManager.Instance.StopManagedCoroutine(removed.OwnerCoroutine, removed.Coroutine); }
                        catch { }
                    }); Debug.Log(4);
                    removed.CancelReg.Dispose();
                }
            });
        }

        try
        {
            Action actionStop = () =>
            {
                Debug.Log(11);
                if (_moves.TryRemove(param.FinishKey, out var finished))
                {
                    Debug.Log(22);
                    finished.Tcs.TrySetResult(true); Debug.Log(33);
                    try { finished.CancelReg.Dispose(); Debug.Log(44); } catch { Debug.Log(55); }
                }
            };

            Player.instance?.SetStateIdleToPlayerAndBlockAnyUpdateFunctions(true);
            mi.OwnerCoroutine = param.Camera.gameObject;

            if (param.MoveToPlayer)
            {
                if (param.Time == 0f)
                {
                    mi.Coroutine = CoroutineManager.Instance.StartManagedCoroutine(param.Camera.gameObject,
                        MoveCameraToPlayerSpeedCoroutine(param.Camera.transform, param.Target, param.Speed, actionStop, param.EnableUpdateFuncAfter));
                }
                else
                {
                    mi.Coroutine = CoroutineManager.Instance.StartManagedCoroutine(param.Camera.gameObject,
                        MoveCameraToPlayerTimeCoroutine(param.Camera.transform, param.Target, param.Time, param.MoveType, actionStop, param.EnableUpdateFuncAfter));
                }
            }
            else
            {
                if (param.Time == 0f)
                {
                    mi.Coroutine = CoroutineManager.Instance.StartManagedCoroutine(param.Camera.gameObject,
                        MoveCameraSpeedCoroutine(param.Camera.transform, param.Target, param.Speed, actionStop, param.EnableUpdateFuncAfter));
                }
                else
                {
                    mi.Coroutine = CoroutineManager.Instance.StartManagedCoroutine(param.Camera.gameObject,
                        MoveCameraTimeCoroutine(param.Camera.transform, param.Target, param.Time, param.MoveType, actionStop, param.EnableUpdateFuncAfter));
                }
            }
        }
        catch (Exception ex)
        {
            if (_moves.TryRemove(param.FinishKey, out var removed))
            {
                Debug.Log(111);
                try { removed.CancelReg.Dispose(); Debug.Log(222); } catch { Debug.Log(333); }
                removed.Tcs.TrySetException(ex);
            }
            else
            {
                Debug.Log(444);
                tcs.TrySetException(ex);
            }
        }

        return tcs.Task;
    }

    private IEnumerator MoveCameraToPlayerSpeedCoroutine(Transform tCam, Transform tTarget, float speed, Action onFinished, bool enableUpdateFuncAfter)
    {
        if (tCam == null || tTarget == null)
        {
            onFinished?.Invoke();
            yield break;
        }

        yield return null;

        float distanceThreshold = 0.01f;
        if (Player.instance == null || tCam == null || tTarget == null) yield break;
        Vector3 desired = tTarget.position + Player.instance.localPositionCamera;

        while (Vector3.Distance(tCam.position, desired) > distanceThreshold)
        {
            //Debug.Log("...");
            if (Player.instance == null || tCam == null || tTarget == null) yield break;
            desired = tTarget.position + Player.instance.localPositionCamera;
            tCam.position = Vector3.MoveTowards(tCam.position, desired, speed * Time.deltaTime);
            yield return null;
        }
        if (Player.instance == null || tCam == null || tTarget == null) yield break;
        tCam.position = tTarget.position + Player.instance.localPositionCamera;
        tCam.SetParent(Player.instance.transform); // по сути это у меня tTarget
        tCam.localPosition = Player.instance.localPositionCamera;
        if (Player.instance == null || tCam == null || tTarget == null) yield break;
        if (enableUpdateFuncAfter)
            Player.instance?.SetStateIdleToPlayerAndBlockAnyUpdateFunctions(false);
        if (Player.instance == null || tCam == null || tTarget == null) yield break;
        onFinished?.Invoke();
    }
    private IEnumerator MoveCameraToPlayerTimeCoroutine(Transform tCam,
                                                        Transform tTarget,
                                                        float moveTime,
                                                        CameraMoveType moveType,
                                                        Action onFinished,
                                                        bool enableUpdateFuncAfter)
    {
        if (tCam == null || tTarget == null)
        {
            onFinished?.Invoke();
            yield break;
        }

        yield return null;

        Vector3 start = tCam.position;
        Vector3 end = tTarget.position + Player.instance.localPositionCamera;
        float elapsed = 0f;

        while (elapsed < moveTime)
        {
            elapsed += Time.deltaTime;

            // обновляем цель, если она движется
            end = tTarget.position + Player.instance.localPositionCamera;

            float t = Mathf.Clamp01(elapsed / moveTime);
            float tFinal = t;

            switch (moveType)
            {
                case CameraMoveType.SmoothStep:
                    tFinal = Mathf.SmoothStep(0f, 1f, t);
                    break;
                case CameraMoveType.CustomCurve:
                    tFinal = customCurve.Evaluate(t);
                    break;
                case CameraMoveType.Linear:
                default:
                    tFinal = t;
                    break;
            }

            tCam.position = Vector3.Lerp(start, end, tFinal);

            yield return null;
        }

        // Финальная позиция
        tCam.position = tTarget.position + Player.instance.localPositionCamera;
        tCam.SetParent(Player.instance.transform);
        tCam.localPosition = Player.instance.localPositionCamera;

        if (enableUpdateFuncAfter)
            Player.instance?.SetStateIdleToPlayerAndBlockAnyUpdateFunctions(false);

        onFinished?.Invoke();
    }
    private IEnumerator MoveCameraSpeedCoroutine(Transform tCam, Transform tTarget, float speed, Action onFinished, bool enableUpdateFuncAfter)
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

        if (enableUpdateFuncAfter)
            Player.instance?.SetStateIdleToPlayerAndBlockAnyUpdateFunctions(false);

        onFinished?.Invoke();
    }
    private IEnumerator MoveCameraTimeCoroutine(Transform tCam,
                                                Transform tTarget,
                                                float moveTime,
                                                CameraMoveType moveType,
                                                Action onFinished,
                                                bool enableUpdateFuncAfter)
    {
        if (tCam == null || tTarget == null)
        {
            onFinished?.Invoke();
            yield break;
        }

        yield return null;

        Vector3 start = tCam.position;
        Vector3 end = new Vector3(tTarget.position.x, tTarget.position.y, -10f);
        float elapsed = 0f;

        while (elapsed < moveTime)
        {
            elapsed += Time.deltaTime;

            // обновляем конечную точку, если цель двигается
            end = new Vector3(tTarget.position.x, tTarget.position.y, -10f);

            float t = Mathf.Clamp01(elapsed / moveTime);
            float tFinal = t;

            // выбираем тип движения
            switch (moveType)
            {
                case CameraMoveType.SmoothStep:
                    tFinal = Mathf.SmoothStep(0f, 1f, t);
                    break;
                case CameraMoveType.CustomCurve:
                    tFinal = customCurve.Evaluate(t);
                    break;
                case CameraMoveType.Linear:
                default:
                    tFinal = t;
                    break;
            }

            tCam.position = Vector3.Lerp(start, end, tFinal);

            yield return null;

            //_moves[C.SS.Level1.CM.MoveAfterEnemyKilling].Tcs.TrySetCanceled();
        }

        tCam.position = new Vector3(tTarget.position.x, tTarget.position.y, -10f);

        if (enableUpdateFuncAfter)
            Player.instance?.SetStateIdleToPlayerAndBlockAnyUpdateFunctions(false);

        onFinished?.Invoke();
    }


    private static void MainThreadPost(Action a)
    {
        var sync = SynchronizationContext.Current;
        if (sync != null) GameManager.UnityContext.Post(_ => a(), null);
        else a();
    }

    public bool IsDisposed => Volatile.Read(ref _disposed) == 1;

    public void Dispose()
    {
        Debug.Log("ДА что за нахрен?");

        Debug.Log($"Dispose thread: {Thread.CurrentThread.ManagedThreadId}, main: {Thread.CurrentThread == Thread.CurrentThread}");
        Debug.Log($"SynchronizationContext: {SynchronizationContext.Current?.GetType().Name ?? "null"}");
        Debug.Log($"UnityContext : {GameManager.UnityContext}");
        if (Interlocked.Exchange(ref _disposed, 1) == 1) return;

        foreach (var kv in _moves)
        {
            var mi = kv.Value;
            try
            {
                GameManager.UnityContext.Post(_ =>
                {
                    try { if (mi.Coroutine != null) { CoroutineManager.Instance.StopManagedCoroutine(mi.OwnerCoroutine, mi.Coroutine); try { mi.Tcs?.TrySetCanceled(); } catch { } } }
                    catch { }
                }, null);
            }
            catch { }
            //Debug.Log("ДА что за нахрен?");
            //try { mi.Tcs?.TrySetCanceled(); } catch { }
            //Debug.Log("ДА что за нахрен?");
            try { mi.CancelReg.Dispose(); } catch { }
        }
        _moves.Clear();
        _instance = null;
    }

    public void DisposeL2()
    {
        Debug.Log("Dispose CameraService");

        if (Interlocked.Exchange(ref _disposed, 1) == 1)
            return;

        foreach (var kv in _moves)
        {
            var mi = kv.Value;


            try
            {
                // 1️⃣ Просто ставим флаг отмены / токен
                mi.Tcs.TrySetCanceled();
                mi.CancelReg.Dispose();

                // 2️⃣ Безопасно отложим остановку корутины
                var coroutine = mi.Coroutine;
                var owner = mi.OwnerCoroutine;

                // Проверяем, что UnityContext жив
                if (GameManager.UnityContext != null && CoroutineManager.Instance != null)
                {
                    // Публикуем в Unity поток, но не блокируем
                    GameManager.UnityContext.Post(_ =>
                    {
                        try
                        {
                            if (coroutine != null && owner != null)
                                CoroutineManager.Instance.StopManagedCoroutine(owner, coroutine);
                        }
                        catch (Exception ex)
                        {
                            Debug.LogWarning($"Stop coroutine failed: {ex}");
                        }
                    }, null);
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Dispose move failed: {ex}");
            }
        }

        _moves.Clear();
        _instance = null;
    }


}