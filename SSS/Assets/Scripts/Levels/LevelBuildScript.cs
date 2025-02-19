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
    private List<Transform> spawnPointsTransforms; // ������ ��� ����������� Transform ���� ����� ������
    private Transform clusterSpawnPointsTransform; // ������� (������������ �������) ���� ����� ������
    private bool isStillEnemyForSpawn;
    private List<Transform> targetTransformsForRandom;
    private Dictionary<Transform, int> targetPointsForEnemy; // ������ �������� ������ �� ��������� transform ����, ��������� - ���������� ������, ������� ���������� � ����.

    void Awake()
    {
        // �������� ���������� transform � �����
        playerTransform = GameObject.Find("Player").transform;
        treasuryTransform = GameObject.Find("Treasury").transform;
        schoolTransform = GameObject.Find("School").transform;

        targetPointsForEnemy = new Dictionary<Transform, int>()
        {
            {playerTransform, 15},
            {schoolTransform, 20},
            {treasuryTransform, 8}
        };

        // �������� ���������� transform � ����� ��� ������
        spawnPointsTransforms = new List<Transform>();
        clusterSpawnPointsTransform = GameObject.Find("SpawnPoints").transform;

        // ���������, ��� ���� ������� ��� ����� ������ ������ �� ������
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
        if (enemy != null && spawnPointTransform != null) // ���������, ��� ������ �����������
        {
            // �������� ������ ������ �� �������. � ������ ��������� ������ �� �������, �� ������� ��� ������ ���� �����
            targetTransformsForRandom = new List<Transform>();
            foreach (var targetPoint in targetPointsForEnemy)
            {
                if (targetPoint.Value > 0)
                {
                    if (targetPoint.Key != null) targetTransformsForRandom.Add(targetPoint.Key); // ������������ � ��� ����� �� ���� �� ������ ����� �� ����������������� � ������� 
                                                                                                 // ������� ����� ��� �����. � ����� ������ ��������������� ���� Transfrom ����� null
                }
            }
            
            // ���� ������ �� ������ (�� ���� ���� ������, �� ������� ��� ������ ���� �����), �� �������� �����
            if (targetTransformsForRandom.Count > 0) {
                // ���������� ��������� ������
                int randomIndex = UnityEngine.Random.Range(0, targetTransformsForRandom.Count);

                // �������� ��������� Transform (����) �� ������
                Transform randomTarget = targetTransformsForRandom[randomIndex];

                // ������������ �����
                Enemy newEnemy = Instantiate(enemy, spawnPointTransform.position, spawnPointTransform.rotation);

                // ��������� ���������� ������ ��� �������� ����� �������
                targetPointsForEnemy[randomTarget]--;
                // ����������� ��������� Transform �����
                newEnemy.currentTargetTransform = randomTarget;
                newEnemy.isInstancedByLevel = true;
            }

        }
    }
}
