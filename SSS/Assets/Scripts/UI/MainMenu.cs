using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public static MainMenu instance;

    [SerializeField] private RectTransform notificationPlacement;

    [NonSerialized] public string nameOfMainMusicTeam = "Project_1";

    public AvailableLevelSet availableLevelSet;


    private void Awake()
    {
        if (instance != null && instance != this) // инициализируем instance в дочернем классе
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        GameManager.Instance.Initialize(); 

        GameManager.Instance.notificationPlacement = notificationPlacement;
    }
    private void Start()
    {
        SettingsMenu[] allObjects = Resources.FindObjectsOfTypeAll<SettingsMenu>();
        allObjects[0].Awake(); // ну и фигн€, нельз€ к Instance обратитьс€, бо он инициализируетс€ у нас в Awake
        SettingsMenu.Instance.OnEnable();
        SettingsMenu.Instance.Start();

        //SaveLoadManager.Instance.ImplementStoredSettings();
        //SaveLoadManager.Instance.LoadSettingsFromFile();
        //SaveLoadManager.Instance.ImplementStoredSettings();
        AudioManager.Instance.StartCertainMusicInLoop("STAND");
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

    private void OnDestroy()
    {
        instance = null;
    }

}
