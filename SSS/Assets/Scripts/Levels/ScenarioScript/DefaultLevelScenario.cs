using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using System.Linq;
using static CoroutineExtensions;
using static StaticClassForAdditionalFunctions;

public class DefaultLevelScenario : ScenarioScript
{
    private int _currentNumberWave = 0;
    private Coroutine _coroutineWaitNextWave; 
    private GameObject _objHourglass; 

    [SerializeField] private readonly string _nameDialogueStart = C.Dilogues.DialogueStart;
    [SerializeField] private readonly string _nameDialogueFinish = C.Dilogues.DialogueFinish;
    [SerializeField] private float _timeAfterFirstDialogueBeforeFirstWave = 15f;
    [SerializeField] private float _timeAfterLastWaveBeforeFinishDialogue = 3f;
    [SerializeField] private float _timeAfterFinishDialogueBeforePassLevel = 15f;
    [SerializeField] private List<InfoAboutEnemyWave> _listInfoAboutEnemiesWaves;

    [System.Serializable]
    class InfoAboutEnemyWave // планируется использовать для шаблона уровней типа: диалог => N-ое кол-во волн => диалог 
    {
        //public int amountEnemiesGenerally;
        public float timeBetweenEnemySpawnIteration = 2f;
        public int scoreRewardIfWaveCompleted;
        public List<TransformIntPair> targetPointAndAmountEnemiesList;
        public float timeBeforeNextWave = 15f;
    }

    protected override void Awake()
    {
        base.Awake(); 

        instance = this;

        dictionaryNamesEnemiesWavesAndRewards = new();

        for (int i = 0; i < _listInfoAboutEnemiesWaves.Count; i++)
        {
            dictionaryNamesEnemiesWavesAndRewards[i.ToString()] = _listInfoAboutEnemiesWaves[i].scoreRewardIfWaveCompleted;
            //Debug.Log(dictionaryNamesEnemiesWavesAndRewards[i.ToString()]);
            //Debug.Log(i.ToString());
        }

        //StartDialogue($"{levelBuildScript.selfName}/{C.Dilogues.DialogueStart}"); 
    }

    protected override void Start()
    {
        base.Start();
        //StartDialogue($"{levelBuildScript.selfName}/{C.Dilogues.DialogueStart}");
        StartScenario();
    }

    protected override void EnemiesWaveWasDestroyedWithoutLosingMainTargets(string nameWave)
    {
        Debug.Log("Текущая волна: " + _currentNumberWave);
        Debug.Log("Текущая волна: " + nameWave);
        Debug.Log("Текущая волна: " + dictionaryNamesEnemiesWavesAndRewards[nameWave]);
        scriptPlayer.GiveRewardScore(dictionaryNamesEnemiesWavesAndRewards[nameWave]);
    }

    private void StartDefaultEnemiesWave()
    {
        if (_listInfoAboutEnemiesWaves.Count > 0)
        {
            Dictionary<Transform, int> dictionaryTargetsAndEnemies = new();

            foreach (TransformIntPair targetCountPair in _listInfoAboutEnemiesWaves[_currentNumberWave].targetPointAndAmountEnemiesList)
            {
                dictionaryTargetsAndEnemies[targetCountPair.target] = targetCountPair.enemyCount;
            }

            LevelBuilder.instance.timeBetweenEnemySpawnIteration = _listInfoAboutEnemiesWaves[_currentNumberWave].timeBetweenEnemySpawnIteration;

            StartWaveEnemies(dictionaryTargetsAndEnemies,
                             _currentNumberWave.ToString());
        }

    }



    /// <summary>
    /// Начинаем интеграцию FSM-сценария на асинхронных методах
    /// </summary>

    #region FSM-async scenario integraion



    public enum StepDS // Step Defeat Scenario
    {
        Start,
        MoveCameraThroughMainTargets,
        MoveCameraToPlayerAndFadeLight,
        StartEndDialogue,
        End
    }
    public enum StepMS // Step Defeat Scenario
    {
        Start,
        StartDialogue,
        WavesFighting,
        WaitBeforeEndDialogue,
        EndDialogue,
        WaitAfterEndDialogue,
        End
    }
    public enum ScenarioMode
    {
        MainScenario,      // Обычный проход уровня
        DefeatScenario,    // Сценарий поражения (проигрыша)
        End
    }


    private volatile ScenarioMode _currentMode = ScenarioMode.MainScenario;
    private volatile StepDS _currentStepDS = StepDS.Start;
    private volatile StepMS _currentStepMS = StepMS.Start;
    private CancellationTokenSource _defeatScenarioCts;

