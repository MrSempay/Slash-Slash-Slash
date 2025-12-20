using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using static StaticClassForAdditionalFunctions;

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
    //private CancellationTokenSource _mainScenarioCts;
    //private CancellationTokenSource _defeatScenarioCts;

    // ожидалки (по старому — словари для внешних событий)
    private Dictionary<string, TaskCompletionSource<bool>> _dialogueTcs = new(StringComparer.OrdinalIgnoreCase);
    //private Dictionary<string, TaskCompletionSource<bool>> _cameraMoveTcs = new(StringComparer.OrdinalIgnoreCase);

    // покупки — (оставил для совместимости, но используем ReplayableEvent)
    private TaskCompletionSource<bool> _firstSpellBuyTcs;
    private TaskCompletionSource<bool> _firstAmmunitionBuyTcs;

    private readonly ReplayableEvent<bool> _spellBought = new ReplayableEvent<bool>();
    private readonly ReplayableEvent<bool> _ammoBought = new ReplayableEvent<bool>();

    // --- State machine ---
    public enum StepMS // Step Main Scenario
    {
        WaitDialogue1_1,
        SpawnFirstEnemyIfNeeded,
        AfterFirstEnemyDelay,
        Dialogue1_2,
        TeleportToSchool_MoveCameraAfterEnemyKill,
        Dialogue2_1,
        WaitForBuyOrAmmo,
        MoveAfterFirstSpellBuy,
        Dialogue3_1,
        WaitAmmo,
        FinishStudyDelayThenDialogue3_2,
        StartWaveAfterLearning,
        WaitDestroyWaveAfterLearning,
        Dialogue4,
        StartSecondWave,
        WaitDestroySecondWave,
        PreThirdWaveDelay,
        StartThirdWave,
        WaitDestroyThirdWave,
        PreFourthWaveDelay,
        StartFourthWave,
        WaitDestroyFourthWave,
        PreFifthWaveDelay,
        StartFifthWave,
        WaitDestroyFifthWave,
        FinishLevel,
        End
    }
    public enum StepDS // Step Defeat Scenario
    {
        Start,
        MoveCameraThroughMainTargets,
        MoveCameraToPlayerAndFadeLight,
        StartEndDialogue,
        End
    }
    public enum ScenarioMode
    {
        MainScenario,      // Обычный проход уровня
        DefeatScenario,    // Сценарий поражения (проигрыша)
        End
    }


    private volatile ScenarioMode _currentMode = ScenarioMode.MainScenario;
    private volatile StepMS _currentStepMS = StepMS.WaitDialogue1_1;
    private volatile StepDS _currentStepDS = StepDS.Start;


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
        _masterScenarioCts?.Cancel();
        _masterScenarioCts?.Dispose();
        _masterScenarioCts = new CancellationTokenSource();

        // запуск FSM (fire-and-forget)
        //_ = RunMainScenarioLoop(_mainScenarioCts.Token);
        _ = RunMasterScenarioLoop(_masterScenarioCts.Token);
    }


    /// <summary>
    /// Запрос на прыжок к другому шагу сценария.
    /// Вызов может идти извне (кнопка skip, событие и т.п.).
    /// </summary>
    public void RequestJumpStep(StepMS target)
    {
        Debug.Log($"[Scenario Step] RequestJump -> {target}");
        _currentStepMS = target;
        var current = Volatile.Read(ref _stepCts);
        try { current?.Cancel(); } catch { }
    }
    public void RequestJumpScenario(ScenarioMode target)
    {
        Debug.Log($"[Scenario Mode] RequestJump -> {target}");
        _currentMode = target;
        var current = Volatile.Read(ref _stepCts);
        try { current?.Cancel(); } catch { }
    }


    private async Task RunMasterScenarioLoop(CancellationToken ct)
    {
        try
        {
            while (_currentMode != ScenarioMode.End && !ct.IsCancellationRequested)
            {
                switch (_currentMode)
                {
                    case ScenarioMode.MainScenario:
                        await RunMainScenarioLoop(ct);
                        break;

                    case ScenarioMode.DefeatScenario:
                        await RunDefeatScenarioLoop(ct);
                        // Сценарий поражения завершился
                        _currentMode = ScenarioMode.End;
                        break;
                }
            }
        }
        catch (OperationCanceledException)
        {
            Debug.Log("Master scenario cancelled");
        }
    }

    private async Task RunMainScenarioLoop(CancellationToken ct)
    {
        try
        {
            while (!ct.IsCancellationRequested && _currentMode == ScenarioMode.MainScenario)
            {
                // per-step CTS — связанный с глобальным cancellation token
                var stepCts = CreateLinkedStepCts(ct);
                var stepToken = stepCts.Token;

                if (_currentMode != ScenarioMode.MainScenario) // or DefeatScenario in defeat loop
                {
                    try { stepCts.Cancel(); } catch { }
                    // cleanup happens in finally
                    continue;
                }

                try
                {
                    switch (_currentStepMS)
                    {
                        case StepMS.WaitDialogue1_1:
                            Door.LockOrDelockAllDoors(true);

                            await StartDialogueAsync(C.SS.Level1.Dialogues.Dialogue1_1, stepToken);
                            _currentStepMS = StepMS.SpawnFirstEnemyIfNeeded;
                            break;

                        case StepMS.SpawnFirstEnemyIfNeeded:
                            if (!_studyWasFinished)
                            {
                                await SpawnFirstEnemyAndWaitKillAsync(_enemyPrefub, _transformPointSpawnFirstEnemy.position, stepToken);
                                // delay 2s after first enemy kill
                                await Task.Delay(TimeSpan.FromSeconds(2), stepToken);
                            }
                            _currentStepMS = StepMS.Dialogue1_2;
                            break;

                        case StepMS.Dialogue1_2:
                            await StartDialogueAsync(C.SS.Level1.Dialogues.Dialogue1_2, stepToken);
                            // teleport to school synchronously (as original)
                            CameraManager.Instance.DelinkCameraPlayer();
                            TeleportObjectToPoint(player, _transformPointTeleportSchool.position);
                            _currentStepMS = StepMS.TeleportToSchool_MoveCameraAfterEnemyKill;
                            break;

                        case StepMS.TeleportToSchool_MoveCameraAfterEnemyKill:

                            var param = new CameraManager.CameraMoveParams
                            {
                                Camera = _cameraPlayer,
                                Target = transformPlayer,
                                FinishKey = C.SS.Level1.CM.MoveAfterEnemyKilling,
                                CancellationToken = stepToken,
                                MoveToPlayer = true,
                                Speed = 16f,
                            };
                            await CameraManager.Instance.MoveCameraToTargetAsync(param);

                            _currentStepMS = StepMS.Dialogue2_1;
                            break;

                        case StepMS.Dialogue2_1:
                            await StartDialogueAsync(C.SS.Level1.Dialogues.Dialogue2_1, stepToken);
                            _currentStepMS = StepMS.WaitForBuyOrAmmo;
                            break;

                        case StepMS.WaitForBuyOrAmmo:
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
                                _currentStepMS = StepMS.MoveAfterFirstSpellBuy;
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

                                CameraManager.Instance.DelinkCameraPlayer();
                                TeleportObjectToPoint(player, _transformPointTeleportTreasury.position);
                                _currentStepMS = StepMS.MoveAfterFirstSpellBuy;
                            }
                            break;

                        case StepMS.MoveAfterFirstSpellBuy:

                            var spellBuyParams = new CameraManager.CameraMoveParams
                            {
                                Camera = _cameraPlayer,
                                Target = transformPlayer,
                                FinishKey = C.SS.Level1.CM.MoveAfterFirstSpellBue,
                                CancellationToken = stepToken,
                                MoveToPlayer = true,
                                Speed = 16f,
                            };
                            await CameraManager.Instance.MoveCameraToTargetAsync(spellBuyParams);

                            _currentStepMS = StepMS.Dialogue3_1;
                            break;

                        case StepMS.Dialogue3_1:
                            await StartDialogueAsync(C.SS.Level1.Dialogues.Dialogue3_1, stepToken);
                            _currentStepMS = StepMS.WaitAmmo;
                            break;

                        case StepMS.WaitAmmo:
                            // Wait for ammo (this will be already created earlier as ammoTask or new WaitAsync)
                            // Use WaitAsync with current token
                            await _ammoBought.WaitAsync(stepToken);
                            _currentStepMS = StepMS.FinishStudyDelayThenDialogue3_2;
                            break;

                        case StepMS.FinishStudyDelayThenDialogue3_2:
                            await Task.Delay(TimeSpan.FromSeconds(1), stepToken);
                            _studyWasFinished = true;
                            OnStudyFinish?.Invoke(true); // нужно именно эмулировать сигнал, бо у нас некоторые товарищи на него подписаны (LevelBuilder, например). Просто вызвать
                            // StudyFinish нельзя
                            await StartDialogueAsync(C.SS.Level1.Dialogues.Dialogue3_2, stepToken);
                            _currentStepMS = StepMS.StartWaveAfterLearning;
                            break;

                        case StepMS.StartWaveAfterLearning:
                            await WaitForTimerWithSkipAsyncNEW(C.SS.Level1.TN.BeforeFirstWave, 10f, C.Other.SkipWaveWait, stepToken);
                            StartWaveEnemies(new Dictionary<Transform, int> {
                                { transformPlayer, 5 },
                                { _transformSchool, 5 },
                                { _transformTreasury, 5 }
                            }, C.SS.Level1.WN.WaveAfterLearning);
                            _currentStepMS = StepMS.WaitDestroyWaveAfterLearning;
                            break;

                        case StepMS.WaitDestroyWaveAfterLearning:
                            await WaitForWaveDestroyAsync(C.SS.Level1.WN.WaveAfterLearning, stepToken);
                            _currentStepMS = StepMS.Dialogue4;
                            break;

                        case StepMS.Dialogue4:
                            await StartDialogueAsync(C.SS.Level1.Dialogues.Dialogue4, stepToken);
                            _currentStepMS = StepMS.StartSecondWave;
                            break;

                        case StepMS.StartSecondWave:
                            await WaitForTimerWithSkipAsyncNEW(C.SS.Level1.TN.BeforeSecondWave, 10f, C.Other.SkipWaveWait, stepToken);
                            StartWaveEnemies(new Dictionary<Transform, int> {
                                { transformPlayer, 7 },
                                { _transformSchool, 7 },
                                { _transformTreasury, 7 }
                            }, C.SS.Level1.WN.Second);
                            _currentStepMS = StepMS.WaitDestroySecondWave;
                            break;

                        case StepMS.WaitDestroySecondWave:
                            await WaitForWaveDestroyAsync(C.SS.Level1.WN.Second, stepToken);
                            _currentStepMS = StepMS.PreThirdWaveDelay;
                            break;

                        case StepMS.PreThirdWaveDelay:
                            await WaitForTimerWithSkipAsyncNEW(C.SS.Level1.TN.BeforeThirdWave, 10f, C.Other.SkipWaveWait, stepToken);
                            StartWaveEnemies(new Dictionary<Transform, int> {
                                { transformPlayer, 9 },
                                { _transformSchool, 9 },
                                { _transformTreasury, 9 }
                            }, C.SS.Level1.WN.Third);
                            _currentStepMS = StepMS.WaitDestroyThirdWave;
                            break;

                        case StepMS.WaitDestroyThirdWave:
                            await WaitForWaveDestroyAsync(C.SS.Level1.WN.Third, stepToken);
                            _currentStepMS = StepMS.PreFourthWaveDelay;
                            break;

                        case StepMS.PreFourthWaveDelay:
                            await WaitForTimerWithSkipAsyncNEW(C.SS.Level1.TN.BeforeFourthWave, 10f, C.Other.SkipWaveWait, stepToken);
                            StartWaveEnemies(new Dictionary<Transform, int> {
                                { transformPlayer, 12 },
                                { _transformSchool, 12 },
                                { _transformTreasury, 12 }
                            }, C.SS.Level1.WN.Fourth);
                            _currentStepMS = StepMS.WaitDestroyFourthWave;
                            break;

                        case StepMS.WaitDestroyFourthWave:
                            await WaitForWaveDestroyAsync(C.SS.Level1.WN.Fourth, stepToken);
                            _currentStepMS = StepMS.PreFifthWaveDelay;
                            break;

                        case StepMS.PreFifthWaveDelay:
                            await WaitForTimerWithSkipAsyncNEW(C.SS.Level1.TN.BeforeFifthWave, 10f, C.Other.SkipWaveWait, stepToken);
                            StartWaveEnemies(new Dictionary<Transform, int> {
                                { transformPlayer, 20 },
                                { _transformSchool, 20 },
                                { _transformTreasury, 20 }
                            }, C.SS.Level1.WN.Fifth);
                            _currentStepMS = StepMS.WaitDestroyFifthWave;
                            break;

                        case StepMS.WaitDestroyFifthWave:
                            await WaitForWaveDestroyAsync(C.SS.Level1.WN.Fifth, stepToken);
                            _currentStepMS = StepMS.FinishLevel;
                            break;

                        case StepMS.FinishLevel:
                            FinishLevel();
                            _currentStepMS = StepMS.End;
                            break;

                        case StepMS.End:
                        default:
                            FinishLevel();
                            await Task.Delay(Timeout.Infinite, stepToken);
                            _currentStepMS = StepMS.End;
                            break;
                    }
                }
                catch (OperationCanceledException)
                {
                    Debug.Log("Ожидание прервано (возможно RequestJump установил новый _currentStep)");
                    if (_currentMode != ScenarioMode.MainScenario) return;
                    // Ожидание прервано (возможно RequestJump установил новый _currentStep).
                    // Не логируем как ошибку — просто идём дальше и обработаем новый шаг.
                }
                finally
                {
                    if (Interlocked.CompareExchange(ref _stepCts, null, stepCts) == stepCts)
                    {
                        try { stepCts.Dispose(); } catch { }
                    }
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

    private async Task RunDefeatScenarioLoop(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested && _currentMode == ScenarioMode.DefeatScenario)
        {
            var stepCts = CreateLinkedStepCts(ct);
            var stepToken = stepCts.Token;

            if (_currentMode != ScenarioMode.DefeatScenario) // or DefeatScenario in defeat loop
            {
                try { stepCts.Cancel(); } catch { }
                // cleanup happens in finally
                break;
            }

            try
            {
                switch (_currentStepDS)
                {
                    case StepDS.Start:

                        CameraManager.Instance.DelinkCameraPlayer();
                        Player.instance.scriptUI.HideAllUI();

                        _currentStepDS = StepDS.MoveCameraThroughMainTargets;
                        break;
                    case StepDS.MoveCameraThroughMainTargets:

                        var paramSchool = new CameraManager.CameraMoveParams
                        {
                            Camera = _cameraPlayer,
                            Target = _transformSchool,
                            FinishKey = C.SS.Level1.CM.Move1AfterDefeat,
                            CancellationToken = stepToken,
                            MoveToPlayer = false,
                            Time = 1f,
                            MoveType = CameraManager.CameraMoveType.EaseInOut33
                        };
                        await CameraManager.Instance.MoveCameraToTargetAsync(paramSchool);

                        var paramTreasury = new CameraManager.CameraMoveParams
                        {
                            Camera = _cameraPlayer,
                            Target = _transformTreasury,
                            FinishKey = C.SS.Level1.CM.Move2AfterDefeat, 
                            CancellationToken = stepToken,
                            MoveToPlayer = false,
                            Time = 1f,
                            MoveType = CameraManager.CameraMoveType.EaseInOut33
                        };
                        await CameraManager.Instance.MoveCameraToTargetAsync(paramTreasury);

                        _currentStepDS = StepDS.MoveCameraToPlayerAndFadeLight;
                        break;
                    case StepDS.MoveCameraToPlayerAndFadeLight:

                        var paramPlayer = new CameraManager.CameraMoveParams
                        {
                            Camera = _cameraPlayer,
                            Target = Player.instance.transform,
                            FinishKey = C.SS.Level1.CM.Move3AfterDefeat,
                            CancellationToken = stepToken,
                            MoveToPlayer = true,
                            Speed = 16f,
                            EnableUpdateFuncAfter = false
                        };
                        _ = CameraManager.Instance.MoveCameraToTargetAsync(paramPlayer);
                        _ = AudioManager.Instance.FadeAllEnviromentSoundsAsync();

                        AudioManager.Instance.StartDefeatMusicInLoop();

                        await LightManager.Instance.FadeAllLightsAsync(1);

                        _currentStepDS = StepDS.StartEndDialogue;
                        break;
                    case StepDS.StartEndDialogue:

                        Task lastDialogueTask = StartDialogueAsync(LevelBuilder.instance.selfName + "/" + C.SS.General.Dialogues.DefeatByTargets, ct); // запустили диалог в фоне, не ждём
                        Task getActualLeaderboardTask = ScoreManager.Instance.GetActualLeaderboardAsync(); // запустили обновление лидерборда в фоне, не ждём
                        Task safeLeaderboardTask = SafeIgnoreErrors(getActualLeaderboardTask);

                        // Короче! Немного о WhenAll - если одна из задач будет по той или иной причине Canel, то await Task.WhenAll, во-первых, перестанет await-ить это всё дело, а
                        // во-вторых - выкинет OperationCanceledException. То есть код после него не выполнится. Но при этом он НЕ ПРЕРВЁТ выполнение других задач в его рамках. Ну, например,
                        // если getActualLeaderboardTask отменится. то произойдёт всё вышеописанное, но lastDialogueTask не отменится и будет в фоне выполняться, вот так.
                        await Task.WhenAll(lastDialogueTask, safeLeaderboardTask); // ждём, покуда выполнятся обе задачи

                        ScoreManager.Instance.ShowLeaderboard(Leaderboard.INSTANTIATION_CONTEXT.Defeat);

                        _currentStepDS = StepDS.End;
                        break;
                    case StepDS.End:
                        await Task.Delay(Timeout.Infinite, stepToken);
                        break;
                }
            }
            catch (OperationCanceledException)
            {
                if (_currentMode != ScenarioMode.DefeatScenario) return;
                Debug.Log("Level1Scenario: сценарий отменён (глобально).");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Level1Scenario: ошибка в RunDefeatScenarioLoop: {ex}");
            }
            finally
            {
                if (Interlocked.CompareExchange(ref _stepCts, null, stepCts) == stepCts)
                {
                    try { stepCts.Dispose(); } catch { }
                }
            }

        }
    }

    // ---------------------- helper-обёртки (как у тебя были) ----------------------


    
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



    // ---------------------- переопределённые обработчики событий ---------------------- 
    protected internal override void Defeat()
    {
        Player.instance.IsInvincible = true;
        RequestJumpScenario(ScenarioMode.DefeatScenario);
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
            RequestJumpStep(StepMS.FinishStudyDelayThenDialogue3_2);
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

        //_mainScenarioCts?.Cancel();
        //_mainScenarioCts?.Dispose();
        //_mainScenarioCts = null;
        //Debug.Log("Чё за нах?&&&&");
        _masterScenarioCts?.Cancel();
        _masterScenarioCts?.Dispose();
        _masterScenarioCts = null;

        // Отменим ожидающие TCS-ы
        foreach (var kv in _dialogueTcs) kv.Value.TrySetCanceled();
        _dialogueTcs.Clear();

        //foreach (var kv in _timerTcs) kv.Value.TrySetCanceled();
        //_timerTcs.Clear();

        //foreach (var kv in _cameraMoveTcs) kv.Value.TrySetCanceled();
        //_cameraMoveTcs.Clear();

        //foreach (var kv in _waveTcs) kv.Value.TrySetCanceled();
        //_waveTcs.Clear();

        _firstSpellBuyTcs?.TrySetCanceled();
        _firstAmmunitionBuyTcs?.TrySetCanceled();

        _scriptSchool.onUpdateAssortment -= AssortmentInBuildingWasUpdated;
        _scriptTreasury.onUpdateAssortment -= AssortmentInBuildingWasUpdated;
        OnStudyFinish -= FinishStudy;
    }
}
