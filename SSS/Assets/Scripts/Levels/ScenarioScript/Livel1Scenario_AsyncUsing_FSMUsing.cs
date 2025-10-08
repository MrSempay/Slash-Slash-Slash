using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class Livel1Scenario_AsyncUsing_FSMUsing : ScenarioScript
{
    // --- Поля из оригинала ---
    private Transform _transformSchool;
    private Transform _transformTreasury;
    private School _scriptSchool;
    private Treasury _scriptTreasury;
    private Unit _scriptFirstEnemyForKill;
    private float _moneyFromKillFirstEnemy = 250;
    private float _experienceFromKillFirstEnemy = 1500;
    private Camera _cameraPlayer;
    private bool _studyWasFinished = false;

    [SerializeField] private Transform _transformPointSpawnFirstEnemy;
    [SerializeField] private Transform _transformPointTeleportSchool;
    [SerializeField] private Transform _transformPointTeleportTreasury;
    [SerializeField] private GameObject _enemyPrefub;

    public GameObject school;
    public GameObject treasury;
    public Action OnStudyStart;
    public Action<bool> OnStudyFinish;

    // --- Async scaffolding ---
    private CancellationTokenSource _scenarioCts;

    // ожидалки (по старому — словари для внешних событий)
    private Dictionary<string, TaskCompletionSource<bool>> _dialogueTcs = new(StringComparer.OrdinalIgnoreCase);
    private Dictionary<string, TaskCompletionSource<bool>> _timerTcs = new(StringComparer.OrdinalIgnoreCase);
    private Dictionary<string, TaskCompletionSource<bool>> _cameraMoveTcs = new(StringComparer.OrdinalIgnoreCase);
    private Dictionary<string, TaskCompletionSource<bool>> _waveTcs = new(StringComparer.OrdinalIgnoreCase);

    // покупки — (оставил для совместимости, но используем ReplayableEvent)
    private TaskCompletionSource<bool> _firstSpellBuyTcs;
    private TaskCompletionSource<bool> _firstAmmunitionBuyTcs;

    private readonly ReplayableEvent<bool> _spellBought = new ReplayableEvent<bool>();
    private readonly ReplayableEvent<bool> _ammoBought = new ReplayableEvent<bool>();

    // --- State machine ---
    public enum Step
    {
        WaitDialogue1_1,
        SpawnFirstEnemyIfNeeded,
        AfterFirstEnemyDelay,
        Dialogue1_2,
        TeleportToSchool_MoveCameraAfterEnemyKill,
        Dialogue2_1,
        WaitForBuyOrAmmo,
        MoveAfterFirstSpellBue,
        Dialogue3_1,
        WaitAmmo,
        FinishStudyDelayThenDialogue3_2,
        StartWaveAfterLearning,
        WaitWaveAfterLearning,
        Dialogue4,
        StartSecondWave,
        WaitSecondWave,
        PreThirdWaveDelay,
        StartThirdWave,
        WaitThirdWave,
        PreFourthWaveDelay,
        StartFourthWave,
        WaitFourthWave,
        PreFifthWaveDelay,
        StartFifthWave,
        WaitFifthWave,
        FinishLevel,
        End
    }

    private volatile Step _currentStep = Step.WaitDialogue1_1;
    private CancellationTokenSource _stepCts; // per-step CTS to cancel waits on jump

    protected override void Awake()
    {
        base.Awake();

        instance = this;

        _transformSchool = school.GetComponent<Transform>();
        _scriptSchool = school.GetComponent<School>();

        _transformTreasury = treasury.GetComponent<Transform>();
        _scriptTreasury = treasury.GetComponent<Treasury>();

        _scriptSchool.onUpdateAssortment += AssortmentInBuildingWasUpdated;
        _scriptTreasury.onUpdateAssortment += AssortmentInBuildingWasUpdated;
        OnStudyFinish += FinishStudy;

        _cameraPlayer = GameObject.Find("CameraPlayer").GetComponent<Camera>();

        dictionaryNamesEnemiesWavesAndRewards = new()
        {
            { "WaveAfterLearning", 40000 },
            { "JustSecondWave", 10000 },
        };
    }

    protected override void Start()
    {
        base.Start();
        OnStudyStart?.Invoke();
        StartScenario();
    }

    private void StartScenario()
    {
        _scenarioCts?.Cancel();
        _scenarioCts?.Dispose();
        _scenarioCts = new CancellationTokenSource();

        // запуск FSM (fire-and-forget)
        _ = RunScenarioLoop(_scenarioCts.Token);
    }

    /// <summary>
    /// Запрос на прыжок к другому шагу сценария.
    /// Вызов может идти извне (кнопка skip, событие и т.п.).
    /// </summary>
    public void RequestJump(Step target)
    {
        Debug.Log($"[Scenario] RequestJump -> {target}");
        _currentStep = target;
        // отменим текущий step token — loop обработает отмену и перейдёт к новому шагу
        try { _stepCts?.Cancel(); } catch { }
    }

    /// <summary>
    /// Главный цикл сценария: выполняет шаги по _currentStep.
    /// При RequestJump устанавливается новый _currentStep и отменяется _stepCts,
    /// что позволяет выйти из текущего ожидания и перейти к необходимому шагу.
    /// </summary>
    private async Task RunScenarioLoop(CancellationToken ct)
    {
        try
        {
            while (_currentStep != Step.End && !ct.IsCancellationRequested)
            {
                // per-step CTS — связанный с глобальным cancellation token
                _stepCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                var stepToken = _stepCts.Token;

                try
                {
                    switch (_currentStep)
                    {
                        case Step.WaitDialogue1_1:
                            Door.LockOrDelockAllDoors(true);

                            await StartDialogueAsync(C.SS.Level1.Dialogues.Dialogue1_1, stepToken);
                            _currentStep = Step.SpawnFirstEnemyIfNeeded;
                            break;

                        case Step.SpawnFirstEnemyIfNeeded:
                            if (!_studyWasFinished)
                            {
                                await SpawnFirstEnemyAndWaitKillAsync(_enemyPrefub, _transformPointSpawnFirstEnemy.position, stepToken);
                                // delay 2s after first enemy kill
                                await Task.Delay(TimeSpan.FromSeconds(2), stepToken);
                            }
                            _currentStep = Step.Dialogue1_2;
                            break;

                        case Step.Dialogue1_2:
                            await StartDialogueAsync(C.SS.Level1.Dialogues.Dialogue1_2, stepToken);
                            // teleport to school synchronously (as original)
                            DelinkCameraPlayer(_cameraPlayer);
                            TeleportObjectToPoint(player, _transformPointTeleportSchool.position);
                            _currentStep = Step.TeleportToSchool_MoveCameraAfterEnemyKill;
                            break;

                        case Step.TeleportToSchool_MoveCameraAfterEnemyKill:
                            await MoveCameraToPlayerAsync(_cameraPlayer, transformPlayer, 16f, C.SS.Level1.CM.MoveAfterEnemyKilling, stepToken);
                            _currentStep = Step.Dialogue2_1;
                            break;

                        case Step.Dialogue2_1:
                            await StartDialogueAsync(C.SS.Level1.Dialogues.Dialogue2_1, stepToken);
                            _currentStep = Step.WaitForBuyOrAmmo;
                            break;

                        case Step.WaitForBuyOrAmmo:
                            // prepare waiting tasks (use ReplayableEvent — safe vs race)
                            _firstSpellBuyTcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                            _firstAmmunitionBuyTcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

                            var spellTask = _spellBought.WaitAsync(stepToken);
                            var ammoTask = _ammoBought.WaitAsync(stepToken);

                            if (_spellBought.HasValue)
                            {
                                // если уже куплено до того, как мы пришли сюда
                                await StartDialogueAsync("О... так ты уже купил заклинание?", stepToken); // прикол в том, что это всё не сработает, если заклинание не было куплено, но просто
                                // есть по умолчанию в инвентаре

                                DelinkCameraPlayer(_cameraPlayer);
                                TeleportObjectToPoint(player, _transformPointTeleportTreasury.position);
                                _currentStep = Step.MoveAfterFirstSpellBue;
                            }
                            else
                            {
                                // ждём покупки заклинания (или пока наступит jump — тогда OperationCanceledException)
                                await spellTask; // если published ранее, вернётся мгновенно
                                try
                                {
                                    await StartDialogueAsync(C.SS.Level1.Dialogues.Dialogue2_2, stepToken);
                                }
                                catch (OperationCanceledException) { throw; }
                                catch (Exception ex) { Debug.LogError($"Ошибка при запуске Dialogue2.2: {ex}"); }

                                DelinkCameraPlayer(_cameraPlayer);
                                TeleportObjectToPoint(player, _transformPointTeleportTreasury.position);
                                _currentStep = Step.MoveAfterFirstSpellBue;
                            }
                            break;

                        case Step.MoveAfterFirstSpellBue:
                            await MoveCameraToPlayerAsync(_cameraPlayer, transformPlayer, 16f, C.SS.Level1.CM.MoveAfterFirstSpellBue, stepToken);
                            _currentStep = Step.Dialogue3_1;
                            break;

                        case Step.Dialogue3_1:
                            await StartDialogueAsync(C.SS.Level1.Dialogues.Dialogue3_1, stepToken);
                            _currentStep = Step.WaitAmmo;
                            break;

                        case Step.WaitAmmo:
                            // Wait for ammo (this will be already created earlier as ammoTask or new WaitAsync)
                            // Use WaitAsync with current token
                            await _ammoBought.WaitAsync(stepToken);
                            _currentStep = Step.FinishStudyDelayThenDialogue3_2;
                            break;

                        case Step.FinishStudyDelayThenDialogue3_2:
                            await Task.Delay(TimeSpan.FromSeconds(1), stepToken);
                            _studyWasFinished = true;
                            OnStudyFinish?.Invoke(true); // нужно именно эмулировать сигнал, бо у нас некоторые товарищи на него подписаны (LevelBuilder, например). Просто вызвать
                            // StudyFinish нельзя
                            await StartDialogueAsync(C.SS.Level1.Dialogues.Dialogue3_2, stepToken);
                            _currentStep = Step.StartWaveAfterLearning;
                            break;

                        case Step.StartWaveAfterLearning:
                            Debug.Log("А что такое...");
                            StartWaveEnemies(new Dictionary<Transform, int> {
                                { transformPlayer, 5 },
                                { _transformSchool, 5 },
                                { _transformTreasury, 5 }
                            }, C.SS.Level1.WN.WaveAfterLearning);
                            _currentStep = Step.WaitWaveAfterLearning;
                            break;

                        case Step.WaitWaveAfterLearning:
                            await WaitForWaveAsync(C.SS.Level1.WN.WaveAfterLearning, stepToken);
                            _currentStep = Step.Dialogue4;
                            break;

                        case Step.Dialogue4:
                            await StartDialogueAsync(C.SS.Level1.Dialogues.Dialogue4, stepToken);
                            _currentStep = Step.StartSecondWave;
                            break;

                        case Step.StartSecondWave:
                            StartWaveEnemies(new Dictionary<Transform, int> {
                                { transformPlayer, 7 },
                                { _transformSchool, 7 },
                                { _transformTreasury, 7 }
                            }, C.SS.Level1.WN.Second);
                            _currentStep = Step.WaitSecondWave;
                            break;

                        case Step.WaitSecondWave:
                            await WaitForWaveAsync(C.SS.Level1.WN.Second, stepToken);
                            _currentStep = Step.PreThirdWaveDelay;
                            break;

                        case Step.PreThirdWaveDelay:
                            await Task.Delay(TimeSpan.FromSeconds(10), stepToken);
                            StartWaveEnemies(new Dictionary<Transform, int> {
                                { transformPlayer, 9 },
                                { _transformSchool, 9 },
                                { _transformTreasury, 9 }
                            }, C.SS.Level1.WN.Third);
                            _currentStep = Step.WaitThirdWave;
                            break;

                        case Step.WaitThirdWave:
                            await WaitForWaveAsync(C.SS.Level1.WN.Third, stepToken);
                            _currentStep = Step.PreFourthWaveDelay;
                            break;

                        case Step.PreFourthWaveDelay:
                            await Task.Delay(TimeSpan.FromSeconds(8), stepToken);
                            StartWaveEnemies(new Dictionary<Transform, int> {
                                { transformPlayer, 12 },
                                { _transformSchool, 12 },
                                { _transformTreasury, 12 }
                            }, C.SS.Level1.WN.Fourth);
                            _currentStep = Step.WaitFourthWave;
                            break;

                        case Step.WaitFourthWave:
                            await WaitForWaveAsync(C.SS.Level1.WN.Fourth, stepToken);
                            _currentStep = Step.PreFifthWaveDelay;
                            break;

                        case Step.PreFifthWaveDelay:
                            await Task.Delay(TimeSpan.FromSeconds(10), stepToken);
                            StartWaveEnemies(new Dictionary<Transform, int> {
                                { transformPlayer, 20 },
                                { _transformSchool, 20 },
                                { _transformTreasury, 20 }
                            }, C.SS.Level1.WN.Fifth);
                            _currentStep = Step.WaitFifthWave;
                            break;

                        case Step.WaitFifthWave:
                            await WaitForWaveAsync(C.SS.Level1.WN.Fifth, stepToken);
                            _currentStep = Step.FinishLevel;
                            break;

                        case Step.FinishLevel:
                            FinishLevel();
                            _currentStep = Step.End;
                            break;

                        case Step.End:
                        default:
                            _currentStep = Step.End;
                            break;
                    }
                }
                catch (OperationCanceledException)
                {
                    // Ожидание прервано (возможно RequestJump установил новый _currentStep).
                    // Не логируем как ошибку — просто идём дальше и обработаем новый шаг.
                }
                finally
                {
                    _stepCts.Dispose();
                    _stepCts = null;
                }
            }
        }
        catch (OperationCanceledException)
        {
            Debug.Log("Level1Scenario: сценарий отменён (глобально).");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Level1Scenario: ошибка в RunScenarioLoop: {ex}");
        }
    }

    // ---------------------- helper-обёртки (как у тебя были) ----------------------


    private Task StartDialogueAsync(string dialogueName, CancellationToken ct)
    {
        var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        // Положим в словарь до вызова StartDialogueNEW — чтобы не пропустить быстрый ответ
        _dialogueTcs[dialogueName] = tcs;

        var reg = ct.Register(() =>
        {
            if (_dialogueTcs.Remove(dialogueName))
                tcs.TrySetCanceled(ct);
        });

        try
        {
            //Debug.Log(dialogueName);
            StartDialogue(dialogueName);
        }
        catch (Exception ex)
        {
            _dialogueTcs.Remove(dialogueName);
            reg.Dispose();
            tcs.TrySetException(ex);
            return tcs.Task;
        }

        // Очистка словаря по завершению
        tcs.Task.ContinueWith(_ => {
            reg.Dispose();
            _dialogueTcs.Remove(dialogueName);
        }, TaskScheduler.Default);

        return tcs.Task;
    }

    private Task WaitForTimerAsync(string timerMarker, float seconds, CancellationToken ct)
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

    private Task MoveCameraToPlayerAsync(Camera cam, Transform playerTransform, float speed, string finishKey, CancellationToken ct)
    {
        var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        _cameraMoveTcs[finishKey] = tcs;

        var reg = ct.Register(() =>
        {
            if (_cameraMoveTcs.Remove(finishKey))
                tcs.TrySetCanceled(ct);
        });

        try
        {
            MovingCameraPlayerToPoint(cam, playerTransform, speed, finishKey);
        }
        catch (Exception ex)
        {
            _cameraMoveTcs.Remove(finishKey);
            reg.Dispose();
            tcs.TrySetException(ex);
            return tcs.Task;
        }

        tcs.Task.ContinueWith(_ => {
            reg.Dispose();
            _cameraMoveTcs.Remove(finishKey);
        }, TaskScheduler.Default);

        return tcs.Task;
    }

    private async Task<Unit> SpawnFirstEnemyAndWaitKillAsync(GameObject enemyPrefab, Vector3 pos, CancellationToken ct)
    {
        //Debug.Log("А хули не спавним?");
        GameObject enemyObj = SpawnObjectAtTargetPosition(enemyPrefab, pos);
        var spawnedUnit = enemyObj.GetComponent<Unit>();
        _scriptFirstEnemyForKill = spawnedUnit;

        if (spawnedUnit == null)
            throw new InvalidOperationException("Спавн не дал Unit'а");

        var tcs = new TaskCompletionSource<Unit>(TaskCreationOptions.RunContinuationsAsynchronously);

        IncreaseRewardFromKillUnit(_scriptFirstEnemyForKill, _moneyFromKillFirstEnemy, _experienceFromKillFirstEnemy);

        // объявляем обработчик того же типа, что и событие в Unit
        Unit.UnitWasKilled handler = null;
        handler = (killedUnit) =>
        {
            // отписываемся и выдаём результат
            spawnedUnit.onUnitWasKilled -= handler;
            tcs.TrySetResult(killedUnit);
        };

        // подписываемся
        spawnedUnit.onUnitWasKilled += handler;

        // Отмена: если ct отменится — убираем подписку и помечаем TCS как отменённую
        using (ct.Register(() =>
        {
            spawnedUnit.onUnitWasKilled -= handler;
            tcs.TrySetCanceled();
        }))
        {
            // ожидаем результата (не используем ConfigureAwait(false) — хотим остаться в Unity main thread)
            return await tcs.Task;
        }
    }

    private Task WaitForWaveAsync(string waveName, CancellationToken ct)
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


    // ---------------------- переопределённые обработчики событий ---------------------- 

    protected override void DialogueFinished(string nameDialogueWithFolder)
    {
        base.DialogueFinished(nameDialogueWithFolder);
        if (_dialogueTcs.TryGetValue(nameDialogueWithFolder, out var tcs))
        {
            tcs.TrySetResult(true);
            return;
        }
        Debug.Log($"DialogueFinished (no awaiter): {nameDialogueWithFolder}");
    }

    protected override void TimerFinished(string markerTimeWait)
    {
        base.TimerFinished(markerTimeWait);
        if (_timerTcs.TryGetValue(markerTimeWait, out var tcs))
        {
            tcs.TrySetResult(true);
            return;
        }
        Debug.Log($"TimerFinished (no awaiter): {markerTimeWait}");
    }

    protected override void MovingCameraPlayerWasFinished(string keyFinishing)
    {
        base.MovingCameraPlayerWasFinished(keyFinishing);
        if (_cameraMoveTcs.TryGetValue(keyFinishing, out var tcs))
        {
            tcs.TrySetResult(true);
            return;
        }
        Debug.Log($"MovingCameraPlayerWasFinished (no awaiter): {keyFinishing}");
    }

    protected override void EnemiesWaveWasDestroyed(string nameWave)
    {
        base.EnemiesWaveWasDestroyed(nameWave);
        if (_waveTcs.TryGetValue(nameWave, out var tcs))
        {
            tcs.TrySetResult(true);
            return;
        }
        Debug.Log($"EnemiesWaveWasDestroyed (no awaiter): {nameWave}");
    }

    protected override void EnemiesWaveWasDestroyedWithoutLosingMainTargets(string nameWave)
    {
        base.EnemiesWaveWasDestroyedWithoutLosingMainTargets(nameWave);
        if (dictionaryNamesEnemiesWavesAndRewards != null && dictionaryNamesEnemiesWavesAndRewards.TryGetValue(nameWave, out var reward))
        {
            scriptPlayer.GiveRewardScore(reward);
        }
    }

    protected override void EquipmentWasSold(Equipment equipment)
    {
        Debug.Log("Не уж то ли Was Sold???");
        base.EquipmentWasSold(equipment);
        if (equipment.isEquipmentASpell) _spellBought.Publish(true);
        else _ammoBought.Publish(true);
    }

    protected override void UnitWasKilled(Unit unit)
    {
        base.UnitWasKilled(unit);
        if (_studyWasFinished)
            return;
    }

    private void FinishStudy(bool wasFinishedByNativeWay)
    {
        if (!wasFinishedByNativeWay)
        {
            RequestJump(Step.FinishStudyDelayThenDialogue3_2);
        }
        Door.LockOrDelockAllDoors(false);
    }

    private void IncreaseRewardFromKillUnit(Unit unitForKill, float moneyFromKill, float experienceFromKill)
    {
        unitForKill.moneyFromKill = moneyFromKill;
        unitForKill.experienceFromKill = experienceFromKill;
    }

    private void DelinkCameraPlayer(Camera cameraPlayer)
    {
        Transform transformCameraPlayer = cameraPlayer.transform;
        transformCameraPlayer.SetParent(null);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        _scenarioCts?.Cancel();
        _scenarioCts?.Dispose();
        _scenarioCts = null;

        // Отменим ожидающие TCS-ы
        foreach (var kv in _dialogueTcs) kv.Value.TrySetCanceled();
        _dialogueTcs.Clear();

        foreach (var kv in _timerTcs) kv.Value.TrySetCanceled();
        _timerTcs.Clear();

        foreach (var kv in _cameraMoveTcs) kv.Value.TrySetCanceled();
        _cameraMoveTcs.Clear();

        foreach (var kv in _waveTcs) kv.Value.TrySetCanceled();
        _waveTcs.Clear();

        _firstSpellBuyTcs?.TrySetCanceled();
        _firstAmmunitionBuyTcs?.TrySetCanceled();

        _scriptSchool.onUpdateAssortment -= AssortmentInBuildingWasUpdated;
        _scriptTreasury.onUpdateAssortment -= AssortmentInBuildingWasUpdated;
    }
}
