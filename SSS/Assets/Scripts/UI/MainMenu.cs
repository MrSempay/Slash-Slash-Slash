using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static StaticClassForAdditionalFunctions;

public class MainMenu : MonoBehaviour
{
    public static MainMenu instance;

    public AvailableLevelSet availableLevelSet;

    [NonSerialized] public string nameOfMainMusicTeam = "Project_1";

    private Vector3 _initialPositionMenu;
    private Button _buttonTitles;

    [SerializeField] private RectTransform _notificationPlacement;
    [SerializeField] private Button _prefubTitlesButton;
    [SerializeField] private RectTransform _rtMenu;
    [SerializeField] private RectTransform _transformPositionMenuVerticalOrientation;
    private void Awake()
    {
        if (instance != null && instance != this) // инициализируем instance в дочернем классе
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        GameManager.Instance.Initialize();

        GameManager.Instance.notificationPlacement = _notificationPlacement;
        GameManager.Instance.OnMaxReachedLevelWasChanged += CheckWasReachedEndOfGame;

        availableLevelSet.OnStartLevel += StartingGameplay;

        _initialPositionMenu = transform.position;

        if (GameManager.Instance.MaxReachedLevel == GameManager.Instance.orderLevels.Count - 1) // если достигли последнего уровня
        {
            //Debug.LogWarning("FFFSAFASFASFasf");
            _buttonTitles = Instantiate(_prefubTitlesButton, _rtMenu); // место находит автоматически
        }
    }
    private void Start()
    {
        SettingsMenu[] allObjects = Resources.FindObjectsOfTypeAll<SettingsMenu>();
        allObjects[0].Awake(); // ну и фигня, нельзя к Instance обратиться, бо он инициализируется у нас в Awake
        //Debug.Log(allObjects[0]);
        //Debug.Log(allObjects[0].GetInstanceID());
        //Debug.Log(SettingsMenu.Instance);
        SettingsMenu.Instance.OnEnable();
        SettingsMenu.Instance.Start();

        //SaveLoadManager.Instance.ImplementStoredSettings();
        //SaveLoadManager.Instance.LoadSettingsFromFile();
        //SaveLoadManager.Instance.ImplementStoredSettings();
        AudioManager.Instance.StartCertainMusicInLoop("MainMenuTheme");

        availableLevelSet.UpdateLevelSet();
        //Debug.Log("Это чё за ебунячая параша?");
        //AudioManager.Instance.StartMusic(nameOfMainMusicTeam); // - что тут делать пока что хз  
    }

    public void mda(TextEdit mda)
    {

    }
    //public void StartGame()
    //{
    //    //GameManager.Instance.ChangeSceneTroughDialogue(C.NameScene.Level1);
    //    GameManager.Instance.GoToRequiredLevel();
    //    StaticClassForAdditionalFunctions.Vibrate();
    //    //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1); // следующая сцена в списке загружаемых для настроек сборки (на 14.02 - Sample Scene)
    //}

    public void ExitGame()
    {
        Debug.Log("Игра закрылась");
        Application.Quit();
    }

    public void OrientationWasChanged(LANGUAGE orientation)
    {
        Debug.Log(" ну и шо за хрень это?");
        Debug.Log(orientation);
        switch (orientation)
        {
            case LANGUAGE.Vertical:
                transform.position = _transformPositionMenuVerticalOrientation.position;
                break;
            case LANGUAGE.Horizontal:
                transform.position = _initialPositionMenu;

                break;
        }
    }


    private void StartingGameplay()
    {
        GameManager.Instance.currentSettings.Orientation = LANGUAGE.Horizontal;
    }
    private void CheckWasReachedEndOfGame()
    {
        if (GameManager.Instance.MaxReachedLevel == GameManager.Instance.orderLevels.Count - 1 && _buttonTitles == null) // если достигли последнего уровня
        {
            _buttonTitles = Instantiate(_prefubTitlesButton, _rtMenu); // место находит автоматически
        }
    }

    private void OnDestroy()
    {
        if (availableLevelSet != null)
        {
            availableLevelSet.OnStartLevel -= StartingGameplay;
        }
        instance = null;
    }


}
