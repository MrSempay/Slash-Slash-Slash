using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using static StaticClassForAdditionalFunctions;

public class MainMenu : MonoBehaviour
{
    public static MainMenu instance;

    public AvailableLevelSet availableLevelSet;
    [NonSerialized] public string nameOfMainMusicTeam = "Project_1";

    private Vector3 _initialPositionMenu;
    [SerializeField] private RectTransform _notificationPlacement;
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

        availableLevelSet.OnStartLevel += StartingGameplay;

        _initialPositionMenu = transform.position;
    }
    private void Start()
    {
        SettingsMenu[] allObjects = Resources.FindObjectsOfTypeAll<SettingsMenu>();
        allObjects[0].Awake(); // ну и фигн€, нельз€ к Instance обратитьс€, бо он инициализируетс€ у нас в Awake
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
        //Debug.Log("Ёто чЄ за ебун€ча€ параша?");
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
    //    //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1); // следующа€ сцена в списке загружаемых дл€ настроек сборки (на 14.02 - Sample Scene)
    //}

    public void ExitGame()
    {
        Debug.Log("»гра закрылась");
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

    private void OnDestroy()
    {
        if (availableLevelSet != null)
        {
            availableLevelSet.OnStartLevel -= StartingGameplay;
        }
        instance = null;
    }


}
