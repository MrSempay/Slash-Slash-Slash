using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FsmStateBuildingNormal : FsmStateBuilding
{

    private Coroutine _coroutineUpdateAssortimentInBuilding;

    public FsmStateBuildingNormal(Fsm fsm, GameObject gameObject) : base(fsm, gameObject)
    {
        _coroutineUpdateAssortimentInBuilding = CoroutineManager.Instance.StartManagedCoroutine(this.gameObject, TimerUpdateAssortmentInBuilding(building.rectTransformEquipmentPlaces));
    }

    public override void Enter(Dictionary<string, object> initialConditionsEntering)
    {
        Debug.Log("BuildingNormal state [ENTER]");
        building.selfCollider.enabled = true;
    }

    public override void Exit()
    {
        Debug.Log("BuildingNormal state [EXIT]");

        if (_coroutineUpdateAssortimentInBuilding != null)
        {
            CoroutineManager.Instance.StopManagedCoroutine(gameObject, _coroutineUpdateAssortimentInBuilding);
            _coroutineUpdateAssortimentInBuilding = null;
        }
    }

    public override void Update()
    {
        base.Update();

        // building.buttonEnter.SetActive(building.IsAroundBuilding); // убрали в рамках расстановки снар€жени€ на полу...
        if (building.buttonEnterWasPressedToEnter) fsm.SetState<FsmStateBuildingOpened>();

    }

    IEnumerator TimerUpdateAssortmentInBuilding(RectTransform rectTransformEquipmentPlaces)
    {
        while (true)
        {
            building.UpdateAssortmentInBuilding(rectTransformEquipmentPlaces);
            yield return new WaitForSeconds(building.timeForUpdateAssortiment); // ∆дем 15 секунд
        }
    }



    public override void OnDestroy()
    {
        base.OnDestroy();

        if (_coroutineUpdateAssortimentInBuilding != null)
        {
            CoroutineManager.Instance.StopManagedCoroutine(gameObject, _coroutineUpdateAssortimentInBuilding);
            _coroutineUpdateAssortimentInBuilding = null;
        }

    }
}
