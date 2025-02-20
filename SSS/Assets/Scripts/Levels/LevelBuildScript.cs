using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class LevelBuildScript : MonoBehaviour
{
    // ¬ коде будем использовать словарь, дл€ удобства, поэтому создадим его на основе списка

    [SerializeField] private Enemy enemy;
    [SerializeField] private List<TransformIntPair> targetPointsList = new List<TransformIntPair>(); // храним массив элементов класса, описывающего элемент словар€ ключ-значени€
                                                                                                     // transform-int дл€ задани€ целей врагам.

    private Dictionary<Transform, int> targetPointsForEnemy = new Dictionary<Transform, int>(); // ключом €вл€етс€ ссылка на компонент transform цели, значением - количество
                                                                                                // врагов, которые направ€тс€ к цели.
    private Dictionary<string, float> percentageIncreaseEnemiesParametersBySpawnIteration = new Dictionary<string, float>() // увеличение параметров на %
    {
            { "healthMax", 5 },
            { "damageReduction", 0 },
            { "speed", 0 },
            { "jumpForce", 0 },
            { "damage", 5 } };

    private Coroutine spawnEnemyByTimerCoroutine;
    private List<Transform> spawnPointsTransforms; // массив дл€ компонентов Transform всех точек спавна
    private Transform clusterSpawnPointsTransform; // кластер (родительский элемент) всех точек спавна
    private bool isStillEnemyForSpawn;
    private int numberOfSpawnIteration = 0; // чтоб знали, насколько усиливать врагов на текущей итерации
    private List<Transform> targetTransformsForRandom;




    void Awake()
    {
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
        // получаем компоненты transform у точек дл€ спавна
        spawnPointsTransforms = new List<Transform>();
        clusterSpawnPointsTransform = GameObject.Find("SpawnPoints").transform;

        // провер€ем, что есть кластер дл€ точек спавна врагов на уровне
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
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    private IEnumerator SpawnEnemyByTimer()
    {
        while (true)
        {
            foreach (var spawnPointTransform in spawnPointsTransforms)
            {
                SpawnEnemy(spawnPointTransform);
            }
            numberOfSpawnIteration++;
            yield return new WaitForSeconds(2f);
        }
    }

    private void SpawnEnemy(Transform spawnPointTransform)
    {
        if (enemy != null && spawnPointTransform != null) // ѕровер€ем, что ссылки установлены
        {
            // получаем список ключей из словар€. ¬ список добавл€ем только те позиции, на которые ещЄ должны идти враги
            targetTransformsForRandom = new List<Transform>();
            foreach (var targetPoint in targetPointsForEnemy)
            {
                if (targetPoint.Value > 0)
                {
                    if (targetPoint.Key != null) targetTransformsForRandom.Add(targetPoint.Key); // теоретически у нас может не быть на уровне одной из предустановленных в скрипте 
                                                                                                 // целевых точек дл€ врага. ¬ таком случае соответствующее поле Transfrom будет null
                }
            }

            // если массив не пустой (то есть были позици, на которые ещЄ должны идти враги), то движемс€ далее
            if (targetTransformsForRandom.Count > 0) {
                // генерируем случайный индекс
                int randomIndex = UnityEngine.Random.Range(0, targetTransformsForRandom.Count);

                // получаем случайный Transform (ключ) из списка
                Transform randomTarget = targetTransformsForRandom[randomIndex];

                // инстанцируем врага
                Enemy newEnemy = Instantiate(enemy, spawnPointTransform.position, spawnPointTransform.rotation);

                // уменьшаем количество врагов дл€ заданной врагу позиции
                targetPointsForEnemy[randomTarget]--;
                // присваиваем случайный Transform врагу
                newEnemy.currentTargetTransform = randomTarget;
                newEnemy.isInstancedByLevel = true;
                Dictionary<string, float> percentageIncreasedEnemiesParametersBySpawnIteration = new Dictionary<string, float>(); // делаем новый словарь чтоб сохрнаить первозданные значени€
                                                                                                                                  // дл€ усилени€ юнитов
                foreach (var increasingValue in percentageIncreaseEnemiesParametersBySpawnIteration)
                {
                    string newKey = string.Copy(increasingValue.Key); // —оздаем новый экземпл€р строки
                    float newValue = increasingValue.Value * numberOfSpawnIteration; // ƒл€ float это простое копирование значени€
                    percentageIncreasedEnemiesParametersBySpawnIteration.Add(newKey, newValue);
                }
                newEnemy.ChangeUnitParametersByPercentage(percentageIncreasedEnemiesParametersBySpawnIteration);
                return;
            }
            CoroutineManager.Instance.StopManagedCoroutine(this.gameObject, spawnEnemyByTimerCoroutine);
        }
    }

    private void OnValidate() // ¬ызываетс€ в редакторе при изменении значений
    {
        // —инхронизируем список со словарем, чтобы изменени€ в инспекторе сохран€лись в словаре
        targetPointsForEnemy.Clear();
        foreach (var pair in targetPointsList)
        {
            if (pair.target != null && !targetPointsForEnemy.ContainsKey(pair.target))
            {
                targetPointsForEnemy[pair.target] = pair.enemyCount;
            }
        }
    }
}
