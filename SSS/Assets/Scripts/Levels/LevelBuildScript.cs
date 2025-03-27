using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class LevelBuildScript : MonoBehaviour
{
    // В коде будем использовать словарь, для удобства, поэтому создадим его на основе списка

    [SerializeField] private Enemy enemy;
    [SerializeField] private List<TransformIntPair> targetPointsList = new List<TransformIntPair>(); // храним массив элементов класса, описывающего элемент словаря ключ-значения
                                                                                                     // transform-int для задания целей врагам. В целом это поле для реализации
                                                                                                     // возможности задавать цели и количество врагов для них через редактор

    private Dictionary<Transform, int> targetPointsForEnemy = new Dictionary<Transform, int>(); // ключом является ссылка на компонент transform цели, значением - количество
                                                                                                // врагов, которые направятся к цели.
    private Dictionary<string, float> percentageIncreaseEnemiesParametersBySpawnIteration = new Dictionary<string, float>() // увеличение параметров на %
    {
            { "healthMax", 5 },
            { "damageReduction", 0 },
            { "speed", 0 },
            { "jumpForce", 0 },
            { "damage", 5 } };

    private Coroutine spawnEnemyByTimerCoroutine;
    private List<Transform> spawnPointsTransforms; // массив для компонентов Transform всех точек спавна
    private Transform clusterSpawnPointsTransform; // кластер (родительский элемент) всех точек спавна
    private bool isStillEnemyForSpawn;
    private int numberOfSpawnIteration = 0; // чтоб знали, насколько усиливать врагов на текущей итерации
    private List<Transform> targetTransformsForRandom;

    public Dictionary<Transform, int> TargetPointsForEnemy
    {
        get { return targetPointsForEnemy; }
        set
        {
            // 1. Обновляем существующий словарь, а не создаем новый
            targetPointsForEnemy.Clear(); // Очищаем старый словарь
            foreach (var targetPoint in value)
            {
                targetPointsForEnemy[targetPoint.Key] = targetPoint.Value; // Копируем элементы из нового словаря
            }

            // 2. Определяем, нужно ли запускать или останавливать корутину
            bool shouldSpawnEnemies = targetPointsForEnemy.Any(targetPoint => targetPoint.Value > 0);

            // 3. Запускаем или останавливаем корутину в зависимости от условия
            if (shouldSpawnEnemies && spawnEnemyByTimerCoroutine == null)
            {
                spawnEnemyByTimerCoroutine = CoroutineManager.Instance.StartManagedCoroutine(this.gameObject, SpawnEnemyByTimer());
            }
            else if (!shouldSpawnEnemies && spawnEnemyByTimerCoroutine != null)
            {
                CoroutineManager.Instance.StopManagedCoroutine(this.gameObject, spawnEnemyByTimerCoroutine);
                spawnEnemyByTimerCoroutine = null;
            }
        }
    }

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
        SettingsMenu[] allObjects = Resources.FindObjectsOfTypeAll<SettingsMenu>();
        Debug.Log(GameObject.Find("SettingsMenu"));
        allObjects[0].Awake(); // ну и фигня, нельзя к Instance обратиться, бо он инициализируется у нас в Awake
        SaveLoadManager.Instance.ImplementStoredSettings(); // чтоб настройки применялись при загрузке сцены сразу, а не после открытия меню настроек
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
                Dictionary<string, float> percentageIncreasedEnemiesParametersBySpawnIteration = new Dictionary<string, float>(); // делаем новый словарь чтоб сохрнаить первозданные значения
                                                                                                                                  // для усиления юнитов
                foreach (var increasingValue in percentageIncreaseEnemiesParametersBySpawnIteration)
                {
                    string newKey = string.Copy(increasingValue.Key); // Создаем новый экземпляр строки
                    float newValue = increasingValue.Value * numberOfSpawnIteration; // Для float это простое копирование значения
                    percentageIncreasedEnemiesParametersBySpawnIteration.Add(newKey, newValue);
                }
                newEnemy.ChangeUnitParametersByPercentage(percentageIncreasedEnemiesParametersBySpawnIteration, true);
                return;
            }
            
        }
    }

    private void OnValidate() // Вызывается в редакторе при изменении значений
    {
        // Синхронизируем список со словарем, чтобы изменения в инспекторе сохранялись в словаре
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
