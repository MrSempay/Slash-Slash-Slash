using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject playMenu;
    [SerializeField] private GameObject settingsMenu;
    [SerializeField] private TextEdit currentLevelUI;
    [SerializeField] private TextEdit currentLevelUpUI;
    [SerializeField] private TextEdit currentComboUI;
    [SerializeField] private TextEdit currentMoneyUI;
    [SerializeField] private TextEdit currentExperienceUI;
    [SerializeField] private Player player;

    public void Awake()
    {
        player.OnExperienceChanged += ChangeExperienceTextUI;
        player.OnMoneyChanged += ChangeMoneyTextUI;
        player.OnLevelChanged += ChangeLevelTextUI;
        player.OnLevelUpChanged += ChangeLevelUpTextUI;
        player.OnKillComboChanged += ChangeComboTextUI;
    }
    public void OpenOrClosePlayMenu()
    {
        //Debug.Log(settingsMenu.activeSelf);
        GameManager.Instance.PauseGame(!(playMenu.activeSelf || settingsMenu.activeSelf)); // если ни одна менюшка не активна, то значит нужна пауза, иначе снимаем её

        if (settingsMenu.activeSelf)
        {
            settingsMenu.SetActive(false);
            return;
        }
        playMenu.SetActive(!playMenu.activeSelf);
    }

    private void ChangeLevelTextUI(int level)
    {
        currentLevelUI.SetNotLocalizableText(level.ToString());
    }
    
    private void ChangeLevelUpTextUI(int levelUp)
    {
        currentLevelUpUI.SetNotLocalizableText(levelUp.ToString());
    }

    private void ChangeMoneyTextUI(float money)
    {
        currentMoneyUI.SetNotLocalizableText(money.ToString());
    }

    private void ChangeExperienceTextUI(float experience)
    {
        currentExperienceUI.SetNotLocalizableText(experience.ToString() + "/" + player.experienceToNextLevel.ToString());
    }
    
    private void ChangeComboTextUI(int combo)
    {
        currentComboUI.SetNotLocalizableText(combo.ToString());
    }

    private void OnDestroy()
    {
        player.OnExperienceChanged -= ChangeExperienceTextUI;
        player.OnMoneyChanged -= ChangeMoneyTextUI;
        player.OnLevelChanged -= ChangeLevelTextUI;
        player.OnLevelUpChanged -= ChangeLevelUpTextUI;
        player.OnKillComboChanged -= ChangeComboTextUI;
    }

}
