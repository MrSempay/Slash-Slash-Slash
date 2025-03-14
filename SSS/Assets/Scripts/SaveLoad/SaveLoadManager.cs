using UnityEngine;
using System.Reflection;
using System.IO;
using System;
using System.Collections.Generic;

public class SaveLoadManager
{



    [System.Serializable]
    public class AllUnitsData
    {
        //public List<Unit> unitDataArray = new List<Unit>();
        public List<UnitData> unitDataArray = new List<UnitData>();
        //public UnitData[] unitDataArray = new UnitData[3];
    }

    [System.Serializable]
    public class UnitData : MonoBehaviour
    {
        public string ibo = "asdasd";
        public float ibo2 = 3;
        public float ibo3 = 4;
    }


    public void SaveGame()
    {
        Unit[] allUnits = MonoBehaviour.FindObjectsByType<Unit>(FindObjectsSortMode.None);
        AllUnitsData allUnitsData = new AllUnitsData();
        //allUnitsData.unitDataArray.Add(new Player());
        //allUnitsData.unitDataArray.Add();
        foreach (Unit unit in allUnits)
        {
            //allUnitsData.unitDataArray.Add(new Unit());
            allUnitsData.unitDataArray.Add(new UnitData());
            // Сериализация


            Type type = unit.GetType(); // Получаем тип текущего класса
            Debug.Log("Найден юнит: " + unit.gameObject.name + " c типом: " + type);

            // Получаем все поля класса
            FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic); // Instance - для экземпляра класса, Public/NonPublic - чтобы получить все поля

            foreach (FieldInfo field in fields)
            {
                // Получаем значение поля
                object value = field.GetValue(unit);

                // Выводим информацию о поле
                Debug.Log("  Поле: " + field.Name + ", Тип: " + field.FieldType + ", Значение: " + value);
            }
        }
        string json = JsonUtility.ToJson(allUnitsData, prettyPrint: false);
        // Сохранение в файл
        string filePath = "C:\\Users\\Fossa2016\\Documents\\GitHub\\Slash-Slash-Slash\\SSS\\data.json";
        File.WriteAllText(filePath, json);
        Debug.Log("Data saved to: " + filePath);
        Debug.Log("Game was saved!");
    }


    public void LoadGame()
    {
        Debug.Log("Game was loaded!");
    }
}
