// Класс для представления типа оружия (Standart, Rare, Legendary и т.д.)
using System.Collections.Generic;

[System.Serializable]
public class WeaponType
{
    public string typeName; // Standart, Rare, Legendary
    public int tsedqweqweypeName; // Standart, Rare, Legendary
    public Dictionary<string, WeaponInfo> weapons = new Dictionary<string, WeaponInfo>(); // Sword, Axe, Bow
}