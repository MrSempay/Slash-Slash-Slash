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

    [SerializeField] private Transform _transformPointSpawnFirstEnemy;
    [SerializeField] private Transform _transformPointTeleportSchool;
    [SerializeField] private Transform _transformPointTeleportTreasury;
    [SerializeField] private GameObject _enemyPrefub;

    public GameObject school;
    public GameObject treasury;


    protected override void Awake()
    {
        base.Awake();
        _transformSchool = school.GetComponent<Transform>();
        _scriptSchool = school.GetComponent<School>();

        _transformTreasury = treasury.GetComponent<Transform>();
        _scriptTreasury = treasury.GetComponent<Treasury>();

        _scriptSchool.onUpdateAssortment += AssortmentInBuildingWasUpdated;
        _scriptTreasury.onUpdateAssortment += AssortmentInBuildingWasUpdated;
    }

    void Start()
    {
        
    }


    void Update()
    {
        
    }


    /* ############################# ЅЋќ  ‘”Ќ ÷»…-—»√ЌјЋќ¬, »Ќ‘ќ–ћ»–”ёў»’ ќ “ќћ, „“ќ —ё∆≈“ ƒ¬»∆≈“—я “ј  »Ћ» »Ќј„≈ ############################# */

    protected override void DialogueFinished(string nameDialogueWithFolder)
    {
        switch (nameDialogueWithFolder)
        {
            case "Level1/Dialogue1":
                SpawnFirstEnemyAndIncreaseReward(_enemyPrefub, _transformPointSpawnFirstEnemy.position);
                break;
        }
        
    }
    protected override void UnitWasKilled(Unit unit)
    {
        // увы, нельз€ использовать switch-case с указанием в case не константы (например case _scriptFirstEnemyForKill:)
        if (unit == _scriptFirstEnemyForKill)
        {
            unit.onUnitWasKilled -= UnitWasKilled;
            MovingCameraPlayerToPoint(cameraPlayer, transformPlayer, 16f); // перемещаем камеру к игроку (предварительно камеру игрока отцепл€ем от игрока в скрипте функции
                                                                           // MovingCameraPlayerToPoint и ждЄм 1 кадр)
            TeleportObjectToPoint(player, _transformPointTeleportSchool.position);

        }

    }

    protected override void EquipmentWasSold(Equipment equipment) 
    {
        if (equipment.isEquipmentASpell)
        {
            MovingCameraPlayerToPoint(cameraPlayer, transformPlayer, 16f);
            TeleportObjectToPoint(player, _transformPointTeleportTreasury.position);
        }

        else Debug.Log("Study was finished");
        
    }



    /* ############################# ЅЋќ  ‘”Ќ ÷»…-–≈ј ÷»…, ƒ¬»√јёў»’ —ё∆≈“ “ј  »Ћ» »Ќј„≈ ############################# */

    private GameObject SpawnFirstEnemyAndIncreaseReward(GameObject enemyPrefub, Vector3 targetPosition) // может стоить дл€ каких-нибудь объектов добавить функцию, чтоб вызывать при таком спавне
    {
        // ћожет тут ещЄ чего добавим... но пока что спавним врага просто по умолчанию. ќн заспавнитс€ и будет ждать, пока кто-нибудь не войдЄт в его триггер-зону
        GameObject enemyObj = SpawnObjectAtTargetPosition(enemyPrefub, targetPosition);
        _scriptFirstEnemyForKill = enemyObj.GetComponent<Unit>();
        _scriptFirstEnemyForKill.onUnitWasKilled += UnitWasKilled;
        _scriptFirstEnemyForKill.moneyFromKill = _moneyFromKillFirstEnemy;
        _scriptFirstEnemyForKill.experienceFromKill = _experienceFromKillFirstEnemy;
        return enemyObj;
    }


    /* ############################# ЅЋќ  —Ћ”∆≈ЅЌџ’ (¬Ќ”“–≈ЌЌ»’) ‘”Ќ ÷»…, я¬Ћяё“—я “≈’Ќ»„≈— »ћ» ƒЋя ќ—Ќќ¬Ќџ’ ‘”Ќ ÷»…-–≈ј ÷»…/—»√ЌјЋќ¬ ############################# */

    protected override void OnDestroy()
    {
        base.OnDestroy();
        _scriptSchool.onUpdateAssortment -= AssortmentInBuildingWasUpdated;
        _scriptTreasury.onUpdateAssortment -= AssortmentInBuildingWasUpdated;
    }
}
