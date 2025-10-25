using NUnit.Framework;
using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using static DialogueParser;
using System.Threading.Tasks;
using System.Threading;
using UnityEngine.UI;
using static StaticClassForAdditionalFunctions;

public class ScenarioScript : MonoBehaviour
{
    // ибо

    private PlayerDialogue _scriptCurrentDialogue;
    private Coroutine _moveCameraCoroutine;
    private Coroutine _moveObjectCoroutine;
    private Coroutine _justTimeWaitCoroutine;
    private Vector3 _velocity = Vector3.zero; // Текущая скорость
    private CancellationTokenSource _cts;

    protected List<IMainTarget> _aliveMTExceptedPlayer = new();
    protected List<IMainTarget> _allDeterminedMTExceptedPlayer = new();
    protected GameObject buttonSkipTime;
    protected LevelBuilder levelBuildScript;

    protected Transform transformPlayer;
    protected Player scriptPlayer;


    [NonSerialized] public static float timeWhenSceneStarted;
    [NonSerialized] public static ScenarioScript instance;
    [NonSerialized] public Dictionary<string, int> dictionaryNamesEnemiesWavesAndRewards; // инициализируем в производных классах

    public GameObject player;

    public PlayerDialogue ScriptCurrentDialogue
    {
        get { return _scriptCurrentDialogue; }
        set
        {
            if (_scriptCurrentDialogue != null) // Проверяем, что _scriptCurrentDialogue не null
            {
                _scriptCurrentDialogue.onDialogueWasFinished -= DialogueFinished; // Отписываемся от предыдущего объекта
            }

            _scriptCurrentDialogue = value; // Присваиваем новое значение

            if (_scriptCurrentDialogue != null) // Проверяем, что новое значение не null
            {
                _scriptCurrentDialogue.onDialogueWasFinished += DialogueFinished; // Подписываемся на новый объект
            }
        }
    }

    protected virtual void Awake()
    {
        if (instance != null && instance != this) // инициализируем instance в дочернем классе
        {
            Destroy(gameObject);
            return;
        }
        //Debug.Log("А мы вообще, тут?");

        //player = Player.instance.gameObject;

        transformPlayer = player.GetComponent<Transform>();
        scriptPlayer = player.GetComponent<Player>();
        levelBuildScript = GameObject.Find("LevelBuildScript").GetComponent<LevelBuilder>();

        GameManager.Instance.onDialogueStarted += DialogueWasStarted;
        scriptPlayer.onUnitWasKilled += UnitWasKilled;
        scriptPlayer.OnEnemiesWaveWasDestroyedWithoutLosingMainTargets += EnemiesWaveWasDestroyedWithoutLosingMainTargets;
        scriptPlayer.OnEnemiesWaveWasDestroyed += EnemiesWaveWasDestroyed;

        timeWhenSceneStarted = Time.time;

        _cts = new CancellationTokenSource(); // пока не используем, но путь будет

        CameraManager.CreateInstance();

    }


    protected virtual void Start()
    {

        FinishLevel();
    }

    public void AddMainTargetNotPlayer(IMainTarget target)
    {
        _aliveMTExceptedPlayer.Add(target);
        _allDeterminedMTExceptedPlayer.Add(target);
    }
    public void RemoveMainTarget(IMainTarget target)
    {
        _aliveMTExceptedPlayer.Remove(target);
        if (_aliveMTExceptedPlayer.Count == 0)
        {
            Debug.Log("Ебанашка");
            Defeat();
        }
    }


    /* ############################# БЛОК ФУНКЦИЙ-СИГНАЛОВ, ИНФОРМИРУЮЩИХ О ТОМ, ЧТО СЮЖЕТ ДВИЖЕТСЯ ТАК ИЛИ ИНАЧЕ ############################# */

