using UnityEngine;
using UnityEngine.Tilemaps;

public class FsmStateEquipmentInsideShop : FsmStateEquipment
{
    public FsmStateEquipmentInsideShop(Fsm fsm, GameObject gameObject) : base(fsm, gameObject)
    {
     
    }


    public override void Enter()
    {
        Debug.Log("Equipment inside shop state [ENTER]");
        equipment.transform.localPosition = equipment.startLocalPosition;
        equipment.selfSprite.sortingOrder = 6; 
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
    }



}
