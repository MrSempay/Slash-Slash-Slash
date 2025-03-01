using System;
using UnityEngine;

[Serializable]
public class StringStringWeaponInfoDictionaryPair
{
    public string key; // Тип оружия (Standart, Rare, Legendary)
    public StringWeaponInfoDictionary value;
}