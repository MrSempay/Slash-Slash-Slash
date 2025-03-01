using System.Collections.Generic;
using System;

[Serializable]
public class StringStringWeaponInfoDictionaryDictionary
{
    public List<StringStringWeaponInfoDictionaryPair> pairs = new List<StringStringWeaponInfoDictionaryPair>();
    public Dictionary<string, Dictionary<string, WeaponInfo>> ToDictionary()
    {
        Dictionary<string, Dictionary<string, WeaponInfo>> dictionary = new Dictionary<string, Dictionary<string, WeaponInfo>>();
        foreach (var pair in pairs)
        {
            dictionary[pair.key] = pair.value.ToDictionary();
        }
        return dictionary;
    }
}