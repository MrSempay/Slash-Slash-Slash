using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayingMenu : MonoBehaviour
{
    public void ExitToMainMenu()
    {
        GameManager.Instance.ChangeSceneToDialogue(C.NameScene.MainMenu);
    }

    public void ExitGame()
    {
        Debug.Log("Игра закрылась");
        Application.Quit();
    }


}
