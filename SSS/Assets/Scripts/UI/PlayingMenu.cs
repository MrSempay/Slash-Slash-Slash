using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayingMenu : MonoBehaviour
{
    public void ExitToMainMenu()
    {
        Time.timeScale = 1;
        GameManager.Instance.ChangeSceneTroughDialogue(C.NameScene.MainMenu);
    }

    public void ExitGame()
    {
        //Debug.Log("Игра закрылась");
        Application.Quit();
    }


}
