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
            { C.DK.TimeForUpdateAssortiment, 15f }, // настраивать “ќЋ№ ќ Ё“ќ!
            { C.DK.FolderImagesOfEquipment, C.Paths.FolderImagesForSpells },
            { C.DK.NameTargetEquipmentPanelPlayer, C.DK.SpellPanel },
            { C.DK.customScriptsEquipment, new Dictionary<string, Type> { { C.DK.ProtectiveField, typeof(ProtectiveField) }, { C.DK.Berserker, typeof(Berserker) }, { C.DK.Healing, typeof(Healing) }, { C.DK.ArcLightning, typeof(ArcLightning) } } },
        } },
        { C.DK.Treasury, new Dictionary<string, object>() {
            { C.DK.TimeForUpdateAssortiment, 15f },
            { C.DK.FolderImagesOfEquipment, C.Paths.FolderImagesForAmmunition }, // вроде как рудиментна€ вещь уже. —сылаемс€ просто сразу на константу
            { C.DK.NameTargetEquipmentPanelPlayer, C.DK.AmmunitionPanel },
            { C.DK.customScriptsEquipment, new Dictionary<string, Type> { { C.DK.Tragicomedy, typeof(Tragicomedy) }, { C.DK.ThirstySakura, typeof(ThirstySakura) } }  },
        } },
    };

}