    //protected virtual void EnemiesWaveWasDestroyed(string nameWave) { AudioManager.Instance.PlayFightOrAmbientMusic(false); } // эмулируется, когда ИГРОК забил всех врагов из текущей волны
    protected virtual void EnemiesWaveWasDestroyedWithoutLosingMainTargets(string nameWave) { } // эмулируется, когда ИГРОК забил всех врагов из текущей волны
    protected virtual void UnitWasKilled(Unit unit)
    {
        if (unit.nameOfUnit == C.DK.Player)
        {
            JustTimeWait(2, "timeAfterPlayerDeathBeforeAdvertisement");
        }
    }
    //protected virtual void TimerFinished(string markerTimeWait)
    //{
    //    switch (markerTimeWait)
    //    {
    //        case "timeAfterPlayerDeathBeforeAdvertisement":
    //            //scriptPlayer.interstitialAds.ShowAd();
    //            break;
    //    }
    //}
    protected virtual void DialogueWasStarted(PlayerDialogue playerDialogue)
    {
        //Debug.Log("А он, блять, начался");
        ScriptCurrentDialogue = playerDialogue;
    }

    protected virtual void MovingCameraPlayerWasFinished(string keyFinishing)
    {
        Player.instance.SetStateIdleToPlayerAndBlockAnyUpdateFunctions(false);
    }

    protected virtual void MovingObjectWasFinished(GameObject obj) { }

    // по идее кучу кода снизу можно было бы заменить на просто эмуляцию сигнала обновления ассортимента в свойстве, которое бы обозначало тот самый список с текущим снаряжением в здании
    // p.s. - нет, нельзя, свойство, как и следовало ожидать, триггерится токмо на присвоение целого объекта: NekoeSvoistvo = new List<Equipment>(); NekoeSvoistvo = null.
    // Впрочем, решение было найдено: создавать во время спавна нового снаряжения отдельный локальный массив, заполнять его и уже потом присваивать его нашему свойству


    private Dictionary<Building, List<Equipment>> _buildingAndEquipmentInBuildingFromLastIteration = new Dictionary<Building, List<Equipment>>(); 
                                                                    // просто переменная для временного хранения ссылок на снаряжение из предыдущей итерации его обновления в здании
                                                                    // чтоб потом можно было отписаться от прослушивания сигналов для данного снаряжения
    protected virtual void AssortmentInBuildingWasUpdated(List<Equipment> equipmentInBuilding, Building building)
    {
        // короче, нижестоящий if придуман лишь для того, чтоб отписываться от сигналов предыдущей партии снаряжения в зданиях при обновлении ассортимента. При каждом обновлении
        // ассортимента мы до того, как удалили предыдущую партию, эмулируем данный сигнал, передавая в него null, что является флагом того, что нам нужно отписаться от событий
        // снаряжения из предыдущей партии (снаряжение из предыдущей партии хранится в переменной _equipmentInBuildingFromLastIteration, которая при каждой эмуляции сигнала 
        // с параметром equipmentInBuilding не равным нулю перезаписывается на, собственно, значение параметра equipmentInBuilding)
        if (equipmentInBuilding == null)
        {
            if (_buildingAndEquipmentInBuildingFromLastIteration.ContainsKey(building))
            {
                foreach (var fieldBuildingAndHisEquipmntFromLastIteration in _buildingAndEquipmentInBuildingFromLastIteration[building])
                {
                    if (fieldBuildingAndHisEquipmntFromLastIteration != null) // если это снаряжение ещё не удалено (на всякий случай, по идее такого и не должно быть, удаляем после данной функции)
                    {
                        fieldBuildingAndHisEquipmntFromLastIteration.onEquipmentWasSold -= EquipmentWasSold;
                    }
                }
            }
            return;
        }
        //Debug.Log(equipmentInBuilding.Count);
        foreach (Equipment equipment in equipmentInBuilding)
        {
            equipment.onEquipmentWasSold += EquipmentWasSold;
            Debug.Log(equipment);
        }
        _buildingAndEquipmentInBuildingFromLastIteration[building] = new List<Equipment>(equipmentInBuilding);
        
    }
    protected virtual void EquipmentWasSold(Equipment equipment) { Debug.Log("Equipment was sold"); }




