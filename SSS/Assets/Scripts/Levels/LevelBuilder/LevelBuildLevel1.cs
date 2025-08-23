using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class LevelBuildLevel1 : LevelBuilder
{
    public int som;

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

    }



    protected override void Start()
    {
        base.Start();

        SettingsMenu[] allObjects = Resources.FindObjectsOfTypeAll<SettingsMenu>();
        allObjects[0].Awake(); // ну и фигня, нельзя к Instance обратиться, бо он инициализируется у нас в Awake
        SaveLoadManager.Instance.ImplementStoredSettings(); // чтоб настройки применялись при загрузке сцены сразу, а не после открытия меню настроек 
    }

}
