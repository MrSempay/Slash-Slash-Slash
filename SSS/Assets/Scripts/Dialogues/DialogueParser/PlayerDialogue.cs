using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerDialogue : DialogueParser
{
    public float newWidth = 2f;
    public float newHeight = 1f;
    //public delegate void DialogueStarted(PlayerDialogue sciptPlayerDialogue);
    //public event DialogueStarted onDialogueStarted;

    protected override void Awake()
    {
        //FinishDialogue(); // по идее нужно сначала завершить предыдущий диалог, если таковой имеется
        base.Awake();

        GameManager.Instance.DialogueWasStarted(this);
        //onDialogueStarted?.Invoke(this); // перенесли это всё дело сюда, бо иначе в GameManager мы не успеваем подписаться на старт диалога, если тот закончится по причине отсутствия 
        // файлика (закончится он тогда на этапе Awake и уничтожится, а в GameManager мы можем подписаться только после Awake. Создание корутины и ожидание одного фрейма задержки для 
        // FinishDialogue является плохим вариантом, ибо тогда всё таки заметно мелькание не валидного диалога на сцене, он успевает отрисоваться. Будем подписываться на этот сигнал в 
        // GameManager

        LoadAndParseDialogueAndShowPhrase();
        /*
        ScenarioScript scenarioScript = GameObject.Find("ScenarioScript").GetComponent<ScenarioScript>(); // на уровнях будут дочерние скрипты от данного класса, для конечных уровней
        if (scenarioScript)
        {
            scenarioScript.ScriptCurrentDialogue = this;
        }*/

        //onDialogueStarted?.Invoke(this); // подписываемся в LevelNScenario
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
    }

    protected override void FinishDialogue()
    {
        base.FinishDialogue();
        Destroy(gameObject); 
    }
}