    /* ############################# БЛОК ФУНКЦИЙ-РЕАКЦИЙ, ДВИГАЮЩИХ СЮЖЕТ ТАК ИЛИ ИНАЧЕ ############################# */

    protected virtual void TeleportObjectToPoint(GameObject someObject, Vector3 targetPoint)
    {
        Transform transformObject = someObject.transform;
        transformObject.position = targetPoint;
    }

    protected virtual PlayerDialogue StartDialogue(string nameDialogue) // взять образец из зоны диалога 
    {
        return GameManager.Instance.StartDialogue(nameDialogue);
    }
    protected virtual PlayerDialogue StartDialogueNEW(string nameDialogue) // взять образец из зоны диалога 
    {
        return GameManager.Instance.StartDialogueNEW(nameDialogue);
    }
    protected virtual void FinishLevel()
    {

        if (GameManager.Instance.currentLevelInOrder < GameManager.Instance.orderLevels.Count - 1) // при прохождении последнего уровня счётчик не увеличиваем
                                                                                                   // после окончания уровня увеличиваем количества пройденных уровень, показатель же
                                                                                                   // текущего уровня GameManager.Instance.currentLevelInOrder увеличиваем в функции
                                                                                                   // GoToRequiredLevel() при нажатии соответствующей кнопки
        {
            GameManager.Instance.MaxReachedLevel = GameManager.Instance.currentLevelInOrder + 1;
        }
        // try/catch НЕ НУЖНЫ! Ибо методы у меня ничего не ловят, бо оба void (StartCloudUpdateMaxReachedLevel вообще даже не асинхронный). Для более подробного описания см. GetAndShowActualLeaderboardAsync
        //try
        //{

        PlayFabManager.Instance.StartCloudUpdateMaxReachedLevel();

        ScoreManager.Instance.GetAndShowActualLeaderboardAsync(Leaderboard.INSTANTIATION_CONTEXT.FinishLevel);

        //}

        //catch (OperationCanceledException)
        //{
        //    // Корректная отмена - игнорируем
        //}
    }

    protected virtual GameObject SpawnObjectAtTargetPosition(GameObject someObject, Vector3 targetPosition) // может стоить для каких-нибудь объектов добавить функцию, чтоб вызывать при таком спавне
    {
        return Instantiate(someObject, targetPosition, Quaternion.identity);
    }
    protected virtual void MovingObjectToPoint(GameObject someObject, Vector3 targetPoint, float speed) 
    {
        Transform transformObject = someObject.transform;
        _moveObjectCoroutine = CoroutineManager.Instance.StartManagedCoroutine(this.gameObject, MoveObjectWithSpeedToPoint(transformObject, targetPoint, speed));
    }
    protected virtual void MovingCameraPlayerToPoint(Camera cameraPlayer, Transform targetTransform, float speed, string keyFinishing) 
    {

        Player.instance.SetStateIdleToPlayerAndBlockAnyUpdateFunctions(true);
        Transform transformCameraPlayer = cameraPlayer.transform;

        transformCameraPlayer.SetParent(null);
        _moveCameraCoroutine = CoroutineManager.Instance.StartManagedCoroutine(gameObject, MoveCameraPlayerWithSpeedToPoint(transformCameraPlayer, targetTransform, speed, keyFinishing));

    }
    protected virtual Coroutine JustTimeWait(float timeWait, string markerTimeWait) 
    {
        return _justTimeWaitCoroutine = CoroutineManager.Instance.StartManagedCoroutine(gameObject, TimeWait(timeWait, markerTimeWait));
    }

