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

    // В коде будем использовать словарь, для удобства, поэтому создадим его на основе списка

    protected List<TransformIntPair> targetPointsList = new List<TransformIntPair>(); // храним массив элементов класса, описывающего элемент словаря ключ-значения
                                                                                      // transform-int для задания целей врагам. В целом это поле для реализации
                                                                                      // возможности задавать цели и количество врагов для них через редактор

    protected Dictionary<Transform, int> targetPointsForEnemy = new Dictionary<Transform, int>(); // ключом является ссылка на объект, реализующий IMainTarget, значением - количество                                                                                                  // врагов, которые направятся к цели.
    protected Coroutine spawnEnemyByTimerCoroutine;
    protected List<Transform> spawnPointsTransforms = new(); // массив для компонентов Transform всех точек спавна
    protected Transform clusterSpawnPointsTransform; // кластер (родительский элемент) всех точек спавна
    protected bool isStillEnemyForSpawn;
    protected int numberOfSpawnIteration = 0; // чтоб знали, насколько усиливать врагов на текущей итерации
    protected List<Transform> targetsForRandom;
    protected List<Enemy> listEnemiesFromLastWave = new();

    [SerializeField] protected Enemy enemy;

    public static LevelBuilder instance;

    [NonSerialized] public string currentWave;
    [NonSerialized] public float timeBetweenEnemySpawnIteration = 2;
    [NonSerialized] public Dictionary<string, float> percentageIncreaseEnemiesParametersBySpawnIteration = new(); // увеличение параметров на %
    [NonSerialized] public Dictionary<string, float> absoluteIncreaseEnemiesParametersBySpawnIteration = new(); // увеличение параметров на %
    [NonSerialized] public string selfName;

    public Transform BoxSplitTargetPointsForEnemies;
    public List<IMainTarget> listMainTargets = new();

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

        //percentageIncreaseEnemiesParametersBySpawnIteration = new Dictionary<string, float>(AdjustLevelParameters.levelParameters[selfName][C.DK.percentageIncreaseEnemiesParametersBySpawnIteration]);
        AssignParametersAndProperties(AdjustLevelParameters.levelParameters, this, selfName);

        BoxSplitTargetPointsForEnemies = InstanceEmptyObjectAndGetTransform(transform, C.NamesObjects.BoxSplitTargetPointsForEnemies, Vector3.zero);
        //foreach (var item in percentageIncreaseEnemiesParametersBySpawnIteration)
        //{
        //    Debug.Log(item.Value);

        //}

    }


    protected virtual void Start()
    {
        SettingsMenu[] allObjects = Resources.FindObjectsOfTypeAll<SettingsMenu>();
        allObjects[0].Awake();
        SettingsMenu.Instance.Start();

        AudioManager.Instance.UpdateMusicLevelSet();
        AudioManager.Instance.StartBeginningMusic();
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
                yield return new WaitForSeconds(timeBetweenEnemySpawnIteration);
            }
        }
        spawnEnemyByTimerCoroutine = null;
    }

    protected void SpawnEnemy(Transform spawnPointTransform)
    {
        if (enemy != null && spawnPointTransform != null) // Проверяем, что ссылки установлены
        {
            // получаем список ключей из словаря. В список добавляем только те позиции, на которые ещё должны идти враги
            targetsForRandom = new List<Transform>(); // этот список только с теми целевыми точками, куда нужно спавнить более 0 врагов, ибо можем задать точку и указать 0 врагов
            List<Transform> allTransformTargetPoints = new List<Transform>(); // этот список для вообще всех Transform-ов всех целевых точек. А может и не надо оно...
            foreach (var targetPoint in TargetPointsForEnemy)
            {
                allTransformTargetPoints.Add(targetPoint.Key);
                if (targetPoint.Value > 0)
                {
                    if (targetPoint.Key != null) targetsForRandom.Add(targetPoint.Key); // теоретически у нас может не быть на уровне одной из предустановленных в скрипте 
                                                                                        // целевых точек для врага. В таком случае соответствующее поле Transfrom будет null
                }
            }

            // если массив не пустой (то есть были позици, на которые ещё должны идти враги), то движемся далее
            if (targetsForRandom.Count > 0)
            {
                // генерируем случайный индекс
                int randomIndex = UnityEngine.Random.Range(0, targetsForRandom.Count);

                // получаем случайный Transform (ключ) из списка
                Transform randomTarget = targetsForRandom[randomIndex];

                // инстанцируем врага
                Enemy newEnemy = Instantiate(enemy, spawnPointTransform.position, spawnPointTransform.rotation);

                // уменьшаем количество врагов для заданной врагу позиции
                //Debug.Log(targetsForRandom.Count);
                targetPointsForEnemy[randomTarget]--;
                // присваиваем случайный Transform врагу
                newEnemy.CurrentTargetTransform = randomTarget;
                newEnemy.isInstancedByLevel = true;
                //Debug.Log(targetsForRandom.Count);
                newEnemy.transformTargets = allTransformTargetPoints;
                listEnemiesFromLastWave.Add(newEnemy);
                //Debug.Log(listEnemiesFromLastWave.Count);

                Dictionary<string, float> percentageIncreasedEnemiesParametersBySpawnIteration = new Dictionary<string, float>(); // делаем новый словарь чтоб сохрнаить первозданные значения
                                                                                                                                  // для усиления юнитов
                Dictionary<string, float> absoluteIncreasedEnemiesParametersBySpawnIteration = new Dictionary<string, float>(); 
                foreach (var increasingValue in percentageIncreaseEnemiesParametersBySpawnIteration)
                {
                    string newKey = string.Copy(increasingValue.Key); // Создаем новый экземпляр строки
                    float newValue = increasingValue.Value * numberOfSpawnIteration; // Для float это простое копирование значения
                    percentageIncreasedEnemiesParametersBySpawnIteration.Add(newKey, newValue);
                } 
                foreach (var increasingValue in absoluteIncreaseEnemiesParametersBySpawnIteration)
                {
                    string newKey = string.Copy(increasingValue.Key); // Создаем новый экземпляр строки
                    float newValue = increasingValue.Value * numberOfSpawnIteration; // Для float это простое копирование значения
                    absoluteIncreasedEnemiesParametersBySpawnIteration.Add(newKey, newValue);
                }
                newEnemy.ChangeUnitParametersByPercentage(percentageIncreasedEnemiesParametersBySpawnIteration, true);
                newEnemy.ChangeUnitParametersAndPropertiesByAbsolute(absoluteIncreasedEnemiesParametersBySpawnIteration, true);
                return;
            }

        }
    }


    // вернём true только в случае, если умрёт последний враг из последней волны
    public bool WasEnemiesWaveDestroyed(Enemy scriptEnemy)
    {
        //Debug.Log(listEnemiesFromLastWave.Count);
        if (listEnemiesFromLastWave.Contains(scriptEnemy))
        {
        //Debug.Log("shit");

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
