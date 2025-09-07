using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using static DialogueParser;

public class FsmStateEquipmentInsideShop : FsmStateEquipment
{


    public FsmStateEquipmentInsideShop(Fsm fsm, GameObject gameObject) : base(fsm, gameObject)
    {

    }


    public override void Enter(Dictionary<string, object> initialConditionsEntering)
    {
        Debug.Log("Equipment inside shop state [ENTER]");
        if (equipment.WasSold) // по идее это вызоветс€ только при перемещении снар€жени€ из инвентар€ геро€ в здание, иначе, если снар€жение просто спавнитс€ в здании, WasSold равно false по умолчанию
        {
            equipment.WasSold = false; // предполагаем, что любое снар€жение, которое попадает в здание, помечаетс€ как "не продано"
            equipment.ownerUnit.Inventory.RemoveEquipmentFromInventory(equipment); 
        }
        equipment.transform.localPosition = equipment.startLocalPosition;
        equipment._areaDetectEnteringExiting.enabled = true;
        equipment.selfSprite.sortingOrder = 11; // р€д UI элементов могут быть над снар€жением, пока то в магазине
    }

    public override void Exit()
    {
        Debug.Log("Equipment inside shop state [EXIT]");
        equipment.selfSprite.sortingOrder = 11;
        equipment._areaDetectEnteringExiting.enabled = false; 
    }

    public override void Update()
    {
        //Debug.Log("—Ќј–я∆≈Ќ»»»»»»»»≈≈≈≈≈≈≈≈≈≈≈≈≈≈≈≈≈ " + equipment);
        //Debug.Log(gameObject.GetInstanceID());
        base.Update();
        if (Input.GetMouseButtonDown(0)) //  огда нажата лева€ кнопка мыши
        {
            if (IsEquipmentPlaceOccupied(Camera.main.ScreenToWorldPoint(Input.mousePosition).x, Camera.main.ScreenToWorldPoint(Input.mousePosition).y)) fsm.SetState<FsmStateEquipmentSelected>();
        }
        else if (Input.touchCount > 0) 
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                if (IsEquipmentPlaceOccupied(Camera.main.ScreenToWorldPoint(Input.mousePosition).x, Camera.main.ScreenToWorldPoint(Input.mousePosition).y)) fsm.SetState<FsmStateEquipmentSelected>();
            }

        }
        SetPositionDescriptionPanel();

    }



}
