using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public static class AdjustBuildingParameters : object
{
    public static readonly Dictionary<string, Dictionary<string, object>> buildingParameters = new Dictionary<string, Dictionary<string, object>>()
    {
        // »нициализируем словарь при объ€влении
        { "School", new Dictionary<string, object>() {
            { "_timeForUpdateAssortiment", 15f },
            { "folderImage", "Spells/" },
            { "_nameTargetEquipmentPanelPlayer", "SpellPanel" },
        } },
        { "Treasury", new Dictionary<string, object>() {
            { "_timeForUpdateAssortiment", 15f },
            { "folderImage", "Ammunition/" },
            { "_nameTargetEquipmentPanelPlayer", "AmmunitionPanel" },
        } },
    };

}
