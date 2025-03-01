using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "WeaponDatabase", menuName = "Data/WeaponDatabase")]
public class WeaponDatabase : ScriptableObject
{
    public List<WeaponCategory> weaponCategories = new List<WeaponCategory>();

    public void OnEnable()
    {
        foreach (var category in weaponCategories)
        {
            category.Init();
        }
    }

    public WeaponInfo GetWeaponInfo(string category, string type, string weaponName)
    {
        foreach (var weaponCategory in weaponCategories)
        {
            if (weaponCategory.categoryName == category)
            {
                if (weaponCategory.weaponTypes.ContainsKey(type))
                {
                    if (weaponCategory.weaponTypes[type].ContainsKey(weaponName))
                    {
                        return weaponCategory.weaponTypes[type][weaponName];
                    }
                    else
                    {
                        Debug.LogError($"Weapon '{weaponName}' not found in type '{type}' and category '{category}'.");
                        return null;
                    }
                }
                Debug.LogError($"Weapon type '{type}' not found in category '{category}'.");
                return null;
            }
        }
        Debug.LogError($"Weapon category '{category}' not found.");
        return null;
    }
}