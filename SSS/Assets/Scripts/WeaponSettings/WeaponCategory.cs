using System;
using UnityEngine;
using System.Collections.Generic;

[Serializable]
public class WeaponCategory
{
    public string categoryName;
    public StringStringWeaponInfoDictionaryDictionary weaponTypesData = new StringStringWeaponInfoDictionaryDictionary();
    [NonSerialized]
    public Dictionary<string, Dictionary<string, WeaponInfo>> weaponTypes;

    public void Init()
    {
        weaponTypes = weaponTypesData.ToDictionary();
    }
}