using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static StaticClassForAdditionalFunctions;

public class SettingsMenu : MonoBehaviour, IControlLifeCicleFunctions
{
    public ParameterInternetSettings parameterInternetSettings;
    public RectTransform toggleClusterGroup;
    public RectTransform rectTransformPlacementForSettings;
    public ParameterChoseList parameterOrientation;
    public List<RectTransform> togglesInGroup; // сделали public для сохранения

    private static SettingsMenu _instance;
    private RectTransform _rectTransformLastToggle;
    private GameObject _objectLastSettingsPanel;
    private GameObject _objectGameSettingsPanel;
    private GameObject _objectSonicSettingsPanel;
    private GameObject _objectVideoSettingsPanel;
    private GameObject _objectLanguageSettingsPanel;
    private GameObject _objectInternetSettingsPanel; 

    [NonSerialized]private bool a = false;
    public bool AwakeWasCalledAlready { get { return a; } set { a = value; Debug.Log("ну и дичь..."); } }
    public bool StartWasCalledAlready { get; set; }

    public static SettingsMenu Instance
    {
        get
        {
            return _instance; // по идее всегда менюшка настроек есть на сцене. Поэтому _instance определяется всегда в Awake и не должен быть равен null
        }
    }

    // метод вообще ничего не делает, но как-то инициализировать наш синглтон надо, создавать переменную и присваивать ей ненужную ссылку на наш объект желания нет. 
    // Увы, просто GameManager.Instance сделать нельзя

   
    public void Awake()
    {
        //Debug.Log($"{name} Awake: a = {a}, AwakeWasCalledAlready = {AwakeWasCalledAlready}, id={GetInstanceID()}");
        if (!AwakeWasCalledAlready)
        {
            Debug.Log(GetInstanceID());
            //Debug.Log(AwakeWasCalledAlready);
            if (_instance != null && _instance != this)
            {
                Debug.Log("Чё за нах?"); 
                Destroy(gameObject);
                return;
            }
            //Debug.Log(GetInstanceID());
            _instance = this;

            toggleClusterGroup = transform.Find("DownTogglesClaster").GetComponent<RectTransform>();
            rectTransformPlacementForSettings = transform.Find("PlacementForSettings").GetComponent<RectTransform>();

            //Debug.Log(toggleClusterGroup);
            //Debug.Log(rectTransformPlacementForSettings); 
            foreach (RectTransform rectTransformToggleGroup in toggleClusterGroup) // пока что хз зачем нам массив всех тумблеров, ведь можно контролировать визуализацию лишь предпоследнего
            {
                foreach (RectTransform rectTransformToggle in rectTransformToggleGroup)
                {
                    togglesInGroup.Add(rectTransformToggle);
                }
            }

            RectTransform _rectTransformPlacementForSettings = transform.Find("PlacementForSettings").GetComponent<RectTransform>(); // компонент RectTransform родителя для панелей настроек

            _objectGameSettingsPanel = _rectTransformPlacementForSettings.Find("GameSettings").gameObject; // игровые настройки
            _objectSonicSettingsPanel = _rectTransformPlacementForSettings.Find("SonicSettings").gameObject; // звуковые настройки
            _objectVideoSettingsPanel = _rectTransformPlacementForSettings.Find("VideoSettings").gameObject; // графические настройки
            _objectLanguageSettingsPanel = _rectTransformPlacementForSettings.Find("LanguageSettings").gameObject; // графические настройки
            _objectInternetSettingsPanel = _rectTransformPlacementForSettings.Find("InternetSettings").gameObject; // графические настройки

            EventBus.Instance.ToggleSonicOfSettingsMenuWasToggled.AddListener(ButtonSonicOfSettingsMenuToggled);
            EventBus.Instance.ToggleGameOfSettingsMenuWasToggled.AddListener(ButtonGameOfSettingsMenuToggled);
            EventBus.Instance.ToggleVideoOfSettingsMenuWasToggled.AddListener(ButtonVideoOfSettingsMenuToggled);
            EventBus.Instance.ToggleLanguageOfSettingsMenuWasToggled.AddListener(ButtonLanguageOfSettingsMenuToggled);
            EventBus.Instance.ToggleInternetOfSettingsMenuWasToggled.AddListener(ButtonInternetOfSettingsMenuToggled);

            EventBus.Instance.ValueBrightnessWasChanged.AddListener(ValueBrightnessWasChanged);
            EventBus.Instance.ValueCameraShakingWasChanged.AddListener(ValueCameraShakingWasChanged);
            EventBus.Instance.ValueLanguageWasChanged.AddListener(ValueLanguageWasChanged);
            EventBus.Instance.ValueOrientationWasChanged.AddListener(ValueOrientationWasChanged);
            EventBus.Instance.ValueVibrationWasChanged.AddListener(ValueVibrationWasChanged);
            EventBus.Instance.ValueShowNotificationsWasChanged.AddListener(ValueShowNotificationsWasChanged);
            EventBus.Instance.ValueVolumEffectsWasChanged.AddListener(ValueVolumEffectsWasChanged);
            EventBus.Instance.ValueVolumMusicWasChanged.AddListener(ValueVolumMusicWasChanged);
            EventBus.Instance.EmailForLinkWasChanged.AddListener(TryingLinkEmail);
            EventBus.Instance.PasswordWasChanged.AddListener(PasswordWasChanged);
            EventBus.Instance.DisplayNameWasChanged.AddListener(DiaplayNameWasChanged);


            AwakeWasCalledAlready = true;
        }
    }
    // чё за хрень 
    public void Start()
    {
        //Debug.Log($"{name} Awake: a = {a}, AwakeWasCalledAlready = {AwakeWasCalledAlready}, id={GetInstanceID()}");
        if (!StartWasCalledAlready)     
        {
            //if (!GameManager.Instance.currentSettings.wasUploaded)
            {
                //Debug.Log("ебала"); 
                //SaveLoadManager.Instance.LoadSettingsFromFile(); // Загрузим из файла один раз в GameManager
                SaveLoadManager.Instance.ImplementStoredSettings();
            }

            StartWasCalledAlready = true;
        }

    }

