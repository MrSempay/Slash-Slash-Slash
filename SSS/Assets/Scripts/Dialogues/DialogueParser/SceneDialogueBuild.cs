using TMPro;
using UnityEngine;

public class SceneDialogueBuild : DialogueParser
{
    [SerializeField] private RectTransform notificationPlacement;

    protected override void Awake()
    {
        base.Awake();
        GameManager.Instance.notificationPlacement = notificationPlacement;
        LoadAndParseDialogueAndShowPhrase();

    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
    }

    protected override void FinishDialogue()
    {
        //Debug.Log("12312312");
        base.FinishDialogue();
        GameManager.Instance.ChangingSceneFinish(); // собсна если мы в диалоговой сцене, то переходим на целевую
    }

}
