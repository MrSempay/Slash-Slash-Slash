using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class Livel1Scenario_AsyncUsing : ScenarioScript
{
    // --- Поля из оригинала ---
    private Transform _transformSchool;
    private Transform _transformTreasury;
    private School _scriptSchool;
    private Treasury _scriptTreasury;
    private Unit _scriptFirstEnemyForKill;
    private float _moneyFromKillFirstEnemy = 250;
    private float _experienceFromKillFirstEnemy = 1500;
    private bool _firstBueSpell = true;
    private bool _firstBueAmmunition = true;
    private Camera _cameraPlayer;
    private bool _studyWasFinished = false;

    [SerializeField] private Transform _transformPointSpawnFirstEnemy;
    [SerializeField] private Transform _transformPointTeleportSchool;
    [SerializeField] private Transform _transformPointTeleportTreasury;
    [SerializeField] private GameObject _enemyPrefub;

    public GameObject school;
    public GameObject treasury;
    public Action OnStudyStart;
    public Action OnStudyFinish;

    // --- Async scaffolding ---
    private CancellationTokenSource _scenarioCts;

    // ожидалки
    private Dictionary<string, TaskCompletionSource<bool>> _dialogueTcs = new(StringComparer.OrdinalIgnoreCase);
    private Dictionary<string, TaskCompletionSource<bool>> _timerTcs = new(StringComparer.OrdinalIgnoreCase);
    private Dictionary<string, TaskCompletionSource<bool>> _cameraMoveTcs = new(StringComparer.OrdinalIgnoreCase);
    private Dictionary<string, TaskCompletionSource<bool>> _waveTcs = new(StringComparer.OrdinalIgnoreCase);

    // покупки
    private TaskCompletionSource<bool> _firstSpellBuyTcs;
    private TaskCompletionSource<bool> _firstAmmunitionBuyTcs;

    private readonly ReplayableEvent<bool> _spellBought = new ReplayableEvent<bool>();
    private readonly ReplayableEvent<bool> _ammoBought = new ReplayableEvent<bool>();

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

        // стартуем старую (синхронную) логику диалога не используем — запускаем async-сценарий.
        //StartDialogueNEW("Dialogue1.1");
        OnStudyStart?.Invoke();

        // Запускаем асинхронный сценарий (fire-and-forget, ошибки ловим внутри)
        StartScenario();
    }

    private void StartScenario()
    {
        _scenarioCts?.Cancel();
        _scenarioCts?.Dispose();
        _scenarioCts = new CancellationTokenSource();

        // fire-and-forget, ошибки обрабатываются внутри RunScenarioAsync
        _ = RunScenarioAsync(_scenarioCts.Token);
    }

    private async Task RunScenarioAsync(CancellationToken ct)
    {
        // Примерно воспроизводим последовательность, заложенную в оригинале.
        try
        {
            // Ждём первого диалога (если он был запущен в Start)
            // (в случае, если StartDialogueNEW уже запустил речь, DialogueFinished закроет tcs)
            await StartDialogueAsync(C.SS.Level1.Dialogues.Dialogue1_1, ct);
            //Debug.Log(1);

            if (!_studyWasFinished)
            {
                //Debug.Log(2);
                // Спавним первого врага и ждём, пока его убьют
                var first = await SpawnFirstEnemyAndWaitKillAsync(_enemyPrefub, _transformPointSpawnFirstEnemy.position, ct);
                // Ждём 2 секунды, как было в первоначальном коде
                await Task.Delay(TimeSpan.FromSeconds(2), ct);

                // Диалог после убийства
                await StartDialogueAsync(C.SS.Level1.Dialogues.Dialogue1_2, ct);

                // Телепорт игрока в школу (синхронно как раньше)

                DelinkCameraPlayer(_cameraPlayer); // нужно предварительно отвязать камеру от игрока прежде чем телепортировать, в MoveCameraToPlayerAsync это делать не вариант, бо он
                                                   // асинхронные теперь
                TeleportObjectToPoint(player, _transformPointTeleportSchool.position);

                // Перемещаем камеру и ждём завершения движения
                await MoveCameraToPlayerAsync(_cameraPlayer, transformPlayer, 16f, C.SS.Level1.CM.MoveAfterEnemyKilling, ct);

                // После перемещения камера вызывает Dialogue2.1 (в оригинале) — делаем это линейно
                await StartDialogueAsync(C.SS.Level1.Dialogues.Dialogue2_1, ct);

                // Теперь ждём покупок: если купили заклинание — воспроизводим диалог 2.2,
                // если купили амуницию — через секунду завершаем "учёбу" и продолжаем.
                _firstSpellBuyTcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                _firstAmmunitionBuyTcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

                // Ожидаем либо покупки заклинания (и возможно покупки амуниции позже), либо сразу покупки амуниции. 
                // боже, на кой тут цикл хD
                //while (!_studyWasFinished)
                //{
                //    //Debug.Log("И снова мы тут...");
                //    var completed = await Task.WhenAny(_firstSpellBuyTcs.Task, _firstAmmunitionBuyTcs.Task);
                //    //Debug.Log("И снова мы тут1...");
                //    ct.ThrowIfCancellationRequested();

                //    if (completed == _firstSpellBuyTcs.Task)
                //    {
                //        //Debug.Log("И снова мы тут...2");

                //        try { await StartDialogueAsync(C.SS.Level1.Dialogues.Dialogue2_2, ct); }
                //        catch (OperationCanceledException) { throw; }
                //        catch (Exception ex) { Debug.LogError($"Ошибка при запуске Dialogue2.2: {ex}"); }
                //        _firstSpellBuyTcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously); // не уверен, зачем это. Возможно нужно для того,
                //                                                                                                // чтобы система могла ожидать следующей покупки заклинания,
                //                            // если такое вдруг потребуется (хотя по логике “первой покупки” это может быть и не нужно, но такой шаблон защиты от повторного срабатывания).
                //        // Короче, понятно, зачем это. Пересоздаём задачу для того, чтоб при новой итерации цикла у нас в
                //        // var completed = await Task.WhenAny(_firstSpellBuyTcs.Task, _firstAmmunitionBuyTcs.Task) не было битой задачи _firstSpellBuyTcs.Task. После певрвого выполнения
                //        // задача завершается и числится завершенной, и поэтому когда мы снова входим в новую итерацию цикла, у нас Task.WhenAny срабатывает сразу же (ибо задача 
                //        // _firstSpellBuyTcs.Task уже была выполнена в первой итерации. Чтоб такого не было, надо её пересоздавать вот таким образом. Повторно всё равно она не сработает
                //        // ибо у нас в callback-е EquipmentWasSold стоит блокиратор для SetResult - допускает к нему только в случае первой покупки, далее у нас там флаг _firstBueSpell срабатывает

                //        // на что тут цикл-то...

                //        DelinkCameraPlayer(_cameraPlayer); 
                //        TeleportObjectToPoint(player, _transformPointTeleportTreasury.position);

                //        await MoveCameraToPlayerAsync(_cameraPlayer, transformPlayer, 16f, C.SS.Level1.CM.MoveAfterFirstSpellBue, ct);

                //        await StartDialogueAsync(C.SS.Level1.Dialogues.Dialogue3_1, ct);
                //    }
                //    else
                //    {
                //        await Task.Delay(TimeSpan.FromSeconds(1), ct);

                //        _studyWasFinished = true;
                //        OnStudyFinish?.Invoke();

                //        await StartDialogueAsync(C.SS.Level1.Dialogues.Dialogue3_2, ct);
                //        break;
                //    }
                //}

                var spellTask = _spellBought.WaitAsync(ct);
                var ammoTask = _ammoBought.WaitAsync(ct);

                if (_spellBought.HasValue) // интересная штука. Позволяет детектить, было ли выполнено событие до _spellBought.WaitAsync(ct) или ему ещё предстоит быть
                {
                    // событие уже произошло раньше
                    await StartDialogueAsync("О, так ты уже купил заклинание?", ct);

                    DelinkCameraPlayer(_cameraPlayer);
                    TeleportObjectToPoint(player, _transformPointTeleportTreasury.position);

                    await MoveCameraToPlayerAsync(_cameraPlayer, transformPlayer, 16f, C.SS.Level1.CM.MoveAfterFirstSpellBue, ct);
                }
                else // ну, по идее, должно всегда это выполняться. Ибо у нас двери-то закрыты в школу... Но сама механика интересная
                {
                    // событие ещё впереди, ждём
                    await spellTask;

                    try { await StartDialogueAsync(C.SS.Level1.Dialogues.Dialogue2_2, ct); }
                    catch (OperationCanceledException) { throw; }
                    catch (Exception ex) { Debug.LogError($"Ошибка при запуске Dialogue2.2: {ex}"); }

                    DelinkCameraPlayer(_cameraPlayer);
                    TeleportObjectToPoint(player, _transformPointTeleportTreasury.position);

                    await MoveCameraToPlayerAsync(_cameraPlayer, transformPlayer, 16f, C.SS.Level1.CM.MoveAfterFirstSpellBue, ct);
                }

                await StartDialogueAsync(C.SS.Level1.Dialogues.Dialogue3_1, ct);

                await ammoTask;

                await Task.Delay(TimeSpan.FromSeconds(1), ct);

                _studyWasFinished = true;
                OnStudyFinish?.Invoke();

                await StartDialogueAsync(C.SS.Level1.Dialogues.Dialogue3_2, ct);

                // После завершения учёбы — стартуем первую учебную волну (WaveAfterLearning)
                StartWaveEnemies(new Dictionary<Transform, int>() {
                    { transformPlayer, 5 },
                    { _transformSchool, 5 },
                    { _transformTreasury, 5 }
                }, C.SS.Level1.WN.WaveAfterLearning);

                await WaitForWaveAsync(C.SS.Level1.WN.WaveAfterLearning, ct);

                // После первой волны — диалог 4
                await StartDialogueAsync(C.SS.Level1.Dialogues.Dialogue4, ct);

                // После диалога 4 — запускаем SecondWave
                StartWaveEnemies(new Dictionary<Transform, int>() {
                    { transformPlayer, 7 },
                    { _transformSchool, 7 },
                    { _transformTreasury, 7 }
                }, C.SS.Level1.WN.Second);

                await WaitForWaveAsync(C.SS.Level1.WN.Second, ct);

                // пауза 10с перед третьей волной (как в оригинале)
                await Task.Delay(TimeSpan.FromSeconds(10), ct);

                StartWaveEnemies(new Dictionary<Transform, int>() {
                    { transformPlayer, 9 },
                    { _transformSchool, 9 },
                    { _transformTreasury, 9 }
                }, C.SS.Level1.WN.Third);

                await WaitForWaveAsync(C.SS.Level1.WN.Third, ct);

                await Task.Delay(TimeSpan.FromSeconds(8), ct);

                StartWaveEnemies(new Dictionary<Transform, int>() {
                    { transformPlayer, 12 },
                    { _transformSchool, 12 },
                    { _transformTreasury, 12 }
                }, C.SS.Level1.WN.Fourth);

                await WaitForWaveAsync(C.SS.Level1.WN.Fourth, ct);

                await Task.Delay(TimeSpan.FromSeconds(10), ct);

                StartWaveEnemies(new Dictionary<Transform, int>() {
                    { transformPlayer, 20 },
                    { _transformSchool, 20 },
                    { _transformTreasury, 20 }
                }, C.SS.Level1.WN.Fifth);

                await WaitForWaveAsync(C.SS.Level1.WN.Fifth, ct);

                // Финал уровня
                FinishLevel();
            }
        }
        catch (OperationCanceledException)
        {
            Debug.Log("Level1Scenario: сценарий отменён.");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Level1Scenario: ошибка в RunScenarioAsync: {ex}");
        }
    }

    // ---------------------- helper-обёртки ----------------------

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
        //Debug.Log("А диалог-то закончили");
        base.DialogueFinished(nameDialogueWithFolder);
        if (_dialogueTcs.TryGetValue(nameDialogueWithFolder, out var tcs))
        {
            tcs.TrySetResult(true);
            return;
        }

        // Если никто не ожидал — просто логируем (или можно сохранять старую ветвь поведения).
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

        // В оригинале тут были случаи, где таймер приводил к завершению обучения:
        // "WaitAfterFirstAmminitionBueBeforeFirstWave" -> OnStudyFinish
        if (markerTimeWait.Equals("WaitAfterFirstAmminitionBueBeforeFirstWave", StringComparison.OrdinalIgnoreCase))
        {
            OnStudyFinish?.Invoke(); // если кто-то слушает — вызовём
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

        // даём вознаграждение, если нужно (в оригинале EnemiesWaveWasDestroyedWithoutLosingMainTargets даёт награду)
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
        base.EquipmentWasSold(equipment);
        if (equipment.isEquipmentASpell)_spellBought.Publish(true);
        else _ammoBought.Publish(true);
    }

    //protected override void EquipmentWasSold(Equipment equipment)
    //{
    //    base.EquipmentWasSold(equipment);

    //    if (_studyWasFinished)
    //        return;

    //    if (equipment.isEquipmentASpell)
    //    {
    //        if (_firstBueSpell)
    //        {
    //            // Срабатывание покупки заклинания
    //            _firstSpellBuyTcs?.TrySetResult(true);
    //            _firstBueSpell = false;
    //        }
    //    }
    //    else
    //    {
    //        if (_firstBueAmmunition)
    //        {
    //            // Срабатывание покупки амуниции
    //            _firstAmmunitionBuyTcs?.TrySetResult(true);
    //            _firstBueAmmunition = false;
    //        }
    //    }
    //}

    protected override void UnitWasKilled(Unit unit)
    {
        base.UnitWasKilled(unit);

        // Оставляем оригинальную защиту: если обучение уже завершено — ничего не делаем
        if (_studyWasFinished)
            return;

        // Здесь нет явной логики: локальные подписки на конкретный unit обрабатывают ожидания,
        // но если нужны дополнительные глобальные реакции — можно добавить их.
    }

    private void FinishStudy()
    {
        _studyWasFinished = true;
        StartDialogueNEW("Dialogue3.2");
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

        // Отменим все ожидающие TCS-ы, чтобы ничего не зависло
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
