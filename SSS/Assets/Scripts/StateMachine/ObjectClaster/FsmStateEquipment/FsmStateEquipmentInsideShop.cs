using UnityEngine;
using UnityEngine.Tilemaps;
using static DialogueParser;

public class FsmStateEquipmentInsideShop : FsmStateEquipment
{
    public FsmStateEquipmentInsideShop(Fsm fsm, GameObject gameObject) : base(fsm, gameObject)
    {
     
    }


    public override void Enter()
    {
        Debug.Log("Equipment inside shop state [ENTER]");
        Debug.Log(equipment.selfSprite.sprite);
        equipment.transform.localPosition = equipment.startLocalPosition;
        equipment.selfSprite.sortingOrder = 11; // ряд UI элементов могут быть над снаряжением, пока то в магазине
        Debug.Log(equipment.selfSprite.sprite);
    }

    public override void Exit()
    {
        Debug.Log("Equipment inside shop state [EXIT]");
        equipment.selfSprite.sortingOrder = 11;
    }

    public override void Update()
    {
        base.Update();
        if (Input.GetMouseButtonDown(0)) // Когда нажата левая кнопка мыши
        {
            if (IsEquipmentPlaceOccupied(Camera.main.ScreenToWorldPoint(Input.mousePosition).x, Camera.main.ScreenToWorldPoint(Input.mousePosition).y)) fsm.SetState<FsmStateEquipmentSelected>();
        }

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                if (IsEquipmentPlaceOccupied(Camera.main.ScreenToWorldPoint(Input.mousePosition).x, Camera.main.ScreenToWorldPoint(Input.mousePosition).y)) fsm.SetState<FsmStateEquipmentSelected>();
            }

        }

    }



}
