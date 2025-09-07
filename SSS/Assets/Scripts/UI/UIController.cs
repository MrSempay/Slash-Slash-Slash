using UnityEngine;

public class UIController : MonoBehaviour
{

    [SerializeField] protected GameObject playMenu;
    [SerializeField] protected GameObject settingsMenu;
    public void OpenOrClosePlayMenu()
    {
        //Debug.Log(settingsMenu.activeSelf);
        GameManager.Instance.PauseGame(!(playMenu.activeSelf || settingsMenu.activeSelf)); // если хотя бы одна менюшка активна, то значит нужна пауза, иначе снимаем её

        if (settingsMenu.activeSelf)
        {
            settingsMenu.SetActive(false);
            return;
        }
        playMenu.SetActive(!playMenu.activeSelf);
    }
}
