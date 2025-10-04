using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

public class LevelBuildLevel1 : LevelBuilder
{
    public int som;

    [SerializeField] private RectTransform _rtPlaceButtonSkipStudy;

    //private Level1Scenario scriptLevelScenario;
    private Livel1Scenario_AsyncUsing scriptLevelScenario;
    private GameObject _buttonSkipStudy;

    protected override void Awake()
    {

        selfName = "Level1";

        base.Awake();
        instance = this;

        // получаем компоненты transform у целей
        /*
        playerTransform = GameObject.Find("Player").transform;
        treasuryTransform = GameObject.Find("Treasury").transform;
        schoolTransform = GameObject.Find("School").transform;

        targetPointsForEnemy = new Dictionary<Transform, int>()
        {
            {playerTransform, 15},
            {schoolTransform, 20},
            {treasuryTransform, 8} 
        }; */
        //OnValidate();


        // получаем компоненты transform у точек для спавна
        clusterSpawnPointsTransform = GameObject.Find("SpawnPoints")?.transform;

        // проверяем, что есть кластер для точек спавна врагов на уровне
        if (clusterSpawnPointsTransform)
        {
            foreach (Transform child in clusterSpawnPointsTransform)
            {
                spawnPointsTransforms.Add(child);
            }
            if (spawnEnemyByTimerCoroutine == null)
            {
                spawnEnemyByTimerCoroutine = CoroutineManager.Instance.StartManagedCoroutine(this.gameObject, SpawnEnemyByTimer());
            }
        }

        //scriptLevelScenario = (Level1Scenario)ScenarioScript.instance;
        scriptLevelScenario = (Livel1Scenario_AsyncUsing)ScenarioScript.instance;
        if (GameManager.Instance.MaxReachedLevel > 0)
        {
            scriptLevelScenario.OnStudyStart += InstanceSkipStudyButton;
            scriptLevelScenario.OnStudyFinish += DestroySkipStudyButton;
        }

    }

    private void InstanceSkipStudyButton()
    {
        _buttonSkipStudy = GameManager.Instance.InstanceTextButton(false, _rtPlaceButtonSkipStudy, C.Other.SkipStudy, () => { scriptLevelScenario.OnStudyFinish?.Invoke(); });
    }

    private void DestroySkipStudyButton()
    {
        Destroy(_buttonSkipStudy);
    }


    protected override void Start()
    {
        base.Start();
    }

}