    protected virtual void StartWaveEnemies(Dictionary<Transform, int> targetPointsForEnemy, string nameWave)
    {
        AudioManager.Instance.PlayFightOrAmbientMusic(true);

        levelBuildScript.currentWave = nameWave;
        levelBuildScript.TargetPointsForEnemy = new(targetPointsForEnemy);
    }
    protected virtual void StartWaveEnemiesNEW(Dictionary<Transform, List<TypesAndAmountEnemies>> targetPointsForEnemiesTypes, string nameWave)
    {
        AudioManager.Instance.PlayFightOrAmbientMusic(true);

        levelBuildScript.currentWave = nameWave;
        levelBuildScript.TargetPointsForEnemyNEW = new(targetPointsForEnemiesTypes);
    }
    protected internal virtual void Defeat()
    {
        
    }



    /* ############################# БЛОК СЛУЖЕБНЫХ (ВНУТРЕННИХ) ФУНКЦИЙ, ЯВЛЯЮТСЯ ТЕХНИЧЕСКИМИ ДЛЯ ОСНОВНЫХ ФУНКЦИЙ-РЕАКЦИЙ/СИГНАЛОВ ############################# */

    IEnumerator MoveObjectWithSpeedToPoint(Transform transformMovingObject, Vector3 targetPoint, float speed)
    {
        float distanceTreshold = 0.01f;
        while (Vector3.Distance(transformMovingObject.position, targetPoint) > distanceTreshold)
        {
            transformMovingObject.position = Vector3.MoveTowards(transformMovingObject.position, targetPoint, speed * Time.deltaTime);
            yield return null;
        }

        // Останавливаем корутину и устанавливаем точную позицию
        transformMovingObject.position = targetPoint;
        MovingObjectWasFinished(transform.gameObject);
    }

    // движение без замедления/ускорения в конце
    IEnumerator MoveCameraPlayerWithSpeedToPoint(Transform transformCameraPlayer, Transform targetTransform, float speed, string keyFinishing)
    {
        yield return null; // очень важно! Ждём следующего кадра, чтоб, если нам нужно следовать за телепортировавшимся героем, у того успела обновиться position

        float distanceTreshold = 0.01f;
        Vector3 specifyTargetPointForCamera = targetTransform.position + scriptPlayer.localPositionCamera; // всегда камера будет иметь смещение относительно целовой точки такое
                                                                                                           // же, как и относительно игрока

        while (Vector3.Distance(transformCameraPlayer.position, specifyTargetPointForCamera) > distanceTreshold)
        {
            specifyTargetPointForCamera = targetTransform.position + scriptPlayer.localPositionCamera;
            transformCameraPlayer.position = Vector3.MoveTowards(transformCameraPlayer.position, specifyTargetPointForCamera, speed * Time.deltaTime);
            yield return null;
        }

        transformCameraPlayer.position = targetTransform.position + scriptPlayer.localPositionCamera; // Устанавливаем точную позицию
        transformCameraPlayer.SetParent(transformPlayer);
        transformCameraPlayer.localPosition = scriptPlayer.localPositionCamera;
        //Debug.Log("А мы реально переместили?");
        MovingCameraPlayerWasFinished(keyFinishing);
    }
    IEnumerator TimeWait(float waitTime, string markerTimeWait)
    {
        yield return new WaitForSeconds(waitTime);
        TimerFinished(markerTimeWait);
    }


    #region FSM-async scenario integraion


    protected internal CancellationTokenSource _stepCts; // per-step CTS to cancel waits on jump
    protected internal SkipTimerStuff _skipTimerStuff;
    protected internal CancellationTokenSource _masterScenarioCts;

    private Dictionary<string, TaskCompletionSource<bool>> _timerTcs = new(StringComparer.OrdinalIgnoreCase);
    private Dictionary<string, TaskCompletionSource<bool>> _waveTcs = new(StringComparer.OrdinalIgnoreCase);
    private TaskCompletionSource<bool> _activeDialogueTcs;
    private PlayerDialogue _activeDialogue;


