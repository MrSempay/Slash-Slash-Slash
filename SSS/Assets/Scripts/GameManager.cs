using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using static DialogueArea;
using static GameManager;
using static ScoreManager;
using static StaticClassForAdditionalFunctions;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;

    private string _nameCurrentScene;
    private string _nameTargetScene;
    private string _pathToFolderWithPrefubs = C.Paths.PrefabDialogueWindowForPlayer;
    private GameObject _prefubPlayerDialogue;
    private LiftGammaGain _liftGammaGain;

    public GameObject prefubAppearingSprite;
    public CustomCombo prefubCustomCombo;

    public delegate void DialogueStarted(PlayerDialogue sciptPlayerDialogue); // шаблон функции
    public event DialogueStarted onDialogueStarted;         // экземляр(?) функции/сигнала(?)

    public string nameDialogueCurrent;
    public DataWrapperSettings dataWrapperSettings = new(); // оболочка настроек для последующей загрузки сохранённых настроек. При каждом сохранении настроек перезаписываем
                                                            // данное поле
    public CurrentSettings currentSettings;
    public PlayFabManager playFabManager;
    public LocalizationManager localizationManager;

    [System.Serializable] public class CurrentSettings
    {
        private static CurrentSettings _instance; // Сделай _instance статическим

        public LiftGammaGain _liftGammaGain;

        public bool vibrationOn = true;
        public bool cameraShakingOn = true;
        public float volumeMusic = 1;
        public float volumeEffects = 1;
        public float volumeBrightness = 1;
        public ENUM orientation = ENUM.Horizontal;
        public ENUM language; //установится в значение по умолчанию в методе Start GameManager, чтоб всегда применялись настройки по умполчанию

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
        public ENUM Orientation
        {
            get { return orientation; }
            set
            {
                orientation = value;
                switch (value)
                {
                    case ENUM.Horizontal:
                        Screen.orientation = ScreenOrientation.LandscapeLeft;
                        break;

                    case ENUM.Vertical:
                        Screen.orientation = ScreenOrientation.Portrait;
                        break;
                }
            }
        }
        public ENUM Language
        {
            get { return language; }
            set
            {
                //Debug.Log(value);
                language = value;
                GameManager.Instance.localizationManager.SetLanguage(value);
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
                AudioManager.Instance._audioMusicComponent.volume = value;
            }
        }
        public float VolumeEffects
        {
            get { return volumeEffects; }
            set
            {
                volumeEffects = value;
                AudioManager.Instance._audioEffectsComponent.volume = value;
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
            if (_instance == null)
            {
                var obj = new GameObject("GameManager");
                _instance = obj.AddComponent<GameManager>();
                DontDestroyOnLoad(obj);
            }
            return _instance;
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

        prefubAppearingSprite = Resources.Load<GameObject>(C.Paths.PrefabAppearingSprite);
        prefubCustomCombo = Resources.Load<CustomCombo>(C.Paths.PrefubCustomCombo);

        currentSettings = CurrentSettings.Instance; // создаём объект настроек и получаем на него ссылку
        PlayFabManager.Instance.Initialize(); // создаём объект PlayFabManager
        localizationManager = LocalizationManager.Instance; // создаём менеджер локализации
        SaveLoadManager.Instance.Initialize(); // просто создаём наш менеджер по управлению загрузки/сохранения сразу же, как только создаётся у нас GameManager
        
        Volume volumeRender = gameObject.AddComponent<Volume>();
        VolumeProfile profile = Resources.Load<VolumeProfile>(C.Paths.IboPostProcessProfile);
        volumeRender.sharedProfile = profile;
        if (volumeRender.sharedProfile.TryGet<LiftGammaGain>(out var liftGammaGain))
        {
            currentSettings._liftGammaGain = liftGammaGain;
        }
        SaveLoadManager.Instance.LoadSettingsFromFile();
        AudioManager.Instance.Initialize();

    }

    void Start()
    {
        // Игнорируем столкновения между слоем "Enemy" и самим собой. Происходит игнорирование также всех зон/коллайдеров для данного слоя (слой можно назначить как для родительского,
        // так и для всех объектов. Если у объекта изменить слой у одного из дочерних элементов, будет происходить детекция коллизий коллайдеров и зон только для этого элемента
        _prefubPlayerDialogue = Resources.Load<GameObject>(_pathToFolderWithPrefubs);
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Enemy"), LayerMask.NameToLayer("Enemy"));
        currentSettings.Language = ENUM.Russian;
    }

    // вызывается в текущей цели (не диалоговой!) для перехода в диалоговоую сцену и определения имени диалога, который будет подгружен на диалоговоую сцену
    public void ChangeSceneToDialogue(string nameTargetScene)
    {

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
        //Debug.Log(_prefubPlayerDialogue);
        PlayerDialogue sciptPlayerDialogue = Instantiate(_prefubPlayerDialogue, rectTransformPositionDialogue.position, rectTransformPositionDialogue.rotation, UI).GetComponent<PlayerDialogue>();
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

    public void PauseGame(bool setPause)
    {
        Time.timeScale = setPause ? 0 : 1;
    }
}