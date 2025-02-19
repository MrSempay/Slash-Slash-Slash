using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class LevelBuildScript : MonoBehaviour
{
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Transform schoolTransform;
    [SerializeField] private Transform treasuryTransform;
    [SerializeField] private Enemy enemy;

    private Coroutine spawnEnemyByTimerCoroutine;
    private List<Transform> spawnPointsTransforms; // массив для компонентов Transform всех точек спавна
    private Transform clusterSpawnPointsTransform; // кластер (родительский элемент) всех точек спавна
    private bool isStillEnemyForSpawn;
    private List<Transform> targetTransformsForRandom;
    private Dictionary<Transform, int> targetPointsForEnemy; // ключом является ссылка на компонент transform цели, значением - количество врагов, которые направятся к цели.

    void Awake()
    {
        // получаем компоненты transform у целей
        playerTransform = GameObject.Find("Player").transform;
        treasuryTransform = GameObject.Find("Treasury").transform;
        schoolTransform = GameObject.Find("School").transform;

        targetPointsForEnemy = new Dictionary<Transform, int>()
        {
            {playerTransform, 15},
            {schoolTransform, 20},
            {treasuryTransform, 8}
        };

        // получаем компоненты transform у точек для спавна
        spawnPointsTransforms = new List<Transform>();
        clusterSpawnPointsTransform = GameObject.Find("SpawnPoints").transform;

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
            yield return new WaitForSeconds(2f);
        }
    }

    private void SpawnEnemy(Transform spawnPointTransform)
    {
        if (enemy != null && spawnPointTransform != null) // Проверяем, что ссылки установлены
        {
            // получаем список ключей из словаря. В список добавляем только те позиции, на которые ещё должны идти враги
            targetTransformsForRandom = new List<Transform>();
            foreach (var targetPoint in targetPointsForEnemy)
            {
                if (targetPoint.Value > 0)
                {
                    if (targetPoint.Key != null) targetTransformsForRandom.Add(targetPoint.Key); // теоретически у нас может не быть на уровне одной из предустановленных в скрипте 
                                                                                                 // целевых точек для врага. В таком случае соответствующее поле Transfrom будет null
                }
            }
            
            // если массив не пустой (то есть были позици, на которые ещё должны идти враги), то движемся далее
            if (targetTransformsForRandom.Count > 0) {
                // генерируем случайный индекс
                int randomIndex = UnityEngine.Random.Range(0, targetTransformsForRandom.Count);

                // получаем случайный Transform (ключ) из списка
                Transform randomTarget = targetTransformsForRandom[randomIndex];

                // инстанцируем врага
                Enemy newEnemy = Instantiate(enemy, spawnPointTransform.position, spawnPointTransform.rotation);

                // уменьшаем количество врагов для заданной врагу позиции
                targetPointsForEnemy[randomTarget]--;
                // присваиваем случайный Transform врагу
                newEnemy.currentTargetTransform = randomTarget;
                newEnemy.isInstancedByLevel = true;
            }

        }
    }
}