    protected internal CancellationTokenSource CreateLinkedStepCts(CancellationToken masterToken)
    {
        var newCts = CancellationTokenSource.CreateLinkedTokenSource(masterToken);
        // atomically replace old stepCts, cancel+dispose old
        var old = Interlocked.Exchange(ref _stepCts, newCts);
        if (old != null)
        {
            try { old.Cancel(); } catch { }
            try { old.Dispose(); } catch { }
        }
        return newCts;
    }



    // ---------------------- helper-обёртки ----------------------


    protected internal Task StartDialogueAsync(string dialogueName, CancellationToken ct)
    {
        // АТОМАРНО обрабатываем предыдущий диалог
        var oldTcs = Interlocked.Exchange(ref _activeDialogueTcs, null);
        oldTcs?.TrySetCanceled();

        if (_activeDialogue)
        {
            Destroy(_activeDialogue.gameObject);
            _activeDialogue = null;
        }

        var newTcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        _activeDialogueTcs = newTcs;

        // Сразу получаем синхронизацию главного потока
        var syncContext = SynchronizationContext.Current;

        var reg = ct.Register(() =>
        {
            // ВСЕ операции с Unity в главном потоке
            if (syncContext != null)
            {
                syncContext.Post(_ =>
                {
                    var tcsToCancel = Interlocked.Exchange(ref _activeDialogueTcs, null);
                    tcsToCancel?.TrySetCanceled(ct);

                    if (_activeDialogue)
                    {
                        Destroy(_activeDialogue.gameObject);
                        _activeDialogue = null;
                    }
                }, null);
            }
        });

        try
        {
            _activeDialogue = StartDialogue(dialogueName);
        }
        catch (Exception ex)
        {
            reg.Dispose();
            newTcs.TrySetException(ex);
            throw;
        }

        // Упрощённый ContinueWith - только диспоз рега
        _ = newTcs.Task.ContinueWith(_ => reg.Dispose(),
            TaskContinuationOptions.ExecuteSynchronously);

        return newTcs.Task;
    }

    protected internal Task WaitForTimerAsync(string timerMarker, float seconds, CancellationToken ct)
    {
        var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        _timerTcs[timerMarker] = tcs;

        var reg = ct.Register(() =>
        {
            if (_timerTcs.Remove(timerMarker))
                tcs.TrySetCanceled(ct);
        });

        try
        {
            JustTimeWait(seconds, timerMarker);
        }
        catch (Exception ex)
        {
            _timerTcs.Remove(timerMarker);
            reg.Dispose();
            tcs.TrySetException(ex);
            return tcs.Task;
        }

        tcs.Task.ContinueWith(_ => {
            reg.Dispose();
            _timerTcs.Remove(timerMarker);
        }, TaskScheduler.Default);

        return tcs.Task;
    }