    private void RequestJumpStep(StepMS target)
    {
        Debug.Log($"[Scenario Step] RequestJump -> {target}");
        _currentStepMS = target;
        var current = Volatile.Read(ref _stepCts);
        try { current?.Cancel(); } catch { }
    }
    private void RequestJumpScenario(ScenarioMode target)
    {
        Debug.Log($"[Scenario Mode] RequestJump -> {target}");
        _currentMode = target;
        var current = Volatile.Read(ref _stepCts);
        try { current?.Cancel(); } catch { }
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
        while (!ct.IsCancellationRequested && _currentMode == ScenarioMode.MainScenario)
        {
            var stepCts = CreateLinkedStepCts(ct);
            var stepToken = stepCts.Token;

            if (_currentMode != ScenarioMode.MainScenario) // or DefeatScenario in defeat loop
            {
                try { stepCts.Cancel(); } catch { }
                // cleanup happens in finally
                break;
            }

            try
            {
                switch (_currentStepMS)
                {
                    case StepMS.Start:

                        _currentStepMS = StepMS.StartDialogue;
                        break;
                    case StepMS.StartDialogue:

                        await StartDialogueAsync(LevelBuilder.instance.selfName + "/" + C.SS.General.Dialogues.DialogueStart, stepToken);
                        //await Task.Delay(TimeSpan.FromSeconds(_timeAfterFinishDialogueBeforePassLevel), stepToken);
                        //FinishLevel();
                        await WaitForTimerWithSkipAsyncNEW(C.SS.LevelDefault.WaitBeforeFirstWave, _listInfoAboutEnemiesWaves[_currentNumberWave].timeBeforeNextWave, C.Other.SkipWaveWait, stepToken);

                        _currentStepMS = StepMS.WavesFighting;
                        break;
                    case StepMS.WavesFighting:

                        foreach (var wave in _listInfoAboutEnemiesWaves)
                        {
                            string nameWave = _currentNumberWave.ToString();
                            if (_listInfoAboutEnemiesWaves.Count > 0)
                            {
                                Dictionary<Transform, int> dictionaryTargetsAndEnemies = new(); // для Legacy

                                Dictionary<Transform, List<TypesAndAmountEnemies>> dicionaryTargetsAndTypesEnemies = new();

                                foreach (TransformIntPair targetCountPair in _listInfoAboutEnemiesWaves[_currentNumberWave].targetPointAndAmountEnemiesList)
                                {
                                    dictionaryTargetsAndEnemies[targetCountPair.target] = targetCountPair.enemyCount; // для Legacy
                                    dicionaryTargetsAndTypesEnemies[targetCountPair.target] = targetCountPair.typesAndAmountEnemies;
                                }

                                // если у нас в рамках волны для хоть одного из таргетов массив wave.targetPointAndAmountEnemiesList[?].typesAndAmountEnemies пусть, то
                                // используем Legacy-вызов волны, иначе - новый
                                // Вообще, это не лучший вариант. Если мы нашли нулевой массив, для какой-то точки, то нужно ссылаться на targetCountPair.enemyCount и спавнить для него
                                // только наших врагов по умолчанию (перворанговых собак), но оставим это на совесть балансировщиков.
                                // Изменили, теперь запускаем Legacy-код только если У ВСЕХ ТАРГЕТОВ список типов врагов пуст!
                                bool shouldStartWaveLEGACY = wave.targetPointAndAmountEnemiesList.All(targetPoint => targetPoint.typesAndAmountEnemies.Count == 0);

                                if (shouldStartWaveLEGACY)
                                {
                                    StartWaveEnemies(dictionaryTargetsAndEnemies,
                                                    _currentNumberWave.ToString());
                                }
                                else
                                {
                                    StartWaveEnemiesNEW(dicionaryTargetsAndTypesEnemies,
                                                    _currentNumberWave.ToString());
                                }

                                LevelBuilder.instance.timeBetweenEnemySpawnIteration = _listInfoAboutEnemiesWaves[_currentNumberWave].timeBetweenEnemySpawnIteration;
                            }

                            await WaitForWaveDestroyAsync(nameWave, stepToken);

                            if (_currentNumberWave < _listInfoAboutEnemiesWaves.Count - 1) // если не достигли последней волны
                            {
                                await WaitForTimerWithSkipAsyncNEW(nameWave, _listInfoAboutEnemiesWaves[_currentNumberWave].timeBeforeNextWave, C.Other.SkipWaveWait, stepToken);
                                _currentNumberWave++;
                            }

                        }

                        //FinishLevel();
                        _currentStepMS = StepMS.WaitBeforeEndDialogue;
                        break;
                    case StepMS.WaitBeforeEndDialogue:

                        //await WaitForTimerAsync(C.SS.LevelDefault.WaitBeforeLastDialogue, _timeAfterLastWaveBeforeFinishDialogue, stepToken);
                        await Task.Delay(TimeSpan.FromSeconds(_timeAfterLastWaveBeforeFinishDialogue), stepToken);

                        _currentStepMS = StepMS.EndDialogue;
                        break;
                    case StepMS.EndDialogue:

                        await StartDialogueAsync(LevelBuilder.instance.selfName + "/" + C.SS.General.Dialogues.DialogueFinish, stepToken);

                        _currentStepMS = StepMS.WaitAfterEndDialogue;
                        break;
                    case StepMS.WaitAfterEndDialogue:

                        //await WaitForTimerAsync(C.SS.LevelDefault.WaitAfterLastDialogue, _timeAfterFinishDialogueBeforePassLevel, stepToken);
                        await Task.Delay(TimeSpan.FromSeconds(_timeAfterFinishDialogueBeforePassLevel), stepToken);
                        _currentStepMS = StepMS.End;
                        break;
                    case StepMS.End:

                        FinishLevel();
                        await Task.Delay(Timeout.Infinite, stepToken);
                        break;
                }
            }
            catch (OperationCanceledException)
            {
                if (_currentMode != ScenarioMode.MainScenario) return;
                Debug.Log($"{levelBuildScript.selfName}: сценарий отменён (глобально).");
            }
            catch (Exception ex)
            {
                Debug.LogError($"{levelBuildScript.selfName}: ошибка в RunDefeatScenarioLoop: {ex}");
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

    private async Task RunDefeatScenarioLoop(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested && _currentMode == ScenarioMode.DefeatScenario)
        {

            var stepCts = CreateLinkedStepCts(ct);
            var stepToken = stepCts.Token;
            //Debug.Log("Ебанейший пиздец");

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

                            //Debug.Log("И чё за параша?");
                        CameraManager.Instance.DelinkCameraPlayer();
                        Player.instance.scriptUI.HideAllUI();

                        _currentStepDS = StepDS.MoveCameraThroughMainTargets;
                        break;
                    case StepDS.MoveCameraThroughMainTargets:

                        foreach (IMainTarget mainTarget in _allDeterminedMTExceptedPlayer)
                        {
                            //Debug.Log("И чё за параша?");
                            var paramSchool = new CameraManager.CameraMoveParams
                            {
                                Camera = Player.instance.mainCamera,
                                Target = mainTarget.targetTransform,
                                FinishKey = C.SS.Level1.CM.MoveThroughSomeMT,
                                CancellationToken = stepToken,
                                MoveToPlayer = false,
                                Time = 1f,
                                MoveType = CameraManager.CameraMoveType.EaseInOut33
                            };
                            await CameraManager.Instance.MoveCameraToTargetAsync(paramSchool);
                        }

                        _currentStepDS = StepDS.MoveCameraToPlayerAndFadeLight;
                        break;
                    case StepDS.MoveCameraToPlayerAndFadeLight:

                        var paramPlayer = new CameraManager.CameraMoveParams
                        {
                            Camera = Player.instance.mainCamera,
                            Target = Player.instance.transform,
                            FinishKey = C.SS.Level1.CM.Move3AfterDefeat,
                            CancellationToken = stepToken,
                            MoveToPlayer = true,
                            Speed = 16f,
                            EnableUpdateFuncAfter = false
                        };
                        _ = CameraManager.Instance.MoveCameraToTargetAsync(paramPlayer);
                        _ = AudioManager.Instance.FadeAllEnviromentSoundsAsync();

                        await LightManager.Instance.FadeAllLightsAsync(1);

                        _currentStepDS = StepDS.StartEndDialogue;
                        break;
                    case StepDS.StartEndDialogue:

                        Task lastDialogueTask = StartDialogueAsync(LevelBuilder.instance.selfName + "/" + C.SS.General.Dialogues.DefeatByTargets, stepToken); // запустили диалог в фоне, не ждём
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

    protected internal override void Defeat()
    {
        //Debug.Log("aaa");
        _defeatScenarioCts?.Cancel();
        _defeatScenarioCts?.Dispose();
        _defeatScenarioCts = new CancellationTokenSource();

        // запуск FSM (fire-and-forget)
        //_ = RunMainScenarioLoop(_mainScenarioCts.Token);
        Player.instance.IsInvincible = true;

        RequestJumpScenario(ScenarioMode.DefeatScenario);
        //_ = RunDefeatScenarioLoop(_defeatScenarioCts.Token);

    }




    #endregion FSM-async scenario integraion

    protected override void OnDestroy()
    {
        base.OnDestroy();
        _defeatScenarioCts?.Cancel();
        _defeatScenarioCts?.Dispose();
        _defeatScenarioCts = null;

        _masterScenarioCts?.Cancel();
        _masterScenarioCts?.Dispose();
        _masterScenarioCts = null;
    }

}
