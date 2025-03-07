using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerDialogue : DialogueParser
{
    public float newWidth = 2f;
    public float newHeight = 1f;
    //public delegate void DialogueStarted(DialogueParser sciptPlayerDialogue); // шаблон функции
    //public event DialogueStarted onDialogueStarted;         // экземл€р(?) функции/сигнала(?)

    protected override void Awake()
    {
        base.Awake();
        LoadAndParseDialogueAndShowFirstPhrase();
        /*
        ScenarioScript scenarioScript = GameObject.Find("ScenarioScript").GetComponent<ScenarioScript>(); // на уровн€х будут дочерние скрипты от данного класса, дл€ конечных уровней
        if (scenarioScript)
        {
            scenarioScript.ScriptCurrentDialogue = this;
        }*/

        //onDialogueStarted?.Invoke(this); // подписываемс€ в LevelNScenario
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
    }

    protected override void FinishDialogue()
    {
        base.FinishDialogue();
        Destroy(gameObject); // собсна если мы в диалоговой сцене, то переходим на целевую
    }

}
