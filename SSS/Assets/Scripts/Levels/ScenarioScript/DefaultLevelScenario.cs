using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class DefaultLevelScenario : ScenarioScript
{
    private int _currentNumberWave = 0;

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
        StartDialogue($"{levelBuildScript.selfName}/{C.Dilogues.DialogueStart}");
    }

    // ########################################        БЛОК ФУНКЦИЙ-РЕАКЦИЙ        ######################################## //


    protected override void DialogueFinished(string nameDialogueWithFolder)
    {
        
        base.DialogueFinished(nameDialogueWithFolder);
        if (nameDialogueWithFolder.Split('/')[1] == C.Dilogues.DialogueStart)
        {
            JustTimeWait(_timeAfterFirstDialogueBeforeFirstWave, "waitTimeBeforeFirstWave");
        }
        else if (nameDialogueWithFolder.Split('/')[1] == C.Dilogues.DialogueFinish) // в теории, может, у нас будут и другие диалоги на уровне кроме этих двух. Ну, в будущем... недалёком...
        {
            JustTimeWait(_timeAfterFinishDialogueBeforePassLevel, "waitTimeAfterFinishDialogueBeforePassLevel");
        }

    }

    protected override void TimerFinished(string markerTimeWait)
    {
        base.TimerFinished(markerTimeWait);
        switch (markerTimeWait)
        {
            case "waitTimeBeforeFirstWave":

                StartDefaultEnemiesWave();

                break;
            case "waitTimeBeforeNextWave":

                _currentNumberWave++;

                StartDefaultEnemiesWave();

                break;

            case "waitAfterLastWaveBeforeFinishDialogue":

                StartDialogue($"{levelBuildScript.selfName}/{C.Dilogues.DialogueFinish}");

                break;

            case "waitTimeAfterFinishDialogueBeforePassLevel":

                FinishLevel();

                break;
        }
    }

    protected override void EnemiesWaveWasDestroyedWithoutLosingMainTargets(string nameWave)
    {
        Debug.Log("Текущая волна: " + _currentNumberWave);
        Debug.Log("Текущая волна: " + nameWave);
        Debug.Log("Текущая волна: " + dictionaryNamesEnemiesWavesAndRewards[nameWave]);
        scriptPlayer.GiveRewardScore(dictionaryNamesEnemiesWavesAndRewards[nameWave]);
    }

    protected override void EnemiesWaveWasDestroyed(string nameWave)
    {
        base.EnemiesWaveWasDestroyed(nameWave);
        if (_currentNumberWave < _listInfoAboutEnemiesWaves.Count - 1)
        {
            JustTimeWait(_listInfoAboutEnemiesWaves[_currentNumberWave].timeBeforeNextWave, "waitTimeBeforeNextWave");
        }
        else
        {
            JustTimeWait(_timeAfterLastWaveBeforeFinishDialogue, "waitAfterLastWaveBeforeFinishDialogue");
        }

        
    }

    // ########################################        СЛУЖЕБНЫЕ ФУНКЦИИ        ######################################## //

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

}
