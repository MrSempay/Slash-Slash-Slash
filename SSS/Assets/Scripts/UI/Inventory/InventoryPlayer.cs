using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using static AdjustEquipmentParameters;
using static UnityEngine.Rendering.DebugUI;

public class InventoryPlayer : Inventory
{






    public override void Start()
    {
        base.Start();
    }




    public override bool SetEquipmentToInventory(Equipment equipment)
    {
        if (base.SetEquipmentToInventory(equipment))
        {
            equipment.OnEquipmentShouldBeActivate += Player.instance.SomeEquipmentShouldBeActivate; // используем Player.instance ибо данный инвентарий у нас может быть только для игрока
        }


        return true;
    }


    public override bool RemoveEquipmentFromInventory(Equipment equipment)
    {
        if (base.SetEquipmentToInventory(equipment))
        {
            equipment.OnEquipmentShouldBeActivate -= Player.instance.SomeEquipmentShouldBeActivate;
        }

        return true;

    }


    // Update is called once per frame   
    void Update()
    {
        
    }
}
