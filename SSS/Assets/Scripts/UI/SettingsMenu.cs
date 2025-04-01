using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static StaticClassForAdditionalFunctions;

public class SettingsMenu : MonoBehaviour, IControlLifeCicleFunctions
{

    private static SettingsMenu _instance;
    private RectTransform _rectTransformLastToggle;
    private GameObject _objectLastSettingsPanel;
    private GameObject _objectGameSettingsPanel;
    private GameObject _objectSonicSettingsPanel;
    private GameObject _objectVideoSettingsPanel;
    private GameObject _objectLanguageSettingsPanel;

    public RectTransform toggleGroup;
    public RectTransform rectTransformPlacementForSettings;
    public List<RectTransform> togglesInGroup; // сделали public для сохранения

    public bool awakeWasCalledAlready { get; set; }

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
        if (!awakeWasCalledAlready)
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;

            toggleGroup = transform.Find("DownTogglesGroup").GetComponent<RectTransform>();
            rectTransformPlacementForSettings = transform.Find("PlacementForSettings").GetComponent<RectTransform>();

            foreach (RectTransform rectTransformToggle in toggleGroup) // пока что хз зачем нам массив всех тумблеров, ведь можно контролировать визуализацию лишь предпоследнего
            {
                togglesInGroup.Add(rectTransformToggle);
            }

            RectTransform _rectTransformPlacementForSettings = transform.Find("PlacementForSettings").GetComponent<RectTransform>(); // компонент RectTransform родителя для панелей настроек

            _objectGameSettingsPanel = _rectTransformPlacementForSettings.Find("GameSettings").gameObject; // игровые настройки
            _objectSonicSettingsPanel = _rectTransformPlacementForSettings.Find("SonicSettings").gameObject; // звуковые настройки
            _objectVideoSettingsPanel = _rectTransformPlacementForSettings.Find("VideoSettings").gameObject; // графические настройки
            _objectLanguageSettingsPanel = _rectTransformPlacementForSettings.Find("LanguageSettings").gameObject; // графические настройки

            EventBus.Instance.ToggleSonicOfSettingsMenuWasToggled.AddListener(ButtonSonicOfSettingsMenuToggled);
            EventBus.Instance.ToggleGameOfSettingsMenuWasToggled.AddListener(ButtonGameOfSettingsMenuToggled);
            EventBus.Instance.ToggleVideoOfSettingsMenuWasToggled.AddListener(ButtonVideoOfSettingsMenuToggled);
            EventBus.Instance.ToggleLanguageOfSettingsMenuWasToggled.AddListener(ButtonLanguageOfSettingsMenuToggled);

            EventBus.Instance.ValueBrightnessWasChanged.AddListener(ValueBrightnessWasChanged);
            EventBus.Instance.ValueCameraShakingWasChanged.AddListener(ValueCameraShakingWasChanged);
            EventBus.Instance.ValueLanguageWasChanged.AddListener(ValueLanguageWasChanged);
            EventBus.Instance.ValueOrientationWasChanged.AddListener(ValueOrientationWasChanged);
            EventBus.Instance.ValueVibrationWasChanged.AddListener(ValueVibrationWasChanged);
            EventBus.Instance.ValueVolumEffectsWasChanged.AddListener(ValueVolumEffectsWasChanged);
            EventBus.Instance.ValueVolumMusicWasChanged.AddListener(ValueVolumMusicWasChanged);


            awakeWasCalledAlready = true;
        }
    }
    void Start()
    {
        SaveLoadManager.Instance.ImplementStoredSettings();

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
    private void ValueOrientationWasChanged(ENUM value, RectTransform rectTransformToggle)
    {
        if (GameManager.Instance.currentSettings.Orientation != value)
            GameManager.Instance.currentSettings.Orientation = value;
    }
    private void ValueLanguageWasChanged(ENUM value, RectTransform rectTransformToggle)
    {
        if (GameManager.Instance.currentSettings.Language != value)
            GameManager.Instance.currentSettings.Language = value;
    }

    // также вызываем на данный момент при нажатии на стрелку BackButton в SettingsMenu
    public void SaveCurrentSettings()
    {
        SaveLoadManager.Instance.SaveSettingsMenu();
    }

    private void OnEnable()
    {
        SaveLoadManager.Instance.LoadSettingsFromFile();
    }

    private void OnDisable()
    {
        SaveCurrentSettings();
    }
    private void OnDestroy()
    {
        SaveCurrentSettings();
        EventBus.Instance.ToggleSonicOfSettingsMenuWasToggled.RemoveListener(ButtonSonicOfSettingsMenuToggled);
        EventBus.Instance.ToggleGameOfSettingsMenuWasToggled.RemoveListener(ButtonGameOfSettingsMenuToggled);
        EventBus.Instance.ToggleVideoOfSettingsMenuWasToggled.RemoveListener(ButtonVideoOfSettingsMenuToggled);
        EventBus.Instance.ToggleLanguageOfSettingsMenuWasToggled.RemoveListener(ButtonLanguageOfSettingsMenuToggled);
    }

}
