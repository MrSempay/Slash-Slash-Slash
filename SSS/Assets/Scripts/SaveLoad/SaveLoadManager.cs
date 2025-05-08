using UnityEngine;
using System.Reflection;
using System.IO;
using System;
using System.Collections.Generic;
using static DataWrapperSettings;
using static StaticClassForAdditionalFunctions;
using UnityEngine.UI;

public class SaveLoadManager : MonoBehaviour
{
    private static SaveLoadManager _instance;


    public static SaveLoadManager Instance
    {
        get
        {

            if (_instance == null)
            {
                var obj = new GameObject("SaveLoadManager");
                _instance = obj.AddComponent<SaveLoadManager>();
                DontDestroyOnLoad(obj);
            }
            return _instance;
        }
    }

    // метод вообще ничего не делает, но как-то инициализировать наш синглтон надо, создавать переменную и присваивать ей ненужную ссылку на наш объект желания нет. 
    // Увы, просто SaveLoadManager.Instance сделать нельзя
    public void Initialize() { }


    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }




    [System.Serializable]
    public class AllUnitsData
    {
        //public List<Unit> unitDataArray = new List<Unit>();
        public List<UnitData> unitDataArray = new List<UnitData>();
        //public UnitData[] unitDataArray = new UnitData[3];
    }

    [System.Serializable]
    public class UnitData
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


    public void SaveSettingsMenu() // пока что publuc
    {
        try
        {
            // объект с массивами оболочек различных параметров настроек (тумблера/кнопки/ползунки) создаётся в GameManager - .Instance.dataWrapperSettings

            FindAndStoreAllInfoInSettingsMenu(); // эта штука как раз записывает данные о тумблерах в GameManager.Instance.dataWrapperSettings.allTogglesData

            string json = JsonUtility.ToJson(GameManager.Instance.dataWrapperSettings, prettyPrint: true);

            // Сохранение в файл
            string filePath = "C:\\Users\\Fossa2016\\Documents\\GitHub\\Slash-Slash-Slash\\SSS\\DataSettings.json";
            File.WriteAllText(filePath, json);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Ошибка при сохранении настроек: {ex.Message}");
        }
    }

    // функция записывает значения всех тумблеров меню настроек в список нашей оболочки для сохранения настроек тумблеров GameManager.Instance.dataWrapperSettings.allTogglesData 
    private void FindAndStoreAllInfoInSettingsMenu()
    {
        GameManager.Instance.dataWrapperSettings.allTogglesData = new();

        GameManager.Instance.dataWrapperSettings.allTogglesData.AddRange(FindAndStoreAllTogglesInGivenRectTransformRecursivly(SettingsMenu.Instance.rectTransformPlacementForSettings)); // родительский RectTransform окон с настройками
        GameManager.Instance.dataWrapperSettings.allTogglesData.AddRange(FindAndStoreAllTogglesInGivenRectTransformRecursivly(SettingsMenu.Instance.toggleGroup)); // родительский RectTransform нижней группы тумблеров

        GameManager.Instance.dataWrapperSettings.allSlidersData = new();

        GameManager.Instance.dataWrapperSettings.allSlidersData.AddRange(FindAndStoreAllSlidersInGivenRectTransformRecursivly(SettingsMenu.Instance.rectTransformPlacementForSettings));
        
        GameManager.Instance.dataWrapperSettings.allChoseListsData = new();

        GameManager.Instance.dataWrapperSettings.allChoseListsData.AddRange(FindAndStoreAllChoseListsInGivenRectTransformRecursivly(SettingsMenu.Instance.rectTransformPlacementForSettings));
    }

    public void LoadSettingsFromFile()
    {
        if (File.Exists("C:\\Users\\Fossa2016\\Documents\\GitHub\\Slash-Slash-Slash\\SSS\\DataSettings.json"))
        {
            string json = File.ReadAllText("C:\\Users\\Fossa2016\\Documents\\GitHub\\Slash-Slash-Slash\\SSS\\DataSettings.json");

            if (!string.IsNullOrEmpty(json)) // Проверяем, что файл не пуст
            {
                try
                {
                    GameManager.Instance.dataWrapperSettings = JsonUtility.FromJson<DataWrapperSettings>(json);
                }
                catch (Exception e)
                {
                    Debug.LogWarning("Error parsing JSON: " + e.Message);
                }
            }
            else
            {
                Debug.LogWarning("DataSettings.json is empty. Creating new DataWrapperSettings.");
            }
        }
    }

    public void ImplementStoredSettings()
    {
        ImplementStoredSettingsToggle();
        ImplementStoredSettingsSliders();
        ImplementStoredSettingsChoseLists();
    }



    private List<DataWrapperToggle> FindAndStoreAllTogglesInGivenRectTransformRecursivly(RectTransform rootRectTransform)
    {
        List<DataWrapperToggle> toggles = new List<DataWrapperToggle>();
        foreach (RectTransform childRectTransform in rootRectTransform)
        {
            ToggleFixed scriptToggle = childRectTransform.GetComponent<ToggleFixed>();
            if (scriptToggle != null)
            {
                // создаём новый объект оболочки для сериализации нашего тумблера
                DataWrapperToggle wrapperToggle = new DataWrapperToggle();
                // просто получаем список полей и свойств нашей оболочки тумблера дабы понимать, какие поля нам нужны у самого объекта тумблера (чтоб записать только те, которые нужны)
                Dictionary<string, object> fieldsAndPropertiesOfWrapper = GetPropertiesAndFields(wrapperToggle);
                // на основе найденных на предыдущем шаге интересующих нас имён полей и свойств, находим уже реальные свойства и поля с их значениями у нашего тумблера
                Dictionary<string, object> fieldsAndPropertiesOfScriptToggle = GetPropertiesAndFieldsSelectively(fieldsAndPropertiesOfWrapper, scriptToggle);
                // присваиваем нашему объекту оболочки тумблера поля и свойства тумблера
                AssignParametersAndProperties(fieldsAndPropertiesOfScriptToggle, wrapperToggle);
                // добавляем в массив оболочек нашу настроенную оболочку для дальнейшей сериализации
                toggles.Add(wrapperToggle);
                continue; // предположим, что у тумблера не будут дочерние тумблеры...
            }
            if (childRectTransform.childCount > 0)
            {
                toggles.AddRange(FindAndStoreAllTogglesInGivenRectTransformRecursivly(childRectTransform));
            }

        }
        return toggles;
    }

    public void ImplementStoredSettingsToggle()
    {
        if (!GameManager.Instance.dataWrapperSettings.IsPristine())
        {
            List<RectTransform> rectTransformsRootes = new List<RectTransform>();
            rectTransformsRootes.Add(SettingsMenu.Instance.rectTransformPlacementForSettings);
            rectTransformsRootes.Add(SettingsMenu.Instance.toggleGroup);
            //Debug.Log(rectTransformsRootes.Count);
            foreach (DataWrapperToggle wrapToggle in GameManager.Instance.dataWrapperSettings.allTogglesData)
            {
                foreach (RectTransform rootRectTransform in rectTransformsRootes) // можно было бы в глобальном трансформ нашей SettingsMenu искать, но так искало бы дольше. В целях
                                                                                  // оптимизации вручную задаём в rectTransformsRootes локальные трансформы, в детях которых будут тумблеры
                {
                    // так как функция вызывается рекурсивно, чтоб постоянно не вызывать в ней GetPropertiesAndFields, мы вызовем это до первого входа в функцию и передадим
                    // словарь fieldsAndPropertiesOfWrapper как параметр в эту функцию (сделано в целях оптимизации)
                    Dictionary<string, object> fieldsAndPropertiesOfWrapper = GetPropertiesAndFields(wrapToggle);
                    FindAndImplementAllTogglesInGivenRectTransformRecursivly(rootRectTransform, wrapToggle, fieldsAndPropertiesOfWrapper);
                }
            }
        }
    }

    // функция, вызываемая рекурсивно. В качестве аргументов функции выступают: родительский RectTransform, в рамках которого ищем все тумблеры рекурсивно, объект оболочки DataWrapperToggle
    // для тумблера для его дальнейшей сериализации в json, словарь параметров и свойств для данного класса (DataWrapperToggle). Подразумевается, конечно, что в рамках одного класса все
    // объекты будут иметь один и тот же набор параметров. По идее это должно работать и с унаследованными классами.
    private void FindAndImplementAllTogglesInGivenRectTransformRecursivly(RectTransform rootRectTransform, DataWrapperToggle wrapToggle, Dictionary<string, object> fieldsAndPropertiesOfWrapper)
    {
        foreach (RectTransform childRectTransform in rootRectTransform)
        {
            if (childRectTransform.gameObject.name == wrapToggle.selfName)
            {
                ToggleFixed scriptToggle = childRectTransform.GetComponent<ToggleFixed>(); // если имена совпадают, то в любом случае на объекте должен быть ToggleFixed

                scriptToggle.Awake(); // дабы если на данный момент объекты будут не активны, то мы бы их инициализировали 
                AssignParametersAndProperties(fieldsAndPropertiesOfWrapper, scriptToggle);

                //scriptToggle.IsToggled = wrapToggle.isToggled;

                continue; // предположим, что у тумблера не будут дочерние тумблеры...
            }
            if (childRectTransform.childCount > 0)
            {
                // если текущий объект не оказался тумблером и у него есть дочерние элементы, то будем искать тумблеры в них
                FindAndImplementAllTogglesInGivenRectTransformRecursivly(childRectTransform, wrapToggle, fieldsAndPropertiesOfWrapper); 
            }

        }
    }
    


    private List<DataWrapperSlider> FindAndStoreAllSlidersInGivenRectTransformRecursivly(RectTransform rootRectTransform)
    {
        List<DataWrapperSlider> sliders = new List<DataWrapperSlider>();
        foreach (RectTransform childRectTransform in rootRectTransform)
        {
            ParameterSlider scriptSlider = childRectTransform.GetComponent<ParameterSlider>();
            if (scriptSlider != null)
            {
                DataWrapperSlider wrapperSlider = new DataWrapperSlider();
                Dictionary<string, object> fieldsAndPropertiesOfWrapper = GetPropertiesAndFields(wrapperSlider);
                Dictionary<string, object> fieldsAndPropertiesOfScriptSlider = GetPropertiesAndFieldsSelectively(fieldsAndPropertiesOfWrapper, scriptSlider);
                AssignParametersAndProperties(fieldsAndPropertiesOfScriptSlider, wrapperSlider);
                sliders.Add(wrapperSlider);
                continue; 
            }
            if (childRectTransform.childCount > 0)
            {
                sliders.AddRange(FindAndStoreAllSlidersInGivenRectTransformRecursivly(childRectTransform));
            }

        }
        return sliders;
    }

    public void ImplementStoredSettingsSliders()
    {
        {
            if (!GameManager.Instance.dataWrapperSettings.IsPristine())
            {
                foreach (DataWrapperSlider wrapSlider in GameManager.Instance.dataWrapperSettings.allSlidersData)
                {
                    foreach (RectTransform rootRectTransform in SettingsMenu.Instance.rectTransformPlacementForSettings)
                    {
                        Dictionary<string, object> fieldsAndPropertiesOfWrapper = GetPropertiesAndFields(wrapSlider);
                        FindAndImplementAllSlidersInGivenRectTransformRecursivly(rootRectTransform, wrapSlider, fieldsAndPropertiesOfWrapper);
                    }
                }
            }
        }
    }

    private void FindAndImplementAllSlidersInGivenRectTransformRecursivly(RectTransform rootRectTransform, DataWrapperSlider wrapSlider, Dictionary<string, object> fieldsAndPropertiesOfWrapper)
    {
        foreach (RectTransform childRectTransform in rootRectTransform)
        {
            if (childRectTransform.gameObject.name == wrapSlider.selfName)
            {
                ParameterSlider scriptSlider = childRectTransform.GetComponent<ParameterSlider>();
                scriptSlider.Awake(); // дабы если на данный момент объекты будут не активны, то мы бы их инициализировали 
                AssignParametersAndProperties(fieldsAndPropertiesOfWrapper, scriptSlider);

                continue;
            }
            if (childRectTransform.childCount > 0)
            {
                // если текущий объект не оказался тумблером и у него есть дочерние элементы, то будем искать тумблеры в них
                FindAndImplementAllSlidersInGivenRectTransformRecursivly(childRectTransform, wrapSlider, fieldsAndPropertiesOfWrapper);
            }

        }
    }


    private List<DataWrapperChoseList> FindAndStoreAllChoseListsInGivenRectTransformRecursivly(RectTransform rootRectTransform)
    {
        List<DataWrapperChoseList> choseLists = new List<DataWrapperChoseList>();
        foreach (RectTransform childRectTransform in rootRectTransform)
        {
            ParameterChoseList scriptChoseList = childRectTransform.GetComponent<ParameterChoseList>();
            if (scriptChoseList != null)
            {
                Debug.Log(scriptChoseList.CurrentTextValue);
                DataWrapperChoseList wrapper = new DataWrapperChoseList();
                Dictionary<string, object> fieldsAndPropertiesOfWrapper = GetPropertiesAndFields(wrapper);
                Dictionary<string, object> fieldsAndPropertiesOfScriptSlider = GetPropertiesAndFieldsSelectively(fieldsAndPropertiesOfWrapper, scriptChoseList);
                AssignParametersAndProperties(fieldsAndPropertiesOfScriptSlider, wrapper);
                choseLists.Add(wrapper);
                continue; 
            }
            if (childRectTransform.childCount > 0)
            {
                choseLists.AddRange(FindAndStoreAllChoseListsInGivenRectTransformRecursivly(childRectTransform));
            }

        }
        return choseLists;
    }

    public void ImplementStoredSettingsChoseLists()
    {
        {
            if (!GameManager.Instance.dataWrapperSettings.IsPristine())
            {
                foreach (DataWrapperChoseList wrapChoseList in GameManager.Instance.dataWrapperSettings.allChoseListsData)
                {
                    foreach (RectTransform rootRectTransform in SettingsMenu.Instance.rectTransformPlacementForSettings)
                    {
                        //Debug.Log(rootRectTransform);
                        //Debug.Log(wrapChoseList);

                        Dictionary<string, object> fieldsAndPropertiesOfWrapper = GetPropertiesAndFields(wrapChoseList);
                        //Debug.Log(fieldsAndPropertiesOfWrapper);
                        FindAndImplementAllSlidersInGivenRectTransformRecursivly(rootRectTransform, wrapChoseList, fieldsAndPropertiesOfWrapper);
                    }
                }
            }
        }
    }

    private void FindAndImplementAllSlidersInGivenRectTransformRecursivly(RectTransform rootRectTransform, DataWrapperChoseList wrapSlider, Dictionary<string, object> fieldsAndPropertiesOfWrapper)
    {
        foreach (RectTransform childRectTransform in rootRectTransform)
        {
            if (childRectTransform.gameObject.name == wrapSlider.selfName)
            {
                ParameterChoseList scriptChoseList = childRectTransform.GetComponent<ParameterChoseList>();
                scriptChoseList.Awake(); // дабы если на данный момент объекты будут не активны, то мы бы их инициализировали 
                AssignParametersAndProperties(fieldsAndPropertiesOfWrapper, scriptChoseList);

                continue;
            }
            if (childRectTransform.childCount > 0)
            {
                // если текущий объект не оказался тумблером и у него есть дочерние элементы, то будем искать тумблеры в них
                FindAndImplementAllSlidersInGivenRectTransformRecursivly(childRectTransform, wrapSlider, fieldsAndPropertiesOfWrapper);
            }

        }
    }
        
    
    public void LoadGame()
    {
        Debug.Log("Game was loaded!");
    }
}
