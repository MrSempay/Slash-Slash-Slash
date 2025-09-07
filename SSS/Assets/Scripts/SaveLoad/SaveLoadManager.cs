using UnityEngine;
using System.Reflection;
using System.IO;
using System;
using System.Collections.Generic;
using static DataWrapperSettings;
using static StaticClassForAdditionalFunctions;
using UnityEngine.UI;
using PlayFab;

public class SaveLoadManager : MonoBehaviour
{
    public Action OnLoadAndImplementGeneralSyncDataFinished;

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
        PlayFabManager.Instance.OnGetIDTitleAccountLogin += LoadAndImplementGeneralSyncData;
    }


    #region Public API


    #region Save General Data Public

    private void Update()
    {
        //Debug.Log(PlayFabManager.Instance.IDTitleAccountLast); 
    }

    public void SaveGeneralData()
    {
        try
        {
            Debug.Log("Сохраняем общие настройки (как локальные, так и синхронизации)");
            //Debug.Log(GameManager.Instance);

            GameManager.Instance.wrapperGlobal.wrapperGeneralData.MaxReachedLevel = GameManager.Instance.MaxReachedLevel;
            GameManager.Instance.wrapperGlobal.wrapperGeneralData.IDTitleLastSignedAccount = PlayFabManager.Instance.IDTitleAccountLast;
            Debug.Log(PlayFabManager.Instance.IDTitleAccountLast);
            string jsonGeneralData = JsonUtility.ToJson(GameManager.Instance.wrapperGlobal.wrapperGeneralData, prettyPrint: true);

            // Сохранение в файл для локального пользования. Происходит всегда.
            string filePathLocalData = "C:\\Users\\Fossa2016\\Documents\\GitHub\\Slash-Slash-Slash\\SSS\\GeneralLocalData.json";
            File.WriteAllText(filePathLocalData, jsonGeneralData);

            string filePathSyncData = "C:\\Users\\Fossa2016\\Documents\\GitHub\\Slash-Slash-Slash\\SSS\\SyncGeneralData\\" + PlayFabManager.Instance.IDTitleAccountLast + ".json";

            if (PlayFabClientAPI.IsClientLoggedIn()) // если мы уже залогинены и сохраняем данные, то сохраняем как в локальный файл, так и в файл синхронизации
            {
                File.WriteAllText(filePathSyncData, jsonGeneralData);
            }
            else // если не залогинены и сохраняем данные
            {
                if (PlayFabManager.Instance.IDTitleAccountLast == "") // по идее, если мы ещё ни разу не логинились и у нас нет файла для синхронизации локальных данных с аккаунтом, то
                                                                  // сохраняем только в локальный файл
                {
                    // Сохранение в файл для локального пользования произошло при входе в функцию, вне условных операторов
                    
                }
                else // если мы ранее были залогинены, но по какой-то причине в текущей сессии мы не авторизованы, сохраняем в локальный файл и файл для синхронизации, который связан
                     // с прошлой активной сессией 
                {
                    File.WriteAllText(filePathSyncData, jsonGeneralData);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Ошибка при сохранении глобальных данных: {ex.Message}");
        }
    }

    public void LoadGeneralLocalDataFromFile()
    {
        if (File.Exists("C:\\Users\\Fossa2016\\Documents\\GitHub\\Slash-Slash-Slash\\SSS\\GeneralLocalData.json"))
        {

            Debug.Log("Загружаем и применяем общие локальные настройки");
            string json = File.ReadAllText("C:\\Users\\Fossa2016\\Documents\\GitHub\\Slash-Slash-Slash\\SSS\\GeneralLocalData.json");

            if (!string.IsNullOrEmpty(json)) // Проверяем, что файл не пуст
            {
                try
                {
                    GameManager.Instance.wrapperGlobal.wrapperGeneralData = JsonUtility.FromJson<WrapperGeneralData>(json);
                }
                catch (Exception e)
                {
                    Debug.LogWarning("Error parsing JSON for GeneralLocalData: " + e.Message);
                }
            }
            else
            {
                Debug.LogWarning("GeneralLocalData.json is empty. Creating new DataWrapperSettings.");
            }
        }
    }    

    public void ImplementStoredGeneralLocalData()
    {
        GameManager.Instance.MaxReachedLevel = GameManager.Instance.wrapperGlobal.wrapperGeneralData.MaxReachedLevel;
        PlayFabManager.Instance.IDTitleAccountLast = GameManager.Instance.wrapperGlobal.wrapperGeneralData.IDTitleLastSignedAccount;
    }


    #endregion Save General Data Public

    #region Save Settings Public

    public void SaveSettingsMenu() // пока что publuc
    {
        try
        {
            // объект с массивами оболочек различных параметров настроек (тумблера/кнопки/ползунки) создаётся в GameManager - .Instance.wrapperSettings

            FindAndStoreAllInfoInSettingsMenu(); // эта штука как раз записывает данные о тумблерах в GameManager.Instance.wrapperGlobal.wrapperSettings.allTogglesData

            string json = JsonUtility.ToJson(GameManager.Instance.wrapperGlobal.wrapperSettings, prettyPrint: true);

            // Сохранение в файл
            string filePath = "C:\\Users\\Fossa2016\\Documents\\GitHub\\Slash-Slash-Slash\\SSS\\DataSettings.json";
            File.WriteAllText(filePath, json);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Ошибка при сохранении настроек: {ex.Message}");
        }
    }

    // функция записывает значения всех тумблеров меню настроек в список нашей оболочки для сохранения настроек тумблеров GameManager.Instance.wrapperGlobal.wrapperSettings.allTogglesData 

    public void LoadSettingsFromFile()
    {
        if (File.Exists("C:\\Users\\Fossa2016\\Documents\\GitHub\\Slash-Slash-Slash\\SSS\\DataSettings.json"))
        {
            string json = File.ReadAllText("C:\\Users\\Fossa2016\\Documents\\GitHub\\Slash-Slash-Slash\\SSS\\DataSettings.json");

            if (!string.IsNullOrEmpty(json)) // Проверяем, что файл не пуст
            {
                try
                {
                    GameManager.Instance.wrapperGlobal.wrapperSettings = JsonUtility.FromJson<DataWrapperSettings>(json);
                }
                catch (Exception e)
                {
                    Debug.LogWarning("Error parsing JSON DataSettings: " + e.Message);
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
        //Debug.Log("Implemented");
        GameManager.Instance.currentSettings.isLoadingSettings = true;
        ImplementStoredSettingsToggle();
        ImplementStoredSettingsSliders();
        ImplementStoredSettingsChoseLists();
        ImplementStoredInternetSettings();
        GameManager.Instance.currentSettings.isLoadingSettings = false; 
    }

    #endregion Save Settings Public


    #endregion Public API




    # region Private Function


    #region Save Settings Private


    private void FindAndStoreAllInfoInSettingsMenu()
    {
        GameManager.Instance.wrapperGlobal.wrapperSettings.allTogglesData = new();

        GameManager.Instance.wrapperGlobal.wrapperSettings.allTogglesData.AddRange(FindAndStoreAllTogglesInGivenRectTransformRecursivly(SettingsMenu.Instance.rectTransformPlacementForSettings)); // родительский RectTransform окон с настройками
        GameManager.Instance.wrapperGlobal.wrapperSettings.allTogglesData.AddRange(FindAndStoreAllTogglesInGivenRectTransformRecursivly(SettingsMenu.Instance.toggleClusterGroup)); // родительский RectTransform нижней группы тумблеров

        GameManager.Instance.wrapperGlobal.wrapperSettings.allSlidersData = new();

        GameManager.Instance.wrapperGlobal.wrapperSettings.allSlidersData.AddRange(FindAndStoreAllSlidersInGivenRectTransformRecursivly(SettingsMenu.Instance.rectTransformPlacementForSettings));
        
        GameManager.Instance.wrapperGlobal.wrapperSettings.allChoseListsData = new();

        GameManager.Instance.wrapperGlobal.wrapperSettings.allChoseListsData.AddRange(FindAndStoreAllChoseListsInGivenRectTransformRecursivly(SettingsMenu.Instance.rectTransformPlacementForSettings));

        GameManager.Instance.wrapperGlobal.wrapperSettings.internetData = StoreInternetSettings();
        Debug.Log(StoreInternetSettings());


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

    private void ImplementStoredSettingsToggle()
    {
        if (!GameManager.Instance.wrapperGlobal.wrapperSettings.IsPristine())
        {
            //Debug.Log("Блядство");
            List<RectTransform> rectTransformsRootes = new List<RectTransform>();
            rectTransformsRootes.Add(SettingsMenu.Instance.rectTransformPlacementForSettings);
            rectTransformsRootes.Add(SettingsMenu.Instance.toggleClusterGroup);
            //Debug.Log(rectTransformsRootes.Count);
            foreach (DataWrapperToggle wrapToggle in GameManager.Instance.wrapperGlobal.wrapperSettings.allTogglesData)
            {
                //Debug.Log(wrapToggle.selfName);
                //Debug.Log(wrapToggle.IsToggled);
                foreach (RectTransform rootRectTransform in rectTransformsRootes) // можно было бы в глобальном трансформ нашей SettingsMenu искать, но так искало бы дольше. В целях
                                                                                  // оптимизации вручную задаём в rectTransformsRootes локальные трансформы, в детях которых будут тумблеры
                {
                    // так как функция вызывается рекурсивно, чтоб постоянно не вызывать в ней GetPropertiesAndFields, мы вызовем это до первого входа в функцию и передадим
                    // словарь fieldsAndPropertiesOfWrapper как параметр в эту функцию (сделано в целях оптимизации)
                    Dictionary<string, object> fieldsAndPropertiesOfWrapper = GetPropertiesAndFields(wrapToggle);
                    //Debug.Log("ебанушка");
                    //Debug.Log(rootRectTransform); 
                    //Debug.Log(wrapToggle); 
                    //Debug.Log(fieldsAndPropertiesOfWrapper); 
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
                //Debug.Log(wrapToggle.selfName);
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

    private void ImplementStoredSettingsSliders()
    {
        {
            if (!GameManager.Instance.wrapperGlobal.wrapperSettings.IsPristine())
            {
                foreach (DataWrapperSlider wrapSlider in GameManager.Instance.wrapperGlobal.wrapperSettings.allSlidersData)
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
                //Debug.Log(scriptChoseList.CurrentTextValue);
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

    private void ImplementStoredSettingsChoseLists()
    {
        {
            if (!GameManager.Instance.wrapperGlobal.wrapperSettings.IsPristine())
            {
                foreach (DataWrapperChoseList wrapChoseList in GameManager.Instance.wrapperGlobal.wrapperSettings.allChoseListsData)
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
        
    
    private DataWrapperInternetSettings StoreInternetSettings()
    {
        DataWrapperInternetSettings wrapper = new DataWrapperInternetSettings();
        Dictionary<string, object> fieldsAndPropertiesOfWrapper = GetPropertiesAndFields(wrapper);
        Dictionary<string, object> fieldsAndPropertiesOfScriptSlider = GetPropertiesAndFieldsSelectively(fieldsAndPropertiesOfWrapper, SettingsMenu.Instance.parameterInternetSettings);
        AssignParametersAndProperties(fieldsAndPropertiesOfScriptSlider, wrapper);
        return wrapper;
    }

    private void ImplementStoredInternetSettings()
    {
        {
            if (!GameManager.Instance.wrapperGlobal.wrapperSettings.IsPristine())
            {
                SettingsMenu.Instance.parameterInternetSettings.Awake();
                DataWrapperInternetSettings wrapper = new DataWrapperInternetSettings();
                Dictionary<string, object> fieldsAndPropertiesOfWrapper = GetPropertiesAndFields(wrapper);
                Dictionary<string, object> fieldsAndPropertiesOfInternetSettings = GetPropertiesAndFieldsSelectively(fieldsAndPropertiesOfWrapper, GameManager.Instance.wrapperGlobal.wrapperSettings.internetData);
                //Debug.Log(fieldsAndPropertiesOfInternetSettings);
                //Debug.Log(GameManager.Instance.currentSettings);

                AssignParametersAndProperties(fieldsAndPropertiesOfInternetSettings, GameManager.Instance.currentSettings);
            }
        }
    }

    #endregion Save Settings Private

    #region Save General Data Private

    private void LoadAndImplementGeneralSyncData(string IDTitleAccount) // по идее у нас всегда последовательно должна идти подгрузка, а после применение синхронизирующих данных, по отдельности
                                                                        // это не имеет смысла
    {
        Debug.Log("Чё за херота2?");
        if (IDTitleAccount != "")
        {
            Debug.Log("Загружаем и применяем настройки синхронизации");
            string filePathSyncData = "C:\\Users\\Fossa2016\\Documents\\GitHub\\Slash-Slash-Slash\\SSS\\SyncGeneralData\\" + IDTitleAccount + ".json";

            if (File.Exists(filePathSyncData))
            {
                string json = File.ReadAllText(filePathSyncData);

                if (!string.IsNullOrEmpty(json)) // Проверяем, что файл не пуст
                {
                    try
                    {
                        Debug.Log("mda?");
                        GameManager.Instance.wrapperGlobal.wrapperGeneralData = JsonUtility.FromJson<WrapperGeneralData>(json);

                        GameManager.Instance.MaxReachedLevel = GameManager.Instance.wrapperGlobal.wrapperGeneralData.MaxReachedLevel;
                        PlayFabManager.Instance.IDTitleAccountLast = GameManager.Instance.wrapperGlobal.wrapperGeneralData.IDTitleLastSignedAccount;

                        OnLoadAndImplementGeneralSyncDataFinished?.Invoke(); 
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning("Error parsing JSON for GeneralLocalData: " + e.Message);
                    }
                }
                else
                {
                    Debug.LogWarning("GeneralLocalData.json is empty. Creating new DataWrapperSettings.");
                }
            }
        }
    }

    #endregion Save General Data Private


    #endregion Private Function

}
