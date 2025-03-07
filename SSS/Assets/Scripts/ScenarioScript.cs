using NUnit.Framework;
using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using static DialogueParser;

public class ScenarioScript : MonoBehaviour
{
    // ибо

    private static ScenarioScript _instance;

    private PlayerDialogue _scriptCurrentDialogue;
    private Coroutine _moveCameraCoroutine;
    private Coroutine _moveObjectCoroutine;
    private Vector3 _velocity = Vector3.zero; // Текущая скорость

    protected Transform transformPlayer;
    protected Player scriptPlayer;

    public GameObject player;
    public Camera cameraPlayer;
    public Transform transformDialogueAreas;


    public PlayerDialogue ScriptCurrentDialogue
    {
        get { return _scriptCurrentDialogue; }
        set
        {
            if (_scriptCurrentDialogue != null) // Проверяем, что _scriptCurrentDialogue не null
            {
                _scriptCurrentDialogue.onDialogueWasFinished -= DialogueFinished; // Отписываемся от предыдущего объекта
            }

            _scriptCurrentDialogue = value; // Присваиваем новое значение

            if (_scriptCurrentDialogue != null) // Проверяем, что новое значение не null
            {
                _scriptCurrentDialogue.onDialogueWasFinished += DialogueFinished; // Подписываемся на новый объект
            }
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

        transformPlayer = player.GetComponent<Transform>();
        scriptPlayer = player.GetComponent<Player>();

        cameraPlayer = GameObject.Find("CameraPlayer").GetComponent<Camera>();

        if (transformDialogueAreas)
        {
            foreach (Transform transformDialogueArea in transformDialogueAreas)
            {
                transformDialogueArea.gameObject.GetComponent<DialogueArea>().onDialogueStarted += DialogueWasStarted;
            }
        }
    }

    /* ############################# БЛОК ФУНКЦИЙ-СИГНАЛОВ, ИНФОРМИРУЮЩИХ О ТОМ, ЧТО СЮЖЕТ ДВИЖЕТСЯ ТАК ИЛИ ИНАЧЕ ############################# */

    protected virtual void DialogueFinished(string nameDialogueWithFolder) { } // сигнал, к которому привязана функция, эмулируется при любом окончании диалога, хоть игрока, хоть сцены
    protected virtual void UnitWasKilled(Unit unit) { }
    protected virtual void DialogueWasStarted(PlayerDialogue playerDialogue)
    {
        ScriptCurrentDialogue = playerDialogue;
    }
    protected virtual void MovingCameraPlayerWasFinished()
    {
        DeblockAnyUpdateFunctions();
    }
    protected virtual void MovingObjectWasFinished(GameObject obj) { }

    private List<Equipment> _equipmentInBuildingFromLastIteration = new List<Equipment>(); // просто переменная для временного хранения ссылок на снаряжение из предыдущей итерации его обновления в здании
    protected virtual void AssortmentInBuildingWasUpdated(List<Equipment> equipmentInBuilding)
    {
        // короче, нижестоящий if придуман лишь для того, чтоб отписываться от сигналов предыдущей партии снаряжения в зданиях при обновлении ассортимента. При каждом обновлении
        // ассортимента мы до того, как удалили предыдущую партию, эмулируем данный сигнал, передавая в него null, что является флагом того, что нам нужно отписаться от событий
        // снаряжения из предыдущей партии (снаряжение из предыдущей партии хранится в переменной _equipmentInBuildingFromLastIteration, которая при каждой эмуляции сигнала 
        // с параметром equipmentInBuilding не равным нулю перезаписывается на, собственно, значение параметра equipmentInBuilding)
        if (equipmentInBuilding == null)
        {
            foreach (Equipment equipment in _equipmentInBuildingFromLastIteration)
            {
                if (equipment) equipment.onEquipmentWasSold -= EquipmentWasSold;
            }
            _equipmentInBuildingFromLastIteration = new List<Equipment>();
            return;
        }

        foreach (Equipment equipment in equipmentInBuilding)
        {
            equipment.onEquipmentWasSold += EquipmentWasSold;
        }
        _equipmentInBuildingFromLastIteration = equipmentInBuilding;
    }
    protected virtual void EquipmentWasSold(Equipment equipment) { }




    /* ############################# БЛОК ФУНКЦИЙ-РЕАКЦИЙ, ДВИГАЮЩИХ СЮЖЕТ ТАК ИЛИ ИНАЧЕ ############################# */

    protected virtual void TeleportObjectToPoint(GameObject someObject, Vector3 targetPoint)
    {
        Transform transformObject = someObject.transform;
        transformObject.position = targetPoint;
    }

    protected virtual void StartDialogue(string nameDialogue) // взять образец из зоны диалога
    {

    }

    protected virtual GameObject SpawnObjectAtTargetPosition(GameObject someObject, Vector3 targetPosition) // может стоить для каких-нибудь объектов добавить функцию, чтоб вызывать при таком спавне
    {
        return Instantiate(someObject, targetPosition, Quaternion.identity);
    }
    protected virtual void SetStateIdleToPlayerAndBlockAnyUpdateFunctions() 
    {
        scriptPlayer._fsm.SetState<FsmStateIdle>();
        scriptPlayer.areUpdatingFunctionsEnabled = false;
    }
    protected virtual void DeblockAnyUpdateFunctions() 
    {
        scriptPlayer.areUpdatingFunctionsEnabled = true;
    }


    protected virtual void MovingObjectToPoint(GameObject someObject, Vector3 targetPoint, float speed) 
    {
        Transform transformObject = someObject.transform;
        _moveObjectCoroutine = CoroutineManager.Instance.StartManagedCoroutine(this.gameObject, MoveObjectWithSpeedToPoint(transformObject, targetPoint, speed));
    }
    protected virtual void MovingCameraPlayerToPoint(Camera cameraPlayer, Transform targetTransform, float speed) 
    {

        SetStateIdleToPlayerAndBlockAnyUpdateFunctions();
        Transform transformCameraPlayer = cameraPlayer.transform;

        transformCameraPlayer.SetParent(null);
        _moveCameraCoroutine = CoroutineManager.Instance.StartManagedCoroutine(this.gameObject, MoveCameraPlayerWithSpeedToPoint(transformCameraPlayer, targetTransform, speed));

    }


    /* ############################# БЛОК СЛУЖЕБНЫХ (ВНУТРЕННИХ) ФУНКЦИЙ, ЯВЛЯЮТСЯ ТЕХНИЧЕСКИМИ ДЛЯ ОСНОВНЫХ ФУНКЦИЙ-РЕАКЦИЙ/СИГНАЛОВ ############################# */

    IEnumerator MoveObjectWithSpeedToPoint(Transform transformMovingObject, Vector3 targetPoint, float speed)
    {
        float distanceTreshold = 0.1f;
        while (Vector3.Distance(transformMovingObject.position, targetPoint) > distanceTreshold)
        {
            transformMovingObject.position = Vector3.SmoothDamp(transformMovingObject.position, targetPoint, ref _velocity, speed);
            yield return null; // Ждем следующий кадр
        }

        // Останавливаем корутину и устанавливаем точную позицию
        transformMovingObject.position = targetPoint;
        MovingObjectWasFinished(transform.gameObject);
    }

    // движение без замедления/ускорения в конце
    IEnumerator MoveCameraPlayerWithSpeedToPoint(Transform transformCameraPlayer, Transform targetTransform, float speed)
    {
        yield return null; // очень важно! Ждём следующего кадра, чтоб, если нам нужно следовать за телепортировавшимся героем, у того успела обновиться position

        float distanceTreshold = 0.01f;
        Vector3 specifyTargetPointForCamera = targetTransform.position + scriptPlayer.localPositionCamera; // всегда камера будет иметь смещение относительно целовой точки такое
                                                                                                           // же, как и относительно игрока

        while (Vector3.Distance(transformCameraPlayer.position, specifyTargetPointForCamera) > distanceTreshold)
        {
            specifyTargetPointForCamera = targetTransform.position + scriptPlayer.localPositionCamera;
            transformCameraPlayer.position = Vector3.MoveTowards(transformCameraPlayer.position, specifyTargetPointForCamera, speed * Time.deltaTime);
            yield return null;
        }

        transformCameraPlayer.position = targetTransform.position + scriptPlayer.localPositionCamera; // Устанавливаем точную позицию
        transformCameraPlayer.SetParent(transformPlayer);
        transformCameraPlayer.localPosition = scriptPlayer.localPositionCamera;
        MovingCameraPlayerWasFinished();
    }


    protected virtual void OnDestroy()
    {
        if (_moveCameraCoroutine != null)
        {
            CoroutineManager.Instance.StopManagedCoroutine(this.gameObject, _moveCameraCoroutine);
        }
        if (_moveObjectCoroutine != null)
        {
            CoroutineManager.Instance.StopManagedCoroutine(this.gameObject, _moveObjectCoroutine);
        }
        _moveCameraCoroutine = null;
        _moveObjectCoroutine = null;
    }


}

