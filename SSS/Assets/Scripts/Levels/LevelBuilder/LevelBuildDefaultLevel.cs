using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using static UnityEngine.GraphicsBuffer;

public class LevelBuildDefaultLevel : LevelBuilder
{

    //[SerializeField] private readonly string _nameLevel = SceneManager.GetActiveScene().name; // багается, нужно вызывать это в Awake() или Start() 
    
    protected override void Awake()
    {
        selfName = SceneManager.GetActiveScene().name;

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

    }



    protected override void Start()
    {
        base.Start();
    }

}
