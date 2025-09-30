using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static DialogueArea;
using static GameManager;
using static ScoreManager;
using static StaticClassForAdditionalFunctions;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;
    private static bool _isShuttingDown = false;

    private string _nameCurrentScene;
    private string _nameTargetScene;
    private GameObject _prefubPlayerDialogue;
    private LiftGammaGain _liftGammaGain;

    [SerializeField] private int _maxReachedLevel = 0; 

    public readonly List<string> orderLevels = new List<string> { "Level1", "Level2", "Level3", "Level4", "Level5", };

    public GameObject prefubAmmunition;
    public GameObject prefubSpell;
    public GameObject prefubAppearingSprite;
    public GameObject prefubAppearingText;
    public GameObject prefubAppearingNotification;
    public GameObject prefubTextButton;
    public GameObject prefubTextButtonPanelChoose;
    public GameObject prefubTextButtonScaled;
    public CustomCombo prefubCustomCombo;
    public EquipmentInfoPanel prefubEquipmentInfoPanel;
    public PlaceForEquipment prefubPlaceForEquipment;
    public int currentLevelInOrder = 0;

    public delegate void DialogueStarted(PlayerDialogue sciptPlayerDialogue); // шаблон функции
    public event DialogueStarted onDialogueStarted;         // экземляр(?) функции/сигнала(?)

    public string nameDialogueCurrent;
    public WrapperGlobal wrapperGlobal = new(); // оболочка для всех данных, которые будут сохранятся и загружаться на локальном устройстве
    public CurrentSettings currentSettings;
    public PlayFabManager playFabManager;
    public LocalizationManager localizationManager;
    public TMP_FontAsset globalFont;
    public RectTransform notificationPlacement;

    [System.Serializable] public class CurrentSettings
    {
        private static CurrentSettings _instance; // Сделай _instance статическим

        public LiftGammaGain _liftGammaGain;
        public bool isLoadingSettings;
        public bool wasUploaded = false;

        public bool vibrationOn = true;
        public bool cameraShakingOn = true;
        public bool showNotifications = true;
        public float volumeMusic = 0.5f;
        public float volumeEffects = 0.5f;
        public float volumeBrightness = 0.5f;
        public LANGUAGE orientation = LANGUAGE.Horizontal;
        public LANGUAGE language = LANGUAGE.Russian; //установится в значение по умолчанию в методе Start GameManager, чтоб всегда применялись настройки по умполчанию
        public string displayName = ""; 
        public string email = "";
        public string password = "";

        public static CurrentSettings Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new CurrentSettings(); // Создаем экземпляр при необходимости
                }
                return _instance;
            }
        }
        public LANGUAGE Orientation
        {
            get { return orientation; }
            set
            {
                if (value != orientation) // чтоб не пытаться установить в то же значение ориентацию, в котором она уже находится, ибо багнет UI-ку. Нужно ещё тут устанавливать, ибо не всегда
                                          // изменям через SettingsMenu
                {
                    orientation = value;
                    switch (value)
                    {
                        case LANGUAGE.Horizontal:
                            Screen.orientation = ScreenOrientation.LandscapeLeft;
                            break;

                        case LANGUAGE.Vertical:
                            Screen.orientation = ScreenOrientation.Portrait;
                            break;
                    }

                    MainMenu.instance?.OrientationWasChanged(value);
                    SettingsMenu.Instance.parameterOrientation.SetValue();
                }
            }
        }
        public LANGUAGE Language
        {
            get { return language; }
            set
            {
                language = value;
                GameManager.Instance.localizationManager.SetLanguage(value);
            }
        }
        public bool ShowNotifications
        {
            get { return showNotifications; }
            set
            {
                showNotifications = value;
                if (!value)
                {
                    GameManager.Instance.DestroyAllNotifications();
                }
            }
        }

        public float VolumeBrightness
        {
            get { return volumeBrightness; }
            set
            {
                volumeBrightness = value;
                _liftGammaGain.gain.value = new Vector4(-0.25f + value, -0.25f + value, -0.25f + value, -0.25f + value); // да, магические константы
            }
        }
        public float VolumeMusic
        {
            get { return volumeMusic; }
            set
            {
                volumeMusic = value;
                AudioManager.Instance.audioMusicComponent.volume = value;
            }
        }
        public float VolumeEffects
        {
            get { return volumeEffects; }
            set
            {
                volumeEffects = value;
                AudioManager.Instance.audioEffectsComponent.volume = value;

                foreach (var objAudioSourcesCluster in AudioManager.Instance.dictionaryObjectsAndTheirAudioSourcesByTypes.Values)
                {
                    foreach (AudioSource audioSource in objAudioSourcesCluster.Values)
                    {
                        audioSource.volume = value;                        
                    }
                }
            }
        }
        public string DisplayName
        {
            get { return displayName; }
            set
            {
                displayName = value;
                PlayFabManager.Instance.GetDisplayName(value);
                if (isLoadingSettings && value != null)
                {
                    SettingsMenu.Instance.parameterInternetSettings.DisplayNameLoaded = value;
                }
            }
        }
        public string Email
        {
            get { return email; }
            set
            {
                email = value;
                PlayFabManager.Instance.GetUserEmail(value);
                if (isLoadingSettings && value != null)
                {
                    SettingsMenu.Instance.parameterInternetSettings.EmailLoaded = value;
                }
            }
        }
        public string Password
        {
            get { return password; }
            set
            {
                password = value;
                PlayFabManager.Instance.GetUserPassword(value);
                if (isLoadingSettings && value != null)
                {
                    SettingsMenu.Instance.parameterInternetSettings.PasswordLoaded = value;
                }
            }
        }

        // Приватный конструктор - запрещает создание экземпляров класса извне
        private CurrentSettings()
        {
            //Debug.Log(this);
            // Инициализация синглтона (если необходимо)
        }
    } // вообще надо придумать, как изначально установить всю визуализацию в эти настройки по умолчанию

    // при первом обращении к этому свойству (а более не к чему в начале) создастся экземпляр класса GlobalGameScript, запишется в _instance и вернёт ссылку на этот
    // экземпляр. При повторных обращениях будет возвращать ссылку на этот же экземпляр (у нас ибо static поле _instance, применится ко всему классу), static же для
    // свойства Instance нужен для того, чтоб можно было изначально создать экземпляр данного класса. Далее в Awake мы проверяем, существует ли уже экземпляр
    // данного класса и равен ли он объекту, из которого вызывается Awake, если да, то сохраням ссылку на него в _instance (вообще, эта логика в Awake нужно для того,
    // чтобы не было проблем при ручном создании (ну мало ли) данного синглтона. Второй раз создать его не даст в любом случае.

    public static GameManager Instance
    {
        get
        {
            if (_instance == null && !_isShuttingDown)
            {
                var obj = new GameObject("GameManager");
                _instance = obj.AddComponent<GameManager>();
                DontDestroyOnLoad(obj);
            }
            return _instance;
        }
    }

    public int MaxReachedLevel
    {
        get { return _maxReachedLevel; }
        set
        {
            if (value > _maxReachedLevel)
            {
                _maxReachedLevel = value;
            }

            MainMenu.instance?.availableLevelSet.UpdateLevelSet();
        }
    }

    // метод вообще ничего не делает, но как-то инициализировать наш синглтон надо, создавать переменную и присваивать ей ненужную ссылку на наш объект желания нет. 
    // Увы, просто GameManager.Instance сделать нельзя
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

        prefubAppearingSprite = Resources.Load<GameObject>(C.Paths.PrefubAppearingSprite);
        prefubAppearingText = Resources.Load<GameObject>(C.Paths.PrefubAppearingText);
        prefubAppearingNotification = Resources.Load<GameObject>(C.Paths.PrefubAppearingNotification);
        prefubCustomCombo = Resources.Load<CustomCombo>(C.Paths.PrefubCustomCombo);
        prefubPlaceForEquipment = Resources.Load<PlaceForEquipment>(C.Paths.PrefubPlaceForEquipment);
        prefubAmmunition = Resources.Load<GameObject>(C.Paths.PrefubAmmunition);
        prefubSpell = Resources.Load<GameObject>(C.Paths.PrefubSpell);
        prefubTextButton = Resources.Load<GameObject>(C.Paths.PrefubTextButton);
        prefubTextButtonPanelChoose = Resources.Load<GameObject>(C.Paths.PrefubTextButtonPanelChoose);
        prefubTextButtonScaled = Resources.Load<GameObject>(C.Paths.PrefubTextButtonBigScaled);
        prefubEquipmentInfoPanel = Resources.Load<EquipmentInfoPanel>(C.Paths.PrefubEquipmentInfoPanel);
        _prefubPlayerDialogue = Resources.Load<GameObject>(C.Paths.PrefubDialogueWindowForPlayer);

        globalFont = Resources.Load<TMP_FontAsset>(C.Paths.FontMonocraft);

        currentSettings = CurrentSettings.Instance; // создаём объект настроек и получаем на него ссылку
        PlayFabManager.Instance.Initialize(); // создаём объект PlayFabManager
        ButtonTextPanelChoose.Initialize(); // там подгружаем все изображения для кнопок панели и оттуда будем их тянуть
        YandexMobileAdsInterstitialDemoScript.Instance.Initialize(); // создаём объект YandexMobileAdsInterstitialDemoScript
        //GlobalClickSound.Instance.Initialize(); 
        SyncManager sm = SyncManager.Instance;
        localizationManager = LocalizationManager.Instance; // создаём менеджер локализации
        SaveLoadManager.Instance.Initialize(); // просто создаём наш менеджер по управлению загрузки/сохранения сразу же, как только создаётся у нас GameManager
        CoroutineManager.Instance.Initialize(); // у нас бывает так, что иногда мы заканчиваем сцену, когда CoroutineManager ещё не инициализирован, но при этом некоторые объекты
                                                // пытаются остановить свои условные коротины в OnDestroy, при этом через Instance пытаясь создать объект CoroutineManager, чего, по словам
                                                // Unity, в OnDestroy (при уничтожении сцены!) лучше не делать. Оно в таком случае ошибку выдаёт. Это может возникнуть, например, когда
                                                // мы не запускаем никаких корутин при старте объектов, а только через какую-то кастомную логику, если логика не будет выполнена - CoroutineManager
                                                // не будет инициализирован, но при этом в OnDestroy мы всё равно пытаетмся от чего-то отписаться, тем самым создавая его. Вот этот код нужен для
                                                // явной инициализации данного синглтона, чтоб не ловить подобные висяки
        Volume volumeRender = gameObject.AddComponent<Volume>();
        VolumeProfile profile = Resources.Load<VolumeProfile>(C.Paths.IboPostProcessProfile);
        volumeRender.sharedProfile = profile;
        if (volumeRender.sharedProfile.TryGet<LiftGammaGain>(out var liftGammaGain))
        {
            currentSettings._liftGammaGain = liftGammaGain;
        }
        SaveLoadManager.Instance.LoadSettingsFromFile();
        SaveLoadManager.Instance.LoadGeneralLocalDataFromFile();
        SaveLoadManager.Instance.ImplementStoredGeneralData(); 
        //MainMenu.instance?.availableLevelSet?.UpdateLevelSet();
        AudioManager.Instance.Initialize();

    }

    void Start()
    {
        // Игнорируем столкновения между слоем "Enemy" и самим собой. Происходит игнорирование также всех зон/коллайдеров для данного слоя (слой можно назначить как для родительского,
        // так и для всех объектов. Если у объекта изменить слой у одного из дочерних элементов, будет происходить детекция коллизий коллайдеров и зон только для этого элемента
        
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Enemy"), LayerMask.NameToLayer("Enemy"));

        if (currentSettings.Language == default)
        {
            currentSettings.Language = currentSettings.Language;
        }   

    }

    public void ResetMaxReachedLevelToZeroAndSetNewValue(int value)
    {
        _maxReachedLevel = 0;
        MaxReachedLevel = value;
    }


    // вызывается в текущей цели (не диалоговой!) для перехода в диалоговоую сцену и определения имени диалога, который будет подгружен на диалоговоую сцену
    public void ChangeSceneTroughDialogue(string nameTargetScene)
    {
        //Debug.Log("Тут мы : " + nameTargetScene);
        _nameCurrentScene = SceneManager.GetActiveScene().name;
        _nameTargetScene = nameTargetScene;
        nameDialogueCurrent = _nameCurrentScene + "-" + nameTargetScene;

        if (SceneManager.GetActiveScene().name != C.NameScene.SceneDialogue)
        {

            SceneManager.LoadScene(C.NameScene.SceneDialogue);
        }
    }

    // всегда должен вызываться после метода ChangeScene (то есть, по идее, только в диалоговой сцене)
    public void ChangingSceneFinish()
    {
        _nameCurrentScene = _nameTargetScene; // по идее это поле нам нужно чтоб просто находить диалоги на целевой сцене. К примеру _nameCurrentScene будет указывать на папку диалогов
        SceneManager.LoadScene(_nameTargetScene);
    }
    public void StartDialogue(string nameDialogueWithFolder)
    {
        nameDialogueCurrent = nameDialogueWithFolder; // Level1/Dialogue1 - пример

        RectTransform rectTransformPositionDialogue = GameObject.Find("PositionForDialogueWindow").GetComponent<RectTransform>();
        RectTransform UI = GameObject.Find("UI").GetComponent<RectTransform>();
        PlayerDialogue sciptPlayerDialogue;
        if (Player.instance.UIUpscaledMod)
        {
            sciptPlayerDialogue = Instantiate(_prefubPlayerDialogue,
                                              rectTransformPositionDialogue.position,
                                              rectTransformPositionDialogue.rotation,
                                              rectTransformPositionDialogue).GetComponent<PlayerDialogue>();
        }
        else
        {
            sciptPlayerDialogue = Instantiate(_prefubPlayerDialogue, rectTransformPositionDialogue.position, rectTransformPositionDialogue.rotation, UI).GetComponent<PlayerDialogue>();
        }
        onDialogueStarted?.Invoke(sciptPlayerDialogue);
    }

    public AppearingSprite InvokeAppearingSprite(string nameAnimation,
                                                 Transform transformParent,
                                                 float timeDisappearing,
                                                 bool shouldBeOnlyOneSpriteInGroup,
                                                 bool shouldBeSpecifyControlPositionSpritesInGroup = false)
    {
        AppearingSprite sciptAppearingSprite = Instantiate(prefubAppearingSprite).GetComponent<AppearingSprite>();
        sciptAppearingSprite.SetProperlyAnimationAndPosition(nameAnimation, transformParent, timeDisappearing, shouldBeOnlyOneSpriteInGroup, shouldBeSpecifyControlPositionSpritesInGroup);

        return sciptAppearingSprite;
    }
    public AppearingNotification InvokeAppearingNotification(string text,
                                                             TYPE_NOTIFICATION typeNotification,
                                                             float liveTime,
                                                             bool shouldBeOnlyOneTextInGroup,
                                                             bool shouldBeSpecifyControlPositionTextsInGroup = false)
    {

        // Устанавливаем родительский Transform
        if (notificationPlacement == null)
        {
            Debug.LogError("Нет родительского объекта для уведомления");
            return null;
        }

        if (!currentSettings.ShowNotifications)
        {
            return null;
        }

        AppearingNotification sciptAppearingSprite = Instantiate(prefubAppearingNotification, notificationPlacement, false).GetComponent<AppearingNotification>();
        sciptAppearingSprite.SetProperlyPositionAndType(text, typeNotification, liveTime, shouldBeOnlyOneTextInGroup, shouldBeSpecifyControlPositionTextsInGroup);

        return sciptAppearingSprite;
    }

    public GameObject InstanceTextButton(bool isScaled, Transform parent, string baseLocalizationKey, UnityAction onClickFunction)
    {
        GameObject objectButton = Instantiate(isScaled? prefubTextButtonScaled : prefubTextButton, parent, false);
        objectButton.transform.GetChild(0).GetComponent<TextEdit>().SetBaseText(baseLocalizationKey);
        objectButton.GetComponent<Button>().onClick.AddListener(onClickFunction);
        return objectButton;
    }

    public void PauseGame(bool setPause)
    {
        Time.timeScale = setPause ? 0 : 1;
    }

    public static void DestroyObject(GameObject obj)
    {
        if (obj != null)
        {
            Destroy(obj);
        }
    }

    public void GoToRequiredLevel()
    {
        if (currentLevelInOrder == orderLevels.Count - 1) // если достигли последнего уровня
        {
            Debug.Log("Конец игры!");
        }
        else
        {
            currentLevelInOrder++;
            ChangeSceneTroughDialogue(orderLevels[currentLevelInOrder]);
        }

    }


    public void ShakeSomething(GameObject obj, float radiusShaking, float timeDuration, float tickTime, bool shouldBeAttenuation) // timeDuration = -1 для бесконечного шатания,
                                                                                                                                  // tickTime = -1 для шатания в каждом фрейме,
                                                                                                                                  // shouldBeAttenuation = true для затухания со вреиенем
                                                                                                                                  // (работает только если timeDuration != -1).
    {
        StartCoroutine(ShakeCoroutine(obj.transform, radiusShaking, timeDuration, tickTime, shouldBeAttenuation));
    }

    public void DestroyAllNotifications()
    {
        if (notificationPlacement.childCount > 0)
        {
            foreach (RectTransform rectTransofrmNotification in notificationPlacement)
            {
                Destroy(rectTransofrmNotification.gameObject);
            }
        }
    }


    private IEnumerator ShakeCoroutine(Transform objTransform, float radiusShaking, float timeDuration, float tickTime, bool shouldBeAttenuation)
    {
        float elapsed = 0.0f;
        Vector3 initialLocalPositionObject = objTransform.localPosition;

        //float shakeDuration = 0.7f; // Длительность тряски
        float shakeMagnitude = 0.1f; // Интенсивность тряски

        if (timeDuration == -1)
        {
            while (true)
            {
                // Генерируем случайное смещение в пределах сферы
                float x = UnityEngine.Random.Range(-1f * radiusShaking, 1f * radiusShaking) * shakeMagnitude;
                float y = UnityEngine.Random.Range(-1f * radiusShaking, 1f * radiusShaking) * shakeMagnitude;

                if (objTransform)
                {
                    objTransform.localPosition = initialLocalPositionObject + new Vector3(x, y, 0);
                }
                else
                {
                    break;
                }

                if (tickTime == -1f)
                {
                    yield return null;
                }
                else
                {
                    yield return new WaitForSeconds(tickTime);
                }
            }
        }
        else
        {
            while (elapsed < timeDuration)
            {
                // Генерируем случайное смещение в пределах сферы
                float x = UnityEngine.Random.Range(-1f * radiusShaking, 1f * radiusShaking) * shakeMagnitude;
                float y = UnityEngine.Random.Range(-1f * radiusShaking, 1f * radiusShaking) * shakeMagnitude;

                if (objTransform)
                {
                    objTransform.localPosition = initialLocalPositionObject + new Vector3(x, y, 0);
                }
                else
                {
                    break;
                }

                elapsed += Time.deltaTime;

                //Затухание: Уменьшаем величину тряски со временем
                if (shouldBeAttenuation)
                {
                    shakeMagnitude = Mathf.Lerp(shakeMagnitude, 0, elapsed / timeDuration);
                }

                if (tickTime == -1f)
                {
                    yield return null;
                }
                else
                {
                    yield return new WaitForSeconds(tickTime);
                }
            }
        }
        if (objTransform)
        {
            objTransform.localPosition = initialLocalPositionObject; // Возвращаем объект в исходную позицию
        }
    }

    private void OnApplicationQuit()
    {
        Debug.Log("Игра закрывается!...");
        _isShuttingDown = true;
        SaveLoadManager.Instance.SaveGeneralData();
        CoroutineManager.Instance.StopAllCoroutinesFor(gameObject);
        CleanupManager.DisposeAll();
    }

    private void OnDestroy()
    {
        _isShuttingDown = true;
    }
}