    /// <summary>
    /// Корутинный таймер (scaled time). Возвращаем Task, который завершается когда таймер сработал или был пропущен/отменён.
    /// Вызывать этот метод из main thread (обычно так и делается в Unity).
    /// </summary>
    private readonly object _timerTcsLock = new object();
    protected internal async Task WaitForTimerWithSkipAsyncNEW(string timerMarker, float seconds, string textButtonSkip, CancellationToken ctExtended = default)
    {

        // создаём tcs правильно
        var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        // атомарно добавляем в словарь
        lock (_timerTcsLock)
        {
            if (_timerTcs.ContainsKey(timerMarker))
                throw new InvalidOperationException($"Timer '{timerMarker}' already exists.");
            _timerTcs[timerMarker] = tcs;
        }

        // захватим текущий sync context (main thread) чтобы можно было безопасно постить Destroy
        var sync = SynchronizationContext.Current;

        // запустим корутину (она в конце вызовет TimerFinished(marker), который должен
        // найти tcs и TrySetResult(true); JustTimeWait возвращает handle, если нужно)
        var coroutineHandle = JustTimeWait(seconds, timerMarker);

        // создаём кнопку — её callback выполняется на main thread
        _skipTimerStuff = new SkipTimerStuff(coroutineHandle, gameObject, textButtonSkip, timerMarker, seconds, _timerTcs, _timerTcsLock);

        // Регистрация внешней отмены. Callback может выполняться на thread-pool,
        // поэтому в нем НЕ вызываем Unity API напрямую.
        CancellationTokenRegistration reg = default;
        if (ctExtended.CanBeCanceled && ctExtended != default)
        {
            reg = ctExtended.Register(() =>
            {
                // этот код может выполняться на любом потоке!
                TaskCompletionSource<bool> removed = null;
                lock (_timerTcsLock)
                {
                    if (_timerTcs.TryGetValue(timerMarker, out var tmp))
                    {
                        removed = tmp;
                        _timerTcs.Remove(timerMarker);
                    }
                }

                if (removed != null)
                {
                    removed.TrySetCanceled(); // пометить как отменённый
                }

                // Постим на main-thread удаление UI/остановку корутины
                if (sync != null)
                {
                    sync.Post(_ =>
                    {
                        //if (skipTimerStuff != null) { skipTimerStuff.Dispose(); skipTimerStuff = null; }

                        SkipTimerStuff lastSTF = Interlocked.Exchange(ref _skipTimerStuff, null); // КОРОЧЕ, ЭТО - ЕБАНННЫЫЫЕЕЕ рудиментные ДЕЙСТВИЯ. Ибо Dispose сам мы уже защитили.
                        if (lastSTF != null) lastSTF.Dispose();

                    }, null);
                }
                else
                {
                    // Если sync == null (редко в Unity), попытаться безопасно выполнить — но это небезопасно.
                    //if (skipTimerStuff != null) { skipTimerStuff.Dispose(); skipTimerStuff = null; }

                    SkipTimerStuff lastSTF = Interlocked.Exchange(ref _skipTimerStuff, null); // КОРОЧЕ, ЭТО - ЕБАНННЫЫЫЕЕЕ рудиментные ДЕЙСТВИЯ. Ибо Dispose сам мы уже защитили.
                    if (lastSTF != null) lastSTF.Dispose();
                }
            });
        }

        try
        {
            // Ожидаем завершения tcs. Этот await вернёт управление на тот же SynchronizationContext,
            // с которого был вызван метод (обычно main thread), поэтому cleanup в finally сможет вызывать Unity API напрямую.
            await tcs.Task;
        }
        finally
        {
            // cleanup
            reg.Dispose();

            // удаляем из словаря, если кто-то ещё не удалил
            lock (_timerTcsLock)
            {
                _timerTcs.Remove(timerMarker);
            }

            // удаление кнопки и стоп корутины — выполняем на main thread либо через sync.Post
            if (SynchronizationContext.Current == sync && sync != null)
            {
                // уже на main thread
                //if (skipTimerStuff != null) { skipTimerStuff.Dispose(); skipTimerStuff = null; }

                SkipTimerStuff lastSTF = Interlocked.Exchange(ref _skipTimerStuff, null); // КОРОЧЕ, ЭТО - ЕБАНННЫЫЫЕЕЕ рудиментные ДЕЙСТВИЯ. Ибо Dispose сам мы уже защитили.
                if (lastSTF != null) lastSTF.Dispose();
            }
            else if (sync != null)
            {
                sync.Post(_ =>
                {
                    //if (skipTimerStuff != null) { skipTimerStuff.Dispose(); skipTimerStuff = null; }
                    SkipTimerStuff lastSTF = Interlocked.Exchange(ref _skipTimerStuff, null); // КОРОЧЕ, ЭТО - ЕБАНННЫЫЫЕЕЕ рудиментные ДЕЙСТВИЯ. Ибо Dispose сам мы уже защитили.
                    if (lastSTF != null) lastSTF.Dispose();

                }, null);
            }
            else
            {
                // no sync — best-effort
                //if (skipTimerStuff != null) { skipTimerStuff.Dispose(); skipTimerStuff = null; }
                SkipTimerStuff lastSTF = Interlocked.Exchange(ref _skipTimerStuff, null); // КОРОЧЕ, ЭТО - ЕБАНННЫЫЫЕЕЕ рудиментные ДЕЙСТВИЯ. Ибо Dispose сам мы уже защитили.
                if (lastSTF != null) lastSTF.Dispose();
            }
        }
    }

