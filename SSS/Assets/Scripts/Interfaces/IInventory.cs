using System;
using UnityEngine;

public interface IInventory
{
    Inventory Inventory { get; set; }
    int CountAvailableAmmunitionPlaces { get; set; }
    int CountAvailableSpellPlaces { get; set; }
    bool IsStaticInventory { get; }
    Transform Transform { get; }
    Type TypeInventory { get; }
    Unit UnitSelf { get; }
}
