using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject playMenu;
    [SerializeField] private GameObject settingsMenu;
    [SerializeField] private TextMeshProUGUI currentLevelUI;
    [SerializeField] private TextMeshProUGUI currentMoneyUI;
    [SerializeField] private TextMeshProUGUI currentExperienceUI;
    [SerializeField] private Player player;

    public void Awake()
    {
        player.OnExperienceChanged += ChangeExperienceTextUI;
        player.OnMoneyChanged += ChangeMoneyTextUI;
        player.OnLevelChanged += ChangeLevelTextUI;
    }
    public void OpenOrClosePlayMenu()
    {
        if (settingsMenu.activeSelf)
        {
            settingsMenu.SetActive(false);
            return;
        }
        playMenu.SetActive(!playMenu.activeSelf);
    }

    private void ChangeLevelTextUI(int level)
    {
        currentLevelUI.text = "Level: " + level.ToString();
    }

    private void ChangeMoneyTextUI(float money)
    {
        currentMoneyUI.text = "Money: " + money.ToString();
    }

    private void ChangeExperienceTextUI(float experience)
    {
        currentExperienceUI.text = "Expe: " + experience.ToString() + "/" + player.experienceToNextLevel;
    }


}
