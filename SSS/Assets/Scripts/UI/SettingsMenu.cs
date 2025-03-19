using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsMenu : MonoBehaviour
{

    private static SettingsMenu _instance;
    private RectTransform _rectTransformLastToggle;
    private GameObject _objectLastSettingsPanel;
    private GameObject _objectGameSettingsPanel;
    private GameObject _objectSonicSettingsPanel;
    private GameObject _objectVideoSettingsPanel;

    public RectTransform toggleGroup;
    public RectTransform rectTransformPlacementForSettings;
    public List<RectTransform> togglesInGroup; // сделали public для сохранения

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

        EventBus.Instance.ToggleSonicOfSettingsMenuWasToggled.AddListener(ButtonSonicOfSettingsMenuToggled);
        EventBus.Instance.ToggleGameOfSettingsMenuWasToggled.AddListener(ButtonGameOfSettingsMenuToggled);
        EventBus.Instance.ToggleVideoOfSettingsMenuWasToggled.AddListener(ButtonVideoOfSettingsMenuToggled);
        Debug.Log("Чё за параша?");

    }
    void Start()
    {
        SaveLoadManager.Instance.ImplementStoredSettingsToggle();
        
    }


    // Update is called once per frame
    void Update()
    {

    }

    public void ButtonSonicOfSettingsMenuToggled(bool wasToggled, RectTransform rectTransformToggle)
    {
        if (wasToggled) // пока что будет реакция только на прожатие тумблера, на отжатие ничего
        {
            AdjustSettingsUI(wasToggled, rectTransformToggle, _objectSonicSettingsPanel);
        }

        //Debug.Log("Ну и чё?132123");
    }
    public void ButtonGameOfSettingsMenuToggled(bool wasToggled, RectTransform rectTransformToggle)
    {
        if (wasToggled) // пока что будет реакция только на прожатие тумблера, на отжатие ничего
        {
            Debug.Log("hgkhghkgkhgkhgkhk");
            AdjustSettingsUI(wasToggled, rectTransformToggle, _objectGameSettingsPanel);
        }

        //Debug.Log("Ну и чё?ываываыва");
    }
    public void ButtonVideoOfSettingsMenuToggled(bool wasToggled, RectTransform rectTransformToggle)
    {
        if (wasToggled) // пока что будет реакция только на прожатие тумблера, на отжатие ничего
        {
            AdjustSettingsUI(wasToggled, rectTransformToggle, _objectVideoSettingsPanel);

        }

        //Debug.Log("Ну и чё?");
    }

    // отжимаем прошедшый нашатый тумблер, если сейчас мы нажали другой в менюшке
    private void ControllOnlyOneToggledToggleInGroup(RectTransform rectTransformCurrentToggle)
    {
        if (_rectTransformLastToggle != null)
        {
            _rectTransformLastToggle.gameObject.GetComponent<ToggleFixed>().IsToggled = false;
        }
        if (rectTransformCurrentToggle != _rectTransformLastToggle) // короче, эта проверка нужна чтоб при двойном Awake _rectTransformLastToggle был всё равно null
            _rectTransformLastToggle = rectTransformCurrentToggle;
    }

    // деактивируем панель с настройками, которая появлялась при прошлом нажатии соответствующего тумблера
    private void ControllOnlyOneSettingsPanelEnabled(GameObject objectCurrentPanel)
    {
        if (_objectLastSettingsPanel != null)
        {
            _objectLastSettingsPanel.SetActive(false);
        }
        if (objectCurrentPanel != _objectLastSettingsPanel) // короче, эта проверка нужна чтоб при двойном Awake _objectLastSettingsPanel был всё равно null
            _objectLastSettingsPanel = objectCurrentPanel;
    }
    // хз зачем мы сюда передали параметр wasToggled, но пущай будет
    private void AdjustSettingsUI(bool wasToggled, RectTransform rectTransformToggle, GameObject objectCurrentPanel)
    {
        ControllOnlyOneToggledToggleInGroup(rectTransformToggle);
        objectCurrentPanel.SetActive(true);
        ControllOnlyOneSettingsPanelEnabled(objectCurrentPanel);
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
    }

}
