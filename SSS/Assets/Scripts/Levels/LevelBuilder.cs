using System.Collections.Generic;
using System;
using UnityEngine;
using static StaticClassForAdditionalFunctions;
using System.Collections;
using System.Linq;
using static UnityEngine.GraphicsBuffer;
using Unity.VisualScripting;


public class LevelBuilder : MonoBehaviour
{
    public List<IMainTarget> listMainTargets = new();

    // В коде будем использовать словарь, для удобства, поэтому создадим его на основе списка

    [SerializeField] protected Enemy enemy;
    [SerializeField] protected List<TransformIntPair> targetPointsList = new List<TransformIntPair>(); // храним массив элементов класса, описывающего элемент словаря ключ-значения
                                                                                                       // transform-int для задания целей врагам. В целом это поле для реализации
                                                                                                       // возможности задавать цели и количество врагов для них через редактор

    protected Dictionary<Transform, int> targetPointsForEnemy = new Dictionary<Transform, int>(); // ключом является ссылка на компонент transform цели, значением - количество
                                                                                                  // врагов, которые направятся к цели.
    protected Dictionary<string, float> percentageIncreaseEnemiesParametersBySpawnIteration = new Dictionary<string, float>() // увеличение параметров на %
    {
            { "healthMax", 5 },
            { "damageReduction", 0 },
            { "speed", 0 },
            { "jumpForce", 0 },
            { "damage", 5 } };

    protected Coroutine spawnEnemyByTimerCoroutine;
    protected List<Transform> spawnPointsTransforms = new(); // массив для компонентов Transform всех точек спавна
    protected Transform clusterSpawnPointsTransform; // кластер (родительский элемент) всех точек спавна
    protected bool isStillEnemyForSpawn;
    protected int numberOfSpawnIteration = 0; // чтоб знали, насколько усиливать врагов на текущей итерации
    protected List<Transform> targetTransformsForRandom;
    protected List<Enemy> listEnemiesFromLastWave = new();

    public static LevelBuilder instance;

    [NonSerialized] public string nameOfMainMusicTeam;
    [NonSerialized] public string currentWave;

    public string selfName;

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
                //Debug.Log(listEnemiesFromLastWave.Count);
                listEnemiesFromLastWave = new(); // при каждом начале новой волны сбрасываем список врагов, которые были заспавнены на предыдущей волне
                spawnEnemyByTimerCoroutine = CoroutineManager.Instance.StartManagedCoroutine(this.gameObject, SpawnEnemyByTimer());
            }
            else if (!shouldSpawnEnemies && spawnEnemyByTimerCoroutine != null) // по идее эта часть вообще рудиментная, мы при завершении корутины сбарсываем spawnEnemyByTimerCoroutine в null
            {
                CoroutineManager.Instance.StopManagedCoroutine(this.gameObject, spawnEnemyByTimerCoroutine); 
                spawnEnemyByTimerCoroutine = null;
            }
        }
    }

    protected virtual void Awake()
    {
        if (instance != null && instance != this) // инициализируем instance в дочернем классе
        {
            Destroy(gameObject);
            return;
        }

        AssignParametersAndProperties(AdjustLevelParameters.levelParameters, this, selfName);
    }


    protected virtual void Start()
    {
        
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        
    }



    protected IEnumerator SpawnEnemyByTimer()
    {
        while (targetPointsForEnemy.Any(targetPoint => targetPoint.Value > 0))
        {
            foreach (var spawnPointTransform in spawnPointsTransforms)
            {
                SpawnEnemy(spawnPointTransform);
            }
            numberOfSpawnIteration++;
            if (targetPointsForEnemy.Any(targetPoint => targetPoint.Value > 0)) // дабы после последней итерации спавна врагов не ждать 2 секунды до "завершения волны". Ибо если за эти
                                                                                // 2 секунды будут убиты все враги, то при проверке spawnEnemyByTimerCoroutine на null в методе 
                                                                                // WasEnemiesWaveDestroyed мы не выдадим true
            {
                yield return new WaitForSeconds(2f);
            }
        }
        spawnEnemyByTimerCoroutine = null;
    }

    protected void SpawnEnemy(Transform spawnPointTransform)
    {
        if (enemy != null && spawnPointTransform != null) // Проверяем, что ссылки установлены
        {
            // получаем список ключей из словаря. В список добавляем только те позиции, на которые ещё должны идти враги
            targetTransformsForRandom = new List<Transform>();
            foreach (var targetPoint in TargetPointsForEnemy)
            {
                if (targetPoint.Value > 0)
                {
                    if (targetPoint.Key != null) targetTransformsForRandom.Add(targetPoint.Key); // теоретически у нас может не быть на уровне одной из предустановленных в скрипте 
                                                                                                 // целевых точек для врага. В таком случае соответствующее поле Transfrom будет null
                }
            }

            // если массив не пустой (то есть были позици, на которые ещё должны идти враги), то движемся далее
            if (targetTransformsForRandom.Count > 0)
            {
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
                Debug.Log(listEnemiesFromLastWave.Count);
                listEnemiesFromLastWave.Add(newEnemy);
                Debug.Log(listEnemiesFromLastWave.Count);

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


    // вернём true только в случае, если умрёт последний враг из последней волны
    public bool WasEnemiesWaveDestroyed(Enemy scriptEnemy)
    {
        Debug.Log(listEnemiesFromLastWave.Count);
        if (listEnemiesFromLastWave.Contains(scriptEnemy))
        {
        Debug.Log("shit");

            listEnemiesFromLastWave.Remove(scriptEnemy);
            return listEnemiesFromLastWave.Count == 0 && spawnEnemyByTimerCoroutine == null; // если враг был последним в списке волны и корутина для их спавна уже не работала




        }
        return false;
    }

    // вернём true только в случае, если умрёт последний враг из последней волны
    public bool IsAllMainTargetsAlive()
    {
        foreach (var mainTarget in listMainTargets) // главное чтоб mainTarget не был равен null, но по идее у нас всегда должны быть состояния мёртв/разрушен для всякого такого
        {
            if (mainTarget.WasDestroyed) { return false; }
        }
        return true;
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
