using TMPro;
using UnityEngine;
using static EnemyNearDetector;
using static ScoreManager;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject playMenu;
    [SerializeField] private GameObject settingsMenu;
    [SerializeField] private TextEdit currentLevelUI;
    [SerializeField] private TextEdit currentLevelUpUI;
    [SerializeField] private TextEdit currentComboUI;
    [SerializeField] private TextEdit currentRankUI;
    [SerializeField] private TextEdit CurrentScoreUI;
    [SerializeField] private TextEdit currentMoneyUI;
    [SerializeField] private TextEdit currentExperienceUI;
    [SerializeField] private Player player;

    public void Awake()
    {
        //Debug.Log("CERFFFFFFFFFFFFFFFFFFFFFFFF");

        player.OnExperienceChanged += ChangeExperienceTextUI;
        player.OnMoneyChanged += ChangeMoneyTextUI;
        player.OnLevelChanged += ChangeLevelTextUI;
        player.OnLevelUpChanged += ChangeLevelUpTextUI;
        //player.OnScoreChanged += ChangeScoreTextUI;
        //player.OnKillComboChanged += ChangeComboTextUI;

        EventBus.Instance.OnKillKomboWasChanged.AddListener(ChangeComboTextUI);
        EventBus.Instance.OnRankWasChanged.AddListener(ChangeRankTextUI);
        EventBus.Instance.OnScoreWasChanged.AddListener(ChangeScoreTextUI);
    }
    public void OpenOrClosePlayMenu()
    {
        //Debug.Log(settingsMenu.activeSelf);
        GameManager.Instance.PauseGame(!(playMenu.activeSelf || settingsMenu.activeSelf)); // если ни одна менюшка не активна, то значит нужна пауза, иначе снимаем еЄ

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
    private void ChangeRankTextUI(STYLE_RANK rank)
    {
        currentRankUI.SetNotLocalizableText(rank.ToString());
    }  
    private void ChangeScoreTextUI(int score)
    {
        CurrentScoreUI.SetNotLocalizableText(score.ToString());
    }

    private void OnDestroy()
    {
            Debug.Log("≈бл€ бл€доносна€");
        player.OnExperienceChanged -= ChangeExperienceTextUI;
        player.OnMoneyChanged -= ChangeMoneyTextUI;
        player.OnLevelChanged -= ChangeLevelTextUI;
        player.OnLevelUpChanged -= ChangeLevelUpTextUI;
        player.OnKillComboChanged -= ChangeComboTextUI;
    }
}
