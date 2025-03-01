using System.Collections.Generic;
using System;

[Serializable]
public class StringWeaponInfoDictionary
{
    public List<StringWeaponInfoPair> pairs = new List<StringWeaponInfoPair>();
    public Dictionary<string, WeaponInfo> ToDictionary()
    {
        Dictionary<string, WeaponInfo> dictionary = new Dictionary<string, WeaponInfo>();
        foreach (var pair in pairs)
        {
            dictionary[pair.key] = pair.nameWeapon;
        }
        return dictionary;
    }
}