    // от кнопок для контроля вкладок меню настроек
    public void ButtonSonicOfSettingsMenuToggled(bool wasToggled, RectTransform rectTransformToggle)
    {
        _objectSonicSettingsPanel.SetActive(wasToggled);
    }
    public void ButtonGameOfSettingsMenuToggled(bool wasToggled, RectTransform rectTransformToggle)
    {
        _objectGameSettingsPanel.SetActive(wasToggled);
    }
    public void ButtonVideoOfSettingsMenuToggled(bool wasToggled, RectTransform rectTransformToggle)
    {
        _objectVideoSettingsPanel.SetActive(wasToggled);
    }
    public void ButtonLanguageOfSettingsMenuToggled(bool wasToggled, RectTransform rectTransformToggle)
    {
        _objectLanguageSettingsPanel.SetActive(wasToggled);
    }
    public void ButtonInternetOfSettingsMenuToggled(bool wasToggled, RectTransform rectTransformToggle)
    {
        _objectInternetSettingsPanel.SetActive(wasToggled);
    }

    // от кнопок для изменения параметров настроек

    private void ValueVolumMusicWasChanged(float value, RectTransform rectTransformToggle)
    {
        if (GameManager.Instance.currentSettings.VolumeMusic != value)
            GameManager.Instance.currentSettings.VolumeMusic = value;
    }
    private void ValueVolumEffectsWasChanged(float value, RectTransform rectTransformToggle)
    {
        if (GameManager.Instance.currentSettings.VolumeEffects != value)
            GameManager.Instance.currentSettings.VolumeEffects = value;
    }
    private void ValueBrightnessWasChanged(float value, RectTransform rectTransformToggle)
    {
            //Debug.Log(value);
        if (GameManager.Instance.currentSettings.VolumeBrightness != value)
        {
            //Debug.Log(value);
            GameManager.Instance.currentSettings.VolumeBrightness = value;
        }
    }
    private void ValueVibrationWasChanged(bool value, RectTransform rectTransformToggle)
    {
        if (GameManager.Instance.currentSettings.vibrationOn != value)
            GameManager.Instance.currentSettings.vibrationOn = value;
    }
    private void ValueCameraShakingWasChanged(bool value, RectTransform rectTransformToggle)
    {
        if (GameManager.Instance.currentSettings.cameraShakingOn != value)
            GameManager.Instance.currentSettings.cameraShakingOn = value;
    }
    private void ValueOrientationWasChanged(LANGUAGE value, RectTransform rectTransformToggle)
    {
        if (GameManager.Instance.currentSettings.Orientation != value)
            GameManager.Instance.currentSettings.Orientation = value;
    }
    private void ValueShowNotificationsWasChanged(bool value, RectTransform rectTransformToggle)
    {
        if (GameManager.Instance.currentSettings.ShowNotifications != value)
            GameManager.Instance.currentSettings.ShowNotifications = value;
    }
    private void ValueLanguageWasChanged(LANGUAGE value, RectTransform rectTransformToggle)
    {
        if (GameManager.Instance.currentSettings.Language != value)
            GameManager.Instance.currentSettings.Language = value;
    }
    
