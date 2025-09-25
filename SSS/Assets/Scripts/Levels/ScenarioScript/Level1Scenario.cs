using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Level1Scenario : ScenarioScript
{
    private Transform _transformSchool;
    private Transform _transformTreasury;
    private School _scriptSchool;
    private Treasury _scriptTreasury;
    private Unit _scriptFirstEnemyForKill;
    private float _moneyFromKillFirstEnemy = 250;
    private float _experienceFromKillFirstEnemy = 1500;
    private bool _firstBueSpell = true;
    private bool _firstBueAmmunition = true;
    private Camera _cameraPlayer;

    [SerializeField] private Transform _transformPointSpawnFirstEnemy;
    [SerializeField] private Transform _transformPointTeleportSchool;
    [SerializeField] private Transform _transformPointTeleportTreasury;
    [SerializeField] private GameObject _enemyPrefub;


    public GameObject school;
    public GameObject treasury;


    protected override void Awake()
    {
        base.Awake();

        instance = this;

        _transformSchool = school.GetComponent<Transform>();
        _scriptSchool = school.GetComponent<School>();

        _transformTreasury = treasury.GetComponent<Transform>();
        _scriptTreasury = treasury.GetComponent<Treasury>();

        _scriptSchool.onUpdateAssortment += AssortmentInBuildingWasUpdated;
        _scriptTreasury.onUpdateAssortment += AssortmentInBuildingWasUpdated;


        _cameraPlayer = GameObject.Find("CameraPlayer").GetComponent<Camera>();

        dictionaryNamesEnemiesWavesAndRewards = new() // по идее это надо будет вынести в Adjust-скрипт // редакция от 10.07.2025 - надо вынести это в редактор для возможности настройки там
        {
            { "WaveAfterAmmunitionBue", 40000 },
            { "JustSecondWave", 10000 },
        };
    }

    protected override void Start()
    {
        base.Start();

        GameManager.Instance.StartDialogue("Level1/Dialogue1");
    }

    /* ############################# БЛОК ФУНКЦИЙ-СИГНАЛОВ, ИНФОРМИРУЮЩИХ О ТОМ, ЧТО СЮЖЕТ ДВИЖЕТСЯ ТАК ИЛИ ИНАЧЕ ############################# */

    protected override void DialogueFinished(string nameDialogueWithFolder)
    {
        base.DialogueFinished(nameDialogueWithFolder);
        switch (nameDialogueWithFolder)
        {
            case "Level1/Dialogue1":
                SpawnFirstEnemyAndIncreaseReward(_enemyPrefub, _transformPointSpawnFirstEnemy.position);
                break;
            case "Level1/Dialogue2":
                JustTimeWait(3f, "waitTimeAfterFirstAmmunitionBue");
                break;
        }
        
    }

    protected override void TimerFinished(string markerTimeWait)
    {
        base.TimerFinished(markerTimeWait);
        switch (markerTimeWait)
        {
            case "waitTimeAfterFirstEnemyKill":
                MovingCameraPlayerToPoint(_cameraPlayer, transformPlayer, 16f); // перемещаем камеру к игроку (предварительно камеру игрока отцепляем от игрока в скрипте функции
                                                                               // MovingCameraPlayerToPoint и ждём 1 кадр)
                TeleportObjectToPoint(player, _transformPointTeleportSchool.position);
                break;

            case "justWait":
                StartWaveEnemies(new Dictionary<Transform, int>() { { transformPlayer, 1 },
                                                                     { _transformSchool, 0 },
                                                                     { _transformTreasury, 0 } },
                                 "JustSecondWave");
                break;

            //        АНДРЕЙ!!! ТРОГАЙ ТОЛЬКО ТО, ЧТО ВНИЗУ!       //

            case "waitTimeAfterFirstAmmunitionBue":
                Debug.Log("Study was finished");
                StartWaveEnemies(new Dictionary<Transform, int>() { { transformPlayer, 15 }, 
                                                                     { _transformSchool, 15 },
                                                                     { _transformTreasury, 15 } },
                                 "WaveAfterLearning");
                break;

            case "waitBefore2Wave":
                StartWaveEnemies(new Dictionary<Transform, int>() { { transformPlayer, 5 },
                                                                     { _transformSchool, 5 },
                                                                     { _transformTreasury, 5 } },
                                 "SecondWave");
                break;

            case "waitBefore3Wave":
                StartWaveEnemies(new Dictionary<Transform, int>() { { transformPlayer, 5 },
                                                                     { _transformSchool, 5 },
                                                                     { _transformTreasury, 5 } },
                                 "ThirdWave");
                break;
            case "waitBefore4Wave":
                StartWaveEnemies(new Dictionary<Transform, int>() { { transformPlayer, 5 },
                                                                     { _transformSchool, 5 },
                                                                     { _transformTreasury, 5 } },
                                 "FourWave");
                break;
        }
    }

    protected override void EnemiesWaveWasDestroyedWithoutLosingMainTargets(string nameWave)
    {
        Debug.Log(nameWave);
        scriptPlayer.GiveRewardScore(dictionaryNamesEnemiesWavesAndRewards[nameWave]);
    }

    protected override void EnemiesWaveWasDestroyed(string nameWave)
    {
        base.EnemiesWaveWasDestroyed(nameWave);
        switch (nameWave)
        {
            case "WaveAfterLearning":
                JustTimeWait(10f, "waitBefore2Wave");
                break;
            case "SecondWave":
                JustTimeWait(10f, "waitBefore3Wave");
                break;
            case "ThirdWave":
                JustTimeWait(8f, "waitBefore4Wave");
                break;
            case "FourWave":
                FinishLevel();
                break;
                
        }
    }

    protected override void UnitWasKilled(Unit unit)
    {
        base.UnitWasKilled(unit);
        // увы, нельзя использовать switch-case с указанием в case не константы (например case _scriptFirstEnemyForKill:)
        if (unit == _scriptFirstEnemyForKill)
        {
            unit.onUnitWasKilled -= UnitWasKilled;
            JustTimeWait(2f, "waitTimeAfterFirstEnemyKill");


        }

    }

    protected override void EquipmentWasSold(Equipment equipment)
    {
        base.EquipmentWasSold(equipment);
        if (equipment.isEquipmentASpell)
        {
            if (_firstBueSpell)
            {
                MovingCameraPlayerToPoint(_cameraPlayer, transformPlayer, 16f);
                TeleportObjectToPoint(player, _transformPointTeleportTreasury.position);
                _firstBueSpell = false;
            }
        }
        else
        {
            if (_firstBueAmmunition)
            {
                StartDialogue("Level1/Dialogue2");
                _firstBueAmmunition = false;
            }
        }

    }

    void Update()
    {
    }


    /* ############################# БЛОК ФУНКЦИЙ-РЕАКЦИЙ, ДВИГАЮЩИХ СЮЖЕТ ТАК ИЛИ ИНАЧЕ ############################# */

    private GameObject SpawnFirstEnemyAndIncreaseReward(GameObject enemyPrefub, Vector3 targetPosition) // может стоить для каких-нибудь объектов добавить функцию, чтоб вызывать при таком спавне
    {
        // Может тут ещё чего добавим... но пока что спавним врага просто по умолчанию. Он заспавнится и будет ждать, пока кто-нибудь не войдёт в его триггер-зону
        GameObject enemyObj = SpawnObjectAtTargetPosition(enemyPrefub, targetPosition);
        _scriptFirstEnemyForKill = enemyObj.GetComponent<Unit>();
        _scriptFirstEnemyForKill.onUnitWasKilled += UnitWasKilled;
        _scriptFirstEnemyForKill.moneyFromKill = _moneyFromKillFirstEnemy;
        _scriptFirstEnemyForKill.experienceFromKill = _experienceFromKillFirstEnemy;
        return enemyObj;
    }


    /* ############################# БЛОК СЛУЖЕБНЫХ (ВНУТРЕННИХ) ФУНКЦИЙ, ЯВЛЯЮТСЯ ТЕХНИЧЕСКИМИ ДЛЯ ОСНОВНЫХ ФУНКЦИЙ-РЕАКЦИЙ/СИГНАЛОВ ############################# */

    protected override void OnDestroy()
    {
        base.OnDestroy();
        _scriptSchool.onUpdateAssortment -= AssortmentInBuildingWasUpdated;
        _scriptTreasury.onUpdateAssortment -= AssortmentInBuildingWasUpdated;
    }
}
