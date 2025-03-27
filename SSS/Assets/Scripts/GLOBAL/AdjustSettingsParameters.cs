using System.Collections.Generic;
using UnityEngine;
using static StaticClassForAdditionalFunctions;

public class AdjustSettingsParameters
{
    public static readonly Dictionary<string, Dictionary<string, object>> settingsParameters =
        new Dictionary<string, Dictionary<string, object>>()
        {
        {
            C.DK.ParameterOrientation, new Dictionary<string, object>()
            {
                { C.DK.listChosing, new List<object> { ENUM.Horizontal, ENUM.Vertical } },

            }
        },
};
}