    protected internal Task WaitForWaveDestroyAsync(string waveName, CancellationToken ct)
    {
        var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        _waveTcs[waveName] = tcs;

        var reg = ct.Register(() =>
        {
            if (_waveTcs.Remove(waveName))
                tcs.TrySetCanceled(ct);
        });

        tcs.Task.ContinueWith(_ =>
        {
            reg.Dispose();
            _waveTcs.Remove(waveName);
        }, TaskScheduler.Default);

        return tcs.Task;
    }
    protected internal virtual void DialogueFinished(string nameDialogueWithFolder)
    {
        ScriptCurrentDialogue = null;

        _activeDialogueTcs?.TrySetResult(true);
        _activeDialogueTcs = null;
        Debug.Log($"DialogueFinished (no awaiter): {nameDialogueWithFolder}");


    } // сигнал, к которому привязана функция, эмулируется при любом окончании диалога, хоть игрока, хоть сцены


    // ---------------------- обработчики событий ---------------------- 


    protected internal virtual void EnemiesWaveWasDestroyed(string nameWave)
    {
        //base.EnemiesWaveWasDestroyed(nameWave);
        AudioManager.Instance.PlayFightOrAmbientMusic(false);
        if (_waveTcs.TryGetValue(nameWave, out var waveInfo))
        {
            waveInfo.TrySetResult(true);

            return;
        }
        Debug.Log($"EnemiesWaveWasDestroyed (no awaiter): {nameWave}");
    }

    protected internal virtual void TimerFinished(string markerTimeWait)
    {
        //base.TimerFinished(markerTimeWait);

        TaskCompletionSource<bool> tcs = null;
        lock (_timerTcsLock)
        {
            if (_timerTcs.TryGetValue(markerTimeWait, out var tmp))
            {
                tcs = tmp;
                _timerTcs.Remove(markerTimeWait);
            }
        }

        if (tcs != null)
        {
            tcs.TrySetResult(true);
            return;
        }

        Debug.Log($"TimerFinished (no awaiter): {markerTimeWait}");
    }

    #endregion FSM-async scenario integraion


    protected virtual void OnDestroy()
    {
        _cts?.Cancel();
        _cts?.Dispose();

        foreach (var kv in _timerTcs) kv.Value.TrySetCanceled();
        _timerTcs.Clear();

        foreach (var kv in _waveTcs) kv.Value.TrySetCanceled();
        _waveTcs.Clear();


        if (_moveCameraCoroutine != null)
        {
            CoroutineManager.Instance?.StopManagedCoroutine(this.gameObject, _moveCameraCoroutine);
        }
        if (_moveObjectCoroutine != null)
        {
            CoroutineManager.Instance?.StopManagedCoroutine(this.gameObject, _moveObjectCoroutine);
        }
        if (_moveObjectCoroutine != null)
        {
            CoroutineManager.Instance?.StopManagedCoroutine(this.gameObject, _justTimeWaitCoroutine);
        }
        _moveCameraCoroutine = null;
        _moveObjectCoroutine = null;
        _justTimeWaitCoroutine = null;
        CoroutineManager.Instance?.StopAllCoroutinesFor(gameObject);

        if (GameManager.Instance != null) // а вот так GameManager.Instance?.onDialogueStarted -= DialogueWasStarted; нельзя, сука
        {
            GameManager.Instance.onDialogueStarted -= DialogueWasStarted;
        }
        scriptPlayer.onUnitWasKilled -= UnitWasKilled;
        scriptPlayer.OnEnemiesWaveWasDestroyedWithoutLosingMainTargets -= EnemiesWaveWasDestroyedWithoutLosingMainTargets;
    }


}

