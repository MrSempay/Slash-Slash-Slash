using System;
using UnityEngine;
using static DialogueParser;

public class ScenarioScript : MonoBehaviour
{
    // ибо

    private static ScenarioScript _instance;

    private PlayerDialogue _scriptCurrentDialogue;



    public PlayerDialogue ScriptCurrentDialogue
    {
        get { return _scriptCurrentDialogue; }
        set
        {
            _scriptCurrentDialogue = value;
            _scriptCurrentDialogue.onDialogueWasFinished += DialogueFinished;
        }
    }

    protected virtual void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
    }

    protected virtual void DialogueFinished(string nameDialogueWithFolder) { }

    protected void TeleportObjectFrom_A_PointTo_B_Point(GameObject someObject, Vector3 positionPointA, Vector3 positionPointB)
    {

    }

    protected void StartDialogue(string nameDialogue) // взять образец из зоны диалога
    {

    }

    protected void SpawnObjectAtTargetPosition(GameObject someObject, Vector3 targetPosition) // может стоить для каких-нибудь объектов добавить функцию, чтоб вызывать при таком спавне
    {

    }
    protected void MovingObjectFrom_A_PointTo_B_Point(GameObject someObject, Vector3 positionPointA, Vector3 positionPointB, float speed) // может стоить для каких-нибудь объектов добавить функцию, чтоб вызывать при таком спавне
    {

    }

}
