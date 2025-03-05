using UnityEngine;

public class Level1Scenario : ScenarioScript
{
    private Transform _transformPlayer;
    private Transform _transformSchool;
    private Transform _transformTreasury;

    public GameObject player;
    public GameObject school;
    public GameObject treasury;


    protected override void Awake()
    {
        base.Awake();
        _transformPlayer = player.GetComponent<Transform>();
        _transformSchool = school.GetComponent<Transform>();
        _transformTreasury = treasury.GetComponent<Transform>();
    }

    void Start()
    {
        
    }


    void Update()
    {
        
    }

    protected override void DialogueFinished(string nameDialogueWithFolder)
    {
        switch (nameDialogueWithFolder)
        {
            case "Level1/Dialogue1": 
                Start();
                break;
        }
        
    }
}
