using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public static class AdjustBuildingParameters : object
{
    public static readonly Dictionary<string, Dictionary<string, object>> buildingParameters = new Dictionary<string, Dictionary<string, object>>()
    {
        // »нициализируем словарь при объ€влении
        { C.DK.School, new Dictionary<string, object>() {
            { C.DK.TimeForUpdateAssortiment, 15f },
            { C.DK.FolderImagesOfEquipment, C.DK.FolderImagesForSpells },
            { C.DK.NameTargetEquipmentPanelPlayer, C.DK.SpellPanel },
        } },
        { C.DK.Treasury, new Dictionary<string, object>() {
            { C.DK.TimeForUpdateAssortiment, 15f },
            { C.DK.FolderImagesOfEquipment, C.DK.FolderImagesForAmmunition },
            { C.DK.NameTargetEquipmentPanelPlayer, C.DK.AmmunitionPanel },
        } },
    };

}
