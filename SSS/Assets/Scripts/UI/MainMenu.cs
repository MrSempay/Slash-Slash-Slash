using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [NonSerialized] public string nameOfMainMusicTeam = "Project_1";

    private void Awake()
    {
        GameManager.Instance.Initialize();
    }
    private void Start()
    {
        SettingsMenu[] allObjects = Resources.FindObjectsOfTypeAll<SettingsMenu>();
        allObjects[0].Awake(); // ну и фигн€, нельз€ к Instance обратитьс€, бо он инициализируетс€ у нас в Awake
        SaveLoadManager.Instance.ImplementStoredSettings();
        //Debug.Log("Ёто чЄ за ебун€ча€ параша?");
        //AudioManager.Instance.StartMusic(nameOfMainMusicTeam); // - что тут делать пока что хз
    }

    public void StartGame()
    {
        GameManager.Instance.ChangeSceneToDialogue(C.NameScene.Level1);
        StaticClassForAdditionalFunctions.Vibrate();
        //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1); // следующа€ сцена в списке загружаемых дл€ настроек сборки (на 14.02 - Sample Scene)
    }

    public void ExitGame()
    {
        Debug.Log("»гра закрылась");
        Application.Quit();
    }

}