    private void TryingLinkEmail(string value, RectTransform rectTransformToggle)
    {
        if (GameManager.Instance.currentSettings.Email != value)
            GameManager.Instance.currentSettings.Email = value;
    }
    private void PasswordWasChanged(string value, RectTransform rectTransformToggle)
    {
        if (GameManager.Instance.currentSettings.Password != value)
            GameManager.Instance.currentSettings.Password = value;
    }
    private void DiaplayNameWasChanged(string value, RectTransform rectTransformToggle)
    {
        if (GameManager.Instance.currentSettings.DisplayName != value)
            GameManager.Instance.currentSettings.DisplayName = value;
    }

    // также вызываем на данный момент при нажатии на стрелку BackButton в SettingsMenu
    public void SaveCurrentSettings()
    {
        SaveLoadManager.Instance.SaveSettingsMenu();
    }

    public void OnEnable()
    {
    }

    private void OnDisable()
    {
        SaveCurrentSettings();
    }
    private void OnDestroy()
    {

        Debug.Log("Что за нахер ебунячий");
        SaveCurrentSettings();

        EventBus.Instance.ToggleSonicOfSettingsMenuWasToggled.RemoveListener(ButtonSonicOfSettingsMenuToggled);
        EventBus.Instance.ToggleGameOfSettingsMenuWasToggled.RemoveListener(ButtonGameOfSettingsMenuToggled);
        EventBus.Instance.ToggleVideoOfSettingsMenuWasToggled.RemoveListener(ButtonVideoOfSettingsMenuToggled);
        EventBus.Instance.ToggleLanguageOfSettingsMenuWasToggled.RemoveListener(ButtonLanguageOfSettingsMenuToggled);
        EventBus.Instance.ToggleInternetOfSettingsMenuWasToggled.RemoveListener(ButtonInternetOfSettingsMenuToggled);

        EventBus.Instance.ValueBrightnessWasChanged.RemoveListener(ValueBrightnessWasChanged);
        EventBus.Instance.ValueCameraShakingWasChanged.RemoveListener(ValueCameraShakingWasChanged);
        EventBus.Instance.ValueLanguageWasChanged.RemoveListener(ValueLanguageWasChanged);
        EventBus.Instance.ValueOrientationWasChanged.RemoveListener(ValueOrientationWasChanged);
        EventBus.Instance.ValueVibrationWasChanged.RemoveListener(ValueVibrationWasChanged);
        EventBus.Instance.ValueVolumEffectsWasChanged.RemoveListener(ValueVolumEffectsWasChanged);
        EventBus.Instance.ValueVolumMusicWasChanged.RemoveListener(ValueVolumMusicWasChanged);
        EventBus.Instance.ValueShowNotificationsWasChanged.RemoveListener(ValueShowNotificationsWasChanged);
        EventBus.Instance.EmailForLinkWasChanged.RemoveListener(TryingLinkEmail);
        EventBus.Instance.DisplayNameWasChanged.RemoveListener(DiaplayNameWasChanged);
    }